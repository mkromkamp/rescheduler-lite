using Moq;
using Rescheduler.Core.Entities;
using Rescheduler.Core.Handlers;
using Rescheduler.Core.Interfaces;
using Shouldly;
using Xunit;

namespace Rescheduler.Core.Tests.Handlers;

public class GetJobsHandlerTests
{
    private readonly IRepository<Job> _repository;

    private readonly GetJobsHandler _handler;

    public GetJobsHandlerTests()
    {
        _repository = Mock.Of<IRepository<Job>>();

        _handler = new GetJobsHandler(_repository);
    }
        
    [Fact]
    public async Task GivenOneExecution_WhenHandling_ShouldReturn()
    {
        // Given
        var request = new GetJobsRequest("subject", 10, 0);
        Mock.Get(_repository)
            .Setup(x => x.GetManyAsync(It.IsAny<Func<IQueryable<Job>, IQueryable<Job>>>(), CancellationToken.None))
            .ReturnsAsync(new List<Job>
            {
                Job.New("subject", "payload", true, DateTime.UtcNow, DateTime.UtcNow, null)
            }.AsReadOnly());

        // When
        var result = await _handler.Handle(request, CancellationToken.None);

        // Then
        result.Jobs.ShouldHaveSingleItem();
    }
        
    [Fact]
    public async Task GivenTwoExecution_WhenHandling_ShouldReturn()
    {
        // Given
        var request = new GetJobsRequest("subject", 10, 0);
        Mock.Get(_repository)
            .Setup(x => x.GetManyAsync(It.IsAny<Func<IQueryable<Job>, IQueryable<Job>>>(), CancellationToken.None))
            .ReturnsAsync(new List<Job>
            {
                Job.New("subject", "payload", true, DateTime.UtcNow, DateTime.UtcNow, null),
                Job.New("subject", "payload", true, DateTime.UtcNow, DateTime.UtcNow, null)
            }.AsReadOnly());

        // When
        var result = await _handler.Handle(request, CancellationToken.None);

        // Then
        result.Jobs.Count().ShouldBe(2);
    }
}