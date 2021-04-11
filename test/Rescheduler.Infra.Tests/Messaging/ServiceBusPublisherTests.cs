using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Rescheduler.Core.Entities;
using Rescheduler.Core.Interfaces;
using Rescheduler.Infra.Messaging;
using Shouldly;
using Xunit;

namespace Rescheduler.Infra.Tests.Messaging
{
    public class ServiceBusPublisherTests
    {
        private readonly ILogger<ServiceBusPublisher> _logger;
        private ServiceBusOptions _options;
        private readonly ServiceBusClient _serviceBusClient;
        private readonly ServiceBusSender _serviceBusSender;

        private readonly IJobPublisher _publisher;

        public ServiceBusPublisherTests()
        {
            _logger = Mock.Of<ILogger<ServiceBusPublisher>>();
            _options = new ServiceBusOptions
            {
                Enabled = true, 
                ConnectionString = "", 
                PartitionedQueue = false
            };

            var optionsMonitor = Mock.Of<IOptionsMonitor<ServiceBusOptions>>();
            Mock.Get(optionsMonitor)
                .SetupGet(x => x.CurrentValue)
                .Returns(_options);

            _serviceBusClient = Mock.Of<ServiceBusClient>();
            _serviceBusSender = Mock.Of<ServiceBusSender>();

            Mock.Get(_serviceBusClient)
                .Setup(x => x.CreateSender(_options.JobsQueue))
                .Returns(_serviceBusSender);

            Mock.Get(_serviceBusSender)
                .Setup(x => x.SendMessageAsync(It.IsAny<ServiceBusMessage>(), CancellationToken.None));

            Mock.Get(_serviceBusSender)
                .Setup(x => x.SendMessagesAsync(It.IsAny<IEnumerable<ServiceBusMessage>>(), CancellationToken.None));

            _publisher = new ServiceBusPublisher(_logger, optionsMonitor, _serviceBusClient);
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
            Mock.Get(_serviceBusSender)
                .Verify(x => x.SendMessageAsync(It.IsAny<ServiceBusMessage>(), CancellationToken.None)
                , Times.Once);
        }

        [Fact]
        public async Task GivenServiceBusIsUnavailable_WhenPublishing_ThenShouldReturnFalse()
        {
            // Given
            var job = Job.New("subject", "payload", true, DateTime.UtcNow, DateTime.UtcNow.AddHours(1), null);
            var jobExecution = JobExecution.New(job, job.RunAt);
            Mock.Get(_serviceBusSender)
                .Setup(x => x.SendMessageAsync(It.IsAny<ServiceBusMessage>(), CancellationToken.None))
                .Throws<Exception>();

            // When
            var result = await _publisher.PublishAsync(jobExecution, CancellationToken.None);

            // Then
            result.ShouldBeFalse();
            Mock.Get(_serviceBusSender)
                .Verify(x => x.SendMessageAsync(It.IsAny<ServiceBusMessage>(), CancellationToken.None)
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
            var result = await _publisher.PublishManyAsync(new []{firstJobExecution, secondJobExecution}, CancellationToken.None);

            // Then
            result.ShouldBeTrue();
            Mock.Get(_serviceBusSender)
                .Verify(x => x.SendMessagesAsync(It.IsAny<IEnumerable<ServiceBusMessage>>(), CancellationToken.None)
                , Times.Once);
        }

        [Fact]
        public async Task GivenTwoJobs_WhenUnsuccessfulPublishing_ThenShouldReturnFalse()
        {
            // Given
            var firstJob = Job.New("subject", "payload", true, DateTime.UtcNow, DateTime.UtcNow.AddHours(1), null);
            var secondJob = Job.New("subject", "payload", true, DateTime.UtcNow, DateTime.UtcNow.AddHours(1), null);
            var firstJobExecution = JobExecution.New(firstJob, firstJob.RunAt);
            var secondJobExecution = JobExecution.New(secondJob, secondJob.RunAt);

            Mock.Get(_serviceBusSender)
                .Setup(x => x.SendMessagesAsync(It.IsAny<IEnumerable<ServiceBusMessage>>(), CancellationToken.None))
                .Throws<Exception>();

            // When
            var result = await _publisher.PublishManyAsync(new []{firstJobExecution, secondJobExecution}, CancellationToken.None);

            // Then
            result.ShouldBeFalse();
            Mock.Get(_serviceBusSender)
                .Verify(x => x.SendMessagesAsync(It.IsAny<IEnumerable<ServiceBusMessage>>(), CancellationToken.None)
                , Times.Once);
        }
    }
}