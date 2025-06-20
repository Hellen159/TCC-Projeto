using Moq;
using SPAA.Business.Interfaces.Repository;
using SPAA.Business.Models;
using SPAA.Business.Services;
using System.Threading.Tasks;
using Xunit;

namespace SPAA.Business.Tests.Services
{
    public class AlunoServiceTests
    {
        private readonly Mock<IAlunoRepository> _alunoRepoMock;
        private readonly Mock<ICursoRepository> _cursoRepoMock;
        private readonly AlunoService _service;

        public AlunoServiceTests()
        {
            _alunoRepoMock = new Mock<IAlunoRepository>();
            _cursoRepoMock = new Mock<ICursoRepository>();
            _service = new AlunoService(_alunoRepoMock.Object, _cursoRepoMock.Object);
        }


        [Fact]
        public async Task AdicionarCurriculoAluno_DeveRetornarTrue_QuandoAlunoExiste()
        {
            // Arrange
            var matricula = "123";
            var aluno = new Aluno { Matricula = matricula };
            _alunoRepoMock.Setup(r => r.ObterPorId(matricula)).ReturnsAsync(aluno);

            // Act
            var resultado = await _service.AdicionarCurriculoAluno(matricula, "Novo Currículo");

            // Assert
            Assert.True(resultado);
            _alunoRepoMock.Verify(r => r.Atualizar(It.Is<Aluno>(a => a.CurriculoAluno == "Novo Currículo")), Times.Once);
        }

        [Fact]
        public async Task AdicionarCurriculoAluno_DeveRetornarFalse_QuandoAlunoNaoExiste()
        {
            _alunoRepoMock.Setup(r => r.ObterPorId("123")).ReturnsAsync((Aluno)null);

            var resultado = await _service.AdicionarCurriculoAluno("123", "Teste");

            Assert.False(resultado);
        }

        [Fact]
        public async Task AlterarNome_DeveRetornarTrue_QuandoAlunoExiste()
        {
            var matricula = "123";
            var aluno = new Aluno { Matricula = matricula };
            _alunoRepoMock.Setup(r => r.ObterPorId(matricula)).ReturnsAsync(aluno);

            var resultado = await _service.AlterarNome(matricula, "Novo Nome");

            Assert.True(resultado);
            _alunoRepoMock.Verify(r => r.Atualizar(It.Is<Aluno>(a => a.NomeAluno == "Novo Nome")), Times.Once);
        }

        [Fact]
        public async Task AlterarNome_DeveRetornarFalse_QuandoAlunoNaoExiste()
        {
            _alunoRepoMock.Setup(r => r.ObterPorId("123")).ReturnsAsync((Aluno)null);

            var resultado = await _service.AlterarNome("123", "Teste");

            Assert.False(resultado);
        }

        [Fact]
        public async Task AlunoJaAnexouHistorico_DeveRetornarTrue_SeHistoricoAnexado()
        {
            var aluno = new Aluno { HistoricoAnexado = true };
            _alunoRepoMock.Setup(r => r.ObterPorId("123")).ReturnsAsync(aluno);

            var resultado = await _service.AlunoJaAnexouHistorico("123");

            Assert.True(resultado);
        }

        [Fact]
        public async Task AlunoJaAnexouHistorico_DeveRetornarFalse_SeHistoricoNaoAnexado()
        {
            var aluno = new Aluno { HistoricoAnexado = false };
            _alunoRepoMock.Setup(r => r.ObterPorId("123")).ReturnsAsync(aluno);

            var resultado = await _service.AlunoJaAnexouHistorico("123");

            Assert.False(resultado);
        }

        [Fact]
        public async Task AlunoJaAnexouHistorico_DeveRetornarFalse_QuandoAlunoNaoExiste()
        {
            _alunoRepoMock.Setup(r => r.ObterPorId(It.IsAny<string>())).ReturnsAsync((Aluno)null);

            var resultado = await _service.AlunoJaAnexouHistorico("qualquerMatricula");

            Assert.False(resultado); 
        }


        [Fact]
        public async Task MarcarHistoricoComoAnexado_DeveAtualizarHistorico_SeNaoEstiverMarcado()
        {
            var aluno = new Aluno { Matricula = "123", HistoricoAnexado = false };
            _alunoRepoMock.Setup(r => r.ObterPorId("123")).ReturnsAsync(aluno);

            var (sucesso, mensagem) = await _service.MarcarHistoricoComoAnexado("123");

            Assert.True(sucesso);
            Assert.Equal("Histórico processado com sucesso!", mensagem);
            _alunoRepoMock.Verify(r => r.Atualizar(It.Is<Aluno>(a => a.HistoricoAnexado)), Times.Once);
        }

        [Fact]
        public async Task MarcarHistoricoComoAnexado_DeveRetornarMensagem_SeJaEstiverMarcado()
        {
            var aluno = new Aluno { Matricula = "123", HistoricoAnexado = true };
            _alunoRepoMock.Setup(r => r.ObterPorId("123")).ReturnsAsync(aluno);

            var (sucesso, mensagem) = await _service.MarcarHistoricoComoAnexado("123");

            Assert.True(sucesso);
            Assert.Equal("O histórico foi atualizado.", mensagem);
        }

        [Fact]
        public async Task MarcarHistoricoComoAnexado_DeveRetornarErro_SeAlunoNaoExistir()
        {
            _alunoRepoMock.Setup(r => r.ObterPorId("123")).ReturnsAsync((Aluno)null);

            var (sucesso, mensagem) = await _service.MarcarHistoricoComoAnexado("123");

            Assert.False(sucesso);
            Assert.Equal("Aluno com matrícula 123 não encontrado.", mensagem);
        }


        [Fact]
        public async Task PorcentagemCurso_DeveCalcularCorretamente_QuandoAlunoECursoExistem()
        {
            // Arrange
            var matricula = "aluno1";
            var aluno = new Aluno
            {
                Matricula = matricula,
                HorasObrigatoriasPendentes = 50,
                HorasOptativasPendentes = 25
            };
            var curso = new Curso
            {
                CodigoCurso= 2,
                CargaHorariaObrigatoria = 100,
                CargaHorariaOptativa = 50
            };

            _alunoRepoMock.Setup(r => r.ObterPorId(matricula)).ReturnsAsync(aluno);
            _cursoRepoMock.Setup(r => r.ObterPorId(2)).ReturnsAsync(curso);

            var expectedPercentage = 50.0;

            // Act
            var actualPercentage = await _service.PorcentagemCurso(matricula);

            // Assert
            Assert.Equal(expectedPercentage, actualPercentage, 2); 
        }

        [Fact]
        public async Task PorcentagemCurso_DeveRetornarZero_QuandoAlunoNaoExiste()
        {
            // Arrange
            _alunoRepoMock.Setup(r => r.ObterPorId(It.IsAny<string>())).ReturnsAsync((Aluno)null);
            _cursoRepoMock.Setup(r => r.ObterPorId(2)).ReturnsAsync(new Curso()); 

            // Act
            var resultado = await _service.PorcentagemCurso("qualquerMatricula");

            // Assert
            Assert.Equal(0, resultado);
        }

        [Fact]
        public async Task PorcentagemCurso_DeveRetornarZero_QuandoCursoNaoExiste()
        {
            // Arrange
            _alunoRepoMock.Setup(r => r.ObterPorId(It.IsAny<string>())).ReturnsAsync(new Aluno()); 
            _cursoRepoMock.Setup(r => r.ObterPorId(2)).ReturnsAsync((Curso)null);

            // Act
            var resultado = await _service.PorcentagemCurso("qualquerMatricula");

            // Assert
            Assert.Equal(0, resultado);
        }

        [Fact]
        public async Task PorcentagemCurso_DeveLidarComCargaHorariaTotalZero()
        {
            // Arrange
            var matricula = "aluno1";
            var aluno = new Aluno
            {
                Matricula = matricula,
                HorasObrigatoriasPendentes = 0,
                HorasOptativasPendentes = 0
            };
            var curso = new Curso
            {
                CodigoCurso = 2,
                CargaHorariaObrigatoria = 0,
                CargaHorariaOptativa = 0
            };

            _alunoRepoMock.Setup(r => r.ObterPorId(matricula)).ReturnsAsync(aluno);
            _cursoRepoMock.Setup(r => r.ObterPorId(2)).ReturnsAsync(curso);

            var expectedPercentage = 0.0;

            // Act
            var actualPercentage = await _service.PorcentagemCurso(matricula);

            // Assert
            Assert.Equal(expectedPercentage, actualPercentage, 2);
        }



        [Fact]
        public async Task SalvarHorasAluno_DeveRetornarTrueEAtualizarHoras_QuandoAlunoExiste()
        {
            // Arrange
            var matricula = "alunoS";
            var aluno = new Aluno { Matricula = matricula, HorasOptativasPendentes = 100, HorasObrigatoriasPendentes = 200 };
            var novasOptativas = 50;
            var novasObrigatorias = 150;

            _alunoRepoMock.Setup(r => r.ObterPorId(matricula)).ReturnsAsync(aluno);

            // Act
            var resultado = await _service.SalvarHorasAluno(matricula, novasOptativas, novasObrigatorias);

            // Assert
            Assert.True(resultado);
            _alunoRepoMock.Verify(r => r.Atualizar(It.Is<Aluno>(a =>
                a.HorasOptativasPendentes == novasOptativas &&
                a.HorasObrigatoriasPendentes == novasObrigatorias
            )), Times.Once);
        }

        [Fact]
        public async Task SalvarHorasAluno_DeveRetornarFalse_QuandoAlunoNaoExiste()
        {
            // Arrange
            _alunoRepoMock.Setup(r => r.ObterPorId(It.IsAny<string>())).ReturnsAsync((Aluno)null);

            // Act
            var resultado = await _service.SalvarHorasAluno("alunoNaoExiste", 10, 20);

            // Assert
            Assert.False(resultado);
            _alunoRepoMock.Verify(r => r.Atualizar(It.IsAny<Aluno>()), Times.Never); 
        }
    }
}