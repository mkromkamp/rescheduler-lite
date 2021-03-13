using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rescheduler.Api.Models;
using Rescheduler.Core.Handlers;
using CreateJobResponse = Rescheduler.Api.Models.CreateJobResponse;

namespace Rescheduler.Api.Controllers
{
    [Route("api/jobs")]
    public class JobsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public JobsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Create a new job
        /// </summary>
        /// <param name="request">The job to create</param>
        /// <param name="ctx">The cancellation token</param>
        /// <response code="201">Returns the created job and next job execution</response>
        /// <response code="422">Invalid request</response>
        [HttpPost]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CreateJobResponse>> CreateAsync([FromBody]Models.CreateJobRequest request, CancellationToken ctx)
        {
            if (!ModelState.IsValid)
            {
                return UnprocessableEntity(ModelState);
            }

            var result = await _mediator.Send(new Core.Handlers.CreateJobRequest(request.ToJob()), ctx);
            var dto = new CreateJobResponse(
                JobResponse.From(result.Job),
                result.JobExecution is null
                    ? null
                    : JobExecutionResponse.From(result.JobExecution));

            return StatusCode((int)HttpStatusCode.Created, dto);
        }
        
        /// <summary>
        /// Get a job by its identifier
        /// </summary>
        /// <param name="jobId">The unique identifier of the job</param>
        /// <param name="ctx">The cancellation token</param>
        /// <response code="200">Returns the job</response>
        /// <response code="404">Returned when the job doesn't exist</response>
        [HttpGet("{jobId}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<JobResponse>> GetByIdAsync([FromRoute]Guid jobId, CancellationToken ctx)
        {
            var result = await _mediator.Send(new GetJobRequest(jobId), ctx);

            if (result.Job is null)
            {
                return NotFound();
            }

            return Ok(JobResponse.From(result.Job));
        }
        
        /// <summary>
        /// Get all jobs
        /// </summary>
        /// <param name="subject">Optionally the subject of the job</param>
        /// <param name="ctx">The cancellation token</param>
        /// <param name="top">Optionally, the maximum number of results, default 20</param>
        /// <param name="skip">Optionally, the results to skip, default 0</param>
        /// <response code="200">Returns the jobs, if any; otherwise an empty array</response>
        [HttpGet("")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<JobResponse>>> GetAllAsync([FromQuery] string? subject, CancellationToken ctx, [FromQuery] int top = 20, [FromQuery] int skip = 0)
        {
            var result = await _mediator.Send(new GetJobsRequest(subject, top, skip), ctx);

            return Ok(result.Jobs);
        }
    }
}