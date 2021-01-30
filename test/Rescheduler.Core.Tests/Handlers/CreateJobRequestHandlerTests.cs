using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Rescheduler.Core.Entities;
using Rescheduler.Core.Handlers;
using Rescheduler.Core.Interfaces;
using Shouldly;
using Xunit;

namespace Rescheduler.Core.Tests.Handlers
{
    public class CreateJobRequestHandlerTests
    {
        private readonly IRepository<Job> _jobRepository;
        private readonly IRepository<JobExecution> _jobExecutionRepository;

        private readonly CreateJobHandler _handler;

        public CreateJobRequestHandlerTests()
        {
            _jobRepository = Mock.Of<IRepository<Job>>();

            _jobExecutionRepository = Mock.Of<IRepository<JobExecution>>();

            _handler = new CreateJobHandler(_jobRepository, _jobExecutionRepository);
        }

        [Fact]
        public async Task GivenEnabledJob_WhenHandling_ShouldStoreJob()
        {
            // Given
            var enabled = true;
            var job = Job.New("test", "test payload", enabled, DateTime.UtcNow, DateTime.UtcNow.AddYears(1), "*/10 * * * *");

            // When
            var createJobResponse = await _handler.Handle(new CreateJobRequest(job), CancellationToken.None);

            // Then
            createJobResponse.job.ShouldNotBeNull();
            Mock.Get(_jobRepository)
                .Verify(x => x.AddAsync(job, CancellationToken.None),
                Times.Once);
        }

        [Fact]
        public async Task GivenDisabledJob_WhenHandling_ShouldStoreJob()
        {
            // Given
            var enabled = false;
            var job = Job.New("test", "test payload", enabled, DateTime.UtcNow, DateTime.UtcNow.AddYears(1), "*/10 * * * *");

            // When
            var createJobResponse = await _handler.Handle(new CreateJobRequest(job), CancellationToken.None);

            // Then
            createJobResponse.job.ShouldNotBeNull();
            Mock.Get(_jobRepository)
                .Verify(x => x.AddAsync(job, CancellationToken.None),
                Times.Once);
        }

        [Fact]
        public async Task GivenJobWithNextRun_WhenHandling_ShouldStoreJobExecution()
        {
            // Given
            var enabled = true;
            var job = Job.New("test", "test payload", enabled, DateTime.UtcNow, DateTime.UtcNow.AddYears(1), "*/10 * * * *");

            // When
            var createJobResponse = await _handler.Handle(new CreateJobRequest(job), CancellationToken.None);

            // Then
            createJobResponse.job.ShouldNotBeNull();
            createJobResponse.firstScheduledRun.ShouldNotBeNull();
            Mock.Get(_jobExecutionRepository)
                .Verify(x => x.AddAsync(It.IsAny<JobExecution>(), CancellationToken.None),
                Times.Once);
        }

        [Fact]
        public async Task GivenDisabledJobWithNextRun_WhenHandling_ShouldNotStoreJobExecution()
        {
            // Given
            var enabled = false;
            var job = Job.New("test", "test payload", enabled, DateTime.UtcNow, DateTime.UtcNow.AddYears(1), "*/10 * * * *");

            // When
            var createJobResponse = await _handler.Handle(new CreateJobRequest(job), CancellationToken.None);

            // Then
            createJobResponse.job.ShouldNotBeNull();
            createJobResponse.firstScheduledRun.ShouldBeNull();
            Mock.Get(_jobExecutionRepository)
                .Verify(x => x.AddAsync(It.IsAny<JobExecution>(), CancellationToken.None),
                Times.Never);
        }


        [Fact]
        public async Task GivenJobWithoutNextRun_WhenHandling_ShouldNotStoreJobExecution()
        {
            // Given
            var enabled = true;
            var job = Job.New("test", "test payload", enabled, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddYears(-1), "*/10 * * * *");

            // When
            var createJobResponse = await _handler.Handle(new CreateJobRequest(job), CancellationToken.None);

            // Then
            createJobResponse.job.ShouldNotBeNull();
            createJobResponse.firstScheduledRun.ShouldBeNull();
            Mock.Get(_jobExecutionRepository)
                .Verify(x => x.AddAsync(It.IsAny<JobExecution>(), CancellationToken.None),
                Times.Never);
        }
    }
}