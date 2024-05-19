using System.Diagnostics;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rescheduler.Core.Entities;
using Rescheduler.Core.Interfaces;
using Rescheduler.Infra.Metrics;

namespace Rescheduler.Infra.Messaging;

internal class ServiceBusPublisher : IJobPublisher
{
    private readonly ILogger _logger;
    private readonly IMessagingMetrics _metrics;
        
    private ServiceBusOptions _options;
    private ServiceBusSender _serviceBusSender;

    public ServiceBusPublisher(ILogger<ServiceBusPublisher> logger, IOptionsMonitor<MessagingOptions> optionsMonitor, ServiceBusClient serviceBusClient, IMessagingMetrics metrics)
    {
        _logger = logger;
        _metrics = metrics;
        _options = optionsMonitor.CurrentValue.ServiceBus;
        _serviceBusSender = serviceBusClient.CreateSender(_options.JobsQueue);

        optionsMonitor.OnChange(newOptions =>
        {
            if (newOptions?.ServiceBus is null) return;

            _options = newOptions.ServiceBus;
            _serviceBusSender = serviceBusClient.CreateSender(_options.JobsQueue);
        });
    }

    public async Task<bool> PublishAsync(JobExecution jobExecution, CancellationToken ctx)
    {
        var t = Stopwatch.StartNew();

        var job = jobExecution.Job;
        var message = new ServiceBusMessage(job.Payload)
        {
            MessageId = jobExecution.Id.ToString(),
            PartitionKey = job.Id.ToString(),
            Subject = job.Subject,
        };

        _metrics.MessagesPublished(job.Subject);
            
        var res = await WithSenderAsync(conn => conn.SendMessageAsync(message, ctx));
        _metrics.TimePublishDuration(t.Elapsed);

        return res;
    }

    public async Task<bool> PublishManyAsync(IEnumerable<JobExecution> jobExecutions, CancellationToken ctx)
    {
        // jobExecutions = jobExecutions.ToList();
        var jobExecutionsList = jobExecutions.ToList();
        if (!jobExecutionsList.Any()) return true;

        if (_options.PartitionedQueue)
        {
            return await PublishPartitioned(jobExecutionsList.ToList());
        }

        return await PublishBatched(jobExecutionsList);

        async Task<bool> PublishPartitioned(List<JobExecution> executions)
        {
            var tasks = executions.Select(jobExecution => PublishAsync(jobExecution, ctx));
            var result = await Task.WhenAll(tasks);

            // Any tasks failed -> we consider the batch to be failed
            return result.Contains(false) is false;
        }

        async Task<bool> PublishBatched(List<JobExecution> executions)
        {
            var t = Stopwatch.StartNew();
                
            var messages = executions.Select(jobExecution =>
                new ServiceBusMessage(jobExecution.Job.Payload)
                {
                    MessageId = jobExecution.Id.ToString(),
                    Subject = jobExecution.Job.Subject,
                });

            executions
                .GroupBy(jobExecution => jobExecution.Job.Subject)
                .ToList()
                .ForEach(jobGroup =>
                    _metrics.MessagesPublished(jobGroup.Key, jobGroup.Count()));

            var res = await WithSenderAsync(conn => conn.SendMessagesAsync(messages, ctx));
            _metrics.TimePublishBatchDuration(t.Elapsed);
            
            return res;
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