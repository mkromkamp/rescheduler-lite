using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Rescheduler.Core.Entities;

namespace Rescheduler.Core.Interfaces
{
    public interface IJobExecutionRepository
    {
         Task<IEnumerable<JobExecution>> GetAndMarkPending(int max, DateTime until, CancellationToken ctx);
    }
}