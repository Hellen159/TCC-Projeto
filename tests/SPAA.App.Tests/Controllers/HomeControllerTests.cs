using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SPAA.App.ViewModels;
using SPAA.APP.Models;
using SPAA.APP.ViewModels;
using SPAA.Business.Interfaces.Repository;
using SPAA.Business.Interfaces.Services;
using SPAA.Business.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using SPAA.APP.Controllers;
using AutoMapper;
// using Newtonsoft.Json; // Não precisaremos mais do Newtonsoft.Json para esta abordagem
using System.Reflection; // Necessário para usar reflexão

namespace SPAA.App.Tests.Controllers
{
    public class HomeControllerTests
    {
        private readonly Mock<ILogger<HomeController>> _mockLogger;
        private readonly Mock<IAlunoRepository> _mockAlunoRepository;
        private readonly Mock<IAlunoDisciplinaRepository> _mockAlunoDisciplinaRepository;
        private readonly Mock<IDisciplinaRepository> _mockDisciplinaRepository;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ICurriculoRepository> _mockCurriculoRepository;
        private readonly Mock<IPreRequisitoRepository> _mockPreRequisitoRepository;
        private readonly Mock<IAreaInteresseAlunoRepository> _mockAreaInteresseAlunoRepository;
        private readonly Mock<ITurmaRepository> _mockTurmaRepository;
        private readonly Mock<IAlunoService> _mockAlunoService;
        private readonly Mock<ITurmaSalvaRepository> _mockTurmaSalvaRepository;
        private readonly Mock<ITarefaAlunoRepository> _mockTarefaAlunoRepository;

        private readonly HomeController _controller;
        private readonly string _matriculaUsuarioLogado = "123456"; // Matrícula de teste

        public HomeControllerTests()
        {
            _mockLogger = new Mock<ILogger<HomeController>>();
            _mockAlunoRepository = new Mock<IAlunoRepository>();
            _mockAlunoDisciplinaRepository = new Mock<IAlunoDisciplinaRepository>();
            _mockDisciplinaRepository = new Mock<IDisciplinaRepository>();
            _mockMapper = new Mock<IMapper>();
            _mockCurriculoRepository = new Mock<ICurriculoRepository>();
            _mockPreRequisitoRepository = new Mock<IPreRequisitoRepository>();
            _mockAreaInteresseAlunoRepository = new Mock<IAreaInteresseAlunoRepository>();
            _mockTurmaRepository = new Mock<ITurmaRepository>();
            _mockAlunoService = new Mock<IAlunoService>();
            _mockTurmaSalvaRepository = new Mock<ITurmaSalvaRepository>();
            _mockTarefaAlunoRepository = new Mock<ITarefaAlunoRepository>();

            _controller = new HomeController(
                _mockLogger.Object,
                _mockAlunoRepository.Object,
                _mockAlunoDisciplinaRepository.Object,
                _mockDisciplinaRepository.Object,
                _mockMapper.Object,
                _mockCurriculoRepository.Object,
                _mockPreRequisitoRepository.Object,
                _mockAreaInteresseAlunoRepository.Object,
                _mockTurmaRepository.Object,
                _mockAlunoService.Object,
                _mockTurmaSalvaRepository.Object,
                _mockTarefaAlunoRepository.Object
            );

            // Setup de um usuário autenticado mockado para a controller
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, _matriculaUsuarioLogado)
            }, "mockAuth"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Configuração de TempData
            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
            _controller.TempData = tempData;
        }

        // --- Métodos Auxiliares para criar ViewModels e Models ---
        private Aluno GetAlunoExemplo()
        {
            return new Aluno
            {
                Matricula = _matriculaUsuarioLogado,
                NomeAluno = "Aluno Teste"
            };
        }

        private List<TurmaSalva> GetTurmasSalvasExemplo()
        {
            return new List<TurmaSalva>
            {
                new TurmaSalva { CodigoTurmaSalva = 1, Matricula = _matriculaUsuarioLogado, CodigoUnicoTurma = 101, Horario = "SEG 08:00", CodigoDisciplina = "DISC101" },
                new TurmaSalva { CodigoTurmaSalva = 2, Matricula = _matriculaUsuarioLogado, CodigoUnicoTurma = 102, Horario = "TER 10:00", CodigoDisciplina = "DISC102" }
            };
        }

        private List<TurmaViewModel> GetTurmasViewModelExemplo()
        {
            return new List<TurmaViewModel>
            {
                new TurmaViewModel { CodigoTurmaUnico = 1, NomeDisciplina = "Disciplina A" },
                new TurmaViewModel { CodigoTurmaUnico = 2, NomeDisciplina = "Disciplina B" }
            };
        }

        private List<string> GetNomesDisciplinasAprovadasExemplo()
        {
            return new List<string> { "Calculo I", "Algebra Linear" };
        }

        private List<string> GetNomesDisciplinasPendentesExemplo()
        {
            return new List<string> { "Fisica I", "Programacao II" };
        }

        private List<TarefaAluno> GetTarefasAlunoExemplo()
        {
            return new List<TarefaAluno>
            {
                new TarefaAluno { CodigoTarefaAluno = 1, NomeTarefa = "Reunião de Projeto", Matricula = _matriculaUsuarioLogado, Horario = "10:00" },
                new TarefaAluno { CodigoTarefaAluno = 2, NomeTarefa = "Estudar para Prova", Matricula = _matriculaUsuarioLogado, Horario = "14:00" }
            };
        }

        private List<TarefaViewModel> GetTarefaViewModelExemplo()
        {
            return new List<TarefaViewModel>
            {
                new TarefaViewModel { Descricao = "Reunião de Projeto", Horario = "10:00" },
                new TarefaViewModel { Descricao = "Estudar para Prova", Horario = "14:00" }
            };
        }

        #region Index Action
        [Fact]
        public async Task Index_NaoAutenticado_RedirecionaParaLogin()
        {
            // Arrange
            // Remover o usuário mockado para simular não autenticado
            _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity());

            // Act
            var result = await _controller.Index();

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
            Assert.Equal("Account", redirectResult.ControllerName);
        }

        [Fact]
        public async Task Index_AlunoNaoAnexouHistorico_RedirecionaParaUploadHistorico()
        {
            // Arrange
            _mockAlunoService.Setup(s => s.AlunoJaAnexouHistorico(It.IsAny<string>()))
                .ReturnsAsync(false); // Simula que o histórico não foi anexado

            // Act
            var result = await _controller.Index();

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("UploadHistorico", redirectResult.ActionName);
            Assert.Equal("Upload", redirectResult.ControllerName);
        }

        [Fact]
        public async Task Index_Sucesso_RetornaViewComDadosCorretos()
        {
            // Arrange
            _mockAlunoService.Setup(s => s.AlunoJaAnexouHistorico(It.IsAny<string>()))
                .ReturnsAsync(true); // Histórico anexado

            _mockTurmaRepository.Setup(r => r.TurmasDisponiveisPorSemestre(It.IsAny<string>()))
                .ReturnsAsync(new List<Turma>()); // Lista de turmas vazia para simplificar

            _mockAlunoRepository.Setup(r => r.ObterPorId(_matriculaUsuarioLogado))
                .ReturnsAsync(GetAlunoExemplo());

            _mockAlunoDisciplinaRepository.Setup(r => r.ObterNomeDisciplinasPorSituacao(_matriculaUsuarioLogado, "APR"))
                .ReturnsAsync(GetNomesDisciplinasAprovadasExemplo());
            _mockAlunoDisciplinaRepository.Setup(r => r.ObterNomeDisciplinasPorSituacao(_matriculaUsuarioLogado, "PEND"))
                .ReturnsAsync(GetNomesDisciplinasPendentesExemplo());

            var turmasSalvasExemplo = GetTurmasSalvasExemplo();
            var turmasSalvasViewModelExemplo = GetTurmasViewModelExemplo();
            _mockMapper.Setup(m => m.Map<List<TurmaViewModel>>(It.IsAny<List<TurmaSalva>>())) // <-- CORREÇÃO AQUI
                .Returns(turmasSalvasViewModelExemplo);

            _mockTurmaSalvaRepository.Setup(r => r.TodasTurmasSalvasAluno(It.IsAny<string>()))
                .ReturnsAsync(turmasSalvasExemplo);

            var tarefasAlunoExemplo = GetTarefasAlunoExemplo();
            _mockTarefaAlunoRepository.Setup(r => r.TodasTarefasDoAluno(_matriculaUsuarioLogado))
                .ReturnsAsync(tarefasAlunoExemplo);


            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);

            // Verifica ViewDatas
            var aprovadasVD = Assert.IsType<DisciplinaListaViewModel>(viewResult.ViewData["Aprovadas"]);
            Assert.Equal("Disciplinas Aprovadas", aprovadasVD.Titulo);
            Assert.Equal(GetNomesDisciplinasAprovadasExemplo().Count, aprovadasVD.Disciplinas.Count);
            Assert.Contains(aprovadasVD.Disciplinas, d => d.NomeDisciplina == "Calculo I");

            var pendentesVD = Assert.IsType<DisciplinaListaViewModel>(viewResult.ViewData["Pendentes"]);
            Assert.Equal("Disciplinas Pendentes", pendentesVD.Titulo);
            Assert.Equal(GetNomesDisciplinasPendentesExemplo().Count, pendentesVD.Disciplinas.Count);
            Assert.Contains(pendentesVD.Disciplinas, d => d.NomeDisciplina == "Fisica I");

            var turmasSalvasVD = Assert.IsType<List<TurmaViewModel>>(viewResult.ViewData["TurmasSalvas"]);
            Assert.Equal(turmasSalvasViewModelExemplo.Count, turmasSalvasVD.Count);
            Assert.Contains(turmasSalvasVD, t => t.CodigoTurmaUnico == 1);

            var tarefasAlunoVD = Assert.IsType<List<TarefaAluno>>(viewResult.ViewData["TarefasAluno"]);
            Assert.Equal(tarefasAlunoExemplo.Count, tarefasAlunoVD.Count);
            Assert.Contains(tarefasAlunoVD, t => t.NomeTarefa == "Reunião de Projeto");

            // Verifica chamadas de métodos
            _mockAlunoService.Verify(s => s.AlunoJaAnexouHistorico(_matriculaUsuarioLogado), Times.Once);
            _mockTurmaRepository.Verify(r => r.TurmasDisponiveisPorSemestre("2025.1"), Times.Once); // Ajustar semestre se necessário
            _mockAlunoRepository.Verify(r => r.ObterPorId(_matriculaUsuarioLogado), Times.Once);
            _mockAlunoDisciplinaRepository.Verify(r => r.ObterNomeDisciplinasPorSituacao(_matriculaUsuarioLogado, "APR"), Times.Once);
            _mockAlunoDisciplinaRepository.Verify(r => r.ObterNomeDisciplinasPorSituacao(_matriculaUsuarioLogado, "PEND"), Times.Once);
            _mockTurmaSalvaRepository.Verify(r => r.TodasTurmasSalvasAluno(_matriculaUsuarioLogado), Times.Once);
            _mockMapper.Verify(m => m.Map<List<TurmaViewModel>>(turmasSalvasExemplo), Times.Once);
            _mockTurmaSalvaRepository.Verify(r => r.HorariosComAulas(_matriculaUsuarioLogado), Times.Once);
            _mockTarefaAlunoRepository.Verify(r => r.TodasTarefasDoAluno(_matriculaUsuarioLogado), Times.Once);
        }
        #endregion

        #region Error Action
        [Fact]
        public void Error_RetornaViewComErrorViewModel()
        {
            // Act
            var result = _controller.Error();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ErrorViewModel>(viewResult.Model);
            Assert.NotNull(model.RequestId); // RequestId é preenchido automaticamente
        }
        #endregion

        #region SalvarTarefas Action
        [Fact]
        public async Task SalvarTarefas_TarefasNulasOuVazias_RetornaBadRequest()
        {
            // Arrange
            List<TarefaViewModel> tarefasRecebidas = null;

            // Act
            var result = await _controller.SalvarTarefas(tarefasRecebidas);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var value = badRequestResult.Value; // Mantenha como object para reflexão
            Assert.NotNull(value); // Garante que não é nulo
            Assert.Equal(false, (bool)value.GetType().GetProperty("success").GetValue(value));
            Assert.Equal("Nenhuma tarefa para salvar foi fornecida.", (string)value.GetType().GetProperty("message").GetValue(value));
        }

        [Fact]
        public async Task SalvarTarefas_MatriculaNulaOuVazia_RetornaUnauthorized()
        {
            // Arrange
            List<TarefaViewModel> tarefasRecebidas = GetTarefaViewModelExemplo();
            // Simula usuário não logado ou sem nome
            _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity());

            // Act
            var result = await _controller.SalvarTarefas(tarefasRecebidas);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var value = unauthorizedResult.Value; // Mantenha como object para reflexão
            Assert.NotNull(value); // Garante que não é nulo
            Assert.Equal(false, (bool)value.GetType().GetProperty("success").GetValue(value));
            Assert.Equal("Matrícula do aluno não pode ser determinada. Certifique-se de estar logado.", (string)value.GetType().GetProperty("message").GetValue(value));
        }

        [Fact]
        public async Task SalvarTarefas_TodasTarefasComDescricaoVazia_RetornaBadRequest()
        {
            // Arrange
            List<TarefaViewModel> tarefasRecebidas = new List<TarefaViewModel>
            {
                new TarefaViewModel { Descricao = "", Horario = "08:00" },
                new TarefaViewModel { Descricao = " ", Horario = "09:00" },
                new TarefaViewModel { Descricao = null, Horario = "10:00" }
            };

            // Act
            var result = await _controller.SalvarTarefas(tarefasRecebidas);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var value = badRequestResult.Value; // Mantenha como object para reflexão
            Assert.NotNull(value); // Garante que não é nulo
            Assert.Equal(false, (bool)value.GetType().GetProperty("success").GetValue(value));
            Assert.Equal("Nenhuma tarefa válida para salvar após a filtragem (descrição vazia).", (string)value.GetType().GetProperty("message").GetValue(value));
        }

        [Fact]
        public async Task SalvarTarefas_Sucesso_RetornaJsonComSucesso()
        {
            // Arrange
            List<TarefaViewModel> tarefasRecebidas = GetTarefaViewModelExemplo();
            _mockTarefaAlunoRepository.Setup(r => r.Adicionar(It.IsAny<TarefaAluno>()))
                .Returns(Task.FromResult(true));

            // Act
            var result = await _controller.SalvarTarefas(tarefasRecebidas);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = jsonResult.Value; // Mantenha como object para reflexão
            Assert.NotNull(value); // Garante que não é nulo
            Assert.Equal(true, (bool)value.GetType().GetProperty("success").GetValue(value));
            Assert.Equal("Tarefas salvas com sucesso!", (string)value.GetType().GetProperty("message").GetValue(value));
            Assert.NotNull(value.GetType().GetProperty("data").GetValue(value));

            // Para verificar o conteúdo de 'data', você precisaria de um casting mais específico
            // ou desserializar para a lista esperada. Para este teste, verificar NotNull é suficiente.
            // Se precisar de mais assertivas sobre 'data', considere criar uma classe de retorno específica.

            // Verifica se o método Adicionar foi chamado para cada tarefa válida
            _mockTarefaAlunoRepository.Verify(r => r.Adicionar(It.IsAny<TarefaAluno>()), Times.Exactly(tarefasRecebidas.Count));
        }

        [Fact]
        public async Task SalvarTarefas_Excecao_RetornaStatusCode500ComErro()
        {
            // Arrange
            List<TarefaViewModel> tarefasRecebidas = GetTarefaViewModelExemplo();
            _mockTarefaAlunoRepository.Setup(r => r.Adicionar(It.IsAny<TarefaAluno>()))
                .ThrowsAsync(new System.Exception("Erro de banco de dados.")); // Simula exceção

            // Act
            var result = await _controller.SalvarTarefas(tarefasRecebidas);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);

            // CORREÇÃO: Usar reflexão para acessar propriedades de objetos anônimos
            var value = statusCodeResult.Value;
            Assert.NotNull(value); // Garante que o Value não é nulo
            Assert.Equal(false, (bool)value.GetType().GetProperty("success").GetValue(value));
            Assert.Equal("Ocorreu um erro interno ao salvar as tarefas.", (string)value.GetType().GetProperty("message").GetValue(value));
            Assert.Equal("Erro de banco de dados.", (string)value.GetType().GetProperty("error").GetValue(value));
        }
        #endregion

        #region ExcluirTarefa Action
        [Fact]
        public async Task ExcluirTarefa_MatriculaNulaOuVazia_RetornaUnauthorized()
        {
            // Arrange
            string horarioTarefa = "10:00";
            // Simula usuário não logado ou sem nome
            _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity());

            // Act
            var result = await _controller.ExcluirTarefa(horarioTarefa);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var value = unauthorizedResult.Value; // Mantenha como object para reflexão
            Assert.NotNull(value); // Garante que não é nulo
            Assert.Equal(false, (bool)value.GetType().GetProperty("success").GetValue(value));
            Assert.Equal("Matrícula do aluno não pode ser determinada.", (string)value.GetType().GetProperty("message").GetValue(value));
        }

        [Fact]
        public async Task ExcluirTarefa_TarefaNaoEncontrada_RetornaNotFound()
        {
            // Arrange
            string horarioTarefa = "10:00";
            _mockTarefaAlunoRepository.Setup(r => r.IdTarefa(horarioTarefa, _matriculaUsuarioLogado))
                .ReturnsAsync((int?)null); // Simula tarefa não encontrada

            // Act
            var result = await _controller.ExcluirTarefa(horarioTarefa);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var value = notFoundResult.Value; // Mantenha como object para reflexão
            Assert.NotNull(value); // Garante que não é nulo
            Assert.Equal(false, (bool)value.GetType().GetProperty("success").GetValue(value));
            Assert.Equal("Tarefa não encontrada para os critérios fornecidos.", (string)value.GetType().GetProperty("message").GetValue(value));
        }

        [Fact]
        public async Task ExcluirTarefa_Sucesso_RetornaJsonComSucesso()
        {
            // Arrange
            string horarioTarefa = "10:00";
            int idTarefaExemplo = 123;
            _mockTarefaAlunoRepository.Setup(r => r.IdTarefa(horarioTarefa, _matriculaUsuarioLogado))
                .ReturnsAsync(idTarefaExemplo);
            _mockTarefaAlunoRepository.Setup(r => r.Remover(idTarefaExemplo))
                .Returns(Task.FromResult(true));

            // Act
            var result = await _controller.ExcluirTarefa(horarioTarefa);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = jsonResult.Value; // Mantenha como object para reflexão
            Assert.NotNull(value); // Garante que não é nulo
            Assert.Equal(true, (bool)value.GetType().GetProperty("success").GetValue(value));
            Assert.Equal("Tarefa removida com sucesso!", (string)value.GetType().GetProperty("message").GetValue(value));

            _mockTarefaAlunoRepository.Verify(r => r.IdTarefa(horarioTarefa, _matriculaUsuarioLogado), Times.Once);
            _mockTarefaAlunoRepository.Verify(r => r.Remover(idTarefaExemplo), Times.Once);
        }

        [Fact]
        public async Task ExcluirTarefa_Excecao_RetornaStatusCode500ComErro()
        {
            // Arrange
            string horarioTarefa = "10:00";
            _mockTarefaAlunoRepository.Setup(r => r.IdTarefa(horarioTarefa, _matriculaUsuarioLogado))
                .ThrowsAsync(new System.Exception("Erro ao buscar ID da tarefa.")); // Simula exceção na busca do ID

            // Act
            var result = await _controller.ExcluirTarefa(horarioTarefa);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);

            // CORREÇÃO: Usar reflexão para acessar propriedades de objetos anônimos
            var value = statusCodeResult.Value;
            Assert.NotNull(value); // Garante que o Value não é nulo
            Assert.Equal(false, (bool)value.GetType().GetProperty("success").GetValue(value));
            Assert.Equal("Ocorreu um erro ao excluir a tarefa.", (string)value.GetType().GetProperty("message").GetValue(value));
            Assert.Equal("Erro ao buscar ID da tarefa.", (string)value.GetType().GetProperty("error").GetValue(value));
        }
        #endregion
    }
}