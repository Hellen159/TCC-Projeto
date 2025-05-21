using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPAA.Business.Interfaces.Services
{
    public interface IPreRequisitoService
    {
        Task<bool> AtendeRequisitos(string expressao, List<string> disciplinasAprovadas);
    }
}
