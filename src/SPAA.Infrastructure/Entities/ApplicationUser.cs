using Microsoft.AspNetCore.Identity;
using SPAA.Business.Models;

namespace Projeto.Infrastructure.Entities
{
    public class ApplicationUser : IdentityUser
    {

        // Propriedade de navegação para Aluno
        public Aluno Aluno { get; set; }
    }
}
