using System.Reflection;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Rescheduler.Core.Handlers;

namespace Rescheduler.Core
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCore(this IServiceCollection services)
        {
            services.AddMediatR(cfg => cfg.AsScoped(), Assembly.GetAssembly(typeof(CreateJobHandler)));

            return services;
        }
    }
}