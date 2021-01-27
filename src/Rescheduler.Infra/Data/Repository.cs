using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Rescheduler.Core.Entities;
using Rescheduler.Core.Interfaces;
using Rescheduler.Infra.Metrics;

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
            using var _ = QueryMetrics.TimeQuery(typeof(T).Name.ToLowerInvariant(), "insert");

            await _dbContext.Set<T>().AddAsync(entity, ctx);
            await _dbContext.SaveChangesAsync(ctx);
        }

        public async Task<T?> GetByIdAsync(Guid id, CancellationToken ctx)
        {
            using var _ = QueryMetrics.TimeQuery(typeof(T).Name.ToLowerInvariant(), "get_by_id");

            var keyValues = new object[] { id };
            return await _dbContext.Set<T>().FindAsync(keyValues, ctx);
        }

        public Task UpdateAsync(T entity, CancellationToken ctx)
        {
            using var _ = QueryMetrics.TimeQuery(typeof(T).Name.ToLowerInvariant(), "update");

            _dbContext.Entry(entity).State = EntityState.Modified;
            return _dbContext.SaveChangesAsync(ctx);
        }

        public Task UpdateManyAsync(IEnumerable<T> entities, CancellationToken ctx)
        {
            using var _ = QueryMetrics.TimeQuery(typeof(T).Name.ToLowerInvariant(), "batch_update");

            entities.ToList().ForEach(e => _dbContext.Entry(e).State = EntityState.Modified);
            return _dbContext.SaveChangesAsync(ctx);
        }

        public async Task<IEnumerable<ScheduledJob>> GetAndMarkPending(int max, DateTime until, CancellationToken ctx)
        {
            using var _ = QueryMetrics.TimeQuery(nameof(ScheduledJob).ToLowerInvariant(), "mark_and_get_pending");

            var jobs = await _dbContext.Set<ScheduledJob>()
                .Where(s => s.ScheduledAt <= until && s.Status == ScheduleStatus.Scheduled && s.Job.Enabled)
                .OrderBy(s => s.ScheduledAt)
                .Take(max)
                .Include(s => s.Job)
                .ToListAsync(ctx);
            
            jobs.ForEach(j => j.InFlight());
            
            await _dbContext.SaveChangesAsync(ctx);

            return jobs;
        }
    }
}