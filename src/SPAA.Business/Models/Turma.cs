using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPAA.Business.Models
{
    public class Turma
    {
        public int Id { get; set; }
        public string CodigoTurma { get; set; }
        public string NomeProfessor { get; set; }
        public int Capacidade { get; set; }
        public string Semestre { get; set; }
        public string CodigoDisciplina { get; set; }
        public string Horario { get; set; }

        [NotMapped]
        public Disciplina Disciplina { get; set; }   // Navegação

    }
}
