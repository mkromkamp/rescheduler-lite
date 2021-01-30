using System;
using Rescheduler.Core.Entities;

namespace Rescheduler.Api.Models
{
    public class JobResponse
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

        public Guid Id { get; private set; }

        public string Subject { get; private set; }

        public string Payload { get; private set; }

        public bool Enabled { get; private set; }

        public DateTime RunAt { get; private set; }

        public DateTime StopAfter { get; private set; }

        public string? Cron { get; private set; }

        public static JobResponse From(Job job)
        {
            return new JobResponse(
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
}