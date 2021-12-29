using Moq;
using Rescheduler.Core.Handlers;
using Rescheduler.Core.Interfaces;
using Xunit;

namespace Rescheduler.Core.Tests.Handlers;

public class CompactionHandlerTests
{
    private readonly IJobExecutionRepository _jobExecutionRepository;

    private readonly CompactionHandler _handler;

    public CompactionHandlerTests()
    {
        _jobExecutionRepository = Mock.Of<IJobExecutionRepository>();

        _handler = new CompactionHandler(_jobExecutionRepository);
    }
        
    [Fact]
    public async Task GivenRequest_WhenCompacting_ThenShouldReturn()
    {
        // Given
        var before = DateTime.UtcNow;
        Mock.Get(_jobExecutionRepository)
            .Setup(x => x.CompactAsync(before, CancellationToken.None));
            
        // When
        await _handler.Handle(new CompactionRequest(before), CancellationToken.None);
            
        // Then
        Mock.Get(_jobExecutionRepository)
            .Verify(x => x.CompactAsync(before, CancellationToken.None),
                Times.Once);
    }
}