using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Rescheduler.Core.Entities;
using Rescheduler.Core.Interfaces;

namespace Rescheduler.Infra.Data
{
    public class Repository<T> : IRepository<T> where T : EntityBase
    {
        private readonly DbContext _dbContext;

        public Repository(DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAsync(T entity, CancellationToken ctx)
        {
            await _dbContext.Set<T>().AddAsync(entity, ctx);
            await _dbContext.SaveChangesAsync(ctx);
        }

        public async Task<T?> GetByIdAsync(Guid id, CancellationToken ctx)
        {
            var keyValues = new object[] { id };
            return await _dbContext.Set<T>().FindAsync(keyValues, ctx);
        }
    }
}