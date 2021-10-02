using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using Rescheduler.Core.Entities;
using Rescheduler.Infra.Data;

namespace Rescheduler.Infra.Tests.Data.JobExecutionsRepository
{
    public class JobExecutionsRepositoryTests
    {
        internal readonly ILogger<Repository<JobExecution>> _logger;

        internal JobExecutionsRepositoryTests()
        {
            _logger = Mock.Of<ILogger<Repository<JobExecution>>>();
        }

        internal static JobContext GetSeededJobContext(Job job, JobExecution jobExecution)
        {
            var contextOptions = new DbContextOptionsBuilder<JobContext>()
                .UseInMemoryDatabase("test")
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            
            var context = new JobContext(contextOptions);

            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            context.Add(job);
            context.Add(jobExecution);

            context.SaveChanges();

            return context;
        }
    }
}