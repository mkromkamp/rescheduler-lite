using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Rescheduler.Core.Entities;

namespace Rescheduler.Core.Interfaces
{
    public interface IJobPublisher
    {
         Task<bool> PublishAsync(Job job, CancellationToken ctx);

         Task<bool> PublishManyAsync(IEnumerable<Job> jobs, CancellationToken ctx);
    }
}