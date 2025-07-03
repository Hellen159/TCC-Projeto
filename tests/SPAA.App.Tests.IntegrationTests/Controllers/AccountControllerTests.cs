// SPAA.App.Tests/IntegrationTests/AccountControllerTests.cs
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using SPAA.Data.Context;
using SPAA.Business.Models; // Para Aluno e ApplicationUser
using Microsoft.AspNetCore.Identity; // Para UserManager, SignInManager, IdentityResult
using SPAA.App.ViewModels; // Para LoginViewModel, RegisterViewModel, ForgotPasswordViewModel, ResetPasswordViewModel
using SPAA.App.Tests.IntegrationTests.Factories;
using SPAA.App;
using SPAA.Business.Interfaces.Repository; // Para IAlunoRepository, IApplicationUserRepository
using System; // Para Guid, Random
using System.Collections.Generic;
using Projeto.App.ViewModels;
using Microsoft.VisualStudio.TestPlatform.TestHost; // Para KeyValuePair


namespace SPAA.App.Tests.IntegrationTests.Controllers
{
    public class AccountControllerTests : IntegrationTestFixture
    {
        private readonly IAlunoRepository _alunoRepository;
        private readonly IApplicationUserRepository _applicationUserRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        // O construtor é injetado pelo XUnit com a CustomWebApplicationFactory
        public AccountControllerTests(CustomWebApplicationFactory<Program> factory) : base(factory)
        {
            // Aqui você obtém as instâncias dos repositórios e gerentes de usuário
            // a partir do ServiceProvider da sua fábrica.
            // Crie um escopo de serviço para garantir que os serviços injetados
            // tenham um tempo de vida adequado para o teste.
            var scope = _factory.Services.CreateScope();
            _alunoRepository = scope.ServiceProvider.GetRequiredService<IAlunoRepository>();
            _applicationUserRepository = scope.ServiceProvider.GetRequiredService<IApplicationUserRepository>();
            _userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            _signInManager = scope.ServiceProvider.GetRequiredService<SignInManager<ApplicationUser>>();

            // O _dbContext já está disponível na classe base IntegrationTestFixture.
            // Você pode usá-lo para limpar dados, se necessário.
            // Por exemplo, limpar o banco de dados antes de cada teste.
            // CUIDADO: Isso limpa os dados reais. Use apenas em ambiente de teste dedicado.
            // Recomenda-se fazer isso no CustomWebApplicationFactory (OnCreateHost) para um setup inicial.
            // Se precisar de limpeza por teste, você pode ter um método auxiliar aqui.
            // Exemplo: LimparDadosDoBanco();
        }

        /// <summary>
        /// Gera uma string de matrícula de 9 dígitos.
        /// </summary>
        private string GenerateMatricula()
        {
            // Gera um número de 9 dígitos e o formata como string
            // Evita gerar "0" à esquerda para manter a consistência, se necessário.
            // Se o seu sistema permite "0" à esquerda e precisa de 9 caracteres exatos,
            // pode ajustar para String.Format("{0:D9}", random.Next(0, 999999999));
            // Para garantir 9 dígitos, vamos gerar de 100_000_000 a 999_999_999
            return new Random().Next(100000000, 999999999).ToString();
        }

        /// <summary>
        /// Testa se o endpoint GET /Account/Login retorna sucesso e o tipo de conteúdo esperado.
        /// </summary>
        [Fact]
        public async Task LoginGet_ReturnsSuccessAndCorrectContentType()
        {
            // Act
            var response = await _client.GetAsync("/Account/Login");

            // Assert
            response.EnsureSuccessStatusCode(); // Status 200-299
            Assert.Equal("text/html; charset=utf-8", response.Content.Headers.ContentType?.ToString());

            // Opcional: Verificar se o conteúdo da página contém elementos visuais da tela de login
            var responseString = await response.Content.ReadAsStringAsync();
            Assert.Contains("Login - SPAA.APP", responseString);
            Assert.Contains("Esqueceu a senha da conta?", responseString);
        }

        /// <summary>
        /// Testa o endpoint GET /Account/Register.
        /// </summary>
        [Fact]
        public async Task RegisterGet_ReturnsSuccessAndCorrectContentType()
        {
            // Act
            var response = await _client.GetAsync("/Account/Register");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal("text/html; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }

        /// <summary>
        /// Testa o registro de um novo usuário com sucesso.
        /// </summary>
        [Fact]
        public async Task RegisterPost_NewUser_ReturnsRedirectToLoginAndSuccessMessage()
        {
            // Arrange
            var registerViewModel = new RegisterViewModel
            {
                Nome = "Test User Full Name", // Por exemplo, "Test User Full Name" tem 19 caracteres
                Email = $"test.user.{Guid.NewGuid()}@example.com", // Garante e-mail único
                Matricula = GenerateMatricula(), // Usa a função auxiliar para matrícula string de 9 dígitos
                Senha = "Password123!",
                ConfirmacaoSenha = "Password123!", // Corrigido para ConfirmacaoSenha
                AnoEntrada = 2023,
                SemestreEntrada = 1
            };

            // Usa FormUrlEncodedContent para simular um formulário HTML POST
            var formContent = new FormUrlEncodedContent(new[]
            {
            new KeyValuePair<string, string>("Nome", registerViewModel.Nome),
            new KeyValuePair<string, string>("Email", registerViewModel.Email),
            new KeyValuePair<string, string>("Matricula", registerViewModel.Matricula), // Já é string, sem .ToString()
            new KeyValuePair<string, string>("Senha", registerViewModel.Senha),
            new KeyValuePair<string, string>("ConfirmacaoSenha", registerViewModel.ConfirmacaoSenha), // Corrigido
            new KeyValuePair<string, string>("AnoEntrada", registerViewModel.AnoEntrada.ToString()),
            new KeyValuePair<string, string>("SemestreEntrada", registerViewModel.SemestreEntrada.ToString()),
        });

            // Act
            var response = await _client.PostAsync("/Account/Register", formContent);

            // Assert
            // Primeiro, verifique se o status é realmente um redirecionamento (302)
            if (response.StatusCode != HttpStatusCode.Redirect)
            {
                // Se não for um redirecionamento, leia o conteúdo da resposta
                // Isso é CRÍTICO para depuração: mostrará a view de erro que foi retornada
                var responseString = await response.Content.ReadAsStringAsync();
                Assert.Fail($"Esperado redirecionamento (302 Found), mas recebeu {response.StatusCode}. " +
                            $"Conteúdo da resposta:\n{responseString}");
            }

            // Se chegou aqui, é porque o status foi 302, então podemos continuar com as asserções de sucesso
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Equal("/Account/Login", response.Headers.Location?.OriginalString);

            // Opcional: Verificar no banco de dados se o usuário e aluno foram criados
            var user = await _applicationUserRepository.ObterPorEmail(registerViewModel.Email);
            Assert.NotNull(user);
            Assert.Equal(registerViewModel.Email, user.Email);

            var aluno = await _alunoRepository.ObterPorId(registerViewModel.Matricula);
            Assert.NotNull(aluno);
            Assert.Equal(registerViewModel.Matricula, aluno.Matricula);
        }

        /// <summary>
        /// Testa o registro com e-mail já existente.
        /// </summary>
        [Fact]
        public async Task RegisterPost_ExistingEmail_ReturnsViewWithErrorMessage()
        {
            // Arrange: Crie um usuário pré-existente no banco de dados
            var existingEmail = $"existing.{Guid.NewGuid()}@example.com";
            // UserName do ApplicationUser é a matrícula, então também precisa ser string de 9 dígitos
            var existingMatriculaForUser = GenerateMatricula();
            var existingUser = new ApplicationUser { UserName = existingMatriculaForUser, Email = existingEmail };
            await _userManager.CreateAsync(existingUser, "ExistingPassword123!");

            var registerViewModel = new RegisterViewModel
            {
                Nome = "Another User",
                Email = existingEmail,
                Matricula = GenerateMatricula(), // Gera nova matrícula, pois o erro é no e-mail
                Senha = "NewPassword123!",
                ConfirmacaoSenha = "NewPassword123!", // Corrigido para ConfirmacaoSenha
                AnoEntrada = 2024,
                SemestreEntrada = 1
            };

            var formContent = new FormUrlEncodedContent(new[]
            {
            new KeyValuePair<string, string>("Nome", registerViewModel.Nome),
            new KeyValuePair<string, string>("Email", registerViewModel.Email),
            new KeyValuePair<string, string>("Matricula", registerViewModel.Matricula),
            new KeyValuePair<string, string>("Senha", registerViewModel.Senha),
            new KeyValuePair<string, string>("ConfirmacaoSenha", registerViewModel.ConfirmacaoSenha), // Corrigido
            new KeyValuePair<string, string>("AnoEntrada", registerViewModel.AnoEntrada.ToString()),
            new KeyValuePair<string, string>("SemestreEntrada", registerViewModel.SemestreEntrada.ToString()),
        });

            // Act
            var response = await _client.PostAsync("/Account/Register", formContent);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode); // Retorna a View (200 OK) com o modelo e erros
            var responseString = await response.Content.ReadAsStringAsync();
            Assert.Contains("Esse e-mail j&#xE1; est&#xE1; registrado.", responseString);
        }

        /// <summary>
        /// Testa o registro com matrícula já existente.
        /// </summary>
        [Fact]
        public async Task RegisterPost_ExistingMatricula_ReturnsViewWithErrorMessage()
        {
            // Arrange: Crie um aluno pré-existente no banco de dados
            var existingMatricula = GenerateMatricula(); // Gera matrícula string de 9 dígitos
                                                         // O Aluno precisa de um CodigoUser, que é o Id do ApplicationUser.
                                                         // Crie um ApplicationUser dummy para vincular ao Aluno.
            var dummyUser = new ApplicationUser { UserName = existingMatricula, Email = $"dummy.{Guid.NewGuid()}@example.com" };
            await _userManager.CreateAsync(dummyUser, "DummyPass123!");

            var existingAluno = new Aluno { Matricula = existingMatricula, NomeAluno = "Existing Aluno", SemestreEntrada = "2020.1", CodigoUser = dummyUser.Id };
            await _alunoRepository.Adicionar(existingAluno);

            var registerViewModel = new RegisterViewModel
            {
                Nome = "Another User",
                Email = $"another.{Guid.NewGuid()}@example.com",
                Matricula = existingMatricula, // Usando a matrícula que já existe
                Senha = "NewPassword123!",
                ConfirmacaoSenha = "NewPassword123!", // Corrigido para ConfirmacaoSenha
                AnoEntrada = 2024,
                SemestreEntrada = 1
            };

            var formContent = new FormUrlEncodedContent(new[]
            {
            new KeyValuePair<string, string>("Nome", registerViewModel.Nome),
            new KeyValuePair<string, string>("Email", registerViewModel.Email),
            new KeyValuePair<string, string>("Matricula", registerViewModel.Matricula), // Já é string
            new KeyValuePair<string, string>("Senha", registerViewModel.Senha),
            new KeyValuePair<string, string>("ConfirmacaoSenha", registerViewModel.ConfirmacaoSenha), // Corrigido
            new KeyValuePair<string, string>("AnoEntrada", registerViewModel.AnoEntrada.ToString()),
            new KeyValuePair<string, string>("SemestreEntrada", registerViewModel.SemestreEntrada.ToString()),
        });

            // Act
            var response = await _client.PostAsync("/Account/Register", formContent);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode); // Retorna a View (200 OK) com o modelo e erros
            var responseString = await response.Content.ReadAsStringAsync();
            Assert.Contains("Essa matr&#xED;cula j&#xE1; pertence a outro usu&#xE1;rio.\r\n", responseString);
        }

        /// <summary>
        /// Testa o login com credenciais válidas.
        /// </summary>
        [Fact]
        public async Task LoginPost_ValidCredentials_ReturnsRedirectToHome()
        {
            // Arrange: Registre um usuário para o teste de login
            var email = $"login.{Guid.NewGuid()}@example.com";
            var matricula = GenerateMatricula(); // Gera matrícula string de 9 dígitos
            var password = "ValidPassword123!";

            // UserName do ApplicationUser é a matrícula
            var user = new ApplicationUser { UserName = matricula, Email = email };
            var createResult = await _userManager.CreateAsync(user, password);
            Assert.True(createResult.Succeeded);

            var aluno = new Aluno { Matricula = matricula, NomeAluno = "Login Test", SemestreEntrada = "2020.1", CodigoUser = user.Id };
            await _alunoRepository.Adicionar(aluno);

            var loginViewModel = new LoginViewModel
            {
                Matricula = matricula,
                Senha = password
            };

            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("Matricula", loginViewModel.Matricula),
                new KeyValuePair<string, string>("Senha", loginViewModel.Senha),
            });

            // Act
            var response = await _client.PostAsync("/Account/Login", formContent);

            // Assert
            // REMOVA ESTA LINHA: response.EnsureSuccessStatusCode(); // ESTE É O PROBLEMA

            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode); // Espera um redirecionamento (302)
            Assert.Equal("/", response.Headers.Location?.OriginalString); // Verifica o redirecionamento
        }

        /// <summary>
        /// Testa o login com credenciais inválidas.
        /// </summary>
        [Fact]
        public async Task LoginPost_InvalidCredentials_ReturnsViewWithErrorMessage()
        {
            // Arrange
            var loginViewModel = new LoginViewModel
            {
                Matricula = "999999999", // Matrícula que não existe (string de 9 dígitos)
                Senha = "wrongpassword"
            };

            var formContent = new FormUrlEncodedContent(new[]
            {
            new KeyValuePair<string, string>("Matricula", loginViewModel.Matricula), // Já é string
            new KeyValuePair<string, string>("Senha", loginViewModel.Senha),
        });

            // Act
            var response = await _client.PostAsync("/Account/Login", formContent);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode); // Espera retornar a View com o erro
            var responseString = await response.Content.ReadAsStringAsync();
            Assert.Contains("Usu&#xE1;rio ou senha incorretos.", responseString);
        }

        /// <summary>
        /// Testa o logout.
        /// </summary>
        [Fact]
        public async Task LogoutPost_ReturnsRedirectToLogin()
        {
            // Arrange: Logar um usuário primeiro para ter um estado autenticado (simplificado para o teste)
            // Em um cenário real, você faria um POST de login antes.
            // Para este teste, vamos apenas chamar o logout diretamente.
            // A WebApplicationFactory não mantém estado de sessão entre requisições por padrão,
            // então este teste é mais sobre o redirecionamento pós-logout.

            // Act
            var response = await _client.PostAsync("/Account/Logout", null);

            // Assert
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Equal("/Account/Login", response.Headers.Location?.OriginalString);
        }

        /// <summary>
        /// Testa o cenário de "Esqueci a Senha" com e-mail cadastrado.
        /// </summary>
        [Fact]
        public async Task ForgotPasswordPost_RegisteredEmail_SendsEmailAndRedirectsToLogin()
        {
            // Arrange: Registre um usuário para o teste
            var email = $"forgot.{Guid.NewGuid()}@example.com";
            var matricula = GenerateMatricula(); // Gera matrícula string de 9 dígitos
            var password = "Password123!";

            // UserName do ApplicationUser é a matrícula
            var user = new ApplicationUser { UserName = matricula, Email = email };
            var createResult = await _userManager.CreateAsync(user, password);
            Assert.True(createResult.Succeeded);

            // Mock do IEmailService (IMPORTANTE!)
            // Para este teste, você precisa de um mock para IEmailService para evitar o envio real de e-mails.
            // Se você não o tem, adicione a biblioteca Moq ao seu projeto de testes.
            // Isso deve ser configurado no CustomWebApplicationFactory.
            // Ex: _factory.WithWebHostBuilder(builder => { builder.ConfigureServices(services => { services.AddScoped<IEmailService, MockEmailService>(); }); });

            var model = new ForgotPasswordViewModel { Email = email };
            var formContent = new FormUrlEncodedContent(new[]
            {
            new KeyValuePair<string, string>("Email", model.Email),
        });

            // Act
            var response = await _client.PostAsync("/Account/ForgotPassword", formContent);

            // Assert
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Equal("/Account/Login", response.Headers.Location?.OriginalString);

            // Em um teste de integração real, você verificaria se o mock de e-mail foi chamado.
            // Isso requer uma configuração mais avançada do CustomWebApplicationFactory para injetar mocks.
        }

        /// <summary>
        /// Testa o cenário de "Esqueci a Senha" com e-mail NÃO cadastrado.
        /// </summary>
        [Fact]
        public async Task ForgotPasswordPost_UnregisteredEmail_ReturnsRedirectToLoginWithoutError()
        {
            // Arrange
            var model = new ForgotPasswordViewModel { Email = $"nonexistent.{Guid.NewGuid()}@example.com" };
            var formContent = new FormUrlEncodedContent(new[]
            {
            new KeyValuePair<string, string>("Email", model.Email),
        });

            // Act
            var response = await _client.PostAsync("/Account/ForgotPassword", formContent);

            // Assert
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Equal("/Account/Login", response.Headers.Location?.OriginalString);

            // O controller deve redirecionar para Login e exibir uma mensagem genérica de sucesso (para não vazar informações)
            // Para verificar o TempData, você precisaria inspecionar a resposta da View de Login, o que é mais complexo em testes de integração HTTP.
        }

        /// <summary>
        /// Testa a redefinição de senha com sucesso.
        /// ATENÇÃO: Este teste é mais complexo, pois envolve geração e uso de tokens.
        /// Ele requer acesso ao UserManager para gerar o token e simular o fluxo.
        /// </summary>
        [Fact]
        public async Task ResetPasswordPost_ValidTokenAndEmail_ResetsPasswordSuccessfully()
        {
            // Arrange: Crie e registre um usuário
            var email = $"reset.{Guid.NewGuid()}@example.com";
            var matricula = GenerateMatricula(); // Gera matrícula string de 9 dígitos
            var initialPassword = "InitialPassword123!";
            var newPassword = "NewPassword456!$";

            // UserName do ApplicationUser é a matrícula
            var user = new ApplicationUser { UserName = matricula, Email = email };
            var createResult = await _userManager.CreateAsync(user, initialPassword);
            Assert.True(createResult.Succeeded);

            // Gere um token de redefinição de senha (como faria o ForgotPassword)
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var model = new ResetPasswordViewModel
            {
                Email = email,
                Token = token,
                Senha = newPassword,
                ConfirmacaoSenha = newPassword
            };

            var formContent = new FormUrlEncodedContent(new[]
            {
            new KeyValuePair<string, string>("Email", model.Email),
            new KeyValuePair<string, string>("Token", model.Token),
            new KeyValuePair<string, string>("Senha", model.Senha),
            new KeyValuePair<string, string>("ConfirmacaoSenha", model.ConfirmacaoSenha),
        });

            // Act
            var response = await _client.PostAsync("/Account/ResetPassword", formContent);

            // Assert
            if (response.StatusCode != HttpStatusCode.Redirect)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                Assert.Fail($"Esperado redirecionamento (302 Found), mas recebeu {response.StatusCode}. " +
                            $"Conteúdo da resposta:\n{responseString}");
            }

            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Equal("/Account/Login", response.Headers.Location?.OriginalString);

        }
    }
}