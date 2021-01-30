using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Rescheduler.Api.Models;
using Rescheduler.Core.Entities;
using Rescheduler.Core.Handlers;

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

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody]Models.CreateJobRequest request, CancellationToken ctx)
        {
            if (!ModelState.IsValid)
            {
                return UnprocessableEntity(ModelState);
            }

            var result = await _mediator.Send(new Core.Handlers.CreateJobRequest(request.ToJob()), ctx);
            var dto = new {
                Job = JobResponse.From(result.Job),
                JobExecution = result.JobExecution is null ? null : JobExecutionResponse.From(result.JobExecution)
            };

            return StatusCode((int)HttpStatusCode.Created, dto);
        }

        [HttpGet("{jobId}")]
        public async Task<IActionResult> GetByIdAsync([FromRoute]Guid jobId, CancellationToken ctx)
        {
            var result = await _mediator.Send(new GetJobRequest(jobId), ctx);

            if (result.Job is null)
            {
                return NotFound();
            }

            return Ok(JobResponse.From(result.Job));
        }
    }
}