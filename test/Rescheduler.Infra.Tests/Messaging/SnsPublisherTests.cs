using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Rescheduler.Core.Entities;
using Rescheduler.Core.Interfaces;
using Rescheduler.Infra.Messaging;
using Shouldly;
using Xunit;

namespace Rescheduler.Infra.Tests.Messaging;

public class SnsPublisherTests
{
    private readonly ILogger<SnsPublisher> _logger;
    private SnsOptions _options;
    private readonly IAmazonSimpleNotificationService _sns;

    private readonly IJobPublisher _publisher;

    public SnsPublisherTests()
    {
        _logger = Mock.Of<ILogger<SnsPublisher>>();
        _options = new SnsOptions
        {
            Enabled = true,
            FifoTopic = true,
            TopicArn = "sns:topic"
        };

        var optionsMonitor = Mock.Of<IOptionsMonitor<MessagingOptions>>();
        Mock.Get(optionsMonitor)
            .SetupGet(x => x.CurrentValue)
            .Returns(new MessagingOptions {Sns = _options});

        _sns = Mock.Of<IAmazonSimpleNotificationService>();

        Mock.Get(_sns)
            .Setup(x => x.PublishAsync(It.IsAny<PublishRequest>(), CancellationToken.None))
            .ReturnsAsync(new PublishResponse {HttpStatusCode = HttpStatusCode.OK});

        _publisher = new SnsPublisher(_logger, _sns, optionsMonitor);
    }

    [Fact]
    public async Task GivenSingleJob_WhenPublishing_ThenShouldPublish()
    {
        // Given
        var job = Job.New("subject", "payload", true, DateTime.UtcNow, DateTime.UtcNow.AddHours(1), null);
        var jobExecution = JobExecution.New(job, job.RunAt);

        // When
        var result = await _publisher.PublishAsync(jobExecution, CancellationToken.None);

        // Then
        result.ShouldBeTrue();
        Mock.Get(_sns)
            .Verify(x => x.PublishAsync(It.IsAny<PublishRequest>(), CancellationToken.None)
                , Times.Once);
    }

    [Fact]
    public async Task GivenSnsIsUnavailable_WhenPublishing_ThenShouldReturnFalse()
    {
        // Given
        var job = Job.New("subject", "payload", true, DateTime.UtcNow, DateTime.UtcNow.AddHours(1), null);
        var jobExecution = JobExecution.New(job, job.RunAt);
        Mock.Get(_sns)
            .Setup(x => x.PublishAsync(It.IsAny<PublishRequest>(), CancellationToken.None))
            .Throws<Exception>();

        // When
        var result = await _publisher.PublishAsync(jobExecution, CancellationToken.None);

        // Then
        result.ShouldBeFalse();
        Mock.Get(_sns)
            .Verify(x => x.PublishAsync(It.IsAny<PublishRequest>(), CancellationToken.None)
                , Times.Once);
    }

    [Fact]
    public async Task GivenTwoJobs_WhenPublishing_ThenShouldPublishBoth()
    {
        // Given
        var firstJob = Job.New("subject", "payload", true, DateTime.UtcNow, DateTime.UtcNow.AddHours(1), null);
        var secondJob = Job.New("subject", "payload", true, DateTime.UtcNow, DateTime.UtcNow.AddHours(1), null);
        var firstJobExecution = JobExecution.New(firstJob, firstJob.RunAt);
        var secondJobExecution = JobExecution.New(secondJob, secondJob.RunAt);

        // When
        var result = await _publisher.PublishManyAsync(new[] {firstJobExecution, secondJobExecution},
            CancellationToken.None);

        // Then
        result.ShouldBeTrue();
        Mock.Get(_sns)
            .Verify(x => x.PublishAsync(It.IsAny<PublishRequest>(), CancellationToken.None)
                , Times.Exactly(2));
    }

    [Fact]
    public async Task GivenTwoJobs_WhenUnsuccessfulPublishing_ThenShouldReturnFalse()
    {
        // Given
        var firstJob = Job.New("subject", "payload", true, DateTime.UtcNow, DateTime.UtcNow.AddHours(1), null);
        var secondJob = Job.New("subject", "payload", true, DateTime.UtcNow, DateTime.UtcNow.AddHours(1), null);
        var firstJobExecution = JobExecution.New(firstJob, firstJob.RunAt);
        var secondJobExecution = JobExecution.New(secondJob, secondJob.RunAt);

        Mock.Get(_sns)
            .Setup(x => x.PublishAsync(It.IsAny<PublishRequest>(), CancellationToken.None))
            .Throws<Exception>();

        // When
        var result = await _publisher.PublishManyAsync(new[] {firstJobExecution, secondJobExecution},
            CancellationToken.None);

        // Then
        result.ShouldBeFalse();
        Mock.Get(_sns)
            .Verify(x => x.PublishAsync(It.IsAny<PublishRequest>(), CancellationToken.None)
                , Times.Exactly(2));
    }
}