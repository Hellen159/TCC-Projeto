// SPAA.App.Tests/Controllers/GridControllerTests.cs
using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using SPAA.App.Controllers;
using SPAA.App.ViewModels;
using SPAA.Business.Interfaces.Repository;
using SPAA.Business.Interfaces.Services;
using SPAA.Business.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Collections.Generic;
using AutoMapper;
using System.Linq;
using System.Text.Json;

namespace SPAA.App.Tests.Controllers
{
    public class GridControllerTests
    {
        // --- Mocks das dependências ---
        private readonly Mock<IDisciplinaService> _mockDisciplinaService;
        private readonly Mock<ITurmaService> _mockTurmaService;
        private readonly Mock<ITurmaRepository> _mockTurmaRepository;
        private readonly Mock<IAlunoRepository> _mockAlunoRepository;
        private readonly Mock<ICurriculoRepository> _mockCurriculoRepository;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ITurmaSalvaRepository> _mockTurmaSalvaRepository;
        private readonly Mock<IDisciplinaRepository> _mockDisciplinaRepository;
        private readonly GridController _controller;
        private readonly string _matriculaUsuarioLogado = "20241010";

        public GridControllerTests()
        {
            // --- Inicialização dos Mocks ---
            _mockDisciplinaService = new Mock<IDisciplinaService>();
            _mockTurmaService = new Mock<ITurmaService>();
            _mockTurmaRepository = new Mock<ITurmaRepository>();
            _mockAlunoRepository = new Mock<IAlunoRepository>();
            _mockCurriculoRepository = new Mock<ICurriculoRepository>();
            _mockMapper = new Mock<IMapper>();
            _mockTurmaSalvaRepository = new Mock<ITurmaSalvaRepository>();
            _mockDisciplinaRepository = new Mock<IDisciplinaRepository>();

            // --- Instanciação do Controller com os Mocks ---
            _controller = new GridController(
                _mockDisciplinaService.Object,
                _mockTurmaService.Object,
                _mockTurmaRepository.Object,
                _mockAlunoRepository.Object,
                _mockCurriculoRepository.Object,
                _mockMapper.Object,
                _mockTurmaSalvaRepository.Object,
                _mockDisciplinaRepository.Object
            );

            // --- Configuração do Contexto do Controller para simular um usuário logado ---
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, _matriculaUsuarioLogado) // Simula User.Identity.Name
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // ViewData é necessário para armazenar os códigos das turmas salvas
            _controller.ViewData = new ViewDataDictionary(new Microsoft.AspNetCore.Mvc.ModelBinding.EmptyModelMetadataProvider(), new Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary());
        }

        // --- Helper para criar uma lista de Turmas de exemplo ---
        private List<Turma> GetTurmasExemplo(string codigoDisciplina = "COMP001", string nomeDisciplina = "Cálculo I")
        {
            return new List<Turma>
            {
                new Turma { CodigoTurmaUnico = 101, CodigoDisciplina = codigoDisciplina, NomeDisciplina = nomeDisciplina, NomeProfessor = "Dr. Newton" }
            };
        }

        // --- Helper para criar uma lista de TurmaViewModel de exemplo ---
        private List<TurmaViewModel> GetTurmasViewModelExemplo(string codigoDisciplina = "COMP001", string nomeDisciplina = "Cálculo I")
        {
            return new List<TurmaViewModel>
            {
                new TurmaViewModel { CodigoTurmaUnico = 101, CodigoDisciplina = codigoDisciplina, NomeDisciplina = nomeDisciplina, NomeProfessor = "Dr. Newton" }
            };
        }

        #region Testes para MontarGrade (GET)

        [Fact]
        public async Task MontarGrade_GET_DeveRetornarViewComTurmasObrigatoriasEOptativas()
        {
            // Arrange
            var disciplinasPendentes = new List<string> { "Cálculo I" };
            var turmasObrigatorias = GetTurmasExemplo("MAT001", "Cálculo I");
            var turmasObrigatoriasVM = GetTurmasViewModelExemplo("MAT001", "Cálculo I");

            var aluno = new Aluno { Matricula = _matriculaUsuarioLogado, CurriculoAluno = "CCO-2020" };
            var curriculosOptativos = new List<Curriculo> { new Curriculo { NomeDisciplina = "Libras", TipoDisciplina = 2 } };
            var turmasOptativas = GetTurmasExemplo("OPT001", "Libras");
            var turmasOptativasVM = GetTurmasViewModelExemplo("OPT001", "Libras");

            // Setup Mocks para Turmas Obrigatórias
            _mockDisciplinaService.Setup(s => s.ObterDisciplinasLiberadas(_matriculaUsuarioLogado)).ReturnsAsync(disciplinasPendentes);
            _mockTurmaRepository.Setup(r => r.TurmasDisponiveisPorDisciplina("Cálculo I")).ReturnsAsync(turmasObrigatorias);

            // Setup Mocks para Turmas Optativas
            _mockAlunoRepository.Setup(r => r.ObterPorId(_matriculaUsuarioLogado)).ReturnsAsync(aluno);
            _mockCurriculoRepository.Setup(r => r.ObterDisciplinasPorCurrciulo(aluno.CurriculoAluno, 2)).ReturnsAsync(curriculosOptativos);
            _mockTurmaRepository.Setup(r => r.TurmasDisponiveisPorDisciplina("Libras")).ReturnsAsync(turmasOptativas);

            // Setup Mock do Mapper
            _mockMapper.Setup(m => m.Map<List<TurmaViewModel>>(turmasObrigatorias)).Returns(turmasObrigatoriasVM);
            _mockMapper.Setup(m => m.Map<List<TurmaViewModel>>(turmasOptativas)).Returns(turmasOptativasVM);

            // Setup Mock para turmas salvas (retornando lista vazia)
            _mockTurmaSalvaRepository.Setup(r => r.TodasTurmasSalvasAluno(_matriculaUsuarioLogado)).ReturnsAsync(new List<TurmaSalva>());

            _mockDisciplinaRepository.Setup(r => r.ObterPorCodigo(It.IsAny<string>())).ReturnsAsync(new Disciplina { Ementa = "Ementa teste" });


            // Act
            var result = await _controller.MontarGrade();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("~/Views/DashBoard/MontarGrade.cshtml", viewResult.ViewName);
            var model = Assert.IsType<MontarGradeResultViewModel>(viewResult.Model);
            Assert.NotEmpty(model.Turmas);
            Assert.NotEmpty(model.TurmasOptativas);
            Assert.Equal("Turmas disponíveis carregadas com sucesso.", model.Mensagem);
        }

        [Fact]
        public async Task MontarGrade_GET_QuandoNaoHaDisciplinasObrigatorias_DeveExibirMensagemDeParabens()
        {
            // Arrange
            // Nenhuma disciplina obrigatória pendente
            _mockDisciplinaService.Setup(s => s.ObterDisciplinasLiberadas(_matriculaUsuarioLogado)).ReturnsAsync(new List<string>());

            // Simula turmas optativas para isolar o teste
            var aluno = new Aluno { Matricula = _matriculaUsuarioLogado, CurriculoAluno = "CCO-2020" };
            var curriculosOptativos = new List<Curriculo> { new Curriculo { NomeDisciplina = "Libras", TipoDisciplina = 2 } };
            var turmasOptativas = GetTurmasExemplo("OPT001", "Libras");
            var turmasOptativasVM = GetTurmasViewModelExemplo("OPT001", "Libras");

            _mockAlunoRepository.Setup(r => r.ObterPorId(_matriculaUsuarioLogado)).ReturnsAsync(aluno);
            _mockCurriculoRepository.Setup(r => r.ObterDisciplinasPorCurrciulo(aluno.CurriculoAluno, 2)).ReturnsAsync(curriculosOptativos);
            _mockTurmaRepository.Setup(r => r.TurmasDisponiveisPorDisciplina("Libras")).ReturnsAsync(turmasOptativas);
            _mockMapper.Setup(m => m.Map<List<TurmaViewModel>>(turmasOptativas)).Returns(turmasOptativasVM);
            _mockTurmaSalvaRepository.Setup(r => r.TodasTurmasSalvasAluno(_matriculaUsuarioLogado)).ReturnsAsync(new List<TurmaSalva>());
            _mockDisciplinaRepository.Setup(r => r.ObterPorCodigo(It.IsAny<string>())).ReturnsAsync(new Disciplina { Ementa = "Ementa teste" });


            // Act
            var result = await _controller.MontarGrade();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<MontarGradeResultViewModel>(viewResult.Model);
            Assert.Empty(model.Turmas); // Nenhuma turma obrigatória
            Assert.NotEmpty(model.TurmasOptativas);
            Assert.Equal("Parabéns! Você não possui disciplinas obrigatórias pendentes.", model.Mensagem);
        }

        [Fact]
        public async Task MontarGrade_GET_QuandoOcorreExcecao_DeveRetornarViewComMensagemDeErro()
        {
            // Arrange
            var errorMessage = "Erro de teste no banco de dados";
            _mockDisciplinaService.Setup(s => s.ObterDisciplinasLiberadas(_matriculaUsuarioLogado)).ThrowsAsync(new System.Exception(errorMessage));

            // Act
            var result = await _controller.MontarGrade();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<MontarGradeResultViewModel>(viewResult.Model);
            Assert.Empty(model.Turmas);
            Assert.Empty(model.TurmasOptativas);
            Assert.Contains("Erro ao carregar as turmas:", model.Mensagem);
        }

        #endregion

        #region Testes para MontarGrade (POST)

        [Fact]
        public async Task MontarGrade_POST_ComHorariosValidos_DeveRetornarTurmasCompativeis()
        {
            // Arrange
            var viewModel = new MontarGradeViewModel { HorariosMarcados = "[\"2M1\",\"3M1\"]" };
            var disciplinasPendentes = new List<string> { "Cálculo I" };
            var turmasCompativeis = GetTurmasExemplo("MAT001", "Cálculo I");
            var turmasCompativeisVM = GetTurmasViewModelExemplo("MAT001", "Cálculo I");

            _mockDisciplinaService.Setup(s => s.ObterDisciplinasLiberadas(_matriculaUsuarioLogado)).ReturnsAsync(disciplinasPendentes);
            _mockTurmaService.Setup(s => s.BuscarTurmasCompativeis(It.IsAny<string>(), It.IsAny<List<AulaHorario>>())).ReturnsAsync(turmasCompativeis);
            _mockMapper.Setup(m => m.Map<List<TurmaViewModel>>(turmasCompativeis)).Returns(turmasCompativeisVM);
            _mockTurmaSalvaRepository.Setup(r => r.TodasTurmasSalvasAluno(_matriculaUsuarioLogado)).ReturnsAsync(new List<TurmaSalva>());
            _mockAlunoRepository.Setup(r => r.ObterPorId(_matriculaUsuarioLogado)).ReturnsAsync(new Aluno());


            // Act
            var result = await _controller.MontarGrade(viewModel);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<MontarGradeResultViewModel>(viewResult.Model);
            Assert.NotEmpty(model.Turmas);
            Assert.Equal("Turmas encontradas com sucesso. Selecione a turma desejada e clique em “Salvar turma”.", model.Mensagem);
        }

        [Fact]
        public async Task MontarGrade_POST_SemTurmasCompativeis_DeveRetornarTodasAsTurmasDisponiveis()
        {
            // Arrange
            var viewModel = new MontarGradeViewModel { HorariosMarcados = "[\"2M1\"]" };
            var disciplinasPendentes = new List<string> { "Cálculo I" };
            var todasAsTurmas = GetTurmasExemplo("MAT001", "Cálculo I");
            var todasAsTurmasVM = GetTurmasViewModelExemplo("MAT001", "Cálculo I");

            _mockDisciplinaService.Setup(s => s.ObterDisciplinasLiberadas(_matriculaUsuarioLogado)).ReturnsAsync(disciplinasPendentes);
            // Retorna lista vazia para turmas compatíveis
            _mockTurmaService.Setup(s => s.BuscarTurmasCompativeis(It.IsAny<string>(), It.IsAny<List<AulaHorario>>())).ReturnsAsync(new List<Turma>());

            // Setup para o recarregamento (_CarregarTurmasObrigatorias)
            _mockTurmaRepository.Setup(r => r.TurmasDisponiveisPorDisciplina("Cálculo I")).ReturnsAsync(todasAsTurmas);
            _mockMapper.Setup(m => m.Map<List<TurmaViewModel>>(todasAsTurmas)).Returns(todasAsTurmasVM);
            _mockTurmaSalvaRepository.Setup(r => r.TodasTurmasSalvasAluno(_matriculaUsuarioLogado)).ReturnsAsync(new List<TurmaSalva>());
            _mockAlunoRepository.Setup(r => r.ObterPorId(_matriculaUsuarioLogado)).ReturnsAsync(new Aluno());
            _mockDisciplinaRepository.Setup(r => r.ObterPorCodigo(It.IsAny<string>())).ReturnsAsync(new Disciplina { Ementa = "Ementa teste" });


            // Act
            var result = await _controller.MontarGrade(viewModel);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<MontarGradeResultViewModel>(viewResult.Model);
            Assert.NotEmpty(model.Turmas); // Deve ter carregado todas as turmas
            Assert.Equal("Nenhuma turma compatível foi encontrada. Por isso, estamos exibindo todas as turmas para as quais você já possui os pré-requisitos. Selecione a turma desejada e clique em “Salvar turma”.", model.Mensagem);
        }

        [Fact]
        public async Task MontarGrade_POST_ComModelStateInvalido_DeveRetornarMensagemDeErro()
        {
            // Arrange
            _controller.ModelState.AddModelError("HorariosMarcados", "Erro de modelo");
            var viewModel = new MontarGradeViewModel();
            _mockTurmaSalvaRepository.Setup(r => r.TodasTurmasSalvasAluno(_matriculaUsuarioLogado)).ReturnsAsync(new List<TurmaSalva>());
            _mockAlunoRepository.Setup(r => r.ObterPorId(_matriculaUsuarioLogado)).ReturnsAsync(new Aluno());

            // Act
            var result = await _controller.MontarGrade(viewModel);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<MontarGradeResultViewModel>(viewResult.Model);
            Assert.Equal("Erro: Selecione os horários antes de enviar a disponibilidade!", model.Mensagem);
        }

        #endregion

        #region Testes para SalvarGrade (POST)

        [Fact]
        public async Task SalvarGrade_ComTurmasValidas_DeveRetornarOkComMensagemDeSucesso()
        {
            // Arrange
            var turmasSelecionadasVM = GetTurmasViewModelExemplo();
            var turmasParaSalvar = new List<TurmaSalva> { new TurmaSalva { CodigoTurmaSalva = turmasSelecionadasVM.First().CodigoTurmaUnico } };

            _mockMapper.Setup(m => m.Map<List<TurmaSalva>>(turmasSelecionadasVM)).Returns(turmasParaSalvar);
            _mockTurmaSalvaRepository.Setup(r => r.ExcluirTurmasSalvasDoAluno(_matriculaUsuarioLogado)).Returns(Task.FromResult(true));
            _mockTurmaSalvaRepository.Setup(r => r.Adicionar(It.IsAny<TurmaSalva>())).ReturnsAsync(true); // CORRIGIDO

            // Act
            var result = await _controller.SalvarGrade(turmasSelecionadasVM);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            dynamic value = okResult.Value;
            Assert.True(value.GetType().GetProperty("success").GetValue(value, null));
            Assert.Equal("Grade salva com sucesso!", value.GetType().GetProperty("message").GetValue(value, null));

            // Verifica se os métodos corretos foram chamados
            _mockTurmaSalvaRepository.Verify(r => r.ExcluirTurmasSalvasDoAluno(_matriculaUsuarioLogado), Times.Once);
            _mockTurmaSalvaRepository.Verify(r => r.Adicionar(It.IsAny<TurmaSalva>()), Times.Exactly(turmasSelecionadasVM.Count));
        }

        [Fact]
        public async Task SalvarGrade_ComListaDeTurmasVazia_DeveRetornarBadRequest()
        {
            // Arrange
            var turmasSelecionadasVM = new List<TurmaViewModel>(); // Lista vazia

            // Act
            var result = await _controller.SalvarGrade(turmasSelecionadasVM);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Nenhuma turma foi selecionada.", badRequestResult.Value);
        }

        [Fact]
        public async Task SalvarGrade_ComListaDeTurmasNula_DeveRetornarBadRequest()
        {
            // Arrange
            List<TurmaViewModel> turmasSelecionadasVM = null; // Lista nula

            // Act
            var result = await _controller.SalvarGrade(turmasSelecionadasVM);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Nenhuma turma foi selecionada.", badRequestResult.Value);
        }

        [Fact]
        public async Task SalvarGrade_QuandoOcorreExcecao_DeveRetornarStatusCode500()
        {
            // Arrange
            var turmasSelecionadasVM = GetTurmasViewModelExemplo();
            var turmasParaSalvar = new List<TurmaSalva> { new TurmaSalva() };
            var errorMessage = "Erro ao acessar o banco";

            _mockMapper.Setup(m => m.Map<List<TurmaSalva>>(turmasSelecionadasVM)).Returns(turmasParaSalvar);
            // Simula uma exceção ao tentar excluir as turmas antigas
            _mockTurmaSalvaRepository.Setup(r => r.ExcluirTurmasSalvasDoAluno(_matriculaUsuarioLogado)).ThrowsAsync(new System.Exception(errorMessage));

            // Act
            var result = await _controller.SalvarGrade(turmasSelecionadasVM);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.Contains(errorMessage, (string)statusCodeResult.Value);
        }

        #endregion
    }
}
