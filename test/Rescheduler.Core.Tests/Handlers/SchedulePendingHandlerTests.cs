using System;
using System.Collections.Generic;
using System.Linq;
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
    public class SchedulePendingHandlerTests
    {
        private readonly IJobPublisher _jobPublisher;
        private readonly IRepository<JobExecution> _jobExecutionRepo;
        private readonly IJobExecutionRepository _jobExecutionRepository;

        private readonly SchedulePendingHandler _handler;

        public SchedulePendingHandlerTests()
        {
            _jobPublisher = Mock.Of<IJobPublisher>();
            _jobExecutionRepo = Mock.Of<IRepository<JobExecution>>();
            _jobExecutionRepository = Mock.Of<IJobExecutionRepository>();

            Mock.Get(_jobPublisher)
                .Setup(x => x.PublishManyAsync(It.IsAny<IEnumerable<JobExecution>>(), CancellationToken.None))
                .ReturnsAsync(true);

            _handler = new SchedulePendingHandler(_jobExecutionRepository, _jobExecutionRepo, _jobPublisher);
        }

        [Fact]
        public async Task GivenNoPendingExecutions_WhenHandling_ShouldReturnZeroScheduled()
        {
            // Given
            var request = new SchedulePendingRequest();
            Mock.Get(_jobExecutionRepository)
                .Setup(x => x.GetAndMarkPending(It.IsAny<int>(), It.IsAny<DateTime>(), CancellationToken.None))
                .ReturnsAsync(Enumerable.Empty<JobExecution>());
            
            // When
            var result = await _handler.Handle(request, CancellationToken.None);
            
            // Then
            result.NumScheduled.ShouldBe(0);
        }
        
        [Fact]
        public async Task GivenNoPendingExecutions_WhenHandling_ShouldNotUpdateExecutions()
        {
            // Given
            var request = new SchedulePendingRequest();
            Mock.Get(_jobExecutionRepository)
                .Setup(x => x.GetAndMarkPending(It.IsAny<int>(), It.IsAny<DateTime>(), CancellationToken.None))
                .ReturnsAsync(Enumerable.Empty<JobExecution>());
            
            // When
            await _handler.Handle(request, CancellationToken.None);
            
            // Then
            Mock.Get(_jobExecutionRepo)
                .Verify(x => x.UpdateManyAsync(It.IsAny<IEnumerable<JobExecution>>(), CancellationToken.None), 
                    Times.Never);
        }
        
        [Fact]
        public async Task GivenNoPendingExecutions_WhenHandling_ShouldNotQueueExecutions()
        {
            // Given
            var request = new SchedulePendingRequest();
            Mock.Get(_jobExecutionRepository)
                .Setup(x => x.GetAndMarkPending(It.IsAny<int>(), It.IsAny<DateTime>(), CancellationToken.None))
                .ReturnsAsync(Enumerable.Empty<JobExecution>());
            
            // When
            await _handler.Handle(request, CancellationToken.None);
            
            // Then
            Mock.Get(_jobPublisher)
                .Verify(x => x.PublishManyAsync(It.IsAny<IEnumerable<JobExecution>>(), CancellationToken.None), 
                    Times.Never);
        }
        
        [Fact]
        public async Task GivenTwoPendingExecutions_WhenHandling_ShouldReturnTwoScheduled()
        {
            // Given
            var request = new SchedulePendingRequest();
            var executions = new[]
            {
                JobExecution.New(Job.New("test", "test payload", true, DateTime.UtcNow, DateTime.UtcNow, "* * * * *"),
                    DateTime.UtcNow),
                JobExecution.New(Job.New("test", "test payload", true, DateTime.UtcNow, DateTime.UtcNow, "* * * * *"),
                    DateTime.UtcNow),
            };
            Mock.Get(_jobExecutionRepository)
                .SetupSequence(x => x.GetAndMarkPending(It.IsAny<int>(), It.IsAny<DateTime>(), CancellationToken.None))
                .ReturnsAsync(executions)
                .ReturnsAsync(Enumerable.Empty<JobExecution>());
            
            // When
            var result = await _handler.Handle(request, CancellationToken.None);
            
            // Then
            result.NumScheduled.ShouldBe(2);
        }
        
        [Fact]
        public async Task GivenTwoPendingExecutions_WhenHandling_ShouldUpdateScheduled()
        {
            // Given
            var request = new SchedulePendingRequest();
            var executions = new[]
            {
                JobExecution.New(Job.New("test", "test payload", true, DateTime.UtcNow, DateTime.UtcNow, "* * * * *"),
                    DateTime.UtcNow),
                JobExecution.New(Job.New("test", "test payload", true, DateTime.UtcNow, DateTime.UtcNow, "* * * * *"),
                    DateTime.UtcNow),
            };
            Mock.Get(_jobExecutionRepository)
                .SetupSequence(x => x.GetAndMarkPending(It.IsAny<int>(), It.IsAny<DateTime>(), CancellationToken.None))
                .ReturnsAsync(executions)
                .ReturnsAsync(Enumerable.Empty<JobExecution>());

            // When
            await _handler.Handle(request, CancellationToken.None);
            
            // Then
            Mock.Get(_jobExecutionRepo)
                .Verify(x => x.UpdateManyAsync(It.IsAny<IEnumerable<JobExecution>>(), CancellationToken.None), 
                    Times.Once);
        }
        
        [Fact]
        public async Task GivenTwoPendingExecutions_WhenHandling_ShouldQueueScheduled()
        {
            // Given
            var request = new SchedulePendingRequest();
            var executions = new[]
            {
                JobExecution.New(Job.New("test", "test payload", true, DateTime.UtcNow, DateTime.UtcNow, "* * * * *"),
                    DateTime.UtcNow),
                JobExecution.New(Job.New("test", "test payload", true, DateTime.UtcNow, DateTime.UtcNow, "* * * * *"),
                    DateTime.UtcNow),
            };
            Mock.Get(_jobExecutionRepository)
                .SetupSequence(x => x.GetAndMarkPending(It.IsAny<int>(), It.IsAny<DateTime>(), CancellationToken.None))
                .ReturnsAsync(executions)
                .ReturnsAsync(Enumerable.Empty<JobExecution>());
            
            // When
            await _handler.Handle(request, CancellationToken.None);
            
            // Then
            Mock.Get(_jobPublisher)
                .Verify(x => x.PublishManyAsync(It.IsAny<IEnumerable<JobExecution>>(), CancellationToken.None), 
                    Times.Once);
        }
        
        [Fact]
        public async Task GivenTwoPendingExecutions_WhenFailingToPublish_ShouldReScheduled()
        {
            // Given
            var request = new SchedulePendingRequest();
            var executions = new[]
            {
                JobExecution.New(Job.New("test", "test payload", true, DateTime.UtcNow, DateTime.UtcNow, "* * * * *"),
                    DateTime.UtcNow),
                JobExecution.New(Job.New("test", "test payload", true, DateTime.UtcNow, DateTime.UtcNow, "* * * * *"),
                    DateTime.UtcNow),
            };
            Mock.Get(_jobExecutionRepository)
                .SetupSequence(x => x.GetAndMarkPending(It.IsAny<int>(), It.IsAny<DateTime>(), CancellationToken.None))
                .ReturnsAsync(executions)
                .ReturnsAsync(Enumerable.Empty<JobExecution>());
            Mock.Get(_jobPublisher)
                .Setup(x => x.PublishManyAsync(It.IsAny<IEnumerable<JobExecution>>(), CancellationToken.None))
                .ReturnsAsync(false);
            
            // When
            await _handler.Handle(request, CancellationToken.None);
            
            // Then
            Mock.Get(_jobExecutionRepo)
                .Verify(x => x.UpdateManyAsync(It.IsAny<IEnumerable<JobExecution>>(), CancellationToken.None), 
                    Times.Once);
        }
    }
}