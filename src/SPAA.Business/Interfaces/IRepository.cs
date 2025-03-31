using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SPAA.Business.Interfaces
{
    public interface IRepository<TEntity> : IDisposable 
    {
        //definicao dos metodos de cada classe-entidade
        //task é uma operacao assincrona melhora o desempenho da aplicacao 
        Task Adicionar(TEntity entity);
        //Task<TEntity> ObterPorId(Guid id);
        //Task<List<TEntity>> ObterTodos();
        //Task Atualizar(TEntity entity);
        //Task Remover(Guid id);
        //Task<IEnumerable<TEntity>> Buscar(Expression<Func<TEntity, bool>> predicate);
        //Task<int> SaveChanges();
    }
}
