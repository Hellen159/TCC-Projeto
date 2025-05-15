using Microsoft.AspNetCore.Identity;
using SPAA.Business.Interfaces;
using SPAA.Business.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPAA.Data.Repository
{
    public class ApplicationUserRepository : IApplicationUserRepository
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public ApplicationUserRepository(UserManager<ApplicationUser> userManager,
                                         SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<IdentityResult> RegistrarApplicationUser(ApplicationUser user, string password)
        {
            var result = await _userManager.CreateAsync(user, password);
            return result;
        }

        public async Task<SignInResult> LogarApplicationUser(string username, string password)
        {
            var result = await _signInManager.PasswordSignInAsync(username, password, false, false);
            return result;
        }

        public async Task LogoutApplicationUser()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task<IdentityResult> RemoverApplicationUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new Exception("Usuário não encontrado.");
            }

            var result = await _userManager.DeleteAsync(user);
            return result;
        }

        public async Task<ApplicationUser> ObterPorEmail(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }

        public async Task<string> GerarTokenResetSenha(ApplicationUser user)
        {
            return await _userManager.GeneratePasswordResetTokenAsync(user);
        }

        public async Task<IdentityResult> ResetarSenha(ApplicationUser user, string token, string novaSenha)
        {
            return await _userManager.ResetPasswordAsync(user, token, novaSenha);
        }

        public async Task<bool> VerificarSenhaAtual(ApplicationUser user, string senhaAtual)
        {
            return await _userManager.CheckPasswordAsync(user, senhaAtual);
        }

        public async Task<IdentityResult> AlterarSenha(ApplicationUser user, string senhaAtual, string novaSenha)
        {
            return await _userManager.ChangePasswordAsync(user, senhaAtual, novaSenha);
        }
    }
}
