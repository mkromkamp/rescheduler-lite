using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Rescheduler.Core.Entities;
using Rescheduler.Core.Interfaces;

namespace Rescheduler.Infra.Data
{
    internal class Repository<T> : IScheduledJobsRepository, IRepository<T> where T : EntityBase
    {
        private readonly JobContext _dbContext;

        public Repository(JobContext dbContext)
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

        public Task UpdateAsync(T entity, CancellationToken ctx)
        {
            _dbContext.Entry(entity).State = EntityState.Modified;
            return _dbContext.SaveChangesAsync(ctx);
        }

        public Task UpdateManyAsync(IEnumerable<T> entities, CancellationToken ctx)
        {
            entities.ToList().ForEach(e => _dbContext.Entry(e).State = EntityState.Modified);
            return _dbContext.SaveChangesAsync(ctx);
        }

        public async Task<IEnumerable<ScheduledJob>> MarkAndGetPending(int max, DateTime until, CancellationToken ctx)
        {
            var jobs = await _dbContext.Set<ScheduledJob>()
                .Where(s => s.ScheduledAt <= until && s.Status == ScheduleStatus.Scheduled)
                .OrderBy(s => s.ScheduledAt)
                .Take(max)
                .ToListAsync(ctx);
            
            jobs.ForEach(j => j.InFlight());
            
            await _dbContext.SaveChangesAsync(ctx);

            return jobs;
        }
    }
}