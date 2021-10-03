using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Rescheduler.Core.Handlers;
using Xunit;

namespace Rescheduler.Worker.Tests
{
    public class JobSchedulerTests
    {
        private readonly ILogger<JobScheduler> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IServiceScope _serviceScope;
        private readonly IServiceProvider _serviceProvider;
        private readonly IMediator _mediator;
        
        private readonly JobScheduler _worker;
        
        public JobSchedulerTests()
        {
            _logger = Mock.Of<ILogger<JobScheduler>>();
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

            _worker = new JobScheduler(_logger, _scopeFactory);
        }
        
        [Fact]
        public async Task GivenServiceStart_WhenRunning_ShouldRecover()
        {
            // Given
            var cts = new CancellationTokenSource();
            Mock.Get(_mediator)
                .Setup(x => x.Send(It.IsAny<RecoverJobExecutionsRequest>(), cts.Token))
                .ReturnsAsync(new RecoverJobExecutionsResponse(0));

            // When 
            await _worker.RecoverJobExecutionsAsync(cts.Token);
            cts.Cancel();

            // Then
            Mock.Get(_mediator)
                .Verify(x => x.Send(It.IsAny<RecoverJobExecutionsRequest>(), cts.Token),
                    Times.Once);
        }
        
        [Fact]
        public async Task GivenServiceStart_WhenRunning_ShouldSchedule()
        {
            // Given
            var cts = new CancellationTokenSource();
            Mock.Get(_mediator)
                .Setup(x => x.Send(It.IsAny<SchedulePendingRequest>(), cts.Token))
                .ReturnsAsync(new SchedulePendingResponse(0));

            // When 
            _worker.RunSchedulerAsync(cts.Token);
            await Task.Delay(100, cts.Token);
            cts.Cancel();

            // Then
            Mock.Get(_mediator)
                .Verify(x => x.Send(It.IsAny<SchedulePendingRequest>(), cts.Token),
                    Times.Once);
        }
    }
}