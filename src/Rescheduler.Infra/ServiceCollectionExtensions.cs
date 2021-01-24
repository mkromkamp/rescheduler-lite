using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using Rescheduler.Core.Entities;
using Rescheduler.Core.Interfaces;
using Rescheduler.Infra.Data;
using Rescheduler.Infra.Messaging;

namespace Rescheduler.Infra
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfra(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddEntityFrameworkSqlite();
            services.AddDbContext<JobContext>(
                ctx => ctx.UseSqlite(configuration.GetConnectionString("Database"), 
                opts => opts.MigrationsAssembly("Rescheduler.Api")));

            services.AddScoped<IRepository<Job>, Repository<Job>>();
            services.AddScoped<IRepository<ScheduledJob>, Repository<ScheduledJob>>();
            services.AddScoped<IScheduledJobsRepository, Repository<ScheduledJob>>();
            services.AddSingleton<IJobPublisher, RabbitJobPublisher>();

            services.AddSingleton<IConnectionFactory>(svc => new ConnectionFactory()
            {
                Uri = new Uri(configuration.GetConnectionString("RabbitMQ")),
                AutomaticRecoveryEnabled = true,
                TopologyRecoveryEnabled = true,
            });

            return services;
        }
    }
}