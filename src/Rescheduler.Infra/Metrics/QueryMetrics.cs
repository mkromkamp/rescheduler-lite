using Prometheus;

namespace Rescheduler.Infra.Metrics
{
    internal class QueryMetrics
    {
        private static readonly Counter ExecutedTotal = 
            Prometheus.Metrics.CreateCounter(
                    "sched_query_executions_total", 
                    "Number of executed queries.", 
                    new CounterConfiguration
                    {
                        LabelNames = new[] { "type", "method" }
                    });

        public static void CountExecution(string type, string method)
        {
            ExecutedTotal.WithLabels(type, method).Inc();
        }
    }
}