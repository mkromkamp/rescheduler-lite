using System;
using System.Collections.Generic;
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
        private readonly IRepository<JobExecution> _jobExecutionRepo;

        public SchedulePendingHandler(IJobExecutionRepository jobExecutionRepository, IRepository<JobExecution> jobExecutionRepo, IJobPublisher jobPublisher)
        {
            _jobExecutionRepository = jobExecutionRepository;
            _jobExecutionRepo = jobExecutionRepo;
            _jobPublisher = jobPublisher;
        }

        public async Task<SchedulePendingResponse> Handle(SchedulePendingRequest request, CancellationToken cancellationToken)
        {
            var until = DateTime.UtcNow.AddSeconds(10);
            var totalScheduled = 0;

            int numScheduled;
            do
            {
                numScheduled = await ScheduleBatchAsync(250, until, cancellationToken);
                totalScheduled += numScheduled;
            }
            while (numScheduled > 0);

            return new SchedulePendingResponse(totalScheduled);
        }

        private async Task<int> ScheduleBatchAsync(int batchSize, DateTime until, CancellationToken ctx)
        {
            var pendingJobs = await _jobExecutionRepository.GetAndMarkPending(batchSize, until, ctx);
            pendingJobs = pendingJobs.ToList();

            if (pendingJobs.Any())
            {
                // Try to queue scheduled jobs
                if (!await _jobPublisher.PublishManyAsync(pendingJobs.Select(p => p.Job), ctx))
                {
                    await RescheduleAsync(pendingJobs, ctx);
                    return 0;
                }

                await MarkAsQueuedAsync(pendingJobs, ctx);
                await ScheduleNextExecutionsAsync(pendingJobs, ctx);
            }

            return pendingJobs.Count();
        }

        private async Task ScheduleNextExecutionsAsync(IEnumerable<JobExecution> pendingJobs, CancellationToken ctx)
        {
            var nextJobExecutions = new List<JobExecution>();
            foreach (var pending in pendingJobs)
            {
                // If there is a next schedule available for the job queue it
                if (pending.Job.TryGetNextRun(DateTime.UtcNow, out var nextRun) && nextRun.HasValue)
                {
                    var nextJobExecution = JobExecution.New(pending.Job, nextRun.Value);
                    nextJobExecutions.Add(nextJobExecution);
                }
            }

            await _jobExecutionRepo.AddManyAsync(nextJobExecutions, ctx);
        }

        private async Task MarkAsQueuedAsync(IEnumerable<JobExecution> pendingJobs, CancellationToken ctx)
        {
            // Mark messages as queued
            var now = DateTime.UtcNow;
            pendingJobs.ToList().ForEach(p => p.Queued(now));
            await _jobExecutionRepo.UpdateManyAsync(pendingJobs, ctx);
        }

        private async Task RescheduleAsync(IEnumerable<JobExecution> pendingJobs, CancellationToken ctx)
        {
            pendingJobs.ToList().ForEach(p => p.Scheduled());
            await _jobExecutionRepo.UpdateManyAsync(pendingJobs, ctx);
        }
    }

    public record SchedulePendingRequest : IRequest<SchedulePendingResponse>;

    public record SchedulePendingResponse(int NumScheduled);
}