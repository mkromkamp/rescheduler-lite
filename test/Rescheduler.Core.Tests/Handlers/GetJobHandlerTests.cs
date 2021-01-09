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
    public class GetJobHandlerTests
    {
        private readonly IRepository<Job> _jobRepository;

        private readonly GetJobHandler _handler;

        public GetJobHandlerTests()
        {
            _jobRepository = Mock.Of<IRepository<Job>>();

            _handler = new GetJobHandler(_jobRepository);
        }

        [Fact]
        public async Task GivenExistingJob_WhenHandling_ShouldReturnJob()
        {
            // Given
            var job = Job.New("subject", "test payload", true, DateTime.UtcNow, DateTime.UtcNow.AddYears(1), "*/10 * * * *");
            Mock.Get(_jobRepository)
                .Setup(x => x.GetByIdAsync(job.Id, CancellationToken.None))
                .ReturnsAsync(job);

            // When
            var getJobResponse = await _handler.Handle(new GetJobRequest(job.Id), CancellationToken.None);

            // Then
            getJobResponse.job.ShouldNotBeNull();
            getJobResponse.job.ShouldBe(job);
        }

        [Fact]
        public async Task GivenNonExistingJob_WhenHandling_ShouldNotReturnJob()
        {
            // Given
            var job = Job.New("subject", "test payload", true, DateTime.UtcNow, DateTime.UtcNow.AddYears(1), "*/10 * * * *");
            Mock.Get(_jobRepository)
                .Setup(x => x.GetByIdAsync(job.Id, CancellationToken.None))
                .ReturnsAsync((Job)null);

            // When
            var getJobResponse = await _handler.Handle(new GetJobRequest(job.Id), CancellationToken.None);

            // Then
            getJobResponse.job.ShouldBeNull();
        }
    }
}