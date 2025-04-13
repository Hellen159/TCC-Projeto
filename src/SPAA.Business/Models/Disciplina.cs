using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPAA.Business.Models
{
    public class Disciplina
    {
        public string CodigoDisciplina { get; set; }
        public string NomeDisciplina { get; set; }
        public int CargaHoraria { get; set; }

        //relacao com tipoDisciplina
        public int CodigoTipoDisciplina { get; set; }
        public TipoDisciplina TipoDisciplina { get; set; }

        //relacao com curso 
        public int CodigoCurso { get; set; }
        public Curso Curso { get; set; }
    }
}
