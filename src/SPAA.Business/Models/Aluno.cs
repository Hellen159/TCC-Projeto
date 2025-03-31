using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPAA.Business.Models
{
    public class Aluno
    {
        public int Matricula { get; set; }
        public string Nome { get; set; }
        public string SemestreEntrada { get; set; }

        // Relacionamento com IdentityUser
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
    }
}
