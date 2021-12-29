using Rescheduler.Core.Entities;
using Rescheduler.Infra.Data;
using Shouldly;
using Xunit;

namespace Rescheduler.Infra.Tests.Data.JobExecutionsRepository;

public class CompactTests : JobExecutionsRepositoryTests
{
    [Fact]
    public async Task GivenOldExecution_WhenCompacting_ShouldDelete()
    {
        // Given 
        var before = DateTime.UtcNow.AddHours(-1);
        var job = new Job(Guid.NewGuid(), "webhooks", "test passed", true, DateTime.UtcNow, DateTime.MaxValue, null);
        var jobExecution = new JobExecution(Guid.NewGuid(), job.RunAt, DateTime.UtcNow.AddHours(-2), ExecutionStatus.Queued, job);

        using var context = GetSeededJobContext(job, jobExecution);
        var repo = new Repository<JobExecution>(_logger, context);

        // When
        await repo.CompactAsync(before, CancellationToken.None);
        var result = await repo.GetByIdAsync(job.Id, CancellationToken.None);

        // Then
        result.ShouldBeNull();
    }
}