using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Rescheduler.Core.Entities;

namespace Rescheduler.Infra.Data
{
    public class JobContext : DbContext
    {
        public JobContext() : base()
        { }

        public JobContext(DbContextOptions<JobContext> options)
            : base(options)
        { }

        public DbSet<Job> Jobs { get; set; }

        public DbSet<ScheduledJob> ScheduledJobs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite("Filename=:memory:") // Data Source=rescheduler.db
            .LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information);

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetAssembly(typeof(JobContext)));

            modelBuilder.Entity<Job>()
                .HasMany<ScheduledJob>()
                .WithOne()
                .HasForeignKey(x => x.JobId)
                .HasPrincipalKey(x => x.Id)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}