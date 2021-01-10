using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Rescheduler.Worker
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddWorker(this IServiceCollection services)
        {
            services.AddTransient<IRequestHandler<SchedulePendingRequest, SchedulePendingResponse>, SchedulePendingHandler>();
            services.AddHostedService<JobScheduler>();

            return services;
        }
    }
}