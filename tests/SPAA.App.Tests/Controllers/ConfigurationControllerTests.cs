// SPAA.App.Tests/Controllers/ConfigurationControllerTests.cs
using Xunit;
using Moq;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SPAA.App.Controllers; // Onde sua ConfigurationController está
using SPAA.App.ViewModels; // Seus ViewModels
using SPAA.Business.Interfaces.Repository;
using SPAA.Business.Interfaces.Services;
using SPAA.Business.Models;
using System.Security.Claims; // Para ClaimsPrincipal
using Microsoft.AspNetCore.Http; // Para DefaultHttpContext
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ViewFeatures; // Para ITempDataDictionary e TempDataDictionaryFactory
using System.Collections.Generic; // Para IdentityError

// Remova este using se não for mais necessário:
// using Microsoft.AspNetCore.Mvc.ViewFeatures.Infrastructure; 

namespace SPAA.App.Tests.Controllers
{
    public class ConfigurationControllerTests
    {
        private readonly Mock<IAlunoRepository> _mockAlunoRepository;
        private readonly Mock<IAlunoService> _mockAlunoService;
        private readonly Mock<IAlunoDisciplinaRepository> _mockAlunoDisciplinaRepository;
        private readonly Mock<IApplicationUserRepository> _mockApplicationUserRepository;
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
        private readonly ConfigurationController _controller;

        // Mocks para o TempData
        private readonly Mock<ITempDataDictionaryFactory> _mockTempDataFactory;
        private readonly ITempDataDictionary _tempDataDictionary;

        public ConfigurationControllerTests()
        {
            _mockAlunoRepository = new Mock<IAlunoRepository>();
            _mockAlunoService = new Mock<IAlunoService>();
            _mockAlunoDisciplinaRepository = new Mock<IAlunoDisciplinaRepository>();
            _mockApplicationUserRepository = new Mock<IApplicationUserRepository>();

            // Configuração para mockar UserManager:
            var userStore = new Mock<IUserStore<ApplicationUser>>();
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(
                userStore.Object, null, null, null, null, null, null, null, null);

            // *******************************************************************
            // NOVA CONFIGURAÇÃO PARA MOCKAR TempData SEM ITempDataSerializer:
            // Basta instanciar TempDataDictionary diretamente, sem o serializer,
            // e então mockar o factory para retorná-lo.
            // Configuração para mockar TempData:
            // 1. Crie um mock para ITempDataProvider
            var mockTempDataProvider = new Mock<ITempDataProvider>();

            // 2. Configure o mock para que o LoadTempData retorne um dicionário vazio
            // (ou um dicionário com dados predefinidos se você precisar de TempData inicial nos seus testes)
            mockTempDataProvider.Setup(p => p.LoadTempData(It.IsAny<HttpContext>()))
                                .Returns(new Dictionary<string, object>());

            // 3. Crie o TempDataDictionary usando o mock do ITempDataProvider
            _tempDataDictionary = new TempDataDictionary(new DefaultHttpContext(), mockTempDataProvider.Object);

            // 4. Mantenha o mock do ITempDataDictionaryFactory para configurar o Controller.TempData
            _mockTempDataFactory = new Mock<ITempDataDictionaryFactory>();
            _mockTempDataFactory.Setup(f => f.GetTempData(It.IsAny<HttpContext>())).Returns(_tempDataDictionary);
            _mockTempDataFactory = new Mock<ITempDataDictionaryFactory>();
            _mockTempDataFactory.Setup(f => f.GetTempData(It.IsAny<HttpContext>())).Returns(_tempDataDictionary);
            // *******************************************************************

            // Instancia o Controller
            _controller = new ConfigurationController(
                _mockAlunoRepository.Object,
                _mockUserManager.Object,
                _mockApplicationUserRepository.Object,
                _mockAlunoDisciplinaRepository.Object,
                _mockAlunoService.Object
            );

            // Configurar o ControllerContext para simular um usuário logado
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "testuser@email.com"), // User.Identity.Name
                new Claim(ClaimTypes.NameIdentifier, "test_user_id") // Para GetUserAsync
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Atribui o TempData mockado ao Controller
            _controller.TempData = _mockTempDataFactory.Object.GetTempData(_controller.HttpContext);
        }

        // --- Testes para Configurations() (HttpGet) ---

        [Fact]
        public void Configurations_DeveRetornarView()
        {
            // Act
            var result = _controller.Configurations();

            // Assert
            Assert.IsType<ViewResult>(result); // Verifica se o tipo de retorno é ViewResult
        }

        // --- Testes para AlterarNome (HttpPost) ---

        [Fact]
        public async Task AlterarNome_ComSucesso_DeveRedirecionarParaHomeIndexESetarTempDataSucesso()
        {
            // Arrange
            var model = new ConfigurationViewModel { NovoNome = "Novo Nome Teste" };
            var userName = "testuser@email.com"; // Corresponde ao ClaimTypes.Name mockado

            // Setup: _alunoService.AlterarNome retorna true (sucesso)
            _mockAlunoService.Setup(s => s.AlterarNome(userName, model.NovoNome))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.AlterarNome(model) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
            Assert.Equal("Home", result.ControllerName);
            // Verifica se o serviço foi chamado corretamente
            _mockAlunoService.Verify(s => s.AlterarNome(userName, model.NovoNome), Times.Once);
            // Verifica o TempData
            Assert.Equal("Nome alterado com sucesso!", _tempDataDictionary["MensagemSucesso"]);
        }

        [Fact]
        public async Task AlterarNome_ComFalha_DeveRedirecionarParaHomeIndexESetarTempDataErro()
        {
            // Arrange
            var model = new ConfigurationViewModel { NovoNome = "Nome Invalido" };
            var userName = "testuser@email.com";

            // Setup: _alunoService.AlterarNome retorna false (falha)
            _mockAlunoService.Setup(s => s.AlterarNome(userName, model.NovoNome))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.AlterarNome(model) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
            Assert.Equal("Home", result.ControllerName);
            _mockAlunoService.Verify(s => s.AlterarNome(userName, model.NovoNome), Times.Once);
            Assert.Equal("Erro ao alterar nome!", _tempDataDictionary["ErrorMessage"]);
        }

        // --- Testes para AlterarSenha (HttpPost) ---

        [Fact]
        public async Task AlterarSenha_UsuarioNaoEncontrado_DeveRedirecionarParaHomeIndexESetarTempDataErro()
        {
            // Arrange
            var model = new ConfigurationViewModel { SenhaAtual = "OldPass", NovaSenha = "NewPass" };
            // Simula UserManager.GetUserAsync retornando null
            _mockUserManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync((ApplicationUser)null);

            // Act
            var result = await _controller.AlterarSenha(model) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
            Assert.Equal("Home", result.ControllerName);
            _mockUserManager.Verify(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>()), Times.Once);
            // Garante que nenhum método de alteração de senha foi chamado
            _mockApplicationUserRepository.Verify(r => r.VerificarSenhaAtual(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
            Assert.Equal("Usuário não encontrado.", _tempDataDictionary["ErrorMessage"]);
        }

        [Fact]
        public async Task AlterarSenha_SenhaAtualIncorreta_DeveRedirecionarParaHomeIndexESetarTempDataErro()
        {
            // Arrange
            var model = new ConfigurationViewModel { SenhaAtual = "WrongOldPass", NovaSenha = "NewPass" };
            var testUser = new ApplicationUser { Id = "test_user_id", UserName = "testuser@email.com" };

            _mockUserManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(testUser);
            // Simula VerificarSenhaAtual retornando false
            _mockApplicationUserRepository.Setup(r => r.VerificarSenhaAtual(testUser, model.SenhaAtual))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.AlterarSenha(model) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
            Assert.Equal("Home", result.ControllerName);
            _mockUserManager.Verify(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>()), Times.Once);
            _mockApplicationUserRepository.Verify(r => r.VerificarSenhaAtual(testUser, model.SenhaAtual), Times.Once);
            // Garante que AlterarSenha não foi chamado
            _mockApplicationUserRepository.Verify(r => r.AlterarSenha(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            Assert.Equal("Senha atual incorreta.", _tempDataDictionary["ErrorMessage"]);
        }

        [Fact]
        public async Task AlterarSenha_ComSucesso_DeveRedirecionarParaHomeIndexESetarTempDataSucesso()
        {
            // Arrange
            var model = new ConfigurationViewModel { SenhaAtual = "OldPass", NovaSenha = "NewPass" };
            var testUser = new ApplicationUser { Id = "test_user_id", UserName = "testuser@email.com" };

            _mockUserManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(testUser);
            _mockApplicationUserRepository.Setup(r => r.VerificarSenhaAtual(testUser, model.SenhaAtual))
                .ReturnsAsync(true);
            // Simula AlterarSenha retornando Success
            _mockApplicationUserRepository.Setup(r => r.AlterarSenha(testUser, model.SenhaAtual, model.NovaSenha))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _controller.AlterarSenha(model) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
            Assert.Equal("Home", result.ControllerName);
            _mockUserManager.Verify(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>()), Times.Once);
            _mockApplicationUserRepository.Verify(r => r.VerificarSenhaAtual(testUser, model.SenhaAtual), Times.Once);
            _mockApplicationUserRepository.Verify(r => r.AlterarSenha(testUser, model.SenhaAtual, model.NovaSenha), Times.Once);
            Assert.Equal("Senha alterada com sucesso!", _tempDataDictionary["MensagemSucesso"]);
        }

        [Fact]
        public async Task AlterarSenha_FalhaAoAlterarSenha_DeveRedirecionarParaHomeIndexESetarTempDataErroComDescricao()
        {
            // Arrange
            var model = new ConfigurationViewModel { SenhaAtual = "OldPass", NovaSenha = "WeakNewPass" };
            var testUser = new ApplicationUser { Id = "test_user_id", UserName = "testuser@email.com" };
            var errors = new List<IdentityError> { new IdentityError { Description = "Password too weak." } };

            _mockUserManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(testUser);
            _mockApplicationUserRepository.Setup(r => r.VerificarSenhaAtual(testUser, model.SenhaAtual))
                .ReturnsAsync(true);
            // Simula AlterarSenha retornando falha
            _mockApplicationUserRepository.Setup(r => r.AlterarSenha(testUser, model.SenhaAtual, model.NovaSenha))
                .ReturnsAsync(IdentityResult.Failed(errors.ToArray()));

            // Act
            var result = await _controller.AlterarSenha(model) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
            Assert.Equal("Home", result.ControllerName);
            _mockUserManager.Verify(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>()), Times.Once);
            _mockApplicationUserRepository.Verify(r => r.VerificarSenhaAtual(testUser, model.SenhaAtual), Times.Once);
            _mockApplicationUserRepository.Verify(r => r.AlterarSenha(testUser, model.SenhaAtual, model.NovaSenha), Times.Once);
            Assert.Equal("Password too weak.", _tempDataDictionary["ErrorMessage"]); // Verifica a descrição do erro
        }

        // --- Testes para ConfirmarExclusao (HttpGet) ---

        [Fact]
        public void ConfirmarExclusao_DeveRetornarView()
        {
            // Act
            var result = _controller.ConfirmarExclusao();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        // --- Testes para ExcluirConta (HttpPost) ---

        [Fact]
        public async Task ExcluirConta_MatriculaNula_DeveRedirecionarParaConfirmarExclusaoESetarTempDataErro()
        {
            // Arrange
            var model = new ConfirmarExclusaoViewModel { Matricula = null };

            // Act
            var result = await _controller.ExcluirConta(model) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("ConfirmarExclusao", result.ActionName);
            Assert.Equal("A matrícula é obrigatória para confirmar a exclusão.", _tempDataDictionary["ErrorMessage"]);
            // Garante que nenhum repositório/service foi chamado
            _mockUserManager.Verify(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>()), Times.Never);
        }

        [Fact]
        public async Task ExcluirConta_UsuarioNaoEncontrado_DeveRedirecionarParaConfirmarExclusaoESetarTempDataErro()
        {
            // Arrange
            var model = new ConfirmarExclusaoViewModel { Matricula = "matricula_invalida" };
            _mockUserManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync((ApplicationUser)null); // Simula usuário não encontrado

            // Act
            var result = await _controller.ExcluirConta(model) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("ConfirmarExclusao", result.ActionName);
            Assert.Equal("A matrícula informada não corresponde ao usuário.", _tempDataDictionary["ErrorMessage"]);
            _mockUserManager.Verify(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>()), Times.Once);
            // Garante que nenhum repositório foi chamado após a verificação do usuário
            _mockAlunoRepository.Verify(r => r.Remover(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ExcluirConta_MatriculaNaoCorresponde_DeveRedirecionarParaConfirmarExclusaoESetarTempDataErro()
        {
            // Arrange
            var model = new ConfirmarExclusaoViewModel { Matricula = "matricula_diferente" };
            var testUser = new ApplicationUser { Id = "test_user_id", UserName = "matricula_correta" }; // Usuário no sistema

            _mockUserManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(testUser); // Usuário encontrado, mas matrícula não bate

            // Act
            var result = await _controller.ExcluirConta(model) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("ConfirmarExclusao", result.ActionName);
            Assert.Equal("A matrícula informada não corresponde ao usuário.", _tempDataDictionary["ErrorMessage"]);
            _mockUserManager.Verify(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>()), Times.Once);
            _mockAlunoRepository.Verify(r => r.Remover(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ExcluirConta_ComSucesso_DeveRemoverTodosOsDadosERedirecionarParaLoginESetarTempDataSucesso()
        {
            // Arrange
            var userMatricula = "testuser@email.com"; // Assumindo que UserName é a matrícula
            var userId = "test_user_id";
            var model = new ConfirmarExclusaoViewModel { Matricula = userMatricula };
            var testUser = new ApplicationUser { Id = userId, UserName = userMatricula };

            _mockUserManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(testUser);
            // Setup para todos os métodos de remoção retornarem sucesso
            _mockAlunoRepository.Setup(r => r.Remover(userMatricula))
                .ReturnsAsync(true);
            _mockAlunoDisciplinaRepository.Setup(r => r.ExcluirDisciplinasDoAluno(userMatricula))
                .ReturnsAsync(true);
            _mockApplicationUserRepository.Setup(r => r.RemoverApplicationUser(userId))
                .ReturnsAsync(IdentityResult.Success);
            _mockApplicationUserRepository.Setup(r => r.LogoutApplicationUser())
                .Returns(Task.CompletedTask); // Logout é void, retorna Task.CompletedTask

            // Act
            var result = await _controller.ExcluirConta(model) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Login", result.ActionName);
            Assert.Equal("Account", result.ControllerName);
            // Verifica que todos os métodos de remoção e logout foram chamados
            _mockUserManager.Verify(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>()), Times.Once);
            _mockAlunoRepository.Verify(r => r.Remover(userMatricula), Times.Once);
            _mockAlunoDisciplinaRepository.Verify(r => r.ExcluirDisciplinasDoAluno(userMatricula), Times.Once);
            _mockApplicationUserRepository.Verify(r => r.RemoverApplicationUser(userId), Times.Once);
            _mockApplicationUserRepository.Verify(r => r.LogoutApplicationUser(), Times.Once);
            Assert.Equal("Conta excluída com sucesso.", _tempDataDictionary["MensagemSucesso"]);
        }

        [Theory]
        [InlineData(false, true, true)] // Falha ao remover Aluno
        [InlineData(true, false, true)] // Falha ao remover AlunoDisciplina
        [InlineData(true, true, false)] // Falha ao remover ApplicationUser
        public async Task ExcluirConta_ComQualquerFalha_DeveRedirecionarParaHomeIndexESetarTempDataErro(bool alunoRemovido, bool alunoDisciplinaRemovido, bool applicationUserRemovidoSucceeded)
        {
            // Arrange
            var userMatricula = "testuser@email.com";
            var userId = "test_user_id";
            var model = new ConfirmarExclusaoViewModel { Matricula = userMatricula };
            var testUser = new ApplicationUser { Id = userId, UserName = userMatricula };

            _mockUserManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(testUser);

            _mockAlunoRepository.Setup(r => r.Remover(userMatricula))
                .ReturnsAsync(alunoRemovido);
            _mockAlunoDisciplinaRepository.Setup(r => r.ExcluirDisciplinasDoAluno(userMatricula))
                .ReturnsAsync(alunoDisciplinaRemovido);
            _mockApplicationUserRepository.Setup(r => r.RemoverApplicationUser(userId))
                .ReturnsAsync(applicationUserRemovidoSucceeded ? IdentityResult.Success : IdentityResult.Failed(new IdentityError[] { new IdentityError { Description = "Erro de exclusão" } }));
            // Note: Não precisamos configurar LogoutApplicationUser, pois ele só é chamado no caminho de sucesso.

            // Act
            var result = await _controller.ExcluirConta(model) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
            Assert.Equal("Home", result.ControllerName);
            _mockUserManager.Verify(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>()), Times.Once);
            _mockAlunoRepository.Verify(r => r.Remover(userMatricula), Times.Once);
            _mockAlunoDisciplinaRepository.Verify(r => r.ExcluirDisciplinasDoAluno(userMatricula), Times.Once);
            _mockApplicationUserRepository.Verify(r => r.RemoverApplicationUser(userId), Times.Once);
            _mockApplicationUserRepository.Verify(r => r.LogoutApplicationUser(), Times.Never); // Logout não deve ser chamado
            Assert.Equal("Erro ao excluir conta.", _tempDataDictionary["ErrorMessage"]);
        }
    }
}