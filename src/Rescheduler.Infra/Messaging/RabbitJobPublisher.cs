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
using Rescheduler.Infra.Metrics;

namespace Rescheduler.Infra.Messaging
{
    internal class RabbitJobPublisher : IJobPublisher
    {
        private readonly ILogger<RabbitJobPublisher> _logger;
        private readonly IConnectionFactory _connectionFactory;
        private RabbitMqOptions _options;
        private IModel? _model;

        public RabbitJobPublisher(IConnectionFactory connectionFactory, ILogger<RabbitJobPublisher> logger, IOptionsMonitor<MessagingOptions> optionsMonitor)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;

            _options = optionsMonitor.CurrentValue.RabbitMq;
            optionsMonitor.OnChange(newOptions => _options = newOptions.RabbitMq);
        }

        public Task<bool> PublishAsync(Job job, CancellationToken ctx)
        {
            try
            {
                if(!TryGetOrCreateModel(out var model) || model is null) return Task.FromResult(false);

                model.EnsureConfig(_options.JobsExchange, job.Subject);
                model.BasicPublish(_options.JobsExchange, job.Subject, true, null, Encoding.UTF8.GetBytes(job.Payload));

                MessagingMetrics.MessagesPublished(job.Subject);
                
                return Task.FromResult(model.WaitForConfirms());
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to publish job {JobId} to RabbitMQ", job.Id);
                return Task.FromResult(false);
            }
        }

        public Task<bool> PublishManyAsync(IEnumerable<Job> jobs, CancellationToken ctx)
        {
            jobs = jobs.ToList();

            try
            {
                if(!TryGetOrCreateModel(out var model) || model is null) return Task.FromResult(false);
                var batchPublish = model.CreateBasicPublishBatch();
                
                jobs.GroupBy(j => j.Subject).ToList().ForEach(g => 
                {
                    model.EnsureConfig(_options.JobsExchange, g.Key);
                    foreach (var job in g.ToList())
                    {
                        batchPublish.Add(_options.JobsExchange, job.Subject, true, null, new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(job.Payload)));
                    }

                    MessagingMetrics.MessagesPublished(g.Key, g.Count());
                });

                batchPublish.Publish();
                return Task.FromResult(model.WaitForConfirms());
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to batch publish {JobIds} to RabbitMQ", jobs.Select(j => j.Id));
                return Task.FromResult(false);
            }
        }

        private bool TryGetOrCreateModel(out IModel? model)
        {
            if (_model is null || _model.IsClosed)
            {
                try
                {   
                    _model = _connectionFactory.CreateConnection().CreateModel();
                    _model.ConfirmSelect();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to create RabbitMQ connection");

                    model = null;
                    return false;
                }
            }

            model = _model;
            return true;
        }
    }
}