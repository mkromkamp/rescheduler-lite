using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Rescheduler.Core.Entities;
using Rescheduler.Infra.Data;
using Shouldly;
using Xunit;

namespace Rescheduler.Infra.Tests.Data
{
    public class RepositoryTests
    {
        [Fact]
        public async Task GivenToBeScheduledJob_WhenMarkingPending_ShouldMarkAndReturn()
        {
            // Given 
            var job = new Job(Guid.NewGuid(), "webhooks", "test passed", true, DateTime.UtcNow, DateTime.MaxValue, null);
            var jobExecution = new JobExecution(Guid.NewGuid(), job.RunAt, null, ExecutionStatus.Scheduled, job);

            using var context = GetSeededJobContext(job, jobExecution);
            var repo = new Repository<JobExecution>(context);

            // When
            var result = await repo.GetAndMarkPending(1, DateTime.UtcNow.AddSeconds(5), CancellationToken.None);            

            // Then
            result.ShouldNotBeEmpty();
            result.First().Status.ShouldBe(ExecutionStatus.InFlight);
        }

        [Fact]
        public async Task GivenDisabledToBeScheduledJob_WhenMarkingPending_ShouldMarkAndReturn()
        {
            // Given 
            var enabled = false;
            var job = new Job(Guid.NewGuid(), "webhooks", "test passed", enabled, DateTime.UtcNow, DateTime.MaxValue, null);
            var jobExecution = new JobExecution(Guid.NewGuid(), job.RunAt, null, ExecutionStatus.Scheduled, job);

            using var context = GetSeededJobContext(job, jobExecution);
            var repo = new Repository<JobExecution>(context);

            // When
            var result = await repo.GetAndMarkPending(1, DateTime.UtcNow.AddSeconds(5), CancellationToken.None);            

            // Then
            result.ShouldBeEmpty();
        }

        [Fact]
        public async Task GivenNoToBeScheduledJob_WhenMarkingPending_ShouldMarkAndReturn()
        {
            // Given 
            var job = new Job(Guid.NewGuid(), "webhooks", "test passed", true, DateTime.UtcNow.AddDays(1), DateTime.MaxValue, null);
            var jobExecution = new JobExecution(Guid.NewGuid(), job.RunAt, null, ExecutionStatus.Scheduled, job);

            using var context = GetSeededJobContext(job, jobExecution);
            var repo = new Repository<JobExecution>(context);

            // When
            var result = await repo.GetAndMarkPending(1, DateTime.UtcNow.AddSeconds(5), CancellationToken.None);            

            // Then
            result.ShouldBeEmpty();
        }

        private static JobContext GetSeededJobContext(Job job, JobExecution jobExecution)
        {
            var contextOptions = new DbContextOptionsBuilder<JobContext>()
                .UseInMemoryDatabase("test")
                .Options;
            
            var context = new JobContext(contextOptions);

            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            context.Add(job);
            context.Add(jobExecution);

            context.SaveChanges();

            return context;
        }
    }
}