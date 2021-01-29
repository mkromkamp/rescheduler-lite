using System.Security.Cryptography.X509Certificates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rescheduler.Core.Entities;

namespace Rescheduler.Infra.Data.Config
{
    public class ScheduledJobConfiguration : IEntityTypeConfiguration<JobExecution>
    {
        public void Configure(EntityTypeBuilder<JobExecution> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasIndex(x => new { x.QueuedAt, x.Status });
        }
    }
}