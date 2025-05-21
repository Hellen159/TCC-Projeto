using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPAA.Business.Models
{
    public class AulaHorario
    {
        public int DiaSemana { get; set; }    // 1 a 7
        public char Turno { get; set; }       // 'M', 'T', 'N'
        public int Horario { get; set; }      // 1 a 15
    }
}
