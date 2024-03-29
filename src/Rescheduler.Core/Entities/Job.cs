using Cronos;

namespace Rescheduler.Core.Entities;

public class Job : EntityBase
{
    internal Job(Guid id, string subject, string payload, bool enabled, DateTime runAt, DateTime stopAfter, string? cron)
        : base(id)
    {
        Subject = subject;
        Payload = payload;
        Enabled = enabled;
        RunAt = runAt;
        StopAfter = stopAfter;
        Cron = cron;
    }

    public string Subject { get; private set; }

    public string Payload { get; private set; }

    public bool Enabled { get; private set; }

    public DateTime RunAt { get; private set; }

    public DateTime StopAfter { get; private set; }

    public string? Cron { get; private set; }

    public bool TryGetNextRun(DateTime from, out DateTime? next) 
    {
        next = from;

        // from is before the first run time
        if (from < RunAt)
        {
            next = RunAt;
        }
            
        // Cron scheduled
        if (!string.IsNullOrEmpty(Cron))
        {
            var expression = CronExpression.Parse(Cron);
            next = expression.GetNextOccurrence(next.Value, true);
        }

        if (next > StopAfter)
            next = null;

        return next != null;
    }

    public static Job New(string subject, string payload, bool enabled, DateTime runAt, DateTime stopAfter, string? cron)
    {
        return new Job(
            Guid.NewGuid(),
            subject,
            payload,
            enabled,
            runAt,
            stopAfter,
            cron
        );
    }
}