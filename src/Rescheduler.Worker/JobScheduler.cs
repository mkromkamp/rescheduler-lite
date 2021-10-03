using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Rescheduler.Core.Handlers;
using Rescheduler.Infra;

namespace Rescheduler.Worker
{
    public class JobScheduler : BackgroundService
    {
        private readonly ILogger _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public JobScheduler(ILogger<JobScheduler> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        protected override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            return Task.Run(() => RunAsync(cancellationToken), cancellationToken);
        }

        private async Task RunAsync(CancellationToken ctx)
        {
            // Wait for the service to start and apply pending db migrations
            await Task.Delay(TimeSpan.FromSeconds(5), ctx);

            using var logScope = _logger.BeginScope("{Service}", "JobScheduler");

            await RecoverJobExecutionsAsync(ctx);
            await RunSchedulerAsync(ctx);
        }

        internal async Task RecoverJobExecutionsAsync(CancellationToken ctx)
        {
            using var scope = _scopeFactory.CreateScope();
            await using var _ = _logger.Time(LogLevel.Information, "Recovery executions");

            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var recovered = await mediator.Send(new RecoverJobExecutionsRequest(), ctx);
            
            if (recovered.NumRecovered > 0)
                _logger.LogInformation("Recovered {NumExecutions}", recovered.NumRecovered);
        }

        internal async Task RunSchedulerAsync(CancellationToken ctx)
        {
            while (!ctx.IsCancellationRequested)
            {
                // Scope this part due to DbContext disposing etc.
                using var scope = _scopeFactory.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                
                var result = await mediator.Send(new SchedulePendingRequest(), ctx);

                if (result.NumScheduled > 0)
                    _logger.LogInformation("Queued {NumbJobs} jobs", result.NumScheduled);
                
                await Task.Delay(TimeSpan.FromSeconds(3), ctx);
            }
        }
    }
}