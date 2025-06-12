//using Moq;
//using SPAA.Business.Interfaces.Repository;
//using SPAA.Business.Models;
//using SPAA.Business.Services;
//using System.Threading.Tasks;
//using Xunit;

//namespace SPAA.Business.Tests.Services
//{
//    public class AlunoServiceTests
//    {
//        private readonly Mock<IAlunoRepository> _alunoRepoMock;
//        private readonly AlunoService _service;

//        public AlunoServiceTests()
//        {
//            _alunoRepoMock = new Mock<IAlunoRepository>();
//            _service = new AlunoService(_alunoRepoMock.Object);
//        }

//        [Fact]
//        public async Task AdicionarCurriculoAluno_DeveRetornarTrue_QuandoAlunoExiste()
//        {
//            // Arrange
//            var matricula = "123";
//            var aluno = new Aluno { Matricula = matricula };
//            _alunoRepoMock.Setup(r => r.ObterPorId(matricula)).ReturnsAsync(aluno);

//            // Act
//            var resultado = await _service.AdicionarCurriculoAluno(matricula, "Novo Currículo");

//            // Assert
//            Assert.True(resultado);
//            _alunoRepoMock.Verify(r => r.Atualizar(It.Is<Aluno>(a => a.CurriculoAluno == "Novo Currículo")), Times.Once);
//        }

//        [Fact]
//        public async Task AdicionarCurriculoAluno_DeveRetornarFalse_QuandoAlunoNaoExiste()
//        {
//            _alunoRepoMock.Setup(r => r.ObterPorId("123")).ReturnsAsync((Aluno)null);

//            var resultado = await _service.AdicionarCurriculoAluno("123", "Teste");

//            Assert.False(resultado);
//        }

//        [Fact]
//        public async Task AlterarNome_DeveRetornarTrue_QuandoAlunoExiste()
//        {
//            var matricula = "123";
//            var aluno = new Aluno { Matricula = matricula };
//            _alunoRepoMock.Setup(r => r.ObterPorId(matricula)).ReturnsAsync(aluno);

//            var resultado = await _service.AlterarNome(matricula, "Novo Nome");

//            Assert.True(resultado);
//            _alunoRepoMock.Verify(r => r.Atualizar(It.Is<Aluno>(a => a.NomeAluno == "Novo Nome")), Times.Once);
//        }

//        [Fact]
//        public async Task AlterarNome_DeveRetornarFalse_QuandoAlunoNaoExiste()
//        {
//            _alunoRepoMock.Setup(r => r.ObterPorId("123")).ReturnsAsync((Aluno)null);

//            var resultado = await _service.AlterarNome("123", "Teste");

//            Assert.False(resultado);
//        }

//        [Fact]
//        public async Task AlunoJaAnexouHistorico_DeveRetornarTrue_SeHistoricoAnexado()
//        {
//            var aluno = new Aluno { HistoricoAnexado = true };
//            _alunoRepoMock.Setup(r => r.ObterPorId("123")).ReturnsAsync(aluno);

//            var resultado = await _service.AlunoJaAnexouHistorico("123");

//            Assert.True(resultado);
//        }

//        [Fact]
//        public async Task AlunoJaAnexouHistorico_DeveRetornarFalse_SeHistoricoNaoAnexado()
//        {
//            var aluno = new Aluno { HistoricoAnexado = false };
//            _alunoRepoMock.Setup(r => r.ObterPorId("123")).ReturnsAsync(aluno);

//            var resultado = await _service.AlunoJaAnexouHistorico("123");

//            Assert.False(resultado);
//        }

//        [Fact]
//        public async Task MarcarHistoricoComoAnexado_DeveAtualizarHistorico_SeNaoEstiverMarcado()
//        {
//            var aluno = new Aluno { Matricula = "123", HistoricoAnexado = false };
//            _alunoRepoMock.Setup(r => r.ObterPorId("123")).ReturnsAsync(aluno);

//            var (sucesso, mensagem) = await _service.MarcarHistoricoComoAnexado("123");

//            Assert.True(sucesso);
//            Assert.Equal("Histórico processado com sucesso!", mensagem);
//            _alunoRepoMock.Verify(r => r.Atualizar(It.Is<Aluno>(a => a.HistoricoAnexado)), Times.Once);
//        }

//        [Fact]
//        public async Task MarcarHistoricoComoAnexado_DeveRetornarMensagem_SeJaEstiverMarcado()
//        {
//            var aluno = new Aluno { Matricula = "123", HistoricoAnexado = true };
//            _alunoRepoMock.Setup(r => r.ObterPorId("123")).ReturnsAsync(aluno);

//            var (sucesso, mensagem) = await _service.MarcarHistoricoComoAnexado("123");

//            Assert.True(sucesso);
//            Assert.Equal("O histórico foi atualizado.", mensagem);
//        }

//        [Fact]
//        public async Task MarcarHistoricoComoAnexado_DeveRetornarErro_SeAlunoNaoExistir()
//        {
//            _alunoRepoMock.Setup(r => r.ObterPorId("123")).ReturnsAsync((Aluno)null);

//            var (sucesso, mensagem) = await _service.MarcarHistoricoComoAnexado("123");

//            Assert.False(sucesso);
//            Assert.Equal("Aluno com matrícula 123 não encontrado.", mensagem);
//        }
//    }
//}
