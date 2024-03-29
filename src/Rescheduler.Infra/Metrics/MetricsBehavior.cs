using MediatR;
using Prometheus;

namespace Rescheduler.Infra.Metrics;

internal class MetricsBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> 
    where TRequest : IRequest<TResponse>
    where TResponse : class
{
    private static readonly Counter RequestProcessedCount = 
        Prometheus.Metrics.CreateCounter(
            "sched_requested_processed_total", 
            "Number of processed requests.", 
            new CounterConfiguration
            {
                LabelNames = new[] { "method", "success" }
            });

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestType = request.GetType().Name.ToLowerInvariant();

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