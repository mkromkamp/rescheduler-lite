using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Rescheduler.Core.Entities;

namespace Rescheduler.Infra.Data
{
    public class JobContext : DbContext
    {
        public JobContext() : base() {}

        public JobContext(DbContextOptions<JobContext> options)
            : base(options)
        { }

        public DbSet<Job> Jobs { get; set; } = default!;

        public DbSet<ScheduledJob> ScheduledJobs { get; set; } = default!;

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.EnableDetailedErrors();
            options.EnableSensitiveDataLogging();
            options.LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetAssembly(typeof(JobContext)));

            modelBuilder.Entity<Job>()
                .HasMany<ScheduledJob>()
                .WithOne(j => j.Job)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}