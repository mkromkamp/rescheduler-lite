using System.Diagnostics.Metrics;

namespace Rescheduler.Infra.Metrics;

internal interface IMessagingMetrics
{
    void MessagesPublished(string subject, int numPublished = 1);
    void TimePublishDuration(TimeSpan timeSpan);
    void TimePublishBatchDuration(TimeSpan timeSpan);
}

internal class MessagingMetrics : IMessagingMetrics
{
    private readonly Counter<int> _messagesPublishedTotal;
    private readonly Histogram<double> _messagePublishDurationSeconds;

    public MessagingMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create("Rescheduler");
        _messagesPublishedTotal = meter.CreateCounter<int>
        (
            "messages.published.total", 
            description: "Total number of tasks published"
        );
        _messagePublishDurationSeconds = meter.CreateHistogram<double>
        (
            "messages.publish.duration", 
            unit: "second",
            description: "Duration to publish tasks"
        );
    }

    public void MessagesPublished(string subject, int numPublished = 1)
    {
        _messagesPublishedTotal.Add
        (
            numPublished, 
            new KeyValuePair<string, object?>("subject", subject)
        );
    }
    
    public void TimePublishDuration(TimeSpan timeSpan)
    {
        _messagePublishDurationSeconds.Record
        (
            timeSpan.TotalSeconds, 
            new KeyValuePair<string, object?>("operation", "single")
        );
    }
    
    public void TimePublishBatchDuration(TimeSpan timeSpan)
    {
        _messagePublishDurationSeconds.Record
        (
            timeSpan.TotalSeconds, 
            new KeyValuePair<string, object?>("operation", "batch")
        );
    }
}