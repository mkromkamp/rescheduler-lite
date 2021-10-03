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
    public class CompactionWorker : BackgroundService
    {
        private readonly ILogger _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly TimeSpan _compactBefore = TimeSpan.FromMinutes(3);

        public CompactionWorker(ILogger<CompactionWorker> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        protected override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                // Wait for the service to start and apply pending db migrations
                Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
                return RunCompaction(cancellationToken);
            }, cancellationToken);
        }

        internal async Task RunCompaction(CancellationToken ctx)
        {
            using var logScope = _logger.BeginScope("{Service}", "CompactionWorker");

            while (!ctx.IsCancellationRequested)
            {
                var scope = _scopeFactory.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                await using (var _ = _logger.Time(LogLevel.Information, "Job compaction"))
                {
                    await mediator.Publish(new CompactionRequest(DateTime.UtcNow.Subtract(_compactBefore)), ctx);
                }

                // Wait delay after first run so compaction is performed at startup
                await Task.Delay(_compactBefore, ctx);
            }
        }
    }
}