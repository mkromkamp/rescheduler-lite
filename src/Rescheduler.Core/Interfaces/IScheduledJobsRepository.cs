using Rescheduler.Core.Entities;

namespace Rescheduler.Core.Interfaces;

public interface IJobExecutionRepository
{
    Task<int> RecoverAsync(CancellationToken ctx);
    Task<IEnumerable<JobExecution>> GetAndMarkPending(int max, DateTime until, CancellationToken ctx);
    Task CompactAsync(DateTime before, CancellationToken ctx);
}