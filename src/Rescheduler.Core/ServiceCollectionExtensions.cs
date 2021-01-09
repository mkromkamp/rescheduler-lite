using System.Reflection;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rescheduler.Core.Handlers;

namespace Rescheduler.Core
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCore(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMediatR(Assembly.GetAssembly(typeof(CreateJobHandler)));

            return services;
        }
    }
}