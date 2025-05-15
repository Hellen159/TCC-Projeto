using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPAA.Business.Models
{
    public class AlunoDisciplina
    {
        public string NomeDisicplina { get; set; }
        public string Matricula { get; set; }
        public string Situacao { get; set; }
        public string Semestre { get; set; }
    }
}
