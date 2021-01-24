using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Prometheus;

namespace Rescheduler.Infra.Metrics
{
    internal class MetricsBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private static readonly Counter RequestProcessedCount = 
            Prometheus.Metrics.CreateCounter(
                "sched_requested_processed_total", 
                "Number of processed requests.", 
                new CounterConfiguration
                {
                    LabelNames = new[] { "method", "success" }
                });

        public MetricsBehavior()
        {
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            var requestType = request?.GetType().Name.ToLowerInvariant() ?? "unknown";

            try
            {
                var result = await next();
                
                RequestProcessedCount.WithLabels(requestType, "true").Inc();
                
                return result;
            }
            catch (System.Exception)
            {
                RequestProcessedCount.WithLabels(requestType, "false").Inc();
                throw;
            }
        }
    }
}