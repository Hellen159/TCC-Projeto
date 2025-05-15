using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPAA.Business.Models
{
    public class Curriculo
    {
        public int Id { get; set; }
        public string NomeDisciplina { get; set; }
        public string AnoCurriculo { get; set; }
        public int TipoDisciplina { get; set; }
        public int CodigoCurso{ get; set; }
    }
}
