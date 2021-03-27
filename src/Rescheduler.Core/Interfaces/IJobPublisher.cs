using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Rescheduler.Core.Entities;

namespace Rescheduler.Core.Interfaces
{
    public interface IJobPublisher
    {
        /// <summary>
        /// Publish a <see cref="Job"/>.
        /// </summary>
        /// <param name="job">The job</param>
        /// <param name="ctx">The CancellationToken</param>
        /// <returns>A <see cref="Task{T}"/> representing the async operation</returns>
        Task<bool> PublishAsync(Job job, CancellationToken ctx);

        /// <summary>
        /// Batch publish a list of <see cref="Job"/>.
        /// </summary>
        /// <param name="jobs">The jobs</param>
        /// <param name="ctx">The CancellationToken</param>
        /// <returns>A <see cref="Task{T}"/> representing the async operation</returns>
        Task<bool> PublishManyAsync(IEnumerable<Job> jobs, CancellationToken ctx);
    }
}