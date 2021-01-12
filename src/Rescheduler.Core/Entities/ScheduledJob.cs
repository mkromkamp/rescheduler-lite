using System;

namespace Rescheduler.Core.Entities
{
    public class ScheduledJob : EntityBase
    {
        private ScheduledJob() : base() {}

        internal ScheduledJob(Guid id, DateTime scheduledAt, DateTime? queuedAt, ScheduleStatus status, Guid jobId)
            : base(id)
        {
            ScheduledAt = scheduledAt;
            QueuedAt = queuedAt;
            Status = status;
            JobId = jobId;
        }

        public DateTime ScheduledAt { get; private set; }

        public DateTime? QueuedAt { get; private set; }

        public ScheduleStatus Status { get; private set; }

        public Guid JobId { get; private set; }

        public Job Job { get; private set; } = default!;

        public void Scheduled() => Status = ScheduleStatus.Scheduled;

        public void InFlight() => Status = ScheduleStatus.InFlight;

        public void Queued(DateTime queuedAt) 
        {
            QueuedAt = queuedAt;
            Status = ScheduleStatus.Queued;
        }
        
        public static ScheduledJob New(Guid jobId, DateTime scheduledAt)
        {
            return new ScheduledJob(
                Guid.NewGuid(), 
                scheduledAt, 
                null,
                ScheduleStatus.Scheduled,
                jobId);
        }
    }
}