using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Rescheduler.Core.Entities;

namespace Rescheduler.Infra.Data
{
    internal class JobContext : DbContext
    {
        public DbSet<Job> Jobs { get; set; }

        public DbSet<ScheduledJob> ScheduledJobs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite(":memory:"); // Data Source=rescheduler.db

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