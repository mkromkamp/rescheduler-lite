using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rescheduler.Core.Entities;
using Rescheduler.Core.Interfaces;

namespace Rescheduler.Infra.Data;

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
        await _dbContext.Set<T>().AddAsync(entity, ctx);
        await _dbContext.SaveChangesAsync(ctx);
    }

    public async Task AddManyAsync(IEnumerable<T> entities, CancellationToken ctx)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync(ctx);

        try
        {
            await _dbContext.Set<T>().AddRangeAsync(entities, ctx);
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
        var keyValues = new object[] { id };
        return await _dbContext.Set<T>().FindAsync(keyValues, ctx);
    }

    public Task UpdateAsync(T entity, CancellationToken ctx)
    {
        _dbContext.Entry(entity).CurrentValues.SetValues(entity);
        return _dbContext.SaveChangesAsync(ctx);
    }

    public async Task UpdateManyAsync(IEnumerable<T> entities, CancellationToken ctx)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync(ctx);

        try
        {
            entities.ToList().ForEach(e => _dbContext.Entry(e).CurrentValues.SetValues(entities));
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(CancellationToken.None);

            _logger.LogError(ex, "Failed batch update transaction");
        }
        finally
        {
            await transaction.CommitAsync(CancellationToken.None);
        }
    }

    public async Task<IReadOnlyList<T>> GetManyAsync(Func<IQueryable<T>, IQueryable<T>> query, CancellationToken ctx)
    {
        var result = await query(_dbContext.Set<T>().AsNoTracking())
            .ToListAsync(ctx);

        return result?.AsReadOnly() ?? new List<T>().AsReadOnly();
    }

    public async Task<int> RecoverAsync(CancellationToken ctx)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(ctx);

        try
        {
            var inFlight = await _dbContext.Set<JobExecution>()
                .Where(s => s.Status == ExecutionStatus.InFlight && s.Job.Enabled)
                .ToListAsync(ctx);
                
            inFlight.ForEach(j => j.Scheduled());

            await _dbContext.SaveChangesAsync(ctx);
            await transaction.CommitAsync(ctx);

            return inFlight.Count;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(ctx);
                
            _logger.LogError(ex, "Failed to recover jobs executions");
            return 0;
        }
    }

    public async Task<IEnumerable<JobExecution>> GetAndMarkPending(int max, DateTime until, CancellationToken ctx)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(ctx);

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
            await transaction.CommitAsync(CancellationToken.None);

            return jobs;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(CancellationToken.None);

            _logger.LogError(ex, "Failed to get pending job executions");
            return Enumerable.Empty<JobExecution>();
        }
    }

    public async Task CompactAsync(DateTime before, CancellationToken ctx)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(ctx);

        try
        {
            await _dbContext.Set<JobExecution>()
                .Where(s => s.Status == ExecutionStatus.Queued && s.QueuedAt <= before)
                .ForEachAsync(j => _dbContext.Set<JobExecution>().Remove(j), ctx);

            await _dbContext.SaveChangesAsync(ctx);
            await transaction.CommitAsync(ctx);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(CancellationToken.None);

            _logger.LogError(ex, "Failed to compact job executions");
        }
    }
}