using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Rescheduler.Core.Entities;
using Rescheduler.Core.Interfaces;

namespace Rescheduler.Worker
{
    public class SchedulePendingHandler : IRequestHandler<SchedulePendingRequest, SchedulePendingResponse>
    {
        private readonly IJobPublisher _jobPublisher;
        private readonly IScheduledJobsRepository _scheduledJobsRepository;
        private readonly IRepository<ScheduledJob> _scheduledJobsRepo;

        public SchedulePendingHandler(IScheduledJobsRepository scheduledJobsRepository, IRepository<ScheduledJob> scheduledJobsRepo, IJobPublisher jobPublisher)
        {
            _scheduledJobsRepository = scheduledJobsRepository;
            _scheduledJobsRepo = scheduledJobsRepo;
            _jobPublisher = jobPublisher;
        }

        public async Task<SchedulePendingResponse> Handle(SchedulePendingRequest request, CancellationToken cancellationToken)
        {
            var pendingJobs = await _scheduledJobsRepository.MarkAndGetPending(20, DateTime.UtcNow.AddSeconds(10), cancellationToken);
            var result = new SchedulePendingResponse(pendingJobs.Count());

            if (pendingJobs.Any())
            {
                var now = DateTime.UtcNow;
                pendingJobs.ToList().ForEach(j => j.Queued(now));
                await _scheduledJobsRepo.UpdateManyAsync(pendingJobs, cancellationToken);

                foreach (var pending in pendingJobs)
                {
                    if (await _jobPublisher.PublishAsync(pending.Job, cancellationToken))
                    {
                        pending.Queued(DateTime.UtcNow);
                    }
                }

                await _scheduledJobsRepo.UpdateManyAsync(pendingJobs, cancellationToken);
            }
            
            return result;
        }
    }

    public record SchedulePendingRequest(int max, DateTime until) : IRequest<SchedulePendingResponse>;

    public record SchedulePendingResponse(int numScheduled);
}