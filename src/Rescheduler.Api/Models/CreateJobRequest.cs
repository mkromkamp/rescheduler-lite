using System;
using System.ComponentModel.DataAnnotations;
using Rescheduler.Core.Entities;

namespace Rescheduler.Api.Models
{
    public class CreateJobRequest
    {
        [Required]
        public string? Subject { get; set; }

        [Required]
        public string? Payload { get; set; }

        [Required]
        public DateTime RunAt { get; set; }

        public DateTime StopAfter { get; set; }

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