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
            var scheduledJob = new ScheduledJob(
                Guid.NewGuid(), 
                DateTime.UtcNow, 
                null, 
                ScheduleStatus.Scheduled, 
                Guid.NewGuid());

            // When
            scheduledJob.Queued(queuedAt);

            // Then
            scheduledJob.Status.ShouldBe(ScheduleStatus.Queued);
            scheduledJob.QueuedAt.ShouldBe(queuedAt);
        }
    }
}