using System;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using Rescheduler.Core.Entities;
using Rescheduler.Core.Interfaces;
using Rescheduler.Infra.Data;
using Rescheduler.Infra.Messaging;
using Rescheduler.Infra.Metrics;

namespace Rescheduler.Infra
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfra(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<RabbitMqOptions>().BindConfiguration("RabbitMQ");
            services.AddEntityFrameworkSqlite();
            services.AddDbContext<JobContext>(
                ctx => ctx.UseSqlite(configuration.GetConnectionString("Database"), 
                opts => opts.MigrationsAssembly("Rescheduler.Api")));

            services.AddScoped<IRepository<Job>, Repository<Job>>();
            services.AddScoped<IRepository<JobExecution>, Repository<JobExecution>>();
            services.AddScoped<IJobExecutionRepository, Repository<JobExecution>>();
            services.AddSingleton<IJobPublisher, RabbitJobPublisher>();

            services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(MetricsBehavior<,>)); 

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