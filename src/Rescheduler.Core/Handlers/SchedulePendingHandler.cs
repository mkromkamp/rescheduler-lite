using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Rescheduler.Core.Entities;
using Rescheduler.Core.Interfaces;

namespace Rescheduler.Core.Handlers
{
    public class SchedulePendingHandler : IRequestHandler<SchedulePendingRequest, SchedulePendingResponse>
    {
        private readonly IJobPublisher _jobPublisher;
        private readonly IJobExecutionRepository _jobExecutionRepository;
        private readonly IRepository<JobExecution> _JobExecutionRepo;

        public SchedulePendingHandler(IJobExecutionRepository jobExecutionRepository, IRepository<JobExecution> jobExecutionRepo, IJobPublisher jobPublisher)
        {
            _jobExecutionRepository = jobExecutionRepository;
            _JobExecutionRepo = jobExecutionRepo;
            _jobPublisher = jobPublisher;
        }

        public async Task<SchedulePendingResponse> Handle(SchedulePendingRequest request, CancellationToken cancellationToken)
        {
            var pendingJobs = await _jobExecutionRepository.GetAndMarkPending(1000, DateTime.UtcNow.AddSeconds(10), cancellationToken);
            var result = new SchedulePendingResponse(pendingJobs.Count());

            if (pendingJobs.Any())
            {
                var now = DateTime.UtcNow;
                pendingJobs.ToList().ForEach(j => j.Queued(now));
                await _JobExecutionRepo.UpdateManyAsync(pendingJobs, cancellationToken);

                foreach (var pending in pendingJobs)
                {
                    if (await _jobPublisher.PublishAsync(pending.Job, cancellationToken))
                    {
                        pending.Queued(DateTime.UtcNow);
                    }
                }

                await _JobExecutionRepo.UpdateManyAsync(pendingJobs, cancellationToken);

                foreach (var pending in pendingJobs)
                {
                    if (pending.Job.TryGetNextRun(DateTime.UtcNow, out var nextRun) && nextRun.HasValue)
                    {
                        var nextJobExecution = JobExecution.New(pending.Job, nextRun.Value);
                        await _JobExecutionRepo.AddAsync(nextJobExecution, cancellationToken);
                    }
                }
            }
            
            return result;
        }
    }

    public record SchedulePendingRequest(int max, DateTime until) : IRequest<SchedulePendingResponse>;

    public record SchedulePendingResponse(int numScheduled);
}