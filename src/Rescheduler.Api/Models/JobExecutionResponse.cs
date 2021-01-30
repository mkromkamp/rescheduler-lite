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

        public Guid Id { get; private set; }

        public DateTime ScheduledAt { get; private set; }

        public DateTime? QueuedAt { get; private set; }

        public string Status { get; private set; }

        public static JobExecutionResponse From(JobExecution jobExecution)
        {
            return new JobExecutionResponse (
                jobExecution.Id,
                jobExecution.ScheduledAt,
                jobExecution.QueuedAt,
                jobExecution.Status.ToString()
            );
        }
    }
}