using System;

namespace Rescheduler.Core.Entities
{
    public class JobExecution : EntityBase
    {
        // EF Core
        #pragma warning disable CS8618
        private JobExecution() : base() {}
        #pragma warning restore CS8618

        internal JobExecution(Guid id, DateTime scheduledAt, DateTime? queuedAt, ExecutionStatus status, Job job)
            : base(id)
        {
            ScheduledAt = scheduledAt;
            QueuedAt = queuedAt;
            Status = status;
            Job = job;
        }

        public DateTime ScheduledAt { get; private set; }

        public DateTime? QueuedAt { get; private set; }

        public ExecutionStatus Status { get; private set; }

        public Job Job { get; private set; }

        public void Scheduled() => Status = ExecutionStatus.Scheduled;

        public void InFlight() => Status = ExecutionStatus.InFlight;

        public void Queued(DateTime queuedAt) 
        {
            QueuedAt = queuedAt;
            Status = ExecutionStatus.Queued;
        }
        
        public static JobExecution New(Job job, DateTime scheduledAt)
        {
            return new JobExecution(
                Guid.NewGuid(), 
                scheduledAt, 
                null,
                ExecutionStatus.Scheduled,
                job);
        }
    }
}