using MediatR;
using Rescheduler.Core.Entities;
using Rescheduler.Core.Interfaces;

namespace Rescheduler.Core.Handlers;

internal class GetJobsHandler : IRequestHandler<GetJobsRequest, GetJobsResponse>
{
    private readonly IRepository<Job> _jobsRepository;

    public GetJobsHandler(IRepository<Job> jobsRepository)
    {
        _jobsRepository = jobsRepository;
    }

    public async Task<GetJobsResponse> Handle(GetJobsRequest request, CancellationToken cancellationToken)
    {
        var jobs = await _jobsRepository.GetManyAsync(q => 
                q.Where(j => 
                        request.Subject == null || j.Subject.Equals(request.Subject)
                    )
                    .Skip(request.Skip)
                    .Take(request.Top)
            , cancellationToken);

        return new GetJobsResponse(jobs);
    }
}

public record GetJobsRequest(string? Subject, int Top, int Skip) : IRequest<GetJobsResponse>;

public record GetJobsResponse(IEnumerable<Job> Jobs);