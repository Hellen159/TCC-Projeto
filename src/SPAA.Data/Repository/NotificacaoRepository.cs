using Microsoft.EntityFrameworkCore;
using SPAA.Business.Interfaces.Repository;
using SPAA.Business.Models;
using SPAA.Data.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPAA.Data.Repository
{
    public class NotificacaoRepository : Repository<Notificacao, int>, INotificacaoRepository
    {
        public NotificacaoRepository(MeuDbContext context) : base(context)
        {
        }

        public async Task<List<Notificacao>> ObterNotificacaoPorStatus(int status)
        {
            return await DbSet.Where(n => n.StatusNotificacao == status).ToListAsync();
        }
    }
}
