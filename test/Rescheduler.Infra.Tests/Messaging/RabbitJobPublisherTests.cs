using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using RabbitMQ.Client;
using Rescheduler.Core.Entities;
using Rescheduler.Infra.Messaging;
using Shouldly;
using Xunit;

namespace Rescheduler.Infra.Tests.Messaging
{
    public class RabbitJobPublisherTests
    {
        private readonly ILogger<RabbitJobPublisher> _logger;
        private readonly IConnectionFactory _connectionFactory;
        private readonly RabbitMqOptions _options;

        private readonly RabbitJobPublisher _publisher;

        private readonly IConnection _connection;
        private readonly IModel _model;

        public RabbitJobPublisherTests()
        {
            _logger = Mock.Of<ILogger<RabbitJobPublisher>>();
            
            _connectionFactory = Mock.Of<IConnectionFactory>();
            _connection = Mock.Of<IConnection>();
            _model = Mock.Of<IModel>();

            Mock.Get(_connectionFactory)
                .Setup(x => x.CreateConnection())
                .Returns(_connection);

            Mock.Get(_connection)
                .Setup(x => x.CreateModel())
                .Returns(_model);

            Mock.Get(_model)
                .Setup(x => x.ExchangeDeclare(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(),
                    It.IsAny<bool>(), It.IsAny<IDictionary<string, object>>()));

            Mock.Get(_model)
                .Setup(x => x.QueueDeclare(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(),
                    It.IsAny<IDictionary<string, object>>()));

            
            _options = new RabbitMqOptions{Enabled = true};
            var optionsMonitor = Mock.Of<IOptionsMonitor<MessagingOptions>>();
            Mock.Get(optionsMonitor)
                .SetupGet(x => x.CurrentValue)
                .Returns(new MessagingOptions {RabbitMq = _options});

            _publisher = new RabbitJobPublisher(_connectionFactory, _logger, optionsMonitor);
        }

        [Fact]
        public async Task GivenSingleJob_WhenPublishing_ThenShouldPublish()
        {
            // Given
            var job = Job.New("subject", "payload", true, DateTime.UtcNow, DateTime.UtcNow.AddHours(1), null);
            var jobExecution = JobExecution.New(job, job.RunAt);
            _options.JobsExchange = "jobs";
            Mock.Get(_model)
                .Setup(x => x.WaitForConfirms())
                .Returns(true);

            Mock.Get(_model)
                .Setup(x => x.BasicPublish(_options.JobsExchange, job.Subject, true, null, It.IsAny<ReadOnlyMemory<byte>>()));

            // When
            var result = await _publisher.PublishAsync(jobExecution, CancellationToken.None);

            // Then
            result.ShouldBeTrue();
            Mock.Get(_model)
                .Verify(x => x.BasicPublish(_options.JobsExchange, job.Subject, true, null, It.IsAny<ReadOnlyMemory<byte>>()),
                    Times.Once);
        }

        [Fact]
        public async Task GivenRabbitIsUnavailable_WhenPublishing_ThenShouldReturnFalse()
        {
            // Given
            var job = Job.New("subject", "payload", true, DateTime.UtcNow, DateTime.UtcNow.AddHours(1), null);
            var jobExecution = JobExecution.New(job, job.RunAt);
            _options.JobsExchange = "jobs";
            Mock.Get(_connectionFactory)
                .Setup(x => x.CreateConnection())
                .Throws(new Exception());

            // When
            var result = await _publisher.PublishAsync(jobExecution, CancellationToken.None);

            // Then
            result.ShouldBeFalse();
            Mock.Get(_model)
                .Verify(x => x.BasicPublish(_options.JobsExchange, job.Subject, true, null, It.IsAny<ReadOnlyMemory<byte>>()),
                    Times.Never);
        }
        
        [Fact]
        public async Task GivenSingleJob_WhenUnsuccessfulPublishing_ThenShouldReturnFalse()
        {
            // Given
            var job = Job.New("subject", "payload", true, DateTime.UtcNow, DateTime.UtcNow.AddHours(1), null);
            var jobExecution = JobExecution.New(job, job.RunAt);
            _options.JobsExchange = "jobs";
            Mock.Get(_model)
                .Setup(x => x.WaitForConfirms())
                .Returns(true);

            Mock.Get(_model)
                .Setup(x => x.BasicPublish(_options.JobsExchange, job.Subject, true, null, It.IsAny<ReadOnlyMemory<byte>>()))
                .Throws<Exception>();

            // When
            var result = await _publisher.PublishAsync(jobExecution, CancellationToken.None);

            // Then
            result.ShouldBeFalse();
        }
        
        [Fact]
        public async Task GivenTwoJobs_WhenPublishing_ThenShouldPublishBoth()
        {
            // Given
            var firstJob = Job.New("subject", "payload", true, DateTime.UtcNow, DateTime.UtcNow.AddHours(1), null);
            var secondJob = Job.New("subject", "payload", true, DateTime.UtcNow, DateTime.UtcNow.AddHours(1), null);
            var firstJobExecution = JobExecution.New(firstJob, firstJob.RunAt);
            var secondJobExecution = JobExecution.New(secondJob, secondJob.RunAt);
            _options.JobsExchange = "jobs";
            Mock.Get(_model)
                .Setup(x => x.WaitForConfirms())
                .Returns(true);

            var batchPublish = Mock.Of<IBasicPublishBatch>();

            Mock.Get(_model)
                .Setup(x => x.CreateBasicPublishBatch())
                .Returns(batchPublish);

            Mock.Get(_model)
                .Setup(x => x.BasicPublish(_options.JobsExchange, firstJob.Subject, true, null, It.IsAny<ReadOnlyMemory<byte>>()));
            
            Mock.Get(_model)
                .Setup(x => x.BasicPublish(_options.JobsExchange, secondJob.Subject, true, null, It.IsAny<ReadOnlyMemory<byte>>()));

            // When
            var result = await _publisher.PublishManyAsync(new []{firstJobExecution, secondJobExecution}, CancellationToken.None);

            // Then
            result.ShouldBeTrue();
            Mock.Get(batchPublish)
                .Verify(x => x.Publish(), Times.Once);
        }
        
        [Fact]
        public async Task GivenTwoJobs_WhenUnsuccessfulPublishing_ThenShouldReturnFalse()
        {
            // Given
            var firstJob = Job.New("subject", "payload", true, DateTime.UtcNow, DateTime.UtcNow.AddHours(1), null);
            var secondJob = Job.New("subject", "payload", true, DateTime.UtcNow, DateTime.UtcNow.AddHours(1), null);
            var firstJobExecution = JobExecution.New(firstJob, firstJob.RunAt);
            var secondJobExecution = JobExecution.New(secondJob, secondJob.RunAt);
            _options.JobsExchange = "jobs";
            Mock.Get(_model)
                .Setup(x => x.WaitForConfirms())
                .Returns(true);

            Mock.Get(_model)
                .Setup(x => x.CreateBasicPublishBatch())
                .Throws<Exception>();

            // When
            var result = await _publisher.PublishManyAsync(new []{firstJobExecution, secondJobExecution}, CancellationToken.None);

            // Then
            result.ShouldBeFalse();
        }
        
        [Fact]
        public async Task GivenRabbitIsUnavailable_WhenPublishingMany_ThenShouldReturnFalse()
        {
            // Given
            var job = Job.New("subject", "payload", true, DateTime.UtcNow, DateTime.UtcNow.AddHours(1), null);
            var jobExecution = JobExecution.New(job, job.RunAt);
            _options.JobsExchange = "jobs";
            Mock.Get(_connectionFactory)
                .Setup(x => x.CreateConnection())
                .Throws(new Exception());

            // When
            var result = await _publisher.PublishManyAsync(new []{jobExecution}, CancellationToken.None);

            // Then
            result.ShouldBeFalse();
        }
    }
}