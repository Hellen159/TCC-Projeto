using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SPAA.Business.Interfaces.Repository
{
    public interface IRepository<TEntity, TKey> : IDisposable
    {
        Task<bool> Adicionar(TEntity entity);
        Task<TEntity> ObterPorId(TKey codigo);
        Task Atualizar(TEntity entity);
        Task<bool> Remover(TKey codigo);
        Task<List<TEntity>> ObterTodos();
        Task<int> SaveChanges();
    }
}
