using System;
using Rescheduler.Core.Entities;

namespace Rescheduler.Api.Models
{
    public class JobExecutionResponse
    {
        public JobExecutionResponse(Guid id, DateTime scheduledAt, DateTime? queuedAt, string status)
        {
            Id = id;
            ScheduledAt = scheduledAt;
            QueuedAt = queuedAt;
            Status = status;
        }
        
        /// <summary>
        /// The unique identifier of this job execution
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// The date and time this execution will be ran
        /// </summary>
        public DateTime ScheduledAt { get; }

        /// <summary>
        /// If the execution is queued, the time the it was queued 
        /// </summary>
        public DateTime? QueuedAt { get; }

        /// <summary>
        /// The current status of the execution
        /// </summary>
        public string Status { get; }

        public static JobExecutionResponse From(JobExecution jobExecution)
        {
            return new(
                jobExecution.Id,
                jobExecution.ScheduledAt,
                jobExecution.QueuedAt,
                jobExecution.Status.ToString()
            );
        }
    }
}