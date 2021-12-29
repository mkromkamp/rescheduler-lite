using MediatR;
using Rescheduler.Core.Entities;
using Rescheduler.Core.Interfaces;

namespace Rescheduler.Core.Handlers;

public class GetExecutionsHandler : IRequestHandler<GetExecutionsRequest, GetExecutionsResponse>
{
    private readonly IRepository<JobExecution> _jobExecutionRepository;

    public GetExecutionsHandler(IRepository<JobExecution> jobExecutionRepository)
    {
        _jobExecutionRepository = jobExecutionRepository;
    }

    public async Task<GetExecutionsResponse> Handle(GetExecutionsRequest request, CancellationToken cancellationToken)
    {
        var jobExecutions = await _jobExecutionRepository.GetManyAsync(q =>
                q.Where(e =>
                        !request.Statuses.Any() || request.Statuses.Contains(e.Status)
                    )
                    .Where(e => request.Subject == null || e.Job.Subject == request.Subject)
                    .OrderByDescending(e => e.ScheduledAt)
                    .Skip(request.Skip)
                    .Take(request.Top),
            cancellationToken);

        return new GetExecutionsResponse(jobExecutions);
    }
}

public record GetExecutionsRequest(IEnumerable<ExecutionStatus> Statuses, string? Subject, int Top, int Skip) : IRequest<GetExecutionsResponse>;

public record GetExecutionsResponse(IEnumerable<JobExecution> Executions);