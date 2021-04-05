using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rescheduler.Core.Entities;
using Rescheduler.Core.Interfaces;

namespace Rescheduler.Infra.Messaging
{
    internal class ServiceBusPublisher : IJobPublisher
    {
        private readonly ILogger _logger;
        private ServiceBusOptions _options;
        private ServiceBusClient _serviceBusClient;

        public ServiceBusPublisher(ILogger<ServiceBusPublisher> logger, IOptionsMonitor<ServiceBusOptions> optionsMonitor, ServiceBusClient serviceBusClient)
        {
            _logger = logger;
            _serviceBusClient = serviceBusClient;
            _options = optionsMonitor.CurrentValue;
            optionsMonitor.OnChange(newOptions =>
            {
                if (newOptions is not null)
                    _options = newOptions;
            });
        }

        public Task<bool> PublishAsync(Job job, CancellationToken ctx)
        {
            var message = new ServiceBusMessage(job.Payload)
            {
                PartitionKey = job.Id.ToString(),
                Subject = job.Subject,
            };
            
            return WithConnectionAsync(conn => conn.SendMessageAsync(message, ctx));
        }

        public Task<bool> PublishManyAsync(IEnumerable<Job> jobs, CancellationToken ctx)
        {
            var jobsList = jobs.ToList();
            if (!jobsList.Any()) return new ValueTask<bool>(true).AsTask();
            
            var messages = jobsList.Select(job => 
                new ServiceBusMessage(job.Payload)
                {
                    PartitionKey = job.Id.ToString(),
                    Subject = job.Subject,
                });
            
            return WithConnectionAsync(conn => conn.SendMessagesAsync(messages, ctx));
        }

        private async Task<bool> WithConnectionAsync(Func<ServiceBusSender, Task> func)
        {
            await func(_serviceBusClient.CreateSender((_options.JobsQueue)));

            return true;
        }
    }
}