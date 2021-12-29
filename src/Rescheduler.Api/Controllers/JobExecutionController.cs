using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rescheduler.Api.Models;
using Rescheduler.Core.Entities;
using Rescheduler.Core.Handlers;

namespace Rescheduler.Api.Controllers;

[Route("api")]
public class JobExecutionController : ControllerBase
{
    private readonly IMediator _mediator;

    public JobExecutionController(IMediator mediator)
    {
        _mediator = mediator;
    }
        
    /// <summary>
    /// Get all execution for a job
    /// </summary>
    /// <param name="jobId">The job identifier</param>
    /// <param name="ctx">The cancellation token</param>
    /// <response code="200">Returns the job executions</response>
    /// <response code="404">Returned if the job doesn't exits</response>
    [HttpGet("jobs/{jobId}/executions")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<JobExecutionResponse>>> GetByJobIdAsync(Guid jobId, CancellationToken ctx)
    {
        var result = await _mediator.Send(new GetJobExecutionsRequest(jobId), ctx);

        if(result.Executions.Any())
            return Ok(result.Executions.Select(JobExecutionResponse.From).ToList());
            
        return NotFound();
    }

    /// <summary>
    /// Get all job executions
    /// </summary>
    /// <param name="statuses">Optionally, one or more execution statuses to filter on</param>
    /// <param name="subject">Optionally the subject of the job</param>
    /// <param name="ctx">The cancellation token</param>
    /// <param name="top">Optionally, the maximum number of results, default 20</param>
    /// <param name="skip">Optionally, the results to skip, default 0</param>
    /// <response code="200">Returns the job executions, if any; otherwise an empty array</response>
    [HttpGet("jobs/executions")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<JobExecutionResponse>>> GetAllAsync([FromQuery] IEnumerable<ExecutionStatus> statuses, [FromQuery] string? subject, CancellationToken ctx, [FromQuery] int top = 20, [FromQuery] int skip = 0)
    {
        var result = await _mediator.Send(new GetExecutionsRequest(statuses, subject, top, skip), ctx);

        if(result.Executions.Any())
            return Ok(result.Executions.Select(JobExecutionResponse.From).ToList());
            
        return NotFound();
    }
}