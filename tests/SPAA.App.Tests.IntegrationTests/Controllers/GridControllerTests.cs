// Caminho: SPAA.App.Tests/IntegrationTests/Controllers/GridControllerTests.cs

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
using System.Linq;
using System.Text.RegularExpressions; // Para parsing do HTML
using System.Net.Http.Json;
using Projeto.App.ViewModels;
using Microsoft.AspNetCore.Mvc.Testing; // Para PostAsJsonAsync

namespace SPAA.App.Tests.IntegrationTests.Controllers
{
    // Aplica a coleção de testes para que a CustomWebApplicationFactory seja compartilhada
    [Collection(nameof(IntegrationTestCollection))]
    public class GridControllerTests : IntegrationTestFixture
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IAlunoRepository _alunoRepository;
        private readonly IApplicationUserRepository _applicationUserRepository;
        private readonly ITurmaSalvaRepository _turmaSalvaRepository;
        private readonly IDisciplinaRepository _disciplinaRepository; // Para obter ementas, se necessário

        public GridControllerTests(CustomWebApplicationFactory<Program> factory) : base(factory)
        {
            var scope = _factory.Services.CreateScope();
            _userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            _signInManager = scope.ServiceProvider.GetRequiredService<SignInManager<ApplicationUser>>();
            _alunoRepository = scope.ServiceProvider.GetRequiredService<IAlunoRepository>();
            _applicationUserRepository = scope.ServiceProvider.GetRequiredService<IApplicationUserRepository>();
            _turmaSalvaRepository = scope.ServiceProvider.GetRequiredService<ITurmaSalvaRepository>();
            _disciplinaRepository = scope.ServiceProvider.GetRequiredService<IDisciplinaRepository>();
        }

        /// <summary>
        /// Gera uma string de matrícula de 9 dígitos.
        /// </summary>
        private string GenerateMatricula()
        {
            return new Random().Next(100000000, 999999999).ToString();
        }

        /// <summary>
        /// Auxiliar para registrar e logar um usuário para os testes autorizados.
        /// Retorna um HttpClient autenticado.
        /// Garante que o usuário e o aluno são criados e estão no DB.
        /// </summary>
        private async Task<(HttpClient authenticatedClient, ApplicationUser user, string password)> AuthenticateUserAsync(string email, string matricula, string password, string nomeAluno = "Teste Aluno")
        {
            using (var creationScope = _factory.Services.CreateScope())
            {
                var userManager = creationScope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var alunoRepository = creationScope.ServiceProvider.GetRequiredService<IAlunoRepository>();
                var dbContext = creationScope.ServiceProvider.GetRequiredService<MeuDbContext>();

                var user = new ApplicationUser { UserName = matricula, Email = email };
                var createResult = await userManager.CreateAsync(user, password);
                Assert.True(createResult.Succeeded, $"Falha ao criar usuário: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");

                var aluno = new Aluno { Matricula = matricula, NomeAluno = nomeAluno, SemestreEntrada = "2023/1", CodigoUser = user.Id, CurriculoAluno = "2" }; // Adicione um currículo padrão
                var alunoAddResult = await alunoRepository.Adicionar(aluno);
                Assert.True(alunoAddResult, "Falha ao adicionar aluno.");
                await dbContext.SaveChangesAsync();
            }

            var loginViewModel = new LoginViewModel { Matricula = matricula, Senha = password };
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("Matricula", loginViewModel.Matricula),
                new KeyValuePair<string, string>("Senha", loginViewModel.Senha),
            });

            var clientForLogin = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
            var loginResponse = await clientForLogin.PostAsync("/Account/Login", formContent);
            Assert.Equal(HttpStatusCode.Redirect, loginResponse.StatusCode);
            Assert.Equal("/", loginResponse.Headers.Location?.OriginalString);

            return (clientForLogin, null, password); // Retorna null para user, pois o objeto original pode não estar rastreado
        }

        /// <summary>
        /// Auxiliar para obter o token anti-forgery de uma página HTML.
        /// </summary>
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

        // ----------------------------------------------------------------------------------------------------
        // TESTES PARA GET: MontarGrade()
        // ----------------------------------------------------------------------------------------------------

        /// <summary>
        /// Testa o endpoint GET /Grid/MontarGrade para usuário autenticado.
        /// Deve retornar sucesso e a view com turmas.
        /// </summary>
        [Fact]
        public async Task MontarGradeGet_AuthenticatedUser_ReturnsSuccessAndCorrectContentType()
        {
            // Arrange
            var (authenticatedClient, _, _) = await AuthenticateUserAsync(
                $"gridget.{Guid.NewGuid()}@example.com", GenerateMatricula(), "Password123!");

            // Act
            var response = await authenticatedClient.GetAsync("/Grid/MontarGrade");

            // Assert
            response.EnsureSuccessStatusCode(); // Status 200-299
            Assert.Equal("text/html; charset=utf-8", response.Content.Headers.ContentType?.ToString());

            var responseString = await response.Content.ReadAsStringAsync();
            Assert.Contains("Montar Grade - SPAA.APP", responseString); // Verifique o título da sua página
            Assert.Contains("Turmas Obrigatórias", responseString); // Verifica se a seção de turmas obrigatórias está presente
            Assert.Contains("Turmas Optativas", responseString);   // Verifica se a seção de turmas optativas está presente
        }

        /// <summary>
        /// Testa o endpoint GET /Grid/MontarGrade para usuário não autenticado.
        /// Deve redirecionar para a página de login.
        /// </summary>
        [Fact]
        public async Task MontarGradeGet_UnauthenticatedUser_RedirectsToLogin()
        {
            // Arrange (usa o _client padrão que não está autenticado)
            // Act
            var response = await _client.GetAsync("/Grid/MontarGrade");

            // Assert
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Contains("/Account/Login", response.Headers.Location?.OriginalString);
        }

        // ----------------------------------------------------------------------------------------------------
        // TESTES PARA POST: MontarGrade(MontarGradeViewModel model)
        // ----------------------------------------------------------------------------------------------------

        /// <summary>
        /// Testa o POST /Grid/MontarGrade com horários válidos.
        /// Deve retornar a view com turmas filtradas.
        /// </summary>
        [Fact]
        public async Task MontarGradePost_ValidHorarios_ReturnsFilteredTurmas()
        {
            // Arrange
            var email = $"gridpost.{Guid.NewGuid()}@example.com";
            var matricula = GenerateMatricula();
            var password = "Password123!";

            var (authenticatedClient, user, _) = await AuthenticateUserAsync(email, matricula, password);

            // Obtenha o token anti-forgery da página GET
            var antiForgeryToken = await GetAntiForgeryTokenAsync(authenticatedClient, "/Grid/MontarGrade");

            // Horários de exemplo que devem filtrar algumas turmas do seeding
            // Ex: "2M1" (Segunda Manhã 1), "3V1" (Terça Noite 1)
            var horariosMarcados = new List<string> { "2M1", "3V1" };
            var model = new MontarGradeViewModel
            {
                HorariosMarcados = JsonSerializer.Serialize(horariosMarcados)
            };

            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("__RequestVerificationToken", antiForgeryToken),
                new KeyValuePair<string, string>("HorariosMarcados", model.HorariosMarcados),
            });

            // Act
            var response = await authenticatedClient.PostAsync("/Grid/MontarGrade", formContent);

            // Assert
            response.EnsureSuccessStatusCode(); // Deve retornar 200 OK com a View
            Assert.Equal("text/html; charset=utf-8", response.Content.Headers.ContentType?.ToString());

            var responseString = await response.Content.ReadAsStringAsync();
            // Verifique se a mensagem de sucesso ou uma mensagem esperada está presente
            Assert.Contains("Turmas encontradas com sucesso.", responseString);

            // Opcional: Tentar parsear o HTML para verificar se as turmas esperadas estão lá.
            // Isso seria complexo e exigiria um parser HTML. Por enquanto, confiamos na mensagem.
            // Exemplo de verificação mais específica (se você souber o HTML exato):
            // Assert.Contains("CALCULO 1", responseString); // Se CALCULO 1 for compatível com 2M1
        }

        /// <summary>
        /// Testa o POST /Grid/MontarGrade com horários vazios/nulos.
        /// Deve retornar a view com mensagem de erro.
        /// </summary>
        [Fact]
        public async Task MontarGradePost_EmptyHorarios_ReturnsViewWithError()
        {
            // Arrange
            var email = $"gridpostempty.{Guid.NewGuid()}@example.com";
            var matricula = GenerateMatricula();
            var password = "Password123!";

            var (authenticatedClient, user, _) = await AuthenticateUserAsync(email, matricula, password);

            var antiForgeryToken = await GetAntiForgeryTokenAsync(authenticatedClient, "/Grid/MontarGrade");

            var model = new MontarGradeViewModel
            {
                HorariosMarcados = JsonSerializer.Serialize(new List<string>()) // Lista de horários vazia
            };

            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("__RequestVerificationToken", antiForgeryToken),
                new KeyValuePair<string, string>("HorariosMarcados", model.HorariosMarcados),
            });

            // Act
            var response = await authenticatedClient.PostAsync("/Grid/MontarGrade", formContent);

            // Assert
            response.EnsureSuccessStatusCode(); // Deve retornar 200 OK com a View
            Assert.Equal("text/html; charset=utf-8", response.Content.Headers.ContentType?.ToString());

            var responseString = await response.Content.ReadAsStringAsync();
            // A mensagem de erro esperada do controller: "Nenhuma turma compatível foi encontrada..."
            Assert.Contains("Nenhuma turma compatível foi encontrada.", responseString);
        }

        // ----------------------------------------------------------------------------------------------------
        // TESTES PARA POST: SalvarGrade([FromBody] List<TurmaViewModel> turmasSelecionadas)
        // ----------------------------------------------------------------------------------------------------

        /// <summary>
        /// Testa o POST /Grid/SalvarGrade com turmas válidas.
        /// Deve retornar sucesso e salvar as turmas no banco de dados.
        /// </summary>
        [Fact]
        public async Task SalvarGradePost_ValidTurmas_ReturnsSuccessAndSavesToDb()
        {
            // Arrange
            var email = $"salvargrade.{Guid.NewGuid()}@example.com";
            var matricula = GenerateMatricula();
            var password = "Password123!";

            var (authenticatedClient, user, _) = await AuthenticateUserAsync(email, matricula, password);

            // Obtenha uma turma existente do DB in-memory para simular a seleção
            Turma turmaParaSalvar;
            using (var scope = _factory.Services.CreateScope())
            {
                var turmaRepository = scope.ServiceProvider.GetRequiredService<ITurmaRepository>();
                turmaParaSalvar = await turmaRepository.ObterPorId(101); // Ex: CALCULO 1
                Assert.NotNull(turmaParaSalvar);
            }

            var turmasSelecionadas = new List<TurmaViewModel>
            {
                new TurmaViewModel
                {
                    CodigoTurmaUnico = turmaParaSalvar.CodigoTurmaUnico,
                    CodigoTurma = turmaParaSalvar.CodigoTurma,
                    NomeDisciplina = turmaParaSalvar.NomeDisciplina,
                    Horario = turmaParaSalvar.Horario,
                    CodigoDisciplina = turmaParaSalvar.CodigoDisciplina,
                    NomeProfessor = turmaParaSalvar.NomeProfessor,
                    Capacidade = turmaParaSalvar.Capacidade,
                    Semestre = turmaParaSalvar.Semestre
                }
            };

            // Act
            // Para [FromBody], enviamos JSON diretamente. Não precisa de FormUrlEncodedContent ou anti-forgery token.
            var response = await authenticatedClient.PostAsJsonAsync("/Grid/SalvarGrade", turmasSelecionadas);

            // Assert
            response.EnsureSuccessStatusCode(); // Espera 200 OK
            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.Contains("\"success\":true", responseContent);
            Assert.Contains("Grade salva com sucesso!", responseContent);

            // Verificar se a turma foi salva no banco de dados
            using (var assertionScope = _factory.Services.CreateScope())
            {
                var turmaSalvaRepository = assertionScope.ServiceProvider.GetRequiredService<ITurmaSalvaRepository>();
                var savedTurmas = await turmaSalvaRepository.TodasTurmasSalvasAluno(matricula);
                Assert.Single(savedTurmas); // Deve haver apenas 1 turma salva
                Assert.Equal(turmaParaSalvar.CodigoTurmaUnico, savedTurmas.First().CodigoUnicoTurma);
                Assert.Equal(matricula, savedTurmas.First().Matricula);
            }
        }

        /// <summary>
        /// Testa o POST /Grid/SalvarGrade com lista de turmas vazia.
        /// Deve retornar BadRequest.
        /// </summary>
        [Fact]
        public async Task SalvarGradePost_EmptyTurmas_ReturnsBadRequest()
        {
            // Arrange
            var email = $"salvargradeempty.{Guid.NewGuid()}@example.com";
            var matricula = GenerateMatricula();
            var password = "Password123!";

            var (authenticatedClient, user, _) = await AuthenticateUserAsync(email, matricula, password);

            var turmasSelecionadas = new List<TurmaViewModel>(); // Lista vazia

            // Act
            var response = await authenticatedClient.PostAsJsonAsync("/Grid/SalvarGrade", turmasSelecionadas);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.Contains("Nenhuma turma foi selecionada.", responseContent);
        }

        /// <summary>
        /// Testa o POST /Grid/SalvarGrade sem autenticação.
        /// Deve retornar Unauthorized (401).
        /// </summary>
        [Fact]
        public async Task SalvarGradePost_UnauthenticatedUser_ReturnsUnauthorized()
        {
            // Arrange (usa o _client padrão que não está autenticado)
            var turmasSelecionadas = new List<TurmaViewModel>
            {
                new TurmaViewModel { CodigoTurmaUnico = 101, NomeDisciplina = "CALCULO 1" }
            };

            // Act
            var response = await _client.PostAsJsonAsync("/Grid/SalvarGrade", turmasSelecionadas);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}
