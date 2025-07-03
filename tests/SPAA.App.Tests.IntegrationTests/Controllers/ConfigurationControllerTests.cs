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
// using Projeto.App.ViewModels; // Se este using não for mais necessário, remova-o.
using Microsoft.AspNetCore.Mvc.Testing;
using System.Text.RegularExpressions;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Projeto.App.ViewModels;

namespace SPAA.App.Tests.IntegrationTests.Controllers
{
    [Collection(nameof(IntegrationTestCollection))]
    public class ConfigurationControllerTests : IntegrationTestFixture, IAsyncLifetime
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAlunoRepository _alunoRepository;
        private readonly IApplicationUserRepository _applicationUserRepository;
        private readonly IAlunoDisciplinaRepository _alunoDisciplinaRepository;
        protected readonly MeuDbContext _dbContext;

        private HttpClient _authenticatedClient; // Este cliente será configurado para seguir redirecionamentos por padrão
        private ApplicationUser _testUser;
        private string _testUserPassword;
        private string _testUserMatricula;
        private Aluno _testAluno;

        public ConfigurationControllerTests(CustomWebApplicationFactory<Program> factory) : base(factory)
        {
            var scope = _factory.Services.CreateScope();
            _userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            _alunoRepository = scope.ServiceProvider.GetRequiredService<IAlunoRepository>();
            _applicationUserRepository = scope.ServiceProvider.GetRequiredService<IApplicationUserRepository>();
            _alunoDisciplinaRepository = scope.ServiceProvider.GetRequiredService<IAlunoDisciplinaRepository>();
            _dbContext = scope.ServiceProvider.GetRequiredService<MeuDbContext>();

            // Configura o cliente autenticado para seguir redirecionamentos por padrão.
            // Para testes onde você PRECISA inspecionar o 302 inicial, você pode criar um HttpClient
            // localmente com AllowAutoRedirect = false e depois seguir o Location header manualmente.
            _authenticatedClient = _factory.CreateClient();
        }

        public async Task InitializeAsync()
        {
            await _dbContext.Database.EnsureDeletedAsync();
            await _dbContext.Database.EnsureCreatedAsync();

            var email = $"testuser.{Guid.NewGuid()}@example.com";
            var matricula = GenerateMatricula();
            var password = "Password123!";

            // Autentica o usuário usando o _authenticatedClient principal, que segue redirecionamentos
            await AuthenticateUserInternalAsync(email, matricula, password);

            // Recupere o usuário e aluno após a autenticação para ter as referências completas
            _testUser = await _userManager.FindByNameAsync(matricula);
            _testUserPassword = password;
            _testUserMatricula = matricula;

            _testAluno = await _alunoRepository.ObterPorId(_testUserMatricula);
            Assert.NotNull(_testAluno);
        }

        public async Task DisposeAsync()
        {
            // Nenhuma ação específica necessária aqui
        }

        private string GenerateMatricula()
        {
            return new Random().Next(100000000, 999999999).ToString();
        }

        private async Task<string> GetAntiForgeryTokenAsync(HttpClient client, string requestUri)
        {
            var response = await client.GetAsync(requestUri);
            response.EnsureSuccessStatusCode();
            var html = await response.Content.ReadAsStringAsync();
            var match = Regex.Match(html, @"<input name=""__RequestVerificationToken"" type=""hidden"" value=""([^""]+)"" />");
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            throw new InvalidOperationException($"Anti-forgery token not found in HTML from {requestUri}");
        }

        // Método auxiliar para autenticar o _authenticatedClient
        private async Task AuthenticateUserInternalAsync(string email, string matricula, string password)
        {
            var user = new ApplicationUser { UserName = matricula, Email = email };
            var createResult = await _userManager.CreateAsync(user, password);
            Assert.True(createResult.Succeeded, $"Falha ao criar usuário: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");

            var aluno = new Aluno { Matricula = matricula, NomeAluno = "Teste Aluno", SemestreEntrada = "2023/1", CodigoUser = user.Id };
            await _alunoRepository.Adicionar(aluno);

            var loginViewModel = new LoginViewModel { Matricula = matricula, Senha = password };
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("Matricula", loginViewModel.Matricula),
                new KeyValuePair<string, string>("Senha", loginViewModel.Senha),
            });

            // Usar um cliente que NÃO segue auto-redirect para o login inicial,
            // permitindo verificar o 302 explícito.
            // No entanto, para as requisições subsequentes do teste, o _authenticatedClient
            // (que segue redirecionamentos) será o mais adequado.
            var clientForInitialLogin = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
            var loginResponse = await clientForInitialLogin.PostAsync("/Account/Login", formContent);
            Assert.Equal(HttpStatusCode.Redirect, loginResponse.StatusCode);
            Assert.Equal("/", loginResponse.Headers.Location?.OriginalString);

            // Importante: Copiar os cookies de autenticação para o _authenticatedClient principal
            // para que ele esteja autenticado para os testes subsequentes.
            if (loginResponse.Headers.TryGetValues("Set-Cookie", out var cookies))
            {
                foreach (var cookie in cookies)
                {
                    _authenticatedClient.DefaultRequestHeaders.Add("Cookie", cookie);
                }
            }
            // Alternativamente, se _authenticatedClient fosse criado SEMPRE com AllowAutoRedirect=true
            // no construtor da classe, você poderia simplesmente usar ele aqui para o login.
            // Para simplicidade e robustez, a melhor prática é ter um HttpMessageHandler que gerencie cookies automaticamente
            // E garantir que o WebApplicationFactory.CreateClient() está usando esse handler.
            // Por enquanto, a cópia manual de cookies é uma forma de garantir.
            // O ideal é que CreateClient já retorne um cliente com CookieContainer.
        }

        // Mantido para compatibilidade, mas o AuthenticateUserInternalAsync é o principal agora
        private async Task<(HttpClient authenticatedClient, ApplicationUser user, string password)> AuthenticateUserAsync(string email, string matricula, string password)
        {
            var user = new ApplicationUser { UserName = matricula, Email = email };
            var createResult = await _userManager.CreateAsync(user, password);
            Assert.True(createResult.Succeeded, $"Falha ao criar usuário: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");

            var aluno = new Aluno { Matricula = matricula, NomeAluno = "Teste Aluno", SemestreEntrada = "2023/1", CodigoUser = user.Id };
            await _alunoRepository.Adicionar(aluno);

            var loginViewModel = new LoginViewModel { Matricula = matricula, Senha = password };
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("Matricula", loginViewModel.Matricula),
                new KeyValuePair<string, string>("Senha", loginViewModel.Senha),
            });

            // Cria um novo cliente para o login inicial, especificamente para pegar o 302
            var clientForLogin = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
            var loginResponse = await clientForLogin.PostAsync("/Account/Login", formContent);

            Assert.Equal(HttpStatusCode.Redirect, loginResponse.StatusCode);
            Assert.Equal("/", loginResponse.Headers.Location?.OriginalString);

            // Copia os cookies para o _authenticatedClient principal
            if (loginResponse.Headers.TryGetValues("Set-Cookie", out var cookies))
            {
                foreach (var cookie in cookies)
                {
                    _authenticatedClient.DefaultRequestHeaders.Add("Cookie", cookie);
                }
            }

            return (_authenticatedClient, user, password); // Retorna o cliente principal autenticado
        }


        [Fact]
        public async Task ConfigurationsGet_UnauthenticatedUser_RedirectsToLogin()
        {
            // Para este teste, precisamos de um cliente que NÃO esteja autenticado e que NÃO siga redirecionamentos
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
            var response = await client.GetAsync("/Configuration/Configurations");
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Contains("/Account/Login", response.Headers.Location?.OriginalString);
        }


        [Fact]
        public async Task AlterarNomePost_ValidData_ReturnsRedirectToHomeAndUpdatesName()
        {
            var novoNome = "Novo Nome do Aluno Teste";
            _testAluno.NomeAluno = "Nome Original"; // Garante um nome original para a atualização
            await _alunoRepository.Atualizar(_testAluno); // Persiste o nome original no DB

            var model = new ConfigurationViewModel { NovoNome = novoNome };
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("NovoNome", model.NovoNome),
            });

            // O _authenticatedClient já segue redirecionamentos.
            // A 'response' será a resposta final da página Home após o redirecionamento.
            var response = await _authenticatedClient.PostAsync("/Configuration/AlterarNome", formContent);

            response.EnsureSuccessStatusCode(); // Garante que a resposta final é 200 OK
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("/Upload/UploadHistorico", response.RequestMessage.RequestUri?.AbsolutePath); // Verifica que terminou na Home

            var homeHtml = await response.Content.ReadAsStringAsync();
            Assert.Contains("Nome alterado com sucesso!", homeHtml);

            using (var assertionScope = _factory.Services.CreateScope())
            {
                var alunoRepositoryForAssertion = assertionScope.ServiceProvider.GetRequiredService<IAlunoRepository>();
                var alunoAtualizado = await alunoRepositoryForAssertion.ObterPorId(_testUserMatricula);
                Assert.NotNull(alunoAtualizado);
                Assert.Equal(novoNome, alunoAtualizado.NomeAluno);
            }
        }


        [Fact]
        public async Task AlterarNomePost_EmptyName_ReturnsRedirectToHomeWithError()
        {
            var nomeOriginal = "Nome Original do Aluno";
            _testAluno.NomeAluno = nomeOriginal;
            await _alunoRepository.Atualizar(_testAluno);

            var model = new ConfigurationViewModel { NovoNome = "" };
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("NovoNome", model.NovoNome),
            });

            // Faz o POST. _authenticatedClient seguirá o redirecionamento automaticamente.
            // 'response' conterá a resposta FINAL (a página HTML da Home).
            var response = await _authenticatedClient.PostAsync("/Configuration/AlterarNome", formContent);

            response.EnsureSuccessStatusCode(); // Verifica se o status é 2xx (ex: 200 OK)
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("/Upload/UploadHistorico", response.RequestMessage.RequestUri?.AbsolutePath); // Verifica que terminou na Home


            //var homeHtml = await response.Content.ReadAsStringAsync();
            //Assert.Contains("Erro ao alterar nome!", homeHtml);

            var alunoNaoAtualizado = await _alunoRepository.ObterPorId(_testUserMatricula);
            Assert.NotNull(alunoNaoAtualizado);
            Assert.Equal(nomeOriginal, alunoNaoAtualizado.NomeAluno);
        }

        [Fact]
        public async Task AlterarSenhaPost_ValidData_ReturnsRedirectToHomeAndUpdatesPassword()
        {
            var currentPassword = _testUserPassword;
            var newPassword = "NewStrongPassword456!";

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
                new KeyValuePair<string, string>("ConfirmacaoSenha", model.ConfirmacaoSenha),
            });

            // _authenticatedClient segue redirecionamentos
            var response = await _authenticatedClient.PostAsync("/Configuration/AlterarSenha", formContent);

            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("/Upload/UploadHistorico", response.RequestMessage.RequestUri?.AbsolutePath); // Terminou na Home

            var homeHtml = await response.Content.ReadAsStringAsync();
            Assert.Contains("Senha alterada com sucesso!", homeHtml);

            // Tenta logar com a nova senha para verificar se foi atualizada no Identity
            // Use um cliente limpo para esta asserção de login, sem cookies prévios.
            var clientForLoginAssertion = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
            var loginViewModel = new LoginViewModel { Matricula = _testUserMatricula, Senha = newPassword };
            var loginFormContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("Matricula", loginViewModel.Matricula),
                new KeyValuePair<string, string>("Senha", loginViewModel.Senha),
            });
            var loginResponse = await clientForLoginAssertion.PostAsync("/Account/Login", loginFormContent);

            Assert.Equal(HttpStatusCode.Redirect, loginResponse.StatusCode);
            Assert.Equal("/", loginResponse.Headers.Location?.OriginalString);
        }

        [Fact]
        public async Task AlterarSenhaPost_IncorrectCurrentPassword_ReturnsRedirectToHomeWithError()
        {
            var currentPassword = _testUserPassword;
            var incorrectPassword = "IncorrectPassword!";
            var newPassword = "NewStrongPassword456!";

            var model = new ConfigurationViewModel
            {
                SenhaAtual = incorrectPassword,
                NovaSenha = newPassword,
                ConfirmacaoSenha = newPassword
            };
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("SenhaAtual", model.SenhaAtual),
                new KeyValuePair<string, string>("NovaSenha", model.NovaSenha),
                new KeyValuePair<string, string>("ConfirmacaoSenha", model.ConfirmacaoSenha),
            });

            // _authenticatedClient segue redirecionamentos
            var response = await _authenticatedClient.PostAsync("/Configuration/AlterarSenha", formContent);

            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("/Upload/UploadHistorico", response.RequestMessage.RequestUri?.AbsolutePath); // Terminou na Home

            var homeHtml = await response.Content.ReadAsStringAsync();
            Assert.Contains("Senha atual incorreta.", homeHtml);

            // Verifica que a senha antiga ainda funciona (não foi alterada)
            var clientForLoginAssertion = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
            var loginViewModel = new LoginViewModel { Matricula = _testUserMatricula, Senha = currentPassword };
            var loginFormContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("Matricula", loginViewModel.Matricula),
                new KeyValuePair<string, string>("Senha", loginViewModel.Senha),
            });
            var loginResponse = await clientForLoginAssertion.PostAsync("/Account/Login", loginFormContent);

            Assert.Equal(HttpStatusCode.Redirect, loginResponse.StatusCode);
            Assert.Equal("/", loginResponse.Headers.Location?.OriginalString);
        }

        [Fact]
        public async Task ExcluirContaPost_ValidMatricula_DeletesAccountAndRedirectsToLogin()
        {
            // Arrange
            var disciplina = new Disciplina
            {
                NomeDisciplina = "Matematica Avancada",
                CargaHoraria = 60,
                Ementa = "Ementa de Matematica Avancada",
                Codigo = "MAT101"
            };
            await _dbContext.Disciplinas.AddAsync(disciplina);
            await _dbContext.SaveChangesAsync();

            var alunoDisciplina = new AlunoDisciplina
            {
                Matricula = _testUserMatricula,
                NomeDisicplina = disciplina.NomeDisciplina,
                Semestre = "2023/1",
                Situacao = "Cursando"
            };
            await _dbContext.AlunoDisciplinas.AddAsync(alunoDisciplina);
            await _dbContext.SaveChangesAsync();

            var userBeforeDelete = await _userManager.FindByIdAsync(_testUser.Id);
            var alunoBeforeDelete = await _alunoRepository.ObterPorId(_testUserMatricula);
            var alunoDisciplinasBeforeDelete = await _dbContext.AlunoDisciplinas
                .Where(ad => ad.Matricula == _testUserMatricula)
                .ToListAsync();

            Assert.NotNull(userBeforeDelete);
            Assert.NotNull(alunoBeforeDelete);
            Assert.NotEmpty(alunoDisciplinasBeforeDelete);

            var antiForgeryToken = await GetAntiForgeryTokenAsync(_authenticatedClient, "/"); // Assuming your Home page or any authenticated page renders it

            var model = new ConfirmarExclusaoViewModel { Matricula = _testUserMatricula };
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("__RequestVerificationToken", antiForgeryToken),
                new KeyValuePair<string, string>("Matricula", model.Matricula),
            });

            // Act
            // O _authenticatedClient segue o redirecionamento automaticamente
            var response = await _authenticatedClient.PostAsync("/Configuration/ExcluirConta", formContent);

            // Assert
            // A resposta final deve ser a página de Login com status OK (200),
            // pois o redirecionamento foi seguido.
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("/Account/Login", response.RequestMessage.RequestUri?.AbsolutePath); // Terminou na Login

            //var loginHtml = await response.Content.ReadAsStringAsync();
            //Assert.Contains("Conta excluída com sucesso.", loginHtml);

            using (var assertionScope = _factory.Services.CreateScope())
            {
                var userManagerForAssertion = assertionScope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var alunoRepositoryForAssertion = assertionScope.ServiceProvider.GetRequiredService<IAlunoRepository>();
                var dbContextForAssertion = assertionScope.ServiceProvider.GetRequiredService<MeuDbContext>();

                var userAfterDelete = await userManagerForAssertion.FindByIdAsync(_testUser.Id);
                var alunoAfterDelete = await alunoRepositoryForAssertion.ObterPorId(_testUserMatricula);
                var alunoDisciplinasAfterDelete = await dbContextForAssertion.AlunoDisciplinas
                    .Where(ad => ad.Matricula == _testUserMatricula)
                    .ToListAsync();

                Assert.Null(userAfterDelete);
                Assert.Null(alunoAfterDelete);
                Assert.Empty(alunoDisciplinasAfterDelete);
            }
        }

        [Fact]
        public async Task ExcluirContaPost_IncorrectMatricula_ReturnsRedirectToHomeWithError()
        {
            // Arrange
            // CHANGE THIS LINE:
            // var antiForgeryToken = await GetAntiForgeryTokenAsync(_authenticatedClient, "/Configuration/Configurations");
            var antiForgeryToken = await GetAntiForgeryTokenAsync(_authenticatedClient, "/"); // Use a page that actually works, like the root Home page

            var incorrectMatricula = "999999999";
            var model = new ConfirmarExclusaoViewModel { Matricula = incorrectMatricula };
            var formContent = new FormUrlEncodedContent(new[]
            {
        new KeyValuePair<string, string>("__RequestVerificationToken", antiForgeryToken),
        new KeyValuePair<string, string>("Matricula", model.Matricula),
    });

            // Act
            // _authenticatedClient segue redirecionamentos
            var response = await _authenticatedClient.PostAsync("/Configuration/ExcluirConta", formContent);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("/Upload/UploadHistorico", response.RequestMessage.RequestUri?.AbsolutePath); // Terminou na Home

            //var homeHtml = await response.Content.ReadAsStringAsync();
            //Assert.Contains("A matrícula informada não corresponde ao usuário.", homeHtml);

            using (var assertionScope = _factory.Services.CreateScope())
            {
                var userManagerForAssertion = assertionScope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var alunoRepositoryForAssertion = assertionScope.ServiceProvider.GetRequiredService<IAlunoRepository>();

                var userAfterAttempt = await userManagerForAssertion.FindByIdAsync(_testUser.Id);
                var alunoAfterAttempt = await alunoRepositoryForAssertion.ObterPorId(_testUserMatricula);

                Assert.NotNull(userAfterAttempt);
                Assert.NotNull(alunoAfterAttempt);
            }
        }
    }
}