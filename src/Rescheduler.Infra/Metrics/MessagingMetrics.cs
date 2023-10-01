using Prometheus;
using ITimer = Prometheus.ITimer;

namespace Rescheduler.Infra.Metrics;

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

    private static readonly Histogram MessagePublishDuration =
        Prometheus.Metrics.CreateHistogram(
            "sched_message_publish_duration",
            "Duration to publish messages",
            new HistogramConfiguration
            {
                LabelNames = new [] { "operation" },
            }
        );

    public static void MessagesPublished(string subject, int numPublished = 1)
    {
        MessagesPublishedTotal.WithLabels(subject).Inc(numPublished);
    }

    public static ITimer TimePublishDuration()
    {
        return MessagePublishDuration.WithLabels("single").NewTimer();
    }

    public static ITimer TimeBatchPublishDuration()
    {
        return MessagePublishDuration.WithLabels("batch").NewTimer();
    }
}