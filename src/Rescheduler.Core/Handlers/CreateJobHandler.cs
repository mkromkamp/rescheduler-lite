using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Rescheduler.Core.Entities;
using Rescheduler.Core.Interfaces;

namespace Rescheduler.Core.Handlers
{
    internal class CreateJobHandler : IRequestHandler<CreateJobRequest, CreateJobResponse>
    {
        private readonly IRepository<Job> _jobRepository;
        private readonly IRepository<JobExecution> _jobExecutionRepository;

        public CreateJobHandler(IRepository<Job> jobRepository, IRepository<JobExecution> jobExecutionRepository)
        {
            _jobRepository = jobRepository;
            _jobExecutionRepository = jobExecutionRepository;
        }

        public async Task<CreateJobResponse> Handle(CreateJobRequest request, CancellationToken cancellationToken)
        {
            await _jobRepository.AddAsync(request.job, cancellationToken);

            JobExecution? jobExecution = null;
            if (request.job.Enabled
                && request.job.TryGetNextRun(DateTime.UtcNow, out var runAt)
                && runAt.HasValue)
            {
                jobExecution = JobExecution.New(request.job, runAt.Value);
                await _jobExecutionRepository.AddAsync(jobExecution, cancellationToken);
            }

            return new CreateJobResponse(request.job, jobExecution);
        }
    }

    public record CreateJobRequest (Job job) : IRequest<CreateJobResponse>;

    public record CreateJobResponse(Job job, JobExecution? firstScheduledRun);
}