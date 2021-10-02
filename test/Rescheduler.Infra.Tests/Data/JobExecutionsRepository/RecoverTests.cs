using System;
using System.Threading;
using System.Threading.Tasks;
using Rescheduler.Core.Entities;
using Rescheduler.Infra.Data;
using Shouldly;
using Xunit;

namespace Rescheduler.Infra.Tests.Data.JobExecutionsRepository
{
    public class RecoverTests : JobExecutionsRepositoryTests
    {
        [Fact]
        public async Task GivenInFlightExecution_WhenRecovering_ShouldRecover()
        {
            // Given 
            var job = new Job(Guid.NewGuid(), "webhooks", "test passed", true, DateTime.UtcNow, DateTime.MaxValue, null);
            var jobExecution = new JobExecution(Guid.NewGuid(), job.RunAt, null, ExecutionStatus.InFlight, job);

            using var context = GetSeededJobContext(job, jobExecution);
            var repo = new Repository<JobExecution>(_logger, context);

            // When
            var result = await repo.RecoverAsync(CancellationToken.None);            

            // Then
            result.ShouldBe(1);
        }
    }
}