using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Rescheduler.Api.Models;

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
        public async Task<IActionResult> CreateAsync([FromBody]CreateJobRequest request, CancellationToken ctx)
        {
            if (!ModelState.IsValid)
            {
                return UnprocessableEntity(ModelState);
            }

            var newJob = await _mediator.Send(new Core.Handlers.CreateJobRequest(request.ToJob()), ctx);

            return StatusCode((int)HttpStatusCode.Created, newJob);
        }
    }
}