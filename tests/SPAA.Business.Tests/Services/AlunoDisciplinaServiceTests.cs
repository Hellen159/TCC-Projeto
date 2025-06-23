using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using SPAA.Business.Interfaces.Repository;
using SPAA.Business.Interfaces.Services;
using SPAA.Business.Models;
using SPAA.Business.Services;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System;
using System.Reflection;

namespace SPAA.Business.Tests.Services
{
    public class AlunoDisciplinaServiceTests
    {
        private readonly Mock<ILogger<AlunoDisciplinaService>> _loggerMock;
        private readonly Mock<IAlunoService> _alunoServiceMock;
        private readonly Mock<IAlunoDisciplinaRepository> _alunoDisciplinaRepoMock;
        private readonly Mock<AlunoDisciplinaService> _serviceMock;
        private readonly AlunoDisciplinaService _service;

        public AlunoDisciplinaServiceTests()
        {
            _loggerMock = new Mock<ILogger<AlunoDisciplinaService>>();
            _alunoServiceMock = new Mock<IAlunoService>();
            _alunoDisciplinaRepoMock = new Mock<IAlunoDisciplinaRepository>();

            _serviceMock = new Mock<AlunoDisciplinaService>(
                _loggerMock.Object,
                _alunoServiceMock.Object,
                _alunoDisciplinaRepoMock.Object
            );
            _service = _serviceMock.Object;

            _serviceMock.Setup(s => s.ExtrairTextoDePdf(It.IsAny<IFormFile>()))
                       .ReturnsAsync((IFormFile file) => {
                           using var stream = file.OpenReadStream();
                           using var reader = new StreamReader(stream);
                           return reader.ReadToEnd();
                       });
        }

        private IFormFile CreateMockFormFile(string content, string fileName = "test.pdf", string contentType = "application/pdf")
        {
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
            var mockFormFile = new Mock<IFormFile>();
            mockFormFile.Setup(f => f.FileName).Returns(fileName);
            mockFormFile.Setup(f => f.Length).Returns(stream.Length);
            mockFormFile.Setup(f => f.ContentType).Returns(contentType);
            mockFormFile.Setup(f => f.OpenReadStream()).Returns(stream);
            mockFormFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                        .Returns((Stream targetStream, CancellationToken cancellationToken) =>
                        {
                            stream.Position = 0;
                            return stream.CopyToAsync(targetStream, cancellationToken);
                        });
            return mockFormFile.Object;
        }

        [Fact]
        public async Task ExtrairBlocos_DeveRetornarListaVazia_QuandoNaoEncontrarSemestres()
        {
            // Arrange
            string textoSemSemestre = "Este é um texto sem nenhum padrão de semestre ou ano.";

            // Act
            var result = await _service.ExtrairBlocos(textoSemSemestre);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task ExtrairBlocos_DeveRetornarListaVazia_QuandoNaoEncontrarSituacoes()
        {
            // Arrange
            string textoSemSituacao = "2023.1 - Disciplina Teste - sem situacao final";

            // Act
            var result = await _service.ExtrairBlocos(textoSemSituacao);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }


        [Fact]
        public void ProcessarBloco_DeveRetornarNull_QuandoSemestreInvalido()
        {
            // Arrange
            var bloco = "DISCIPLINA X - APR";
            var matricula = "12345";
            var method = typeof(AlunoDisciplinaService).GetMethod("ProcessarBloco", BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null) throw new MissingMethodException("ProcessarBloco não encontrado.");

            // Act
            var result = (AlunoDisciplina)method.Invoke(_service, new object[] { bloco, matricula });

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void ProcessarBloco_DeveRetornarNull_QuandoSituacaoInvalida()
        {
            // Arrange
            var bloco = "2023.1 - DISCIPLINA Y - XYZ";
            var matricula = "12345";
            var method = typeof(AlunoDisciplinaService).GetMethod("ProcessarBloco", BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null) throw new MissingMethodException("ProcessarBloco não encontrado.");

            // Act
            var result = (AlunoDisciplina)method.Invoke(_service, new object[] { bloco, matricula });

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void ProcessarBloco_DeveLidarComBlocosDeUmaLinha_SemNomeCompleto()
        {
            // Arrange
            var bloco = "2023.1 - APR";
            var matricula = "12345";
            var method = typeof(AlunoDisciplinaService).GetMethod("ProcessarBloco", BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null) throw new MissingMethodException("ProcessarBloco não encontrado.");

            // Act
            var result = (AlunoDisciplina)method.Invoke(_service, new object[] { bloco, matricula });

            // Assert
            Assert.NotNull(result);
            Assert.Equal("2023.1", result.Semestre);
            Assert.Equal("APR", result.Situacao);
            Assert.Equal("", result.NomeDisicplina);
        }

        [Fact]
        public void ProcessarBloco_DeveLidarComBlocosDeMultiplasLinhas()
        {
            // Arrange
            var bloco = "2023.1\nDISCIPLINA EM DUAS LINHAS\nAPR";
            var matricula = "12345";
            var method = typeof(AlunoDisciplinaService).GetMethod("ProcessarBloco", BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null) throw new MissingMethodException("ProcessarBloco não encontrado.");

            // Act
            var result = (AlunoDisciplina)method.Invoke(_service, new object[] { bloco, matricula });

            // Assert
            Assert.NotNull(result);
            Assert.Equal("2023.1", result.Semestre);
            Assert.Equal("APR", result.Situacao);
            Assert.Equal(AlunoDisciplinaService.NormalizarTexto("DISCIPLINA EM DUAS LINHAS"), result.NomeDisicplina); // Chamada direta
        }

        [Fact]
        public async Task ConverterBlocos_DeveRetornarListaVazia_QuandoNaoHaBlocosValidos()
        {
            // Arrange
            var blocos = new List<string>
            {
                "INVALIDO - BLOCO 1",
                "OUTRO BLOCO INVALIDO"
            };
            var matricula = "ABCDE";

            // Act
            var result = await _service.ConverterBlocos(blocos, matricula);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task ExtrairTextoDePdf_DeveRetornarTextoDoStream()
        {
            // Arrange
            var content = "Conteúdo de teste para extração de PDF.";
            var mockFile = CreateMockFormFile(content);

            // Act
            var result = await _service.ExtrairTextoDePdf(mockFile);

            // Assert
            Assert.Equal(content, result);
        }

        [Fact]
        public async Task ObterEquivalenciasCurriculo_DeveRetornarNull_QuandoNaoEncontrarMarcador()
        {
            // Arrange
            string texto = "Texto sem a seção de Equivalências.";
            var matricula = "ALU123";

            // Act
            var result = await _service.ObterEquivalenciasCurriculo(texto, matricula);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task ObterEquivalenciasCurriculo_DeveRetornarListaVazia_QuandoNaoEncontrarPadraoCumpriu()
        {
            // Arrange
            string texto = "Equivalências:\n DISC001 - Disciplina (Não Cumpriu)";
            var matricula = "ALU123";

            // Act
            var result = await _service.ObterEquivalenciasCurriculo(texto, matricula);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task ObterEquivalenciasCurriculo_DeveRetornarListaVazia_QuandoLidarComParentesesForaDaPosicaoEsperada()
        {
            // Arrange
            string texto = "Equivalências:\n (Cumpriu) DISC001 - Disciplina Invalida";
            var matricula = "ALU123";

            // Act
            var result = await _service.ObterEquivalenciasCurriculo(texto, matricula);

            // Assert
            Assert.Empty(result);
        }

        // --- Testes para ObterInformacoesCurriculo ---
        [Fact]
        public async Task ObterInformacoesCurriculo_DeveExtrairCorretamente()
        {
            // Arrange
            string texto = "Algum texto. Currículo: 2019.2. Mais texto.";
            var expectedCurriculo = "2019.2";

            // Act
            var result = await _service.ObterInformacoesCurriculo(texto);

            // Assert
            Assert.Equal(expectedCurriculo, result);
        }

        [Fact]
        public async Task ObterInformacoesCurriculo_DeveRetornarNull_QuandoNaoEncontrarMarcador()
        {
            // Arrange
            string texto = "Texto sem a palavra Currículo.";

            // Act
            var result = await _service.ObterInformacoesCurriculo(texto);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task ObterInformacoesCurriculo_DeveRetornarNull_QuandoEncontrarMarcadorMasNaoFormatoAnoSemestre()
        {
            // Arrange
            string texto = "Currículo: ABCDE.";

            // Act
            var result = await _service.ObterInformacoesCurriculo(texto);

            // Assert
            Assert.Null(result);
        }

        // --- Testes para ObterObrigatoriasPendentes ---
        [Fact]
        public async Task ObterObrigatoriasPendentes_DeveExtrairCorretamente()
        {
            // Arrange
            string texto = @"
                Componentes Curriculares Obrigatórios Pendentes:
                Disciplina Pendente 1 60 h BSB0001
                Disciplina Pendente 2 30 h BSB0002 Matriculado em Equivalente
                Disciplina Pendente 3 90 h ABC1234
                Componentes Curriculares Opcionais Pendentes:";
            var matricula = "ALU456";

            // Act
            var result = await _service.ObterObrigatoriasPendentes(texto, matricula);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.Contains(result, d => d.NomeDisicplina == AlunoDisciplinaService.NormalizarTexto("Disciplina Pendente 1") && d.Situacao == "PEND"); // Chamada direta
            Assert.Contains(result, d => d.NomeDisicplina == AlunoDisciplinaService.NormalizarTexto("Disciplina Pendente 2") && d.Situacao == "PEND"); // Chamada direta
            Assert.Contains(result, d => d.NomeDisicplina == AlunoDisciplinaService.NormalizarTexto("Disciplina Pendente 3") && d.Situacao == "PEND"); // Chamada direta
        }

        [Fact]
        public async Task ObterObrigatoriasPendentes_DeveRetornarListaVazia_QuandoNaoEncontrarTitulo()
        {
            // Arrange
            string texto = "Texto sem a seção de obrigatórias pendentes.";
            var matricula = "ALU456";

            // Act
            var result = await _service.ObterObrigatoriasPendentes(texto, matricula);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            _alunoServiceMock.Verify(s => s.SalvarHorasAluno(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task ObterObrigatoriasPendentes_DeveRetornarListaVazia_QuandoNaoEncontrarPadraoDeLinha()
        {
            // Arrange
            string texto = @"
                Componentes Curriculares Obrigatórios Pendentes:
                Linha sem padrao
                Outra linha sem padrao
                Componentes Curriculares Opcionais Pendentes:";
            var matricula = "ALU456";

            // Act
            var result = await _service.ObterObrigatoriasPendentes(texto, matricula);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            _alunoServiceMock.Verify(s => s.SalvarHorasAluno(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task ObterHorasPendentes_DeveRetornarListaVazia_QuandoNaoEncontrarMarcadorInicio()
        {
            // Arrange
            string texto = "Texto sem o marcador 'Exigido Integralizado Pendente'";
            var matricula = "ALU789";

            // Act
            var result = await _service.ObterHorasPendentes(texto, matricula);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            _alunoServiceMock.Verify(s => s.SalvarHorasAluno(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task ObterHorasPendentes_DeveRetornarListaVazia_QuandoNaoHaHorasSuficientes()
        {
            // Arrange
            string texto = @"
                Exigido
                Integralizado
                Pendente
                Carga Horaria Optativa: 100 h
                ";
            var matricula = "ALU789";

            // Act
            var result = await _service.ObterHorasPendentes(texto, matricula);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            _alunoServiceMock.Verify(s => s.SalvarHorasAluno(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task ConsumirHistoricoPdf_DeveRetornarErro_QuandoArquivoNulo()
        {
            // Arrange
            IFormFile arquivoNulo = null;
            var matricula = "ALU999";

            // Act
            var (isValid, mensagem) = await _service.ConsumirHistoricoPdf(arquivoNulo, matricula);

            // Assert
            Assert.False(isValid);
            Assert.Equal("Nenhum arquivo enviado.", mensagem);
            _loggerMock.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Never);
        }

        [Fact]
        public async Task ConsumirHistoricoPdf_DeveRetornarErro_QuandoArquivoVazio()
        {
            // Arrange
            var mockArquivoVazio = CreateMockFormFile("", "vazio.pdf");
            var matricula = "ALU999";

            // Act
            var (isValid, mensagem) = await _service.ConsumirHistoricoPdf(mockArquivoVazio, matricula);

            // Assert
            Assert.False(isValid);
            Assert.Equal("Nenhum arquivo enviado.", mensagem);
            _loggerMock.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Never);
        }

       

        [Fact]
        public async Task ConsumirHistoricoPdf_DeveRetornarErro_QuandoMarcarHistoricoFalha()
        {
            // Arrange
            var matricula = "ALU999";
            var textoSimuladoPdf = @"
                2022.1 - DISCIPLINA 1 - APR
                Exigido
                Integralizado
                Pendente
                Carga Horaria Optativa: 150 h
                Carga Horaria Obrigatoria: 300 h
                ";
            var mockPdfFile = CreateMockFormFile(textoSimuladoPdf);

            _alunoServiceMock.Setup(s => s.MarcarHistoricoComoAnexado(matricula))
                .ReturnsAsync((false, "Falha ao marcar histórico."))
                .Verifiable();

            // Act
            var (isValid, mensagem) = await _service.ConsumirHistoricoPdf(mockPdfFile, matricula);

            // Assert
            Assert.False(isValid);
            Assert.Equal("Falha ao marcar histórico.", mensagem);
            _alunoServiceMock.Verify(s => s.MarcarHistoricoComoAnexado(matricula), Times.Once);
        }

        [Fact]
        public async Task ConsumirHistoricoPdf_DeveLidarComExcecaoDuranteProcessamento()
        {
            // Arrange
            var matricula = "ALU999";
            var mockPdfFile = CreateMockFormFile("Conteúdo irrelevante, pois o ExtrairTextoDePdf será mockado para lançar exceção.");

            _serviceMock.Setup(s => s.ExtrairTextoDePdf(It.IsAny<IFormFile>()))
                        .ThrowsAsync(new InvalidOperationException("Erro simulado durante a extração de texto do PDF."));

            // Act
            var (isValid, mensagem) = await _service.ConsumirHistoricoPdf(mockPdfFile, matricula);

            // Assert
            Assert.False(isValid);
            Assert.Equal("Erro ao processar o arquivo: Erro simulado durante a extração de texto do PDF.. Detalhes internos: sem inner exception", mensagem);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Erro ao processar o histórico do aluno.")),
                    It.IsAny<InvalidOperationException>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}