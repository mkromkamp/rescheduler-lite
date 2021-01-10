using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Rescheduler.Core.Entities;

namespace Rescheduler.Core.Interfaces
{
    public interface IRepository<T> where T : EntityBase
    {
        /// <summary>
        /// Retreive <see cref="T"/> by its unique identifier
        /// </summary>
        /// <param name="id">The id</param>
        /// <param name="ctx">The cancellation token</param>
        /// <returns>The instance of <see cref="T"/>, if any; otherwise null</returns>
        Task<T?> GetByIdAsync(Guid id, CancellationToken ctx);

        /// <summary>
        /// Store the instance of entity <see cref="T"/>
        /// </summary>
        /// <param name="entity">The entity</param>
        /// <param name="ctx">The cancellation token</param>
        /// <returns>A <see cref="Task"/> representing the async operation</returns>
        Task AddAsync(T entity, CancellationToken ctx);

        /// <summary>
        /// Update the state of entity <see cref="T"/>
        /// </summary>
        /// <param name="entity">The entity</param>
        /// <param name="ctx">The cancellation token</param>
        /// <returns>A <see cref="Task"/> representing the async operation</returns>
        Task UpdateAsync(T entity, CancellationToken ctx);

        /// <summary>
        /// Update the state of entities <see cref="T"/>
        /// </summary>
        /// <param name="entities">The entities</param>
        /// <param name="ctx">The cancellation token</param>
        /// <returns>A <see cref="Task"/> representing the async operation</returns>
        Task UpdateManyAsync(IEnumerable<T> entities, CancellationToken ctx);
    }
}