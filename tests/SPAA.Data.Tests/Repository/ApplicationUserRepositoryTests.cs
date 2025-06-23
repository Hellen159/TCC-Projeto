// SPAA.Data.Tests.Repository/ApplicationUserRepositoryTests.cs
using Moq;
using Xunit;
using Microsoft.AspNetCore.Identity;
using SPAA.Business.Models;
using SPAA.Data.Repository; // Onde ApplicationUserRepository está
using System.Threading.Tasks;
using System;
using System.Linq; // Para IdentityResult.Errors.Any()
using Moq;
using Xunit;
using Microsoft.AspNetCore.Identity;
using SPAA.Business.Models;
using SPAA.Data.Repository;
using System.Threading.Tasks;
using System;
using System.Linq;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SPAA.Data.Tests.Mocks;

namespace SPAA.Data.Tests.Repository
{
    public class ApplicationUserRepositoryTests
    {
        // Mocks para as dependências
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
        private readonly Mock<FakeSignInManager> _mockSignInManager;
        private readonly ApplicationUserRepository _repository;

        public ApplicationUserRepositoryTests()
        {
            var userStore = new Mock<IUserStore<ApplicationUser>>();
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(
                userStore.Object,
                null,
                null,
                new IUserValidator<ApplicationUser>[0],
                new IPasswordValidator<ApplicationUser>[0],
                null,
                null,
                null,
                null
            );

            // --- CORREÇÃO AQUI PARA O SIGNINMANAGER ---
            // Crie os mocks para as 6 dependências do construtor de SignInManager
            var contextAccessor = new Mock<IHttpContextAccessor>();
            var claimsFactory = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();
            var optionsSignIn = new Mock<IOptions<IdentityOptions>>();
            var loggerSignIn = new Mock<ILogger<SignInManager<ApplicationUser>>>();
            var schemes = new Mock<IAuthenticationSchemeProvider>(); // Este continua sendo necessário

            // Instancie o Mock do FakeSignInManager passando os Mocks das 6 dependências
            _mockSignInManager = new Mock<FakeSignInManager>(
                _mockUserManager.Object,
                contextAccessor.Object,
                claimsFactory.Object,
                optionsSignIn.Object,
                loggerSignIn.Object,
                schemes.Object // <-- Apenas 6 mocks passados aqui
            );
            // --- FIM DA CORREÇÃO ---

            _repository = new ApplicationUserRepository(_mockUserManager.Object, _mockSignInManager.Object);

        }

        // --- Testes para RegistrarApplicationUser ---

        [Fact]
        public async Task RegistrarApplicationUser_Sucesso_DeveRetornarIdentityResultSuccess()
        {
            // Arrange
            var user = new ApplicationUser { UserName = "testuser", Email = "test@example.com" };
            var password = "Password123!";

            // Configura o mock para que CreateAsync retorne Success
            _mockUserManager.Setup(m => m.CreateAsync(user, password))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _repository.RegistrarApplicationUser(user, password);

            // Assert
            Assert.True(result.Succeeded);
            // Verifica se CreateAsync foi chamado exatamente uma vez com os parâmetros corretos
            _mockUserManager.Verify(m => m.CreateAsync(user, password), Times.Once);
        }

        [Fact]
        public async Task RegistrarApplicationUser_Falha_DeveRetornarIdentityResultFailed()
        {
            // Arrange
            var user = new ApplicationUser { UserName = "testuser", Email = "test@example.com" };
            var password = "WeakPassword";
            var errors = new IdentityError[] { new IdentityError { Description = "Password too weak." } };

            // Configura o mock para que CreateAsync retorne um resultado falho
            _mockUserManager.Setup(m => m.CreateAsync(user, password))
                .ReturnsAsync(IdentityResult.Failed(errors));

            // Act
            var result = await _repository.RegistrarApplicationUser(user, password);

            // Assert
            Assert.False(result.Succeeded);
            Assert.NotNull(result.Errors);
            Assert.Contains(result.Errors, e => e.Description == "Password too weak.");
            _mockUserManager.Verify(m => m.CreateAsync(user, password), Times.Once);
        }

        // --- Testes para LogarApplicationUser ---

        [Fact]
        public async Task LogarApplicationUser_CredenciaisCorretas_DeveRetornarSignInResultSuccess()
        {
            // Arrange
            var username = "validuser";
            var password = "ValidPassword123!";

            // Configura o mock para que PasswordSignInAsync retorne sucesso
            _mockSignInManager.Setup(m => m.PasswordSignInAsync(username, password, false, false))
                .ReturnsAsync(SignInResult.Success);

            // Act
            var result = await _repository.LogarApplicationUser(username, password);

            // Assert
            Assert.True(result.Succeeded);
            _mockSignInManager.Verify(m => m.PasswordSignInAsync(username, password, false, false), Times.Once);
        }

        [Fact]
        public async Task LogarApplicationUser_CredenciaisInvalidas_DeveRetornarSignInResultFailed()
        {
            // Arrange
            var username = "invaliduser";
            var password = "WrongPassword";

            // Configura o mock para que PasswordSignInAsync retorne falha
            _mockSignInManager.Setup(m => m.PasswordSignInAsync(username, password, false, false))
                .ReturnsAsync(SignInResult.Failed);

            // Act
            var result = await _repository.LogarApplicationUser(username, password);

            // Assert
            Assert.False(result.Succeeded);
            _mockSignInManager.Verify(m => m.PasswordSignInAsync(username, password, false, false), Times.Once);
        }

        // --- Testes para LogoutApplicationUser ---

        [Fact]
        public async Task LogoutApplicationUser_DeveChamarSignOutAsync()
        {
            // Arrange
            // Não precisa de setup específico, apenas verificar a chamada

            // Act
            await _repository.LogoutApplicationUser();

            // Assert
            // Verifica se SignOutAsync foi chamado exatamente uma vez
            _mockSignInManager.Verify(m => m.SignOutAsync(), Times.Once);
        }

        // --- Testes para RemoverApplicationUser ---

        [Fact]
        public async Task RemoverApplicationUser_UsuarioExistente_DeveRemoverERetornarIdentityResultSuccess()
        {
            // Arrange
            var userId = "user123";
            var user = new ApplicationUser { Id = userId, UserName = "user_to_delete" };

            // Configura o mock para FindByIdAsync
            _mockUserManager.Setup(m => m.FindByIdAsync(userId))
                .ReturnsAsync(user);
            // Configura o mock para DeleteAsync
            _mockUserManager.Setup(m => m.DeleteAsync(user))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _repository.RemoverApplicationUser(userId);

            // Assert
            Assert.True(result.Succeeded);
            _mockUserManager.Verify(m => m.FindByIdAsync(userId), Times.Once);
            _mockUserManager.Verify(m => m.DeleteAsync(user), Times.Once);
        }

        [Fact]
        public async Task RemoverApplicationUser_UsuarioInexistente_DeveLancarExcecao()
        {
            // Arrange
            var userId = "nonexistentuser";

            // Configura o mock para FindByIdAsync retornar null (usuário não encontrado)
            _mockUserManager.Setup(m => m.FindByIdAsync(userId))
                .ReturnsAsync((ApplicationUser)null);

            // Act & Assert
            // Espera que o método lance uma exceção
            var exception = await Assert.ThrowsAsync<Exception>(() => _repository.RemoverApplicationUser(userId));
            Assert.Equal("Usuário não encontrado.", exception.Message);
            _mockUserManager.Verify(m => m.FindByIdAsync(userId), Times.Once);
            // Garante que DeleteAsync NUNCA é chamado se o usuário não for encontrado
            _mockUserManager.Verify(m => m.DeleteAsync(It.IsAny<ApplicationUser>()), Times.Never);
        }

        [Fact]
        public async Task RemoverApplicationUser_FalhaAoRemover_DeveRetornarIdentityResultFailed()
        {
            // Arrange
            var userId = "user123";
            var user = new ApplicationUser { Id = userId, UserName = "user_to_delete" };
            var errors = new IdentityError[] { new IdentityError { Description = "Failed to delete user." } };

            _mockUserManager.Setup(m => m.FindByIdAsync(userId))
                .ReturnsAsync(user);
            _mockUserManager.Setup(m => m.DeleteAsync(user))
                .ReturnsAsync(IdentityResult.Failed(errors));

            // Act
            var result = await _repository.RemoverApplicationUser(userId);

            // Assert
            Assert.False(result.Succeeded);
            Assert.NotNull(result.Errors);
            Assert.Contains(result.Errors, e => e.Description == "Failed to delete user.");
            _mockUserManager.Verify(m => m.FindByIdAsync(userId), Times.Once);
            _mockUserManager.Verify(m => m.DeleteAsync(user), Times.Once);
        }

        // --- Testes para ObterPorEmail ---

        [Fact]
        public async Task ObterPorEmail_EmailExistente_DeveRetornarUsuario()
        {
            // Arrange
            var email = "existing@example.com";
            var user = new ApplicationUser { UserName = "existinguser", Email = email };

            _mockUserManager.Setup(m => m.FindByEmailAsync(email))
                .ReturnsAsync(user);

            // Act
            var result = await _repository.ObterPorEmail(email);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(email, result.Email);
            _mockUserManager.Verify(m => m.FindByEmailAsync(email), Times.Once);
        }

        [Fact]
        public async Task ObterPorEmail_EmailInexistente_DeveRetornarNull()
        {
            // Arrange
            var email = "nonexistent@example.com";

            _mockUserManager.Setup(m => m.FindByEmailAsync(email))
                .ReturnsAsync((ApplicationUser)null);

            // Act
            var result = await _repository.ObterPorEmail(email);

            // Assert
            Assert.Null(result);
            _mockUserManager.Verify(m => m.FindByEmailAsync(email), Times.Once);
        }

        // --- Testes para GerarTokenResetSenha ---

        [Fact]
        public async Task GerarTokenResetSenha_DeveRetornarToken()
        {
            // Arrange
            var user = new ApplicationUser { UserName = "user_for_token" };
            var expectedToken = "some_generated_token";

            _mockUserManager.Setup(m => m.GeneratePasswordResetTokenAsync(user))
                .ReturnsAsync(expectedToken);

            // Act
            var token = await _repository.GerarTokenResetSenha(user);

            // Assert
            Assert.Equal(expectedToken, token);
            _mockUserManager.Verify(m => m.GeneratePasswordResetTokenAsync(user), Times.Once);
        }

        // --- Testes para ResetarSenha ---

        [Fact]
        public async Task ResetarSenha_Sucesso_DeveRetornarIdentityResultSuccess()
        {
            // Arrange
            var user = new ApplicationUser { UserName = "user_reset_pass" };
            var token = "valid_token";
            var newPassword = "NewStrongPassword123!";

            _mockUserManager.Setup(m => m.ResetPasswordAsync(user, token, newPassword))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _repository.ResetarSenha(user, token, newPassword);

            // Assert
            Assert.True(result.Succeeded);
            _mockUserManager.Verify(m => m.ResetPasswordAsync(user, token, newPassword), Times.Once);
        }

        [Fact]
        public async Task ResetarSenha_Falha_DeveRetornarIdentityResultFailed()
        {
            // Arrange
            var user = new ApplicationUser { UserName = "user_reset_pass" };
            var token = "invalid_token";
            var newPassword = "WeakPassword";
            var errors = new IdentityError[] { new IdentityError { Description = "Invalid token." } };

            _mockUserManager.Setup(m => m.ResetPasswordAsync(user, token, newPassword))
                .ReturnsAsync(IdentityResult.Failed(errors));

            // Act
            var result = await _repository.ResetarSenha(user, token, newPassword);

            // Assert
            Assert.False(result.Succeeded);
            Assert.NotNull(result.Errors);
            Assert.Contains(result.Errors, e => e.Description == "Invalid token.");
            _mockUserManager.Verify(m => m.ResetPasswordAsync(user, token, newPassword), Times.Once);
        }

        // --- Testes para VerificarSenhaAtual ---

        [Fact]
        public async Task VerificarSenhaAtual_SenhaCorreta_DeveRetornarTrue()
        {
            // Arrange
            var user = new ApplicationUser { UserName = "user_check_pass" };
            var currentPassword = "CurrentPassword123!";

            _mockUserManager.Setup(m => m.CheckPasswordAsync(user, currentPassword))
                .ReturnsAsync(true);

            // Act
            var result = await _repository.VerificarSenhaAtual(user, currentPassword);

            // Assert
            Assert.True(result);
            _mockUserManager.Verify(m => m.CheckPasswordAsync(user, currentPassword), Times.Once);
        }

        [Fact]
        public async Task VerificarSenhaAtual_SenhaIncorreta_DeveRetornarFalse()
        {
            // Arrange
            var user = new ApplicationUser { UserName = "user_check_pass" };
            var currentPassword = "WrongPassword";

            _mockUserManager.Setup(m => m.CheckPasswordAsync(user, currentPassword))
                .ReturnsAsync(false);

            // Act
            var result = await _repository.VerificarSenhaAtual(user, currentPassword);

            // Assert
            Assert.False(result);
            _mockUserManager.Verify(m => m.CheckPasswordAsync(user, currentPassword), Times.Once);
        }

        // --- Testes para AlterarSenha ---

        [Fact]
        public async Task AlterarSenha_Sucesso_DeveRetornarIdentityResultSuccess()
        {
            // Arrange
            var user = new ApplicationUser { UserName = "user_change_pass" };
            var currentPassword = "OldPassword123!";
            var newPassword = "NewPassword123!";

            _mockUserManager.Setup(m => m.ChangePasswordAsync(user, currentPassword, newPassword))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _repository.AlterarSenha(user, currentPassword, newPassword);

            // Assert
            Assert.True(result.Succeeded);
            _mockUserManager.Verify(m => m.ChangePasswordAsync(user, currentPassword, newPassword), Times.Once);
        }

        [Fact]
        public async Task AlterarSenha_Falha_DeveRetornarIdentityResultFailed()
        {
            // Arrange
            var user = new ApplicationUser { UserName = "user_change_pass" };
            var currentPassword = "OldPassword123!";
            var newPassword = "NewPassword"; // Assuming this is weak
            var errors = new IdentityError[] { new IdentityError { Description = "New password is too weak." } };

            _mockUserManager.Setup(m => m.ChangePasswordAsync(user, currentPassword, newPassword))
                .ReturnsAsync(IdentityResult.Failed(errors));

            // Act
            var result = await _repository.AlterarSenha(user, currentPassword, newPassword);

            // Assert
            Assert.False(result.Succeeded);
            Assert.NotNull(result.Errors);
            Assert.Contains(result.Errors, e => e.Description == "New password is too weak.");
            _mockUserManager.Verify(m => m.ChangePasswordAsync(user, currentPassword, newPassword), Times.Once);
        }
    }
}