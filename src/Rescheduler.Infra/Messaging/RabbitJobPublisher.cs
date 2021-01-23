using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using Rescheduler.Core.Entities;
using Rescheduler.Core.Interfaces;

namespace Rescheduler.Infra.Messaging
{
    public class RabbitJobPublisher : IJobPublisher
    {
        private readonly ILogger<RabbitJobPublisher> _logger;
        private readonly IConnectionFactory _connectionFactory;
        private IModel _model;

        public RabbitJobPublisher(IConnectionFactory connectionFactory, ILogger<RabbitJobPublisher> logger)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;

            _model = GetOrCreateModel();
        }

        public Task<bool> PublishAsync(Job job, CancellationToken ctx)
        {
            try
            {
                var model = GetOrCreateModel();

                model.EnsureTopic(job.Subject);
                model.BasicPublish(job.Subject, "job", true, null, Encoding.UTF8.GetBytes(job.Payload));
                
                return Task.FromResult(model.WaitForConfirms());
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to publish job {JobId} to RabbitMQ", job.Id);
                throw;
            }
        }

        public Task<(Guid jobId, bool success)> PublishManyAsync(IEnumerable<Job> jobs, CancellationToken ctx)
        {
            throw new System.NotImplementedException();
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
        public static void EnsureTopic(this IModel model, string topicName)
        {
            model.ExchangeDeclare(topicName, ExchangeType.Topic, true, false);
        }
    }
}