using System;

namespace Rescheduler.Core.Entities
{
    public class ScheduledJob : EntityBase
    {
        internal ScheduledJob(Guid id, DateTime runAt, DateTime? scheduledAt, ScheduleStatus status, Job job)
            : base(id)
        {
            ScheduledAt = runAt;
            QueuedAt = scheduledAt;
            Status = status;
            Job = job;
        }

        public DateTime ScheduledAt { get; private set; }

        public DateTime? QueuedAt { get; private set; }

        public ScheduleStatus Status { get; private set; }

        public Job Job { get; private set; }

        internal void Scheduled() => Status = ScheduleStatus.Scheduled;

        internal void InFlight() => Status = ScheduleStatus.InFlight;

        internal void Queued(DateTime queuedAt) 
        {
            QueuedAt = queuedAt;
            Status = ScheduleStatus.Queued;
        }

        public static ScheduledJob New(Job job, DateTime runAt)
        {
            return new ScheduledJob(
                Guid.NewGuid(), 
                runAt, 
                null, 
                ScheduleStatus.Scheduled,
                job);
        }
    }
}