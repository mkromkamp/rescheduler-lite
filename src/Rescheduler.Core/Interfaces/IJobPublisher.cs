using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Rescheduler.Core.Entities;

namespace Rescheduler.Core.Interfaces
{
    public interface IJobPublisher
    {
        /// <summary>
        /// Publish a <see cref="JobExecution"/>.
        /// </summary>
        /// <param name="jobExecution">The job execution</param>
        /// <param name="ctx">The CancellationToken</param>
        /// <returns>A <see cref="Task{T}"/> representing the async operation</returns>
        Task<bool> PublishAsync(JobExecution jobExecution, CancellationToken ctx);

        /// <summary>
        /// Batch publish a list of <see cref="JobExecution"/>.
        /// </summary>
        /// <param name="jobExecutions">The job execution</param>
        /// <param name="ctx">The CancellationToken</param>
        /// <returns>A <see cref="Task{T}"/> representing the async operation</returns>
        Task<bool> PublishManyAsync(IEnumerable<JobExecution> jobExecutions, CancellationToken ctx);
    }
}