using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Rescheduler.Core.Entities;
using Rescheduler.Core.Interfaces;

namespace Rescheduler.Infra.Messaging
{
    internal class RabbitJobPublisher : IJobPublisher
    {
        private readonly ILogger<RabbitJobPublisher> _logger;
        private readonly IConnectionFactory _connectionFactory;
        private RabbitMqOptions _options;
        private IModel? _model;

        public RabbitJobPublisher(IConnectionFactory connectionFactory, ILogger<RabbitJobPublisher> logger, IOptionsMonitor<RabbitMqOptions> optionsMonitor)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;

            _options = optionsMonitor.CurrentValue;
            optionsMonitor.OnChange(newOptions => _options = newOptions);
        }

        public Task<bool> PublishAsync(Job job, CancellationToken ctx)
        {
            try
            {
                var model = GetOrCreateModel();

                model.EnsureConfig(_options.JobsExchange, job.Subject);
                model.BasicPublish(_options.JobsExchange, job.Subject, true, null, Encoding.UTF8.GetBytes(job.Payload));
                
                return Task.FromResult(model.WaitForConfirms());
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to publish job {JobId} to RabbitMQ", job.Id);
                throw;
            }
        }

        public Task<bool> PublishManyAsync(IEnumerable<Job> jobs, CancellationToken ctx)
        {
            jobs = jobs.ToList();

            try
            {
                var model = GetOrCreateModel();
                var batchPublish = model.CreateBasicPublishBatch();
                
                jobs.GroupBy(j => j.Subject).ToList().ForEach(g => 
                {
                    model.EnsureConfig(_options.JobsExchange, g.Key);
                    foreach (var job in g.ToList())
                    {
                        batchPublish.Add(_options.JobsExchange, job.Subject, true, null, new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(job.Payload)));
                    }
                });

                batchPublish.Publish();
                return Task.FromResult(model.WaitForConfirms());
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to batch publish {JobIds} to RabbitMQ", jobs.Select(j => j.Id));
                throw;
            }
        }

        private IModel GetOrCreateModel()
        {
            if (_model is null || _model.IsClosed)
            {
                _model = _connectionFactory.CreateConnection().CreateModel();
                _model.ConfirmSelect();
            }

            return _model;
        }
    }

    internal static class ModelExtensions
    {
        public static void EnsureConfig(this IModel model, string topicName, string queueName)
        {
            model.EnsureTopic(topicName);
            model.EnsureQueue(queueName);

            model.EnsureRoute(topicName, queueName, queueName);
        }

        public static void EnsureTopic(this IModel model, string topicName)
        {
            model.ExchangeDeclare(topicName, ExchangeType.Topic, true, false);
        }

        public static void EnsureQueue(this IModel model, string queueName)
        {
            var props = new Dictionary<string, object> { {"x-queue-type", "quorum"} };
            model.QueueDeclare(queueName, true, false, false, props);
        }

        public static void EnsureRoute(this IModel model, string topicName, string queueName, string routingKey)
        {
            model.QueueBind(queueName, topicName, routingKey);
        }
    }
}