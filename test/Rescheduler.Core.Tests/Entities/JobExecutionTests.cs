using Rescheduler.Core.Entities;
using Shouldly;
using Xunit;

namespace Rescheduler.Core.Tests.Entities;

public class JobExecutionTests
{
    [Fact]
    public void GivenValidJobExecution_WhenQueued_ShouldSetScheduledAt()
    {
        // Given
        var queuedAt = DateTime.UtcNow;
        var jobExecution = GetValidJobExecution();

        // When
        jobExecution.Queued(queuedAt);

        // Then
        jobExecution.Status.ShouldBe(ExecutionStatus.Queued);
        jobExecution.QueuedAt.ShouldBe(queuedAt);
    }

    [Fact]
    public void GivenValidJobExecution_WhenMarkedInFlight_ShouldHaveStatusInFlight()
    {
        // Given
        var jobExecution = GetValidJobExecution();

        // When
        jobExecution.InFlight();

        // Then
        jobExecution.Status.ShouldBe(ExecutionStatus.InFlight);
    }

    [Fact]
    public void GivenValidJobExecution_WhenMarkedScheduled_ShouldHaveStatusScheduled()
    {
        // Given
        var jobExecution = GetValidJobExecution();
        jobExecution.InFlight();

        // When
        jobExecution.Scheduled();

        // Then
        jobExecution.Status.ShouldBe(ExecutionStatus.Scheduled);
    }

    private JobExecution GetValidJobExecution()
    {
        return new (
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
    }
}