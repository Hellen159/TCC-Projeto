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
    public abstract class Repository<TEntity, TKey> : IRepository<TEntity, TKey> where TEntity : class, new()
    {
        protected readonly MeuDbContext _context;
        protected readonly DbSet<TEntity> DbSet;

        protected Repository(MeuDbContext context)
        {
            _context = context;
            DbSet = context.Set<TEntity>();
        }

        public async Task<bool> Adicionar(TEntity entity)
        {
            DbSet.Add(entity);
            await SaveChanges();
            return true;
        }

        public async Task<TEntity> ObterPorId(TKey codigo)
        {
            return await DbSet.FindAsync(codigo);
        }

        public async Task<bool> Remover(TKey codigo)
        {
            var entity = await ObterPorId(codigo);
            if (entity == null)
                return false;

            DbSet.Remove(entity);
            await SaveChanges();
            return true;
        }

        public async Task Atualizar(TEntity entity)
        {
            if (entity == null)
                throw new InvalidOperationException("Entidade não encontrada.");

            DbSet.Update(entity);
            await SaveChanges();
        }

        public async Task<int> SaveChanges()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
        public async Task<List<TEntity>> ObterTodos()
        {
            return await DbSet.ToListAsync();
        }
    }
}
