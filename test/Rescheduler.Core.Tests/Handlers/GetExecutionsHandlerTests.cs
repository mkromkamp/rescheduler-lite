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
    public class GetExecutionsHandlerTests
    {
        private readonly IRepository<JobExecution> _jobExecutionRepository;

        private readonly GetExecutionsHandler _handler;

        public GetExecutionsHandlerTests()
        {
            _jobExecutionRepository = Mock.Of<IRepository<JobExecution>>();

            _handler = new GetExecutionsHandler(_jobExecutionRepository);
        }

        [Fact]
        public async Task GivenOneExecution_WhenHandling_ShouldReturn()
        {
            // Given
            var request = new GetExecutionsRequest(Enumerable.Empty<ExecutionStatus>(), "subject", 10, 0);
            Mock.Get(_jobExecutionRepository)
                .Setup(x => x.GetManyAsync(It.IsAny<Func<IQueryable<JobExecution>, IQueryable<JobExecution>>>(), CancellationToken.None))
                .ReturnsAsync(new List<JobExecution>
                {
                    JobExecution.New(Job.New("subject", "payload", true, DateTime.UtcNow, DateTime.UtcNow, null), DateTime.UtcNow)
                }.AsReadOnly());

            // When
            var result = await _handler.Handle(request, CancellationToken.None);

            // Then
            result.Executions.ShouldHaveSingleItem();
        }
        
        [Fact]
        public async Task GivenTwoExecution_WhenHandling_ShouldReturn()
        {
            // Given
            var request = new GetExecutionsRequest(Enumerable.Empty<ExecutionStatus>(), "subject", 10, 0);
            Mock.Get(_jobExecutionRepository)
                .Setup(x => x.GetManyAsync(It.IsAny<Func<IQueryable<JobExecution>, IQueryable<JobExecution>>>(), CancellationToken.None))
                .ReturnsAsync(new List<JobExecution>
                {
                    JobExecution.New(Job.New("subject", "payload", true, DateTime.UtcNow, DateTime.UtcNow, null), DateTime.UtcNow),
                    JobExecution.New(Job.New("subject", "payload", true, DateTime.UtcNow, DateTime.UtcNow, null), DateTime.UtcNow)
                }.AsReadOnly());

            // When
            var result = await _handler.Handle(request, CancellationToken.None);

            // Then
            result.Executions.Count().ShouldBe(2);
        }
    }
}