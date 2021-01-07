namespace Rescheduler.Core.Entities
{
    /// <summary>
    /// Defines the status of a scheduled job run
    /// </summary>
    public enum ScheduleStatus
    {
        // Scheduled for future execution
        Scheduled,

        // Schedule is in flight
        InFlight,
        
        // Schedule is queued and considered done
        Queued
    }
}