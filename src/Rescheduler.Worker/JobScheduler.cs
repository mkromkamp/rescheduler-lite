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
            using var logScope = _logger.BeginScope("{service}", "JobScheduler");

            _logger.LogInformation("Job scheduler started");

            while (!ctx.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(30), ctx);

                // Scope this part due to DbContext disposing etc.
                using var scope = _scopeFactory.CreateScope();
                
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                var result = await mediator.Send(new SchedulePendingRequest(20, DateTime.UtcNow.AddSeconds(10)), ctx);

                _logger.LogInformation("Queued {numbJobs} jobs", result.NumScheduled);
            }

            _logger.LogInformation("Job scheduler stopped");
        }
    }
}