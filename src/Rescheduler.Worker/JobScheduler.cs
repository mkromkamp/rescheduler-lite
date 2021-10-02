using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Rescheduler.Core.Handlers;

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

        protected override async Task ExecuteAsync(CancellationToken ctx)
        {
            // Wait for the service to start and apply pending db migrations
            await Task.Delay(TimeSpan.FromSeconds(5), ctx);
            
            using var logScope = _logger.BeginScope("{Service}", "JobScheduler");

            await RecoverJobExecutionsAsync(ctx);

            _logger.LogInformation("Job scheduler started");

            await RunSchedulerAsync(ctx);

            _logger.LogInformation("Job scheduler stopped");
        }

        private async Task RecoverJobExecutionsAsync(CancellationToken ctx)
        {
            using var scope = _scopeFactory.CreateScope();

            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var recovered = await mediator.Send(new RecoverJobExecutionsRequest(), ctx);
            
            _logger.LogInformation("Recovered {NumExecutions} executions", recovered);
        }

        private async Task RunSchedulerAsync(CancellationToken ctx)
        {
            while (!ctx.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(3), ctx);

                // Scope this part due to DbContext disposing etc.
                using var scope = _scopeFactory.CreateScope();

                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                var result = await mediator.Send(new SchedulePendingRequest(), ctx);

                _logger.LogInformation("Queued {NumbJobs} jobs", result.NumScheduled);
            }
        }
    }
}