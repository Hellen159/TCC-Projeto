using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http; // Para IFormFile, DefaultHttpContext
using System.Security.Claims; // Para ClaimsPrincipal
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ViewFeatures; // Para ITempDataDictionary, ITempDataProvider
using System.Collections.Generic; // Para Dictionary<string, object>

using SPAA.App.Controllers;
using SPAA.Business.Interfaces.Repository;
using SPAA.Business.Interfaces.Services;
// Remover ou comentar esta linha, já que você não tem ServiceResult
// using SPAA.Business.Models.ServiceResult; 

namespace SPAA.App.Tests.Controllers
{
    public class UploadControllerTests
    {
        private readonly Mock<IAlunoDisciplinaRepository> _mockAlunoDisciplinaRepository;
        private readonly Mock<IAlunoRepository> _mockAlunoRepository;
        private readonly Mock<IAlunoDisciplinaService> _mockAlunoDisciplinaService;
        private readonly UploadController _controller;

        // Mocks para o TempData
        private readonly ITempDataDictionary _tempDataDictionary;

        public UploadControllerTests()
        {
            _mockAlunoDisciplinaRepository = new Mock<IAlunoDisciplinaRepository>();
            _mockAlunoRepository = new Mock<IAlunoRepository>();
            _mockAlunoDisciplinaService = new Mock<IAlunoDisciplinaService>();

            // Configuração do TempData (conforme discutido anteriormente)
            var mockTempDataProvider = new Mock<ITempDataProvider>();
            mockTempDataProvider.Setup(p => p.LoadTempData(It.IsAny<HttpContext>()))
                                .Returns(new Dictionary<string, object>());
            _tempDataDictionary = new TempDataDictionary(new DefaultHttpContext(), mockTempDataProvider.Object);

            // Instancia o Controller
            _controller = new UploadController(
                _mockAlunoDisciplinaRepository.Object,
                _mockAlunoRepository.Object,
                _mockAlunoDisciplinaService.Object
            );

            // Configurar o ControllerContext para simular um usuário logado
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "testuser@email.com") // User.Identity.Name
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Atribui o TempData mockado ao Controller
            _controller.TempData = _tempDataDictionary;
        }

        // --- Testes para UploadHistorico() (HttpGet) ---

        [Fact]
        public void UploadHistorico_GET_DeveRetornarView()
        {
            // Act
            var result = _controller.UploadHistorico();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        // --- Testes para UploadHistorico (HttpPost) ---

        [Fact]
        public async Task UploadHistorico_POST_ComSucesso_DeveRedirecionarParaHomeIndexESetarTempDataSucesso()
        {
            // Arrange
            var mockHistoricoFile = new Mock<IFormFile>();
            var userName = "testuser@email.com";
            var successMessage = "Histórico processado com sucesso!";

            // Setup: ExcluirDisciplinasDoAluno retorna true (existem dados para excluir ou processo ocorreu)
            _mockAlunoDisciplinaRepository.Setup(r => r.ExcluirDisciplinasDoAluno(userName))
                                          .ReturnsAsync(true);

            // Setup: ConsumirHistoricoPdf retorna uma tupla (true, mensagem)
            _mockAlunoDisciplinaService.Setup(s => s.ConsumirHistoricoPdf(mockHistoricoFile.Object, userName))
                                      .ReturnsAsync((true, successMessage)); // <--- MUDANÇA AQUI!

            // Act
            var result = await _controller.UploadHistorico(mockHistoricoFile.Object) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
            Assert.Equal("Home", result.ControllerName);

            // Verifica se os métodos do repositório/serviço foram chamados
            _mockAlunoDisciplinaRepository.Verify(r => r.ExcluirDisciplinasDoAluno(userName), Times.Once);
            _mockAlunoDisciplinaService.Verify(s => s.ConsumirHistoricoPdf(mockHistoricoFile.Object, userName), Times.Once);

            // Verifica o TempData
            Assert.Equal(successMessage, _tempDataDictionary["MensagemSucesso"]);
            Assert.Null(_tempDataDictionary["ErrorMessage"]); // Garante que não há mensagem de erro
        }

        [Fact]
        public async Task UploadHistorico_POST_ComFalhaNoServico_DeveRedirecionarParaUploadHistoricoESetarTempDataErro()
        {
            // Arrange
            var mockHistoricoFile = new Mock<IFormFile>();
            var userName = "testuser@email.com";
            var errorMessage = "Erro ao processar o histórico.";

            // Setup: ExcluirDisciplinasDoAluno retorna true
            _mockAlunoDisciplinaRepository.Setup(r => r.ExcluirDisciplinasDoAluno(userName))
                                          .ReturnsAsync(true);

            // Setup: ConsumirHistoricoPdf retorna uma tupla (false, mensagem)
            _mockAlunoDisciplinaService.Setup(s => s.ConsumirHistoricoPdf(mockHistoricoFile.Object, userName))
                                      .ReturnsAsync((false, errorMessage)); // <--- MUDANÇA AQUI!

            // Act
            var result = await _controller.UploadHistorico(mockHistoricoFile.Object) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("UploadHistorico", result.ActionName); // Redireciona para a mesma tela de upload
            Assert.Equal("Upload", result.ControllerName); // Verifica o controlador padrão de retorno

            _mockAlunoDisciplinaRepository.Verify(r => r.ExcluirDisciplinasDoAluno(userName), Times.Once);
            _mockAlunoDisciplinaService.Verify(s => s.ConsumirHistoricoPdf(mockHistoricoFile.Object, userName), Times.Once);

            // Verifica o TempData
            Assert.Equal(errorMessage, _tempDataDictionary["ErrorMessage"]);
            Assert.Null(_tempDataDictionary["MensagemSucesso"]); // Garante que não há mensagem de sucesso
        }

        [Fact]
        public async Task UploadHistorico_POST_ComFalhaNoServico_E_ReturnActionControllerEspecificado_DeveRedirecionarCorretamente()
        {
            // Arrange
            var mockHistoricoFile = new Mock<IFormFile>();
            var userName = "testuser@email.com";
            var errorMessage = "Erro específico!";
            var customAction = "MinhaAcao";
            var customController = "MeuControle";

            _mockAlunoDisciplinaRepository.Setup(r => r.ExcluirDisciplinasDoAluno(userName))
                                          .ReturnsAsync(true);
            _mockAlunoDisciplinaService.Setup(s => s.ConsumirHistoricoPdf(mockHistoricoFile.Object, userName))
                                      .ReturnsAsync((false, errorMessage)); // <--- MUDANÇA AQUI!

            // Act
            var result = await _controller.UploadHistorico(mockHistoricoFile.Object, customAction, customController) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(customAction, result.ActionName);
            Assert.Equal(customController, result.ControllerName);
            Assert.Equal(errorMessage, _tempDataDictionary["ErrorMessage"]);
        }

        [Fact]
        public async Task UploadHistorico_POST_ComSucesso_E_ReturnActionControllerEspecificado_DeveRedirecionarCorretamente()
        {
            // Arrange
            var mockHistoricoFile = new Mock<IFormFile>();
            var userName = "testuser@email.com";
            var successMessage = "Upload customizado com sucesso!";
            var customAction = "Dashboard";
            var customController = "Admin";

            _mockAlunoDisciplinaRepository.Setup(r => r.ExcluirDisciplinasDoAluno(userName))
                                          .ReturnsAsync(true);
            _mockAlunoDisciplinaService.Setup(s => s.ConsumirHistoricoPdf(mockHistoricoFile.Object, userName))
                                      .ReturnsAsync((true, successMessage)); // <--- MUDANÇA AQUI!

            // Act
            var result = await _controller.UploadHistorico(mockHistoricoFile.Object, customAction, customController) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(customAction, result.ActionName);
            Assert.Equal(customController, result.ControllerName);
            Assert.Equal(successMessage, _tempDataDictionary["MensagemSucesso"]);
        }
    }
}