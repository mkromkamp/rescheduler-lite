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
            pendingJobs = pendingJobs.ToList();
            var result = new SchedulePendingResponse(pendingJobs.Count());

            if (pendingJobs.Any())
            {
                // Try to queue scheduled jobs
                if(!await _jobPublisher.PublishManyAsync(pendingJobs.Select(p => p.Job), cancellationToken))
                {
                    // Failed to publish, reschedule
                    pendingJobs.ToList().ForEach(p => p.Scheduled());
                    await _JobExecutionRepo.UpdateManyAsync(pendingJobs, cancellationToken);
                    
                    return new SchedulePendingResponse(0);
                }

                // Mark messages as queued
                var now = DateTime.UtcNow;
                pendingJobs.ToList().ForEach(p => p.Queued(now));
                await _JobExecutionRepo.UpdateManyAsync(pendingJobs, cancellationToken);

                foreach (var pending in pendingJobs)
                {
                    // If there is a next schedule available for the job queue it
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