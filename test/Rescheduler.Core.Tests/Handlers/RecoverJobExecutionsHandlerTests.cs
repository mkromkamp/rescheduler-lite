using System.Threading;
using System.Threading.Tasks;
using Moq;
using Rescheduler.Core.Handlers;
using Rescheduler.Core.Interfaces;
using Shouldly;
using Xunit;

namespace Rescheduler.Core.Tests.Handlers
{
    public class RecoverJobExecutionsHandlerTests
    {
        private readonly IJobExecutionRepository _jobExecutionRepository;

        private readonly RecoverJobExecutionsHandler _handler;

        public RecoverJobExecutionsHandlerTests()
        {
            _jobExecutionRepository = Mock.Of<IJobExecutionRepository>();

            _handler = new RecoverJobExecutionsHandler(_jobExecutionRepository);
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(1, 1)]
        [InlineData(9999, 9999)]
        [InlineData(int.MaxValue, int.MaxValue)]
        public async Task GivenNumberOfFlightExecutions_WhenRecovering_ThenShouldReturnRecovered(int numInFlight, int numExpected)
        {
            // Given
            Mock.Get(_jobExecutionRepository)
                .Setup(x => x.RecoverAsync(CancellationToken.None))
                .ReturnsAsync(numInFlight);
            
            // When
            var result = await _handler.Handle(new(), CancellationToken.None);
            
            // Then
            result.NumRecovered.ShouldBe(numExpected);
        }
    }
}