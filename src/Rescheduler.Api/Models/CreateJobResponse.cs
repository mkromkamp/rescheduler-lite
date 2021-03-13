namespace Rescheduler.Api.Models
{
    public record CreateJobResponse(JobResponse Job, JobExecutionResponse? JobExecution);
}