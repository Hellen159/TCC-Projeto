using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPAA.Business.Models
{
    public class Disciplina
    {
        public int Id { get; set; }
        public string CodigoDisciplina { get; set; }
        public string NomeDisciplina { get; set; }
        public int CargaHoraria { get; set; }

        public string Curriculo { get; set; }

        public int CodigoTipoDisciplina { get; set; }

        public int CodigoCurso { get; set; }
    }
}
