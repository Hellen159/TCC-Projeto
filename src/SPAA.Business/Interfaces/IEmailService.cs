using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPAA.Business.Interfaces
{
    public interface IEmailService
    {
        Task EnviarEmailAsync(string email, string assunto, string mensagem);
    }
}
