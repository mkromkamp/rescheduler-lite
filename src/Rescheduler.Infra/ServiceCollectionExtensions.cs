using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rescheduler.Core.Entities;
using Rescheduler.Core.Interfaces;
using Rescheduler.Infra.Data;

namespace Rescheduler.Infra
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfra(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddEntityFrameworkSqlite();
            services.AddDbContext<JobContext>();
            services.AddScoped<IRepository<Job>, Repository<Job>>();
            services.AddScoped<IRepository<ScheduledJob>, Repository<ScheduledJob>>();

            return services;
        }
    }
}