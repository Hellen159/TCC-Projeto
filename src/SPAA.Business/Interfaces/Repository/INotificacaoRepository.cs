using SPAA.Business.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPAA.Business.Interfaces.Repository
{
    public interface INotificacaoRepository : IRepository<Notificacao, int>
    {
        Task<List<Notificacao>> ObterNotificacaoPorStatus(int status);
    }
}
