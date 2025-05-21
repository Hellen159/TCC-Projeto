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
        //definicao dos metodos de cada classe-entidade
        //task é uma operacao assincrona melhora o desempenho da aplicacao 
        Task<bool> Adicionar(TEntity entity);
        Task<TEntity> ObterPorId(TKey codigo);
        //Task<List<TEntity>> ObterTodos();
        Task Atualizar(TEntity entity);
        Task<bool> Remover(TKey codigo);
        //Task<IEnumerable<TEntity>> Buscar(Expression<Func<TEntity, bool>> predicate);
        Task<List<TEntity>> ObterTodos();
        Task<int> SaveChanges();
    }
}
