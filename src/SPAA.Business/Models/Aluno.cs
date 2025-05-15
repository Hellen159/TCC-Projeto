using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPAA.Business.Models
{
    public class Aluno
    {
        public string Matricula { get; set; }
        public string NomeAluno { get; set; }
        public string SemestreEntrada { get; set; }
        public bool HistoricoAnexado { get; set; }
        public string? CurriculoAluno { get; set; }

        // Relacionamento com IdentityUser
        public string CodigoUser { get; set; }
        public ApplicationUser User { get; set; }
    }
}
