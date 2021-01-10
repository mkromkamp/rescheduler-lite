using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Rescheduler.Core.Entities;

namespace Rescheduler.Core.Interfaces
{
    public interface IScheduledJobsRepository
    {
         Task<IEnumerable<ScheduledJob>> MarkAndGetPending(int max, DateTime until, CancellationToken ctx);
    }
}