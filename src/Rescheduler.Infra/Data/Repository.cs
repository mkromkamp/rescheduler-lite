using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Rescheduler.Core.Entities;
using Rescheduler.Core.Interfaces;
using Rescheduler.Infra.Metrics;

namespace Rescheduler.Infra.Data
{
    internal class Repository<T> : IJobExecutionRepository, IRepository<T> where T : EntityBase
    {
        private readonly ILogger _logger;
        private readonly JobContext _dbContext;

        public Repository(ILogger<Repository<T>> logger, JobContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        public async Task AddAsync(T entity, CancellationToken ctx)
        {
            using var _ = QueryMetrics.TimeQuery(typeof(T).Name.ToLowerInvariant(), "insert");

            await _dbContext.Set<T>().AddAsync(entity, ctx);
            await _dbContext.SaveChangesAsync(ctx);
        }

        public async Task AddManyAsync(IEnumerable<T> entities, CancellationToken ctx)
        {
            using var _ = QueryMetrics.TimeQuery(typeof(T).Name.ToLowerInvariant(), "insert_many");
            using var transaction = await _dbContext.Database.BeginTransactionAsync(ctx);

            try
            {
                await _dbContext.Set<T>().AddRangeAsync(entities, ctx);
                await _dbContext.SaveChangesAsync(ctx);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(CancellationToken.None);

                _logger.LogError(ex, "Failed batch insert transaction");
            }
            finally
            {
                await transaction.CommitAsync(CancellationToken.None);
            }
            
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

            _dbContext.Entry(entity).CurrentValues.SetValues(entity);
            return _dbContext.SaveChangesAsync(ctx);
        }

        public Task UpdateManyAsync(IEnumerable<T> entities, CancellationToken ctx)
        {
            using var _ = QueryMetrics.TimeQuery(typeof(T).Name.ToLowerInvariant(), "batch_update");

            entities.ToList().ForEach(e => _dbContext.Entry(e).CurrentValues.SetValues(entities));
            return _dbContext.SaveChangesAsync(ctx);
        }

        public async Task<IReadOnlyList<T>> GetManyAsync(Func<IQueryable<T>, IQueryable<T>> query, CancellationToken ctx)
        {
            var result = await query(_dbContext.Set<T>().AsNoTracking())
                .ToListAsync(ctx);

            return result?.AsReadOnly() ?? new List<T>().AsReadOnly();
        }

        public async Task<IEnumerable<JobExecution>> GetAndMarkPending(int max, DateTime until, CancellationToken ctx)
        {
            using var _ = QueryMetrics.TimeQuery(nameof(JobExecution).ToLowerInvariant(), "mark_and_get_pending");
            using var transaction = await _dbContext.Database.BeginTransactionAsync(ctx);

            try
            {
                var jobs = await _dbContext.Set<JobExecution>()
                    .Where(s => s.ScheduledAt <= until && s.Status == ExecutionStatus.Scheduled && s.Job.Enabled)
                    .OrderBy(s => s.ScheduledAt)
                    .Take(max)
                    .Include(s => s.Job)
                    .ToListAsync(ctx);
                
                jobs.ForEach(j => j.InFlight());
                
                await _dbContext.SaveChangesAsync(ctx);

                return jobs;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(CancellationToken.None);

                _logger.LogError(ex, "Failed to get pending job executions");
                return Enumerable.Empty<JobExecution>();
            }
            finally
            {
                await transaction.CommitAsync(CancellationToken.None);
            }
        }
    }
}