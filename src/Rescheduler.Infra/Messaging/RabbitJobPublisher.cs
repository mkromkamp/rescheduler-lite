using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;
using Rescheduler.Core.Entities;
using Rescheduler.Core.Interfaces;

namespace Rescheduler.Infra.Messaging
{
    public class RabbitJobPublisher : IJobPublisher
    {
        private readonly IConnectionFactory _connectionFactory;

        public RabbitJobPublisher(IConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public Task<bool> PublishAsync(Job job, CancellationToken ctx)
        {
            using var conn = _connectionFactory.CreateConnection();
            using var model = conn.CreateModel();

            model.EnsureTopic(job.Subject);
            model.BasicPublish(job.Subject, "job", true, null, Encoding.UTF8.GetBytes(job.Payload));
            
            return Task.FromResult(model.WaitForConfirms());
        }

        public Task<(Guid jobId, bool success)> PublishManyAsync(IEnumerable<Job> jobs, CancellationToken ctx)
        {
            throw new System.NotImplementedException();
        }
    }

    internal static class ModelExtensions
    {
        public static void EnsureTopic(this IModel model, string topicName)
        {
            model.ExchangeDeclare(topicName, null, true, false, null);
        }
    }
}