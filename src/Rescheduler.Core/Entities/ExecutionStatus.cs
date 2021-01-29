namespace Rescheduler.Core.Entities
{
    /// <summary>
    /// Defines the status of a scheduled job run
    /// </summary>
    public enum ExecutionStatus
    {
        // Scheduled for future execution
        Scheduled,

        // Execution is in flight
        InFlight,
        
        // Execution is queued and considered done
        Queued
    }
}