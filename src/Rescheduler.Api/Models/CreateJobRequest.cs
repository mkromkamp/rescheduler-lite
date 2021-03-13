using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Rescheduler.Core.Entities;

namespace Rescheduler.Api.Models
{
    public class CreateJobRequest
    {
        /// <summary>
        /// The subject of the job used when scheduling the job
        /// </summary>
        [Required, NotNull]
        public string? Subject { get; init; }

        /// <summary>
        /// The payload of this job
        /// </summary>
        [Required, NotNull]
        public string? Payload { get; init; }

        /// <summary>
        /// The first time the job should be scheduled
        /// </summary>
        [Required]
        public DateTime RunAt { get; set; }

        /// <summary>
        /// Optionally, the time after which the job should not be scheduled anymore
        /// only used when a cron schedule is supplied
        /// </summary>
        public DateTime StopAfter { get; set; }

        /// <summary>
        /// Optionally, the cron schedule for this job 
        /// </summary>
        public string? Cron { get; set; }

        public Job ToJob()
        {
            return Job.New(
                Subject,
                Payload,
                true,
                RunAt,
                StopAfter,
                Cron
            );
        }
    }
}