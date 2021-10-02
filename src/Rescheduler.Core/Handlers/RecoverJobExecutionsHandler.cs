using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Rescheduler.Core.Interfaces;

namespace Rescheduler.Core.Handlers
{
    public class RecoverJobExecutionsHandler : IRequestHandler<RecoverJobExecutionsRequest, RecoverJobExecutionsResponse>
    {
        private readonly IJobExecutionRepository _jobExecutionsRepository;


        public RecoverJobExecutionsHandler(IJobExecutionRepository jobExecutionsRepository)
        {
            _jobExecutionsRepository = jobExecutionsRepository;
        }

        public async Task<RecoverJobExecutionsResponse> Handle(RecoverJobExecutionsRequest request, CancellationToken ctx)
        {
            var numRecovered = await _jobExecutionsRepository.RecoverAsync(ctx);

            return new RecoverJobExecutionsResponse(numRecovered);
        }
    }

    public record RecoverJobExecutionsRequest() : IRequest<RecoverJobExecutionsResponse>;

    public record RecoverJobExecutionsResponse(int NumRecovered);
}