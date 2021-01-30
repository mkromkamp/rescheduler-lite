using System;
using Rescheduler.Core.Entities;
using Shouldly;
using Xunit;

namespace Rescheduler.Core.Tests.Entities
{
    public class JobExecutionTests
    {
        [Fact]
        public void GivenValidJobExecution_WhenQueued_ShouldSetScheduledAt()
        {
            // Given
            var queuedAt = DateTime.UtcNow;
            var jobExecution = new JobExecution(
                Guid.NewGuid(), 
                DateTime.UtcNow, 
                null, 
                ExecutionStatus.Scheduled, 
                Job.New(
                    "test",
                    "test payload",
                    true,
                    DateTime.UtcNow,
                    DateTime.UtcNow.AddDays(1),
                    null
                ));

            // When
            jobExecution.Queued(queuedAt);

            // Then
            jobExecution.Status.ShouldBe(ExecutionStatus.Queued);
            jobExecution.QueuedAt.ShouldBe(queuedAt);
        }
    }
}