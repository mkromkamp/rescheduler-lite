using Prometheus;

namespace Rescheduler.Infra.Metrics
{
    internal class QueryMetrics
    {
        private static readonly Histogram QueryDuration = 
            Prometheus.Metrics.CreateHistogram(
                    "sched_query_duration_seconds", 
                    "Histogram of query durations.",
                    new HistogramConfiguration
                    {
                        LabelNames = new [] { "type", "method" },
                        Buckets = Prometheus.Histogram.ExponentialBuckets(0.01, 2, 10)
                    });

        public static ITimer TimeQuery(string type, string method)
        {
            return QueryDuration.WithLabels(type, method).NewTimer();
        }
    }
}