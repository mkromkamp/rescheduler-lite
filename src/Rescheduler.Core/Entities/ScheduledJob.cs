using System;

namespace Rescheduler.Core.Entities
{
    public class ScheduledJob : EntityBase
    {
        // EF Core
        private ScheduledJob() : base() {}

        internal ScheduledJob(Guid id, DateTime scheduledAt, DateTime? queuedAt, ScheduleStatus status, Job job)
            : base(id)
        {
            ScheduledAt = scheduledAt;
            QueuedAt = queuedAt;
            Status = status;
            Job = job;
        }

        public DateTime ScheduledAt { get; private set; }

        public DateTime? QueuedAt { get; private set; }

        public ScheduleStatus Status { get; private set; }

        public Job Job { get; private set; }

        public void Scheduled() => Status = ScheduleStatus.Scheduled;

        public void InFlight() => Status = ScheduleStatus.InFlight;

        public void Queued(DateTime queuedAt) 
        {
            QueuedAt = queuedAt;
            Status = ScheduleStatus.Queued;
        }
        
        public static ScheduledJob New(Job job, DateTime scheduledAt)
        {
            return new ScheduledJob(
                Guid.NewGuid(), 
                scheduledAt, 
                null,
                ScheduleStatus.Scheduled,
                job);
        }
    }
}