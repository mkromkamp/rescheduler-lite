using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Rescheduler.Core.Handlers;
using Xunit;

namespace Rescheduler.Worker.Tests;

public class CompactionWorkerTests
{
    private readonly ILogger<CompactionWorker> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IServiceScope _serviceScope;
    private readonly IServiceProvider _serviceProvider;
    private readonly IMediator _mediator;
        
    private readonly CompactionWorker _worker;

    public CompactionWorkerTests()
    {
        _logger = Mock.Of<ILogger<CompactionWorker>>();
        _scopeFactory = Mock.Of<IServiceScopeFactory>();
        _serviceScope = Mock.Of<IServiceScope>();
        _serviceProvider = Mock.Of<IServiceProvider>();
        _mediator = Mock.Of<IMediator>();

        Mock.Get(_scopeFactory)
            .Setup(x => x.CreateScope())
            .Returns(_serviceScope);

        Mock.Get(_serviceScope)
            .SetupGet(x => x.ServiceProvider)
            .Returns(_serviceProvider);

        Mock.Get(_serviceProvider)
            .Setup(x => x.GetService(typeof(IMediator)))
            .Returns(_mediator);

        _worker = new CompactionWorker(_logger, _scopeFactory);
    }

    [Fact]
    public async Task GivenServiceStart_WhenRunning_ShouldCompact()
    {
        // Given
        var cts = new CancellationTokenSource();
        var compactionRequest = new CompactionRequest(DateTime.UtcNow);
        Mock.Get(_mediator)
            .Setup(x => x.Publish(compactionRequest, cts.Token));

        // When 
#pragma warning disable 4014
        _worker.RunCompaction(cts.Token);
#pragma warning restore 4014
            
        await Task.Delay(100, CancellationToken.None);
        cts.Cancel();

        // Then
        Mock.Get(_mediator)
            .Verify(x => x.Publish(It.IsAny<CompactionRequest>(), cts.Token),
                Times.Once);
    }
}