using Microsoft.Extensions.DependencyInjection;
using Rescheduler.Core.Handlers;

namespace Rescheduler.Core;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCore(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<CreateJobHandler>();
            cfg.Lifetime = ServiceLifetime.Scoped;
        });

        return services;
    }
}