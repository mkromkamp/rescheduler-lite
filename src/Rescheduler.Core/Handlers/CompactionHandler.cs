using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Rescheduler.Core.Interfaces;

namespace Rescheduler.Core.Handlers;

public class CompactionHandler : INotificationHandler<CompactionRequest>
{
    private readonly IJobExecutionRepository _jobExecutionRepository;

    public CompactionHandler(IJobExecutionRepository jobExecutionRepository)
    {
        _jobExecutionRepository = jobExecutionRepository;
    }

    public async Task Handle(CompactionRequest notification, CancellationToken ctx)
    {
        await _jobExecutionRepository.CompactAsync(notification.Before, ctx);
    }
}

public record CompactionRequest(DateTime Before) : INotification;