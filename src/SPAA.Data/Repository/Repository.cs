using Microsoft.EntityFrameworkCore;
using SPAA.Business.Interfaces;
using SPAA.Data.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPAA.Data.Repository
{
    public abstract class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        protected readonly MeuDbContext _context;
        protected readonly DbSet<TEntity> DbSet;

        protected Repository(MeuDbContext context)
        {
            _context = context;
            DbSet = context.Set<TEntity>();
        }

        public async Task Adicionar(TEntity entity)
        {
            DbSet.Add(entity);
            await SaveChanges();
        }
        public async Task<int> SaveChanges()
        {
            return await _context.SaveChangesAsync();
        }
        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
