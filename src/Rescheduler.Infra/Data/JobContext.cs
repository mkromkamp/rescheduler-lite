using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Rescheduler.Core.Entities;

namespace Rescheduler.Infra.Data;

public class JobContext : DbContext
{
    public JobContext() {}

    public JobContext(DbContextOptions<JobContext> options)
        : base(options)
    { }

    public DbSet<Job> Jobs { get; set; } = default!;

    public DbSet<JobExecution> JobExecutions { get; set; } = default!;

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.EnableDetailedErrors();
        options.EnableSensitiveDataLogging();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetAssembly(typeof(JobContext)));

        modelBuilder.Entity<Job>()
            .HasMany<JobExecution>()
            .WithOne(j => j.Job)
            .OnDelete(DeleteBehavior.Cascade);
    }
}