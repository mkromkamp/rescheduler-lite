using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Rescheduler.Api.Controllers;
using Rescheduler.Core.Entities;
using Rescheduler.Core.Handlers;
using Shouldly;
using Xunit;

namespace Rescheduler.Api.Tests.Controllers
{
    public class JobExecutionControllerTests
    {
        private readonly IMediator _mediator;

        private JobExecutionController _controller;

        public JobExecutionControllerTests()
        {
            _mediator = Mock.Of<IMediator>();

            _controller = new JobExecutionController(_mediator);
        }
        
        [Fact]
        public async Task GivenExistingId_WhenGettingAll_ShouldReturn()
        {
            // Given
            var jobExecution = JobExecution.New(Job.New("subject", "payload", true, DateTime.UtcNow, DateTime.UtcNow, null), DateTime.UtcNow);
            Mock.Get(_mediator)
                .Setup(x => x.Send(It.IsAny<GetExecutionsRequest>(), CancellationToken.None))
                .ReturnsAsync(new GetExecutionsResponse(new []{jobExecution}));

            // When
            var result = await _controller.GetAllAsync(Enumerable.Empty<ExecutionStatus>(), null, CancellationToken.None);
            var actionResult = result as OkObjectResult;

            // Then
            result.ShouldBeAssignableTo<OkObjectResult>();
            actionResult.StatusCode.ShouldBe(StatusCodes.Status200OK);
        }
        
        [Fact]
        public async Task GivenNonExistingId_WhenGettingAll_ShouldReturn()
        {
            // Given
            Mock.Get(_mediator)
                .Setup(x => x.Send(It.IsAny<GetExecutionsRequest>(), CancellationToken.None))
                .ReturnsAsync(new GetExecutionsResponse(Enumerable.Empty<JobExecution>()));

            // When
            var result = await _controller.GetAllAsync(Enumerable.Empty<ExecutionStatus>(), null, CancellationToken.None);
            var actionResult = result as NotFoundResult;

            // Then
            result.ShouldBeAssignableTo<NotFoundResult>();
            actionResult.StatusCode.ShouldBe(StatusCodes.Status404NotFound);
        }
        
        [Fact]
        public async Task GivenExistingId_WhenGettingById_ShouldReturn()
        {
            // Given
            var jobExecution = JobExecution.New(Job.New("subject", "payload", true, DateTime.UtcNow, DateTime.UtcNow, null), DateTime.UtcNow);
            Mock.Get(_mediator)
                .Setup(x => x.Send(It.IsAny<GetJobExecutionsRequest>(), CancellationToken.None))
                .ReturnsAsync(new GetJobExecutionsResponse(new []{jobExecution}));

            // When
            var result = await _controller.GetByJobIdAsync(Guid.NewGuid(), CancellationToken.None);
            var actionResult = result as OkObjectResult;

            // Then
            result.ShouldBeAssignableTo<OkObjectResult>();
            actionResult.StatusCode.ShouldBe(StatusCodes.Status200OK);
        }
        
        [Fact]
        public async Task GivenNonExistingId_WhenGettingById_ShouldReturnNotFound()
        {
            // Given
            Mock.Get(_mediator)
                .Setup(x => x.Send(It.IsAny<GetJobExecutionsRequest>(), CancellationToken.None))
                .ReturnsAsync(new GetJobExecutionsResponse(Enumerable.Empty<JobExecution>()));

            // When
            var result = await _controller.GetByJobIdAsync(Guid.NewGuid(), CancellationToken.None);
            var actionResult = result as NotFoundResult;

            // Then
            result.ShouldBeAssignableTo<NotFoundResult>();
            actionResult.StatusCode.ShouldBe(StatusCodes.Status404NotFound);
        }
    }
}