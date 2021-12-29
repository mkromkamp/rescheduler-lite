using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Rescheduler.Core.Entities;
using Rescheduler.Core.Interfaces;

namespace Rescheduler.Core.Handlers;

internal class GetJobExecutionsHandler : IRequestHandler<GetJobExecutionsRequest, GetJobExecutionsResponse>
{
    private readonly IRepository<JobExecution> _repository;

    public GetJobExecutionsHandler(IRepository<JobExecution> repository)
    {
        _repository = repository;
    }

    public async Task<GetJobExecutionsResponse> Handle(GetJobExecutionsRequest request, CancellationToken cancellationToken)
    {
        var result = await _repository.GetManyAsync(
            (q) => q.Where(x => x.Job.Id == request.JobId)
                .OrderByDescending(x => x.ScheduledAt),
            cancellationToken);

        return new GetJobExecutionsResponse(result);
    }
}

public record GetJobExecutionsRequest(Guid JobId) : IRequest<GetJobExecutionsResponse>;

public record GetJobExecutionsResponse(IEnumerable<JobExecution> Executions);