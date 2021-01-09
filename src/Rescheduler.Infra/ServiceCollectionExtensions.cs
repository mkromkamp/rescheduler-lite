using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rescheduler.Core.Interfaces;
using Rescheduler.Infra.Data;

namespace Rescheduler.Infra
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfra(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<JobContext>();
            services.AddSingleton(typeof(Repository<>), typeof(IRepository<>));

            return services;
        }
    }
}