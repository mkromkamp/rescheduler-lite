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
        private readonly IRepository<ScheduledJob> _jobScheduleRepository;

        public CreateJobHandler(IRepository<Job> jobRepository, IRepository<ScheduledJob> jobScheduleRepository)
        {
            _jobRepository = jobRepository;
            _jobScheduleRepository = jobScheduleRepository;
        }

        public async Task<CreateJobResponse> Handle(CreateJobRequest request, CancellationToken cancellationToken)
        {
            await _jobRepository.AddAsync(request.job, cancellationToken);

            ScheduledJob? jobSchedule = null;
            if (request.job.Enabled
                && request.job.TryGetNextRun(DateTime.UtcNow, out var runAt)
                && runAt.HasValue)
            {
                jobSchedule = ScheduledJob.New(request.job.Id, runAt.Value);
                await _jobScheduleRepository.AddAsync(jobSchedule, cancellationToken);
            }

            return new CreateJobResponse(request.job, jobSchedule);
        }
    }

    public record CreateJobRequest (Job job) : IRequest<CreateJobResponse>;

    public record CreateJobResponse(Job job, ScheduledJob? firstScheduledRun);
}