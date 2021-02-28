using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Moq;
using Rescheduler.Api.Controllers;
using Rescheduler.Api.Models;
using Rescheduler.Core.Entities;
using Rescheduler.Core.Handlers;
using Shouldly;
using Xunit;
using CreateJobRequest = Rescheduler.Api.Models.CreateJobRequest;

namespace Rescheduler.Api.Tests.Controllers
{
    public class JobsControllerTests
    {
        private readonly IMediator _mediator;

        private readonly JobsController _controller;

        public JobsControllerTests()
        {
            _mediator = Mock.Of<IMediator>();

            _controller = new JobsController(_mediator);
        }
        
        [Fact]
        public async Task GivenInValidRequest_WhenCreating_ShouldReturnValidationError()
        {
            // Given
            _controller.ModelState.AddModelError("Some", "IsRequired");
            var request = new CreateJobRequest();

            // When
            var result = await _controller.CreateAsync(request, CancellationToken.None);
            var actionResult = result as UnprocessableEntityObjectResult;

            // Then
            result.ShouldBeAssignableTo<UnprocessableEntityObjectResult>();
            actionResult.StatusCode.ShouldBe(StatusCodes.Status422UnprocessableEntity);
        }

        [Fact]
        public async Task GivenValidRequest_WhenCreating_ShouldReturnCreated()
        {
            // Given
            var request = new CreateJobRequest
            {
                Subject = "test",
                Payload = "do things",
                RunAt = DateTime.UtcNow.AddDays(1),
            };

            var job = Job.New(request.Subject, request.Payload, true, request.RunAt, request.StopAfter, null);
            var jobExecution = JobExecution.New(job, request.RunAt);

            Mock.Get(_mediator)
                .Setup(x => x.Send(It.IsAny<Core.Handlers.CreateJobRequest>(), CancellationToken.None))
                .ReturnsAsync(new CreateJobResponse(job, jobExecution));

            // When
            var result = await _controller.CreateAsync(request, CancellationToken.None);
            var actionResult = result as ObjectResult;

            // Then
            result.ShouldBeAssignableTo<ObjectResult>();
            actionResult.StatusCode.ShouldBe(StatusCodes.Status201Created);
        }
        
        [Fact]
        public async Task GivenExistingId_WhenGettingById_ShouldReturn()
        {
            // Given
            var job = Job.New("subject", "payload", true, DateTime.UtcNow, DateTime.UtcNow, null);
            Mock.Get(_mediator)
                .Setup(x => x.Send(It.IsAny<GetJobRequest>(), CancellationToken.None))
                .ReturnsAsync(new GetJobResponse(job));

            // When
            var result = await _controller.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);
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
                .Setup(x => x.Send(It.IsAny<GetJobRequest>(), CancellationToken.None))
                .ReturnsAsync(new GetJobResponse(null));

            // When
            var result = await _controller.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);
            var actionResult = result as NotFoundResult;

            // Then
            result.ShouldBeAssignableTo<NotFoundResult>();
            actionResult.StatusCode.ShouldBe(StatusCodes.Status404NotFound);
        }
        
        [Fact]
        public async Task GivenValidRequest_WhenGettingAll_ShouldReturn()
        {
            // Given
            Mock.Get(_mediator)
                .Setup(x => x.Send(It.IsAny<GetJobsRequest>(), CancellationToken.None))
                .ReturnsAsync(new GetJobsResponse(Enumerable.Empty<Job>()));

            // When
            var result = await _controller.GetAllAsync(null, CancellationToken.None);
            var actionResult = result as OkObjectResult;

            // Then
            result.ShouldBeAssignableTo<OkObjectResult>();
            actionResult.StatusCode.ShouldBe(StatusCodes.Status200OK);
        }
    }
}