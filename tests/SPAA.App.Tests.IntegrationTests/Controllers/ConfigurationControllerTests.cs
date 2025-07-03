// Caminho: SPAA.App.Tests/IntegrationTests/Controllers/ConfigurationControllerTests.cs

using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using SPAA.Data.Context;
using SPAA.Business.Models;
using Microsoft.AspNetCore.Identity;
using SPAA.App.ViewModels;
using SPAA.App.Tests.IntegrationTests.Factories;
using SPAA.Business.Interfaces.Repository;
using System;
using System.Collections.Generic;
using Projeto.App.ViewModels;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Text.RegularExpressions; // Para KeyValuePair, se for o caso. Se não, remova.

namespace SPAA.App.Tests.IntegrationTests.Controllers
{
    // Aplica a coleção de testes para que a CustomWebApplicationFactory seja compartilhada
    [Collection(nameof(IntegrationTestCollection))]
    public class ConfigurationControllerTests : IntegrationTestFixture
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IAlunoRepository _alunoRepository;
        private readonly IApplicationUserRepository _applicationUserRepository;
        private readonly IAlunoDisciplinaRepository _alunoDisciplinaRepository; // Para o teste de exclusão

        public ConfigurationControllerTests(CustomWebApplicationFactory<Program> factory) : base(factory)
        {
            // Crie um escopo de serviço para obter os serviços necessários para os testes
            // Isso garante que os serviços obtidos aqui são descartados corretamente ao final do teste.
            var scope = _factory.Services.CreateScope();
            _userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            _signInManager = scope.ServiceProvider.GetRequiredService<SignInManager<ApplicationUser>>();
            _alunoRepository = scope.ServiceProvider.GetRequiredService<IAlunoRepository>();
            _applicationUserRepository = scope.ServiceProvider.GetRequiredService<IApplicationUserRepository>();
            _alunoDisciplinaRepository = scope.ServiceProvider.GetRequiredService<IAlunoDisciplinaRepository>();

            // Limpa o banco de dados in-memory antes de cada teste (ou uma estratégia de limpeza por teste)
            // Para testes de integração, é comum limpar o DB para garantir isolamento.
            // A fábrica já faz EnsureDeleted/EnsureCreated na primeira vez.
            // Para cada teste, podemos limpar os dados específicos.
            // Ou, para simplificar, podemos confiar que a fábrica recria o DB para cada CollectionFixture.
            // Para testes de integração, muitas vezes é melhor limpar o DB no início de CADA TESTE.
            // No entanto, para fins de demonstração, vamos focar na lógica do teste.
            // Se precisar de limpeza por teste, considere um método auxiliar ou um xUnit IAsyncLifetime.
        }

        /// <summary>
        /// Gera uma string de matrícula de 9 dígitos.
        /// </summary>
        private string GenerateMatricula()
        {
            return new Random().Next(100000000, 999999999).ToString();
        }
        private async Task<string> GetAntiForgeryTokenAsync(HttpClient client, string requestUri)
        {
            var response = await client.GetAsync(requestUri);
            response.EnsureSuccessStatusCode(); // Garante que a página foi carregada com sucesso
            var html = await response.Content.ReadAsStringAsync();

            // Expressão regular para encontrar o input hidden com o nome __RequestVerificationToken
            var match = Regex.Match(html, @"<input name=""__RequestVerificationToken"" type=""hidden"" value=""([^""]+)"" />");
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            throw new InvalidOperationException($"Anti-forgery token not found in HTML from {requestUri}");
        }
        /// <summary>
        /// Auxiliar para registrar e logar um usuário para os testes autorizados.
        /// Retorna um HttpClient autenticado.
        /// </summary>
        private async Task<(HttpClient authenticatedClient, ApplicationUser user, string password)> AuthenticateUserAsync(string email, string matricula, string password)
        {
            // 1. Registra o usuário
            var user = new ApplicationUser { UserName = matricula, Email = email };
            var createResult = await _userManager.CreateAsync(user, password);
            Assert.True(createResult.Succeeded, $"Falha ao criar usuário: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");

            // 2. Cria um aluno associado
            var aluno = new Aluno { Matricula = matricula, NomeAluno = "Teste Aluno", SemestreEntrada = "2023/1", CodigoUser = user.Id };
            await _alunoRepository.Adicionar(aluno);

            // 3. Loga o usuário usando o cliente da fábrica
            var loginViewModel = new LoginViewModel { Matricula = matricula, Senha = password };
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("Matricula", loginViewModel.Matricula),
                new KeyValuePair<string, string>("Senha", loginViewModel.Senha),
            });

            // Cria um novo cliente para esta autenticação para evitar cookies de testes anteriores
            var clientForLogin = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
            var loginResponse = await clientForLogin.PostAsync("/Account/Login", formContent);
            Assert.Equal(HttpStatusCode.Redirect, loginResponse.StatusCode); // Espera redirecionamento para Home
            Assert.Equal("/", loginResponse.Headers.Location?.OriginalString);

            // Retorna o cliente autenticado (clientForLogin agora tem os cookies de autenticação)
            return (clientForLogin, user, password);
        }

        /// <summary>
        /// Testa o endpoint GET /Configuration/Configurations.
        /// Deve retornar sucesso para usuário autenticado.
        /// </summary>

        /// <summary>
        /// Testa o endpoint GET /Configuration/Configurations sem autenticação.
        /// Deve redirecionar para a página de login.
        /// </summary>
        [Fact]
        public async Task ConfigurationsGet_UnauthenticatedUser_RedirectsToLogin()
        {
            // Arrange (usa o _client padrão que não está autenticado)
            // Act
            var response = await _client.GetAsync("/Configuration/Configurations");

            // Assert
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Contains("/Account/Login", response.Headers.Location?.OriginalString);
        }

        /// <summary>
        /// Testa a alteração de nome com sucesso.
        /// </summary>
        [Fact]
        public async Task AlterarNomePost_ValidData_ReturnsRedirectToHomeAndUpdatesName()
        {
            // Arrange
            var email = $"alterarnome.{Guid.NewGuid()}@example.com";
            var matricula = GenerateMatricula();
            var password = "Password123!";
            var novoNome = "Novo Nome do Aluno";

            var (authenticatedClient, user, _) = await AuthenticateUserAsync(email, matricula, password);

            var model = new ConfigurationViewModel { NovoNome = novoNome };
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("NovoNome", model.NovoNome),
            });

            // Act
            var response = await authenticatedClient.PostAsync("/Configuration/AlterarNome", formContent);

            // Assert
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Equal("/", response.Headers.Location?.OriginalString);

            using (var assertionScope = _factory.Services.CreateScope())
            {
                var alunoRepositoryForAssertion = assertionScope.ServiceProvider.GetRequiredService<IAlunoRepository>();
                var alunoAtualizado = await alunoRepositoryForAssertion.ObterPorId(matricula);
                Assert.NotNull(alunoAtualizado);
                Assert.Equal(novoNome, alunoAtualizado.NomeAluno);
            }
        }

        /// <summary>
        /// Testa a alteração de nome com nome vazio/nulo.
        /// </summary>
        [Fact]
        public async Task AlterarNomePost_EmptyName_ReturnsRedirectToHomeWithError()
        {
            // Arrange
            var email = $"alterarnomeempty.{Guid.NewGuid()}@example.com";
            var matricula = GenerateMatricula();
            var password = "Password123!";
            var nomeOriginal = "Nome Original";

            var (authenticatedClient, user, _) = await AuthenticateUserAsync(email, matricula, password);

            // Atualiza o nome original do aluno para ter certeza que é diferente do vazio
            var aluno = await _alunoRepository.ObterPorId(matricula);
            aluno.NomeAluno = nomeOriginal;
            await _alunoRepository.Atualizar(aluno);

            var model = new ConfigurationViewModel { NovoNome = "" }; // Nome vazio
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("NovoNome", model.NovoNome),
            });

            // Act
            var response = await authenticatedClient.PostAsync("/Configuration/AlterarNome", formContent);

            // Assert
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Equal("/", response.Headers.Location?.OriginalString);

            // Verificar que o nome NÃO foi alterado no banco de dados e a mensagem de erro foi definida
            var alunoNaoAtualizado = await _alunoRepository.ObterPorId(matricula);
            Assert.NotNull(alunoNaoAtualizado);
            Assert.Equal(nomeOriginal, alunoNaoAtualizado.NomeAluno); // Nome deve permanecer o original
            // Para verificar TempData, precisaríamos ler a resposta HTML da página de redirecionamento,
            // o que é mais complexo. Por enquanto, confiamos na lógica do controller.
        }

        /// <summary>
        /// Testa a alteração de senha com sucesso.
        /// </summary>
        [Fact]
        public async Task AlterarSenhaPost_ValidData_ReturnsRedirectToHomeAndUpdatesPassword()
        {
            // Arrange
            var email = $"alterarsenha.{Guid.NewGuid()}@example.com";
            var matricula = GenerateMatricula();
            var currentPassword = "CurrentPassword123!";
            var newPassword = "NewStrongPassword456!";

            var (authenticatedClient, user, _) = await AuthenticateUserAsync(email, matricula, currentPassword);

            var model = new ConfigurationViewModel
            {
                SenhaAtual = currentPassword,
                NovaSenha = newPassword,
                ConfirmacaoSenha = newPassword
            };
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("SenhaAtual", model.SenhaAtual),
                new KeyValuePair<string, string>("NovaSenha", model.NovaSenha),
                new KeyValuePair<string, string>("ConfirmacaoNovaSenha", model.ConfirmacaoSenha),
            });

            // Act
            var response = await authenticatedClient.PostAsync("/Configuration/AlterarSenha", formContent);

            // Assert
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Equal("/", response.Headers.Location?.OriginalString);

            // Tentar logar com a nova senha para verificar a alteração
            var logoutResponse = await authenticatedClient.PostAsync("/Account/Logout", null); // Desloga o usuário atual
            Assert.Equal(HttpStatusCode.Redirect, logoutResponse.StatusCode);

            var loginClient = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
            var loginViewModel = new LoginViewModel { Matricula = matricula, Senha = newPassword };
            var loginFormContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("Matricula", loginViewModel.Matricula),
                new KeyValuePair<string, string>("Senha", loginViewModel.Senha),
            });
            var loginResponse = await loginClient.PostAsync("/Account/Login", loginFormContent);

            Assert.Equal(HttpStatusCode.Redirect, loginResponse.StatusCode);
            Assert.Equal("/", loginResponse.Headers.Location?.OriginalString);
        }

        /// <summary>
        /// Testa a alteração de senha com senha atual incorreta.
        /// </summary>
        [Fact]
        public async Task AlterarSenhaPost_IncorrectCurrentPassword_ReturnsRedirectToHomeWithError()
        {
            // Arrange
            var email = $"wrongpass.{Guid.NewGuid()}@example.com";
            var matricula = GenerateMatricula();
            var currentPassword = "CorrectPassword123!";
            var newPassword = "NewStrongPassword456!";

            var (authenticatedClient, user, _) = await AuthenticateUserAsync(email, matricula, currentPassword);

            var model = new ConfigurationViewModel
            {
                SenhaAtual = "IncorrectPassword!", // Senha atual incorreta
                NovaSenha = newPassword,
                ConfirmacaoSenha = newPassword
            };
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("SenhaAtual", model.SenhaAtual),
                new KeyValuePair<string, string>("NovaSenha", model.NovaSenha),
                new KeyValuePair<string, string>("ConfirmacaoNovaSenha", model.ConfirmacaoSenha),
            });

            // Act
            var response = await authenticatedClient.PostAsync("/Configuration/AlterarSenha", formContent);

            // Assert
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Equal("/", response.Headers.Location?.OriginalString);

            // Tentar logar com a senha antiga para verificar que não foi alterada
            var logoutResponse = await authenticatedClient.PostAsync("/Account/Logout", null);
            Assert.Equal(HttpStatusCode.Redirect, logoutResponse.StatusCode);

            var loginClient = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
            var loginViewModel = new LoginViewModel { Matricula = matricula, Senha = currentPassword }; // Tenta com a senha original
            var loginFormContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("Matricula", loginViewModel.Matricula),
                new KeyValuePair<string, string>("Senha", loginViewModel.Senha),
            });
            var loginResponse = await loginClient.PostAsync("/Account/Login", loginFormContent);

            Assert.Equal(HttpStatusCode.Redirect, loginResponse.StatusCode); // Ainda deve conseguir logar com a senha antiga
            Assert.Equal("/", loginResponse.Headers.Location?.OriginalString);
        }

        ///// <summary>
        ///// Testa a exclusão de conta com sucesso.
        ///// </summary>
        ///// <summary>
        ///// Testa a exclusão de conta com sucesso.
        ///// </summary>
        //[Fact]
        //public async Task ExcluirContaPost_ValidMatricula_DeletesAccountAndRedirectsToLogin()
        //{
        //    // Arrange
        //    var email = $"delete.{Guid.NewGuid()}@example.com";
        //    var matricula = GenerateMatricula();
        //    var password = "DeletePassword123!";

        //    var (authenticatedClient, user, _) = await AuthenticateUserAsync(email, matricula, password);

        //    // Verifique se o usuário e aluno existem antes da exclusão
        //    using (var assertionScope = _factory.Services.CreateScope())
        //    {
        //        var userManagerForAssertion = assertionScope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        //        var alunoRepositoryForAssertion = assertionScope.ServiceProvider.GetRequiredService<IAlunoRepository>();

        //        var userBeforeDelete = await userManagerForAssertion.FindByIdAsync(user.Id);
        //        var alunoBeforeDelete = await alunoRepositoryForAssertion.ObterPorId(matricula);
        //        Assert.NotNull(userBeforeDelete);
        //        Assert.NotNull(alunoBeforeDelete);
        //    }

        //    // Obtenha o token anti-forgery da página de configurações
        //    var antiForgeryToken = await GetAntiForgeryTokenAsync(authenticatedClient, "/Configuration/Configurations");

        //    var model = new ConfirmarExclusaoViewModel { Matricula = matricula };
        //    var formContent = new FormUrlEncodedContent(new[]
        //    {
        //        new KeyValuePair<string, string>("__RequestVerificationToken", antiForgeryToken), // Adicione o token
        //        new KeyValuePair<string, string>("Matricula", model.Matricula),
        //    });

        //    // Act
        //    var response = await authenticatedClient.PostAsync("/Configuration/ExcluirConta", formContent);

        //    // Assert
        //    Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        //    Assert.Equal("/Account/Login", response.Headers.Location?.OriginalString);

        //    // Verificar se o usuário e aluno foram removidos do banco de dados
        //    using (var assertionScope = _factory.Services.CreateScope())
        //    {
        //        var userManagerForAssertion = assertionScope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        //        var alunoRepositoryForAssertion = assertionScope.ServiceProvider.GetRequiredService<IAlunoRepository>();

        //        var userAfterDelete = await userManagerForAssertion.FindByIdAsync(user.Id);
        //        var alunoAfterDelete = await alunoRepositoryForAssertion.ObterPorId(matricula);
        //        Assert.Null(userAfterDelete);
        //        Assert.Null(alunoAfterDelete);
        //    }
        //}

        ///// <summary>
        ///// Testa a exclusão de conta com matrícula incorreta.
        ///// </summary>
        //[Fact]
        //public async Task ExcluirContaPost_IncorrectMatricula_ReturnsRedirectToHomeWithError()
        //{
        //    // Arrange
        //    var email = $"deletefail.{Guid.NewGuid()}@example.com";
        //    var matricula = GenerateMatricula();
        //    var password = "DeletePassword123!";

        //    var (authenticatedClient, user, _) = await AuthenticateUserAsync(email, matricula, password);

        //    // Verifique se o usuário e aluno existem antes da exclusão
        //    using (var assertionScope = _factory.Services.CreateScope())
        //    {
        //        var userManagerForAssertion = assertionScope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        //        var alunoRepositoryForAssertion = assertionScope.ServiceProvider.GetRequiredService<IAlunoRepository>();

        //        var userBeforeDelete = await userManagerForAssertion.FindByIdAsync(user.Id);
        //        var alunoBeforeDelete = await alunoRepositoryForAssertion.ObterPorId(matricula);
        //        Assert.NotNull(userBeforeDelete);
        //        Assert.NotNull(alunoBeforeDelete);
        //    }

        //    // Obtenha o token anti-forgery da página de configurações
        //    var antiForgeryToken = await GetAntiForgeryTokenAsync(authenticatedClient, "/Configuration/Configurations");

        //    var model = new ConfirmarExclusaoViewModel { Matricula = "999999999" }; // Matrícula incorreta
        //    var formContent = new FormUrlEncodedContent(new[]
        //    {
        //        new KeyValuePair<string, string>("__RequestVerificationToken", antiForgeryToken), // Adicione o token
        //        new KeyValuePair<string, string>("Matricula", model.Matricula),
        //    });

        //    // Act
        //    var response = await authenticatedClient.PostAsync("/Configuration/ExcluirConta", formContent);

        //    // Assert
        //    Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        //    Assert.Equal("/Home/Index", response.Headers.Location?.OriginalString);

        //    // Verificar que o usuário e aluno NÃO foram removidos
        //    using (var assertionScope = _factory.Services.CreateScope())
        //    {
        //        var userManagerForAssertion = assertionScope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        //        var alunoRepositoryForAssertion = assertionScope.ServiceProvider.GetRequiredService<IAlunoRepository>();

        //        var userAfterAttempt = await userManagerForAssertion.FindByIdAsync(user.Id);
        //        var alunoAfterAttempt = await alunoRepositoryForAssertion.ObterPorId(matricula);
        //        Assert.NotNull(userAfterAttempt);
        //        Assert.NotNull(alunoAfterAttempt);
        //    }
        //}
    }
}
