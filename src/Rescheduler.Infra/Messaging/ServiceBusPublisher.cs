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
using Rescheduler.Infra.Metrics;

namespace Rescheduler.Infra.Messaging
{
    internal class ServiceBusPublisher : IJobPublisher
    {
        private readonly ILogger _logger;
        
        private ServiceBusOptions _options;
        private ServiceBusSender _serviceBusSender;

        public ServiceBusPublisher(ILogger<ServiceBusPublisher> logger, IOptionsMonitor<ServiceBusOptions> optionsMonitor, ServiceBusClient serviceBusClient)
        {
            _logger = logger;
            _options = optionsMonitor.CurrentValue;
            _serviceBusSender = serviceBusClient.CreateSender(_options.JobsQueue);

            optionsMonitor.OnChange(newOptions =>
            {
                if (newOptions is null) return;

                _options = newOptions;
                _serviceBusSender = serviceBusClient.CreateSender(_options.JobsQueue);
            });
        }

        public Task<bool> PublishAsync(JobExecution jobExecution, CancellationToken ctx)
        {
            using var _ = MessagingMetrics.TimePublishDuration();

            var job = jobExecution.Job;
            var message = new ServiceBusMessage(job.Payload)
            {
                MessageId = jobExecution.Id.ToString(),
                PartitionKey = job.Id.ToString(),
                Subject = job.Subject,
            };

            MessagingMetrics.MessagesPublished(job.Subject);
            
            return WithSenderAsync(conn => conn.SendMessageAsync(message, ctx));
        }

        public async Task<bool> PublishManyAsync(IEnumerable<JobExecution> jobExecutions, CancellationToken ctx)
        {
            using var _ = MessagingMetrics.TimeBatchPublishDuration();

            jobExecutions = jobExecutions.ToList();
            if (!jobExecutions.Any()) return true;

            if (_options.PartitionedQueue)
            {
                return await PublishPartitioned(jobExecutions, ctx);
            }
            else
            {
                return await PublishBatched(jobExecutions, ctx);
            }

            async Task<bool> PublishPartitioned(IEnumerable<JobExecution> jobExecutions, CancellationToken ctx)
            {
                var tasks = jobExecutions.ToList().Select(jobExecution => PublishAsync(jobExecution, ctx));
                var result = await Task.WhenAll(tasks);

                // Any tasks failed -> we consider the batch to be failed
                return result.Contains(false) is false;
            }

            async Task<bool> PublishBatched(IEnumerable<JobExecution> jobExecutions, CancellationToken ctx)
            {
                var messages = jobExecutions.Select(jobExecution =>
                    new ServiceBusMessage(jobExecution.Job.Payload)
                    {
                        MessageId = jobExecution.Id.ToString(),
                        Subject = jobExecution.Job.Subject,
                    });

                jobExecutions
                    .GroupBy(jobExecution => jobExecution.Job.Subject)
                    .ToList()
                    .ForEach(jobGroup =>
                        MessagingMetrics.MessagesPublished(jobGroup.Key, jobGroup.Count()));

                return await WithSenderAsync(conn => conn.SendMessagesAsync(messages, ctx));
            }
        }

        private async Task<bool> WithSenderAsync(Func<ServiceBusSender, Task> func)
        {
            try
            {
                await func(_serviceBusSender);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish jobs to Azure Service Bus");
                return false;
            }
        }
    }
}