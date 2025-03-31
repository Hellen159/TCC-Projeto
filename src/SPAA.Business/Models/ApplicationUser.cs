using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPAA.Business.Models
{
    public class ApplicationUser : IdentityUser
    {

        // Propriedade de navegação para Aluno
        public Aluno Aluno { get; set; }
    }
}
