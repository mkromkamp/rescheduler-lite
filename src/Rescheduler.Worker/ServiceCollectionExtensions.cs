using Microsoft.Extensions.DependencyInjection;

namespace Rescheduler.Worker;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWorker(this IServiceCollection services)
    {
        services.AddHostedService<JobScheduler>();
        services.AddHostedService<CompactionWorker>();

        return services;
    }
}