using System;
using Cronos;
using Rescheduler.Core.Entities;
using Shouldly;
using Xunit;

namespace Rescheduler.Core.Tests.Entities
{
    public class JobTests
    {
        [Fact]
        public void GivenValidJob_WhenGettingInitialRunDate_ShouldReturnRunAt()
        {
            // Given
            var now = DateTime.UtcNow;
            var job = new Job(Guid.NewGuid(), "test", "test payload", true, now.AddDays(1), now.AddDays(2), "*/10 * * * *");

            // When
            var hasNext = job.TryGetNextRun(now, out var nextRun);

            // Then
            hasNext.ShouldBe(true);
            nextRun.ShouldBe(job.RunAt);
        }

        [Fact]
        public void GivenValidJob_WhenGettingSecondRunDate_ShouldReturnNextCronOccurrence()
        {
            // Given
            var now = DateTime.UtcNow;
            var job = new Job(Guid.NewGuid(), "test", "test payload", true, now.AddDays(1), now.AddDays(2), "*/10 * * * *");

            // When
            var hasNext = job.TryGetNextRun(job.RunAt, out var nextRun);

            // Then
            hasNext.ShouldBe(true);
            nextRun.ShouldBe(CronExpression.Parse(job.Cron).GetNextOccurrence(job.RunAt, true));
        }

        [Fact]
        public void GivenValidJob_WhenGettingRunDateAfterStop_ShouldNotReturnNextCronOccurrence()
        {
            // Given
            var now = DateTime.UtcNow;
            var job = new Job(Guid.NewGuid(), "test", "test payload", true, now.AddDays(1), now.AddDays(2), "*/10 * * * *");

            // When
            var hasNext = job.TryGetNextRun(job.StopAfter.AddSeconds(1), out var nextRun);

            // Then
            hasNext.ShouldBe(false);
            nextRun.ShouldBeNull();
        }
    }
}