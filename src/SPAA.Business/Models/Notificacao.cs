using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPAA.Business.Models
{
    public class Notificacao
    {
        public int CodigoNotificacao { get; set; }
        public int StatusNotificacao { get; set; }
        public string Mensagem {  get; set; }
    }
}
