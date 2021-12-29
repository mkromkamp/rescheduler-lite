using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Rescheduler.Core.Entities;
using Rescheduler.Core.Interfaces;

namespace Rescheduler.Core.Handlers;

public class GetJobHandler : IRequestHandler<GetJobRequest, GetJobResponse>
{
    private readonly IRepository<Job> _jobRepository;

    public GetJobHandler(IRepository<Job> jobRepository)
    {
        _jobRepository = jobRepository;
    }

    public async Task<GetJobResponse> Handle(GetJobRequest request, CancellationToken cancellationToken)
    {
        var job = await _jobRepository.GetByIdAsync(request.Id, cancellationToken);

        return new GetJobResponse(job);
    }
}

public record GetJobRequest(Guid Id) : IRequest<GetJobResponse>;

public record GetJobResponse(Job? Job);