using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SPAA.App.ViewModels; // Supondo que seus ViewModels estão neste namespace
using SPAA.Business.Interfaces.Repository;
using SPAA.Business.Interfaces.Services;
using SPAA.Business.Models;
using Xunit;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http; // Para configurar TempData
using Microsoft.AspNetCore.Mvc.ViewFeatures; // Para configurar TempData
using Microsoft.AspNetCore.Mvc.Routing;
using Projeto.App.ViewModels;

namespace SPAA.App.Tests.Controllers
{
    public class AccountControllerTests
    {
        private readonly Mock<IAlunoRepository> _mockAlunoRepository;
        private readonly Mock<IApplicationUserRepository> _mockApplicationUserRepository;
        private readonly Mock<IEmailService> _mockEmailService;
        private readonly AccountController _controller;
        private readonly Mock<IUrlHelper> _mockUrlHelper;
        public AccountControllerTests()
        {
            _mockAlunoRepository = new Mock<IAlunoRepository>();
            _mockApplicationUserRepository = new Mock<IApplicationUserRepository>();
            _mockEmailService = new Mock<IEmailService>();
            _mockUrlHelper = new Mock<IUrlHelper>();

            _controller = new AccountController(
                _mockAlunoRepository.Object,
                _mockApplicationUserRepository.Object,
                _mockEmailService.Object
            );

            // Configuração de TempData para a controller
            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
            _controller.TempData = tempData;

            _mockUrlHelper
                        .Setup(x => x.Action(It.IsAny<UrlActionContext>()))
                        .Returns<UrlActionContext>(uac =>
                        {
                            return $"http://localhost/{uac.Controller}/{uac.Action}";
                        });

            _controller.Url = _mockUrlHelper.Object; // Atribuir o mock à propriedade Url da controller
        }

        // Geração de dados de exemplo para os ViewModels
        private RegisterViewModel GetRegisterViewModelExemplo()
        {
            return new RegisterViewModel
            {
                Matricula = "12345",
                Nome = "Teste User",
                Email = "teste@example.com",
                Senha = "Password123!",
                ConfirmacaoSenha = "Password123!",
                AnoEntrada = 2023,
                SemestreEntrada = 1
            };
        }

        private LoginViewModel GetLoginViewModelExemplo()
        {
            return new LoginViewModel
            {
                Matricula = "12345",
                Senha = "Password123!"
            };
        }

        private ForgotPasswordViewModel GetForgotPasswordViewModelExemplo()
        {
            return new ForgotPasswordViewModel
            {
                Email = "teste@example.com"
            };
        }

        private ResetPasswordViewModel GetResetPasswordViewModelExemplo()
        {
            return new ResetPasswordViewModel
            {
                Email = "teste@example.com",
                Token = "some_token",
                Senha = "NewPassword123!",
                ConfirmacaoSenha = "NewPassword123!"
            };
        }

        #region Register GET
        [Fact]
        public void Register_GET_RetornaView()
        {
            // Act
            var result = _controller.Register();

            // Assert
            Assert.IsType<ViewResult>(result);
        }
        #endregion

        #region Register POST
        [Fact]
        public async Task Register_POST_ComModelStateInvalido_RetornaViewComMesmoViewModel()
        {
            // Arrange
            var registerViewModel = GetRegisterViewModelExemplo();
            _controller.ModelState.AddModelError("Email", "Email inválido"); // Simula um erro de validação

            // Act
            var result = await _controller.Register(registerViewModel);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<RegisterViewModel>(viewResult.Model);
            Assert.Equal(registerViewModel, model);
            Assert.False(_controller.ModelState.IsValid);
        }

        [Fact]
        public async Task Register_POST_ComEmailJaRegistrado_RetornaViewComErrorMessage()
        {
            // Arrange
            var registerViewModel = GetRegisterViewModelExemplo();
            _mockApplicationUserRepository.Setup(r => r.ObterPorEmail(registerViewModel.Email))
                .ReturnsAsync(new ApplicationUser { Email = registerViewModel.Email }); // Simula usuário existente

            // Act
            var result = await _controller.Register(registerViewModel);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<RegisterViewModel>(viewResult.Model);
            Assert.Equal("Esse e-mail já está registrado.", _controller.TempData["ErrorMessage"]);
            Assert.Equal(registerViewModel, model);
        }

        [Fact]
        public async Task Register_POST_ComMatriculaJaRegistrada_RetornaViewComErrorMessage()
        {
            // Arrange
            var registerViewModel = GetRegisterViewModelExemplo();
            _mockApplicationUserRepository.Setup(r => r.ObterPorEmail(registerViewModel.Email))
                .ReturnsAsync((ApplicationUser)null); // E-mail não existe
            _mockAlunoRepository.Setup(r => r.ObterPorId(registerViewModel.Matricula))
                .ReturnsAsync(new Aluno { Matricula = registerViewModel.Matricula }); // Simula aluno existente

            // Act
            var result = await _controller.Register(registerViewModel);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<RegisterViewModel>(viewResult.Model);
            Assert.Equal("Essa matrícula já pertence a outro usuário.", _controller.TempData["ErrorMessage"]);
            Assert.Equal(registerViewModel, model);
        }

        [Fact]
        public async Task Register_POST_FalhaAoRegistrarApplicationUser_RetornaViewComErrosDeIdentity()
        {
            // Arrange
            var registerViewModel = GetRegisterViewModelExemplo();
            _mockApplicationUserRepository.Setup(r => r.ObterPorEmail(It.IsAny<string>()))
                .ReturnsAsync((ApplicationUser)null);
            _mockAlunoRepository.Setup(r => r.ObterPorId(It.IsAny<string>()))
                .ReturnsAsync((Aluno)null);

            // Simula falha no registro com erros do Identity
            var identityErrors = new List<IdentityError>
        {
            new IdentityError { Code = "Error1", Description = "Erro de teste 1" },
            new IdentityError { Code = "Error2", Description = "Erro de teste 2" }
        };
            _mockApplicationUserRepository.Setup(r => r.RegistrarApplicationUser(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed(identityErrors.ToArray()));

            // Act
            var result = await _controller.Register(registerViewModel);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<RegisterViewModel>(viewResult.Model);
            Assert.False(_controller.ModelState.IsValid);
            Assert.Contains("Erro de teste 1", _controller.ModelState[string.Empty].Errors[0].ErrorMessage);
            Assert.Contains("Erro de teste 2", _controller.ModelState[string.Empty].Errors[1].ErrorMessage);
            Assert.Equal(registerViewModel, model);
        }

        [Fact]
        public async Task Register_POST_SucessoNoCadastro_RedirecionaParaLoginComMensagemSucesso()
        {
            // Arrange
            var registerViewModel = GetRegisterViewModelExemplo();
            _mockApplicationUserRepository.Setup(r => r.ObterPorEmail(It.IsAny<string>()))
                .ReturnsAsync((ApplicationUser)null);
            _mockAlunoRepository.Setup(r => r.ObterPorId(It.IsAny<string>()))
                .ReturnsAsync((Aluno)null);

            // Simula sucesso no registro do ApplicationUser
            var createdUser = new ApplicationUser { Id = "some-guid", UserName = registerViewModel.Matricula.ToString(), Email = registerViewModel.Email };
            _mockApplicationUserRepository.Setup(r => r.RegistrarApplicationUser(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success)
                .Callback<ApplicationUser, string>((user, pass) => user.Id = createdUser.Id); // Atribui um Id ao user mockado

            _mockAlunoRepository.Setup(r => r.Adicionar(It.IsAny<Aluno>()))
                .Returns(Task.FromResult(true)); // Aluno adicionado com sucesso

            // Act
            var result = await _controller.Register(registerViewModel);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
            Assert.Equal("Account", redirectResult.ControllerName);
            Assert.Equal("Cadastro realizado com sucesso!", _controller.TempData["MensagemSucesso"]);

            // Verifica se os métodos foram chamados
            _mockApplicationUserRepository.Verify(r => r.ObterPorEmail(registerViewModel.Email), Times.Once);
            _mockAlunoRepository.Verify(r => r.ObterPorId(registerViewModel.Matricula), Times.Once);
            _mockApplicationUserRepository.Verify(r => r.RegistrarApplicationUser(
                It.Is<ApplicationUser>(u => u.Email == registerViewModel.Email && u.UserName == registerViewModel.Matricula.ToString()),
                registerViewModel.Senha), Times.Once);
            _mockAlunoRepository.Verify(r => r.Adicionar(
                It.Is<Aluno>(a => a.Matricula == registerViewModel.Matricula && a.NomeAluno == registerViewModel.Nome && a.CodigoUser == createdUser.Id)), Times.Once);
        }

        [Fact]
        public async Task Register_POST_ErroAoSalvarAluno_RemoveApplicationUserERetornaViewComErro()
        {
            // Arrange
            var registerViewModel = GetRegisterViewModelExemplo();
            _mockApplicationUserRepository.Setup(r => r.ObterPorEmail(It.IsAny<string>()))
                .ReturnsAsync((ApplicationUser)null);
            _mockAlunoRepository.Setup(r => r.ObterPorId(It.IsAny<string>()))
                .ReturnsAsync((Aluno)null);

            var createdUser = new ApplicationUser { Id = "test-user-id", UserName = registerViewModel.Matricula.ToString(), Email = registerViewModel.Email };
            _mockApplicationUserRepository.Setup(r => r.RegistrarApplicationUser(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success)
                .Callback<ApplicationUser, string>((user, pass) => user.Id = createdUser.Id);

            // Simula erro ao adicionar aluno
            _mockAlunoRepository.Setup(r => r.Adicionar(It.IsAny<Aluno>()))
                .ThrowsAsync(new System.Exception("Erro de banco de dados ao adicionar aluno."));

            // CORREÇÃO: RemoverApplicationUser retorna IdentityResult
            _mockApplicationUserRepository.Setup(r => r.RemoverApplicationUser(It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success); // ou .ReturnsAsync(IdentityResult.Failed()) se quiser testar falha na remoção

            // Act
            var result = await _controller.Register(registerViewModel);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<RegisterViewModel>(viewResult.Model);
            Assert.False(_controller.ModelState.IsValid);
            Assert.Contains("Erro ao salvar o aluno no banco de dados: Erro de banco de dados ao adicionar aluno.", _controller.ModelState[string.Empty].Errors[0].ErrorMessage);
            Assert.Equal(registerViewModel, model);

            // Verifica se o ApplicationUser foi removido
            _mockApplicationUserRepository.Verify(r => r.RemoverApplicationUser(createdUser.Id), Times.Once);
        }

        [Fact]
        public async Task Register_POST_ExcecaoGeral_RetornaViewComErrorMessage()
        {
            // Arrange
            var registerViewModel = GetRegisterViewModelExemplo();
            // Simula uma exceção geral (por exemplo, na primeira verificação de e-mail)
            _mockApplicationUserRepository.Setup(r => r.ObterPorEmail(It.IsAny<string>()))
                .ThrowsAsync(new System.Exception("Erro inesperado durante a verificação de e-mail."));

            // Act
            var result = await _controller.Register(registerViewModel);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<RegisterViewModel>(viewResult.Model);
            Assert.Equal("Erro durante o cadastro: Erro inesperado durante a verificação de e-mail.", _controller.TempData["ErrorMessage"]);
            Assert.Equal(registerViewModel, model);
        }

        #endregion

        #region Login GET
        [Fact]
        public void Login_GET_RetornaView()
        {
            // Act
            var result = _controller.Login();

            // Assert
            Assert.IsType<ViewResult>(result);
        }
        #endregion

        #region Login POST
        [Fact]
        public async Task Login_POST_ComModelStateInvalido_RetornaViewComMesmoViewModel()
        {
            // Arrange
            var loginViewModel = GetLoginViewModelExemplo();
            _controller.ModelState.AddModelError("Matricula", "Matrícula inválida");

            // Act
            var result = await _controller.Login(loginViewModel);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<LoginViewModel>(viewResult.Model);
            Assert.Equal(loginViewModel, model);
            Assert.False(_controller.ModelState.IsValid);
        }

        [Fact]
        public async Task Login_POST_CredenciaisCorretas_RedirecionaParaHomeIndex()
        {
            // Arrange
            var loginViewModel = GetLoginViewModelExemplo();
            _mockApplicationUserRepository.Setup(r => r.LogarApplicationUser(loginViewModel.Matricula, loginViewModel.Senha))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success); // Simula login bem-sucedido

            // Act
            var result = await _controller.Login(loginViewModel);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Home", redirectResult.ControllerName);
        }

        [Fact]
        public async Task Login_POST_CredenciaisIncorretas_RetornaViewComErrorMessage()
        {
            // Arrange
            var loginViewModel = GetLoginViewModelExemplo();
            _mockApplicationUserRepository.Setup(r => r.LogarApplicationUser(loginViewModel.Matricula, loginViewModel.Senha))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed); // Simula login falho

            // Act
            var result = await _controller.Login(loginViewModel);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<LoginViewModel>(viewResult.Model); // Esperamos o mesmo model de volta na view
            Assert.Equal("Usuário ou senha incorretos.", _controller.TempData["ErrorMessage"]);
            // Você pode adicionar mais asserts aqui se a view model for limpa ou alterada
        }
        #endregion

        #region Logout POST
        [Fact]
        public async Task Logout_POST_RedirecionaParaLogin()
        {
            // Arrange
            _mockApplicationUserRepository.Setup(r => r.LogoutApplicationUser())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Logout();

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
            Assert.Equal("Account", redirectResult.ControllerName);
            _mockApplicationUserRepository.Verify(r => r.LogoutApplicationUser(), Times.Once);
        }
        #endregion

        #region ForgotPassword GET
        [Fact]
        public void ForgotPassword_GET_RetornaView()
        {
            // Act
            var result = _controller.ForgotPassword();

            // Assert
            Assert.IsType<ViewResult>(result);
        }
        #endregion

        #region ForgotPassword POST
        [Fact]
        public async Task ForgotPassword_POST_ComModelStateInvalido_RetornaViewComMesmoViewModel()
        {
            // Arrange
            var model = GetForgotPasswordViewModelExemplo();
            _controller.ModelState.AddModelError("Email", "Email inválido");

            // Act
            var result = await _controller.ForgotPassword(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var returnedModel = Assert.IsType<ForgotPasswordViewModel>(viewResult.Model);
            Assert.Equal(model, returnedModel);
            Assert.False(_controller.ModelState.IsValid);
        }

        [Fact]
        public async Task ForgotPassword_POST_EmailNaoCadastrado_RetornaLoginComMensagemSucessoGenerica()
        {
            // Arrange
            var model = GetForgotPasswordViewModelExemplo();
            _mockApplicationUserRepository.Setup(r => r.ObterPorEmail(model.Email))
                .ReturnsAsync((ApplicationUser)null); // Simula usuário não encontrado

            // Act
            var result = await _controller.ForgotPassword(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
            Assert.Equal("Se o e-mail estiver cadastrado, você receberá um link de redefinição de senha.", _controller.TempData["MensagemSucesso"]);
            _mockApplicationUserRepository.Verify(r => r.ObterPorEmail(model.Email), Times.Once);
            _mockApplicationUserRepository.Verify(r => r.GerarTokenResetSenha(It.IsAny<ApplicationUser>()), Times.Never);
            _mockEmailService.Verify(e => e.EnviarEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

       
        #endregion

        #region ResetPassword GET
        [Fact]
        public void ResetPassword_GET_RetornaViewComViewModelCorreto()
        {
            // Arrange
            var token = "test_token";
            var email = "test@example.com";

            // Act
            var result = _controller.ResetPassword(token, email);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ResetPasswordViewModel>(viewResult.Model);
            Assert.Equal(token, model.Token);
            Assert.Equal(email, model.Email);
        }
        #endregion

        #region ResetPassword POST
        [Fact]
        public async Task ResetPassword_POST_ComModelStateInvalido_RetornaViewComMesmoViewModel()
        {
            // Arrange
            var model = GetResetPasswordViewModelExemplo();
            _controller.ModelState.AddModelError("Senha", "Senha inválida");

            // Act
            var result = await _controller.ResetPassword(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var returnedModel = Assert.IsType<ResetPasswordViewModel>(viewResult.Model);
            Assert.Equal(model, returnedModel);
            Assert.False(_controller.ModelState.IsValid);
        }

        [Fact]
        public async Task ResetPassword_POST_UsuarioNaoEncontrado_RetornaLoginComErrorMessage()
        {
            // Arrange
            var model = GetResetPasswordViewModelExemplo();
            _mockApplicationUserRepository.Setup(r => r.ObterPorEmail(model.Email))
                .ReturnsAsync((ApplicationUser)null); // Simula usuário não encontrado

            // Act
            var result = await _controller.ResetPassword(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
            Assert.Equal("Usuário não encontrado.", _controller.TempData["ErrorMessageF"]);
            _mockApplicationUserRepository.Verify(r => r.ObterPorEmail(model.Email), Times.Once);
            _mockApplicationUserRepository.Verify(r => r.ResetarSenha(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ResetPassword_POST_RedefinicaoDeSenhaComSucesso_RetornaLoginComMensagemSucesso()
        {
            // Arrange
            var model = GetResetPasswordViewModelExemplo();
            var user = new ApplicationUser { Email = model.Email };
            _mockApplicationUserRepository.Setup(r => r.ObterPorEmail(model.Email))
                .ReturnsAsync(user);
            _mockApplicationUserRepository.Setup(r => r.ResetarSenha(user, model.Token, model.Senha))
                .ReturnsAsync(IdentityResult.Success); // Simula redefinição bem-sucedida

            // Act
            var result = await _controller.ResetPassword(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
            Assert.Equal("Senha redefinida com sucesso!", _controller.TempData["MensagemSucesso"]);
            _mockApplicationUserRepository.Verify(r => r.ObterPorEmail(model.Email), Times.Once);
            _mockApplicationUserRepository.Verify(r => r.ResetarSenha(user, model.Token, model.Senha), Times.Once);
        }

        [Fact]
        public async Task ResetPassword_POST_FalhaNaRedefinicaoDeSenha_RetornaViewComErrosDeIdentity()
        {
            // Arrange
            var model = GetResetPasswordViewModelExemplo();
            var user = new ApplicationUser { Email = model.Email };
            _mockApplicationUserRepository.Setup(r => r.ObterPorEmail(model.Email))
                .ReturnsAsync(user);

            var identityErrors = new List<IdentityError>
        {
            new IdentityError { Code = "PasswordTooShort", Description = "A senha é muito curta." }
        };
            _mockApplicationUserRepository.Setup(r => r.ResetarSenha(user, model.Token, model.Senha))
                .ReturnsAsync(IdentityResult.Failed(identityErrors.ToArray()));

            // Act
            var result = await _controller.ResetPassword(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var returnedModel = Assert.IsType<ResetPasswordViewModel>(viewResult.Model);
            Assert.Equal(model, returnedModel);
            Assert.False(_controller.ModelState.IsValid);
            Assert.Contains("A senha é muito curta.", _controller.ModelState[string.Empty].Errors[0].ErrorMessage);
            _mockApplicationUserRepository.Verify(r => r.ObterPorEmail(model.Email), Times.Once);
            _mockApplicationUserRepository.Verify(r => r.ResetarSenha(user, model.Token, model.Senha), Times.Once);
        }
        #endregion
    }
}