using System;
using Rescheduler.Core.Entities;
using Shouldly;
using Xunit;

namespace Rescheduler.Core.Tests.Entities
{
    public class ScheduledJobTests
    {
        [Fact]
        public void GivenValidScheduledJob_WhenQueued_ShouldSetScheduledAt()
        {
            // Given
            var queuedAt = DateTime.UtcNow;
            var scheduledJob = new JobExecution(
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
            scheduledJob.Queued(queuedAt);

            // Then
            scheduledJob.Status.ShouldBe(ExecutionStatus.Queued);
            scheduledJob.QueuedAt.ShouldBe(queuedAt);
        }
    }
}