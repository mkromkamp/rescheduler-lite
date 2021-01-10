using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
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

            var newJob = await _mediator.Send(new CreateJobRequest(request.ToJob()), ctx);

            return StatusCode((int)HttpStatusCode.Created, newJob);
        }

        [HttpGet("{jobId}")]
        public async Task<IActionResult> GetByIdAsync([FromRoute]Guid jobId, CancellationToken ctx)
        {
            var job = await _mediator.Send(new GetJobRequest(jobId), ctx);

            if (job is null)
            {
                return NotFound();
            }

            return Ok(job);
        }
    }
}