using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SimpleNotificationService;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rescheduler.Core.Entities;
using Rescheduler.Core.Interfaces;

namespace Rescheduler.Infra.Messaging
{
    internal class SnsPublisher : IJobPublisher
    {
        private readonly ILogger _logger;
        private readonly IAmazonSimpleNotificationService _sns;
        
        private SnsOptions _options;

        public SnsPublisher(ILogger logger, IAmazonSimpleNotificationService sns, IOptionsMonitor<MessagingOptions> optionsMonitor)
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

        public Task<bool> PublishAsync(JobExecution jobExecution, CancellationToken ctx)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> PublishManyAsync(IEnumerable<JobExecution> jobExecutions, CancellationToken ctx)
        {
            throw new System.NotImplementedException();
        }
    }
}