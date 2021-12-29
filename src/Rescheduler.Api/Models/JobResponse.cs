using Rescheduler.Core.Entities;

namespace Rescheduler.Api.Models;

public record JobResponse
{
    public JobResponse(Guid id, string subject, string payload, bool enabled, DateTime runAt, DateTime stopAfter, string? cron)
    {
        Id = id;
        Subject = subject;
        Payload = payload;
        Enabled = enabled;
        RunAt = runAt;
        StopAfter = stopAfter;
        Cron = cron;
    }

    /// <summary>
    /// The unique identifier of this job
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// The subject of this job
    /// </summary>
    public string Subject { get; }

    /// <summary>
    /// The payload of this job
    /// </summary>
    public string Payload { get; }

    /// <summary>
    /// If this job is currently enabled
    /// </summary>
    public bool Enabled { get; }

    /// <summary>
    /// The first time this job is configured to run
    /// </summary>
    public DateTime RunAt { get; }

    /// <summary>
    /// The last time this job is configured to run
    /// </summary>
    public DateTime StopAfter { get; }

    /// <summary>
    /// The cron schedule of this job
    /// </summary>
    public string? Cron { get; }

    public static JobResponse From(Job job)
    {
        return new(
            job.Id,
            job.Subject,
            job.Payload,
            job.Enabled,
            job.RunAt,
            job.StopAfter,
            job.Cron
        );
    }
}