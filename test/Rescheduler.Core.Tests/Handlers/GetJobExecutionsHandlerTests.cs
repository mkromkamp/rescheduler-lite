using Moq;
using Rescheduler.Core.Entities;
using Rescheduler.Core.Handlers;
using Rescheduler.Core.Interfaces;
using Shouldly;
using Xunit;

namespace Rescheduler.Core.Tests.Handlers;

public class GetJobExecutionsHandlerTests
{
    private readonly IRepository<JobExecution> _repository;

    private readonly GetJobExecutionsHandler _handler;

    public GetJobExecutionsHandlerTests()
    {
        _repository = Mock.Of<IRepository<JobExecution>>();

        _handler = new GetJobExecutionsHandler(_repository);
    }
        
    [Fact]
    public async Task GivenOneExecution_WhenHandling_ShouldReturn()
    {
        // Given
        var request = new GetJobExecutionsRequest(Guid.NewGuid());
        Mock.Get(_repository)
            .Setup(x => x.GetManyAsync(It.IsAny<Func<IQueryable<JobExecution>, IQueryable<JobExecution>>>(), CancellationToken.None))
            .ReturnsAsync(new List<JobExecution>
            {
                JobExecution.New(Job.New("subject", "payload", true, DateTime.UtcNow, DateTime.UtcNow, null), DateTime.UtcNow)
            }.AsReadOnly());

        // When
        var result = await _handler.Handle(request, CancellationToken.None);

        // Then
        result.Executions.ShouldHaveSingleItem();
    }
        
    [Fact]
    public async Task GivenTwoExecution_WhenHandling_ShouldReturn()
    {
        // Given
        var request = new GetJobExecutionsRequest(Guid.NewGuid());
        Mock.Get(_repository)
            .Setup(x => x.GetManyAsync(It.IsAny<Func<IQueryable<JobExecution>, IQueryable<JobExecution>>>(), CancellationToken.None))
            .ReturnsAsync(new List<JobExecution>
            {
                JobExecution.New(Job.New("subject", "payload", true, DateTime.UtcNow, DateTime.UtcNow, null), DateTime.UtcNow),
                JobExecution.New(Job.New("subject", "payload", true, DateTime.UtcNow, DateTime.UtcNow, null), DateTime.UtcNow)
            }.AsReadOnly());

        // When
        var result = await _handler.Handle(request, CancellationToken.None);

        // Then
        result.Executions.Count().ShouldBe(2);
    }
}