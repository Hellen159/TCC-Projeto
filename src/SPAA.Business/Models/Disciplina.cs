using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPAA.Business.Models
{
    public class Disciplina
    {
        public int CodigoDisciplina { get; set; }
        public string NomeDisciplina { get; set; }
        public int CargaHoraria { get; set; }

        [NotMapped]
        public ICollection<Turma> Turmas { get; set; }
    }
}
