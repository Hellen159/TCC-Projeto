using Microsoft.AspNetCore.Identity;
using SPAA.Business.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPAA.Business.Interfaces
{
    public interface IApplicationUserRepository
    {
        Task<IdentityResult> RegistrarApplicationUser(ApplicationUser user, string password);
        Task<SignInResult> LogarApplicationUser(string username, string password);
        Task LogoutApplicationUser();
        Task<IdentityResult> RemoverApplicationUser(string userId);
        Task<ApplicationUser> ObterPorEmail(string email);
        Task<string> GerarTokenResetSenha(ApplicationUser user);
        Task<IdentityResult> ResetarSenha(ApplicationUser user, string token, string novaSenha);
        Task<bool> VerificarSenhaAtual(ApplicationUser user, string senhaAtual);
        Task<IdentityResult> AlterarSenha(ApplicationUser user, string senhaAtual, string novaSenha);
    }
}
