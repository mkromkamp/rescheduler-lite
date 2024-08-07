using System.Diagnostics;
using System.Net;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rescheduler.Core.Entities;
using Rescheduler.Core.Interfaces;
using Rescheduler.Infra.Metrics;

namespace Rescheduler.Infra.Messaging;

internal class SnsPublisher : IJobPublisher
{
    private readonly ILogger _logger;
    private readonly IMessagingMetrics _metrics;
    private readonly IAmazonSimpleNotificationService _sns;
        
    private SnsOptions _options;

    public SnsPublisher(ILogger<SnsPublisher> logger, IAmazonSimpleNotificationService sns, IOptionsMonitor<MessagingOptions> optionsMonitor, IMessagingMetrics metrics)
    {
        _logger = logger;
        _sns = sns;
        _metrics = metrics;
        _options = optionsMonitor.CurrentValue.Sns;
            
        optionsMonitor.OnChange(newOptions =>
        {
            if (newOptions?.Sns is null) return;

            _options = newOptions.Sns;
        });
    }

    public async Task<bool> PublishAsync(JobExecution jobExecution, CancellationToken ctx)
    {
        var t = Stopwatch.StartNew();
            
        var res = await PublishJobExecutionAsync(jobExecution, ctx);
        
        _metrics.TimePublishDuration(t.Elapsed);

        return res;
    }

    public async Task<bool> PublishManyAsync(IEnumerable<JobExecution> jobExecutions, CancellationToken ctx)
    {
        var t = Stopwatch.StartNew();

        var jobExecutionsList = jobExecutions.ToList();
        try
        {
            var tasks = jobExecutionsList.Select(execution => PublishJobExecutionAsync(execution, ctx));
            var results = await Task.WhenAll(tasks);

            // Any tasks failed -> we consider the batch to be failed
            return results.Contains(false) is false;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to batch publish {JobIds} to Sns", jobExecutionsList.Select(j => j.Job.Id));
            return false;
        }
        finally
        {
            _metrics.TimePublishBatchDuration(t.Elapsed);
        }
    }
        
    private async Task<bool> PublishJobExecutionAsync(JobExecution jobExecution, CancellationToken ctx)
    {
        try
        {
            var request = new PublishRequest(_options.TopicArn, jobExecution.Job.Payload, jobExecution.Job.Subject)
            {
                MessageGroupId = _options.FifoTopic ? jobExecution.Job.Id.ToString() : null,
                MessageDeduplicationId = _options.FifoTopic ? jobExecution.Id.ToString() : null
            };

            return (await _sns.PublishAsync(request, ctx))
                .HttpStatusCode.Equals(HttpStatusCode.OK);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to publish job {JobId} to Sns", jobExecution.Job.Id);
            return false;
        }
    }
}