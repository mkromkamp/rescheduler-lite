using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Rescheduler.Api.Models;
using Rescheduler.Core.Entities;
using Rescheduler.Core.Handlers;

namespace Rescheduler.Api.Controllers
{
    [Route("api")]
    public class JobExecutionController : ControllerBase
    {
        private readonly IMediator _mediator;

        public JobExecutionController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("jobs/{jobId}/executions")]
        public async Task<IActionResult> GetByJobIdAsync(Guid jobId, CancellationToken ctx)
        {
            var result = await _mediator.Send(new GetJobExecutionsRequest(jobId), ctx);

            if(result.Executions.Any())
                return Ok(result.Executions.Select(j => JobExecutionResponse.From(j)).ToList());
            
            return NotFound();
        }

        [HttpGet("jobs/executions")]
        public async Task<IActionResult> GetAllAsync([FromQuery] IEnumerable<ExecutionStatus> statuses, [FromQuery] string? subject, CancellationToken ctx, [FromQuery] int top = 20, [FromQuery] int skip = 0)
        {
            var result = await _mediator.Send(new GetExecutionsRequest(statuses, subject, top, skip), ctx);

            if(result.Executions.Any())
                return Ok(result.Executions.Select(j => JobExecutionResponse.From(j)).ToList());
            
            return NotFound();
        }
    }
}