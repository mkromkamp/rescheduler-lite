using Prometheus;

namespace Rescheduler.Infra.Metrics
{
    internal class MessagingMetrics
    {
        private static readonly Counter MessagesPublishedTotal = 
            Prometheus.Metrics.CreateCounter(
                    "sched_messages_published_count", 
                    "Total messages published",
                    new CounterConfiguration
                    {
                        LabelNames = new [] { "subject" },
                    });

        public static void MessagesPublished(string subject, int numPublished = 1)
        {
            MessagesPublishedTotal.WithLabels(subject).Inc(numPublished);
        }
    }
}