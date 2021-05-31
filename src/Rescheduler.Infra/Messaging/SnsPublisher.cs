using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rescheduler.Core.Entities;
using Rescheduler.Core.Interfaces;
using Rescheduler.Infra.Metrics;

namespace Rescheduler.Infra.Messaging
{
    internal class SnsPublisher : IJobPublisher
    {
        private readonly ILogger _logger;
        private readonly IAmazonSimpleNotificationService _sns;
        
        private SnsOptions _options;

        public SnsPublisher(ILogger<SnsPublisher> logger, IAmazonSimpleNotificationService sns, IOptionsMonitor<MessagingOptions> optionsMonitor)
        {
            _logger = logger;
            _sns = sns;
            _options = optionsMonitor.CurrentValue.SnsOptions;
            
            optionsMonitor.OnChange(newOptions =>
            {
                if (newOptions?.ServiceBus is null) return;

                _options = newOptions.SnsOptions;
            });
        }

        public async Task<bool> PublishAsync(JobExecution jobExecution, CancellationToken ctx)
        {
            using var _ = MessagingMetrics.TimePublishDuration();
            
            return await PublishJobExecutionAsync(jobExecution, ctx);
            }

        public async Task<bool> PublishManyAsync(IEnumerable<JobExecution> jobExecutions, CancellationToken ctx)
        {
            using var _ = MessagingMetrics.TimeBatchPublishDuration();

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
        }
        
        private async Task<bool> PublishJobExecutionAsync(JobExecution jobExecution, CancellationToken ctx)
        {
            try
            {
                var request = new PublishRequest(_options.TopicArn, jobExecution.Job.Payload, jobExecution.Job.Subject)
                {
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
}