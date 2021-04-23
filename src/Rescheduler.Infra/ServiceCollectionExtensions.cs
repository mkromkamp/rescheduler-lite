using System;
using Azure.Messaging.ServiceBus;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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
            services.AddEntityFrameworkSqlite();
            services.AddDbContext<JobContext>(
                ctx => ctx.UseSqlite(configuration.GetConnectionString("Database"), 
                opts => opts.MigrationsAssembly("Rescheduler.Api")));

            services.AddScoped<IRepository<Job>, Repository<Job>>();
            services.AddScoped<IRepository<JobExecution>, Repository<JobExecution>>();
            services.AddScoped<IJobExecutionRepository, Repository<JobExecution>>();
            
            services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(MetricsBehavior<,>));

            services.AddMessaging(configuration);

            return services;
        }

        private static IServiceCollection AddMessaging(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions();
            services.Configure<MessagingOptions>(configuration.GetSection("Messaging"));
            var options = services.BuildServiceProvider().GetRequiredService<IOptions<MessagingOptions>>();

            if(!(options.Value.RabbitMq?.Enabled ?? false) && !(options.Value.ServiceBus?.Enabled ?? false)) 
                throw new ArgumentException("No message bus is configured");
            
            if (options.Value.RabbitMq?.Enabled ?? false)
            {
                services.AddSingleton<IJobPublisher, RabbitJobPublisher>();
                services.AddSingleton<IConnectionFactory>(_ => new ConnectionFactory()
                {
                    Uri = new Uri(options.Value.RabbitMq.ConnectionString),
                    AutomaticRecoveryEnabled = true,
                    TopologyRecoveryEnabled = true,
                });
            }

            if (options.Value.ServiceBus?.Enabled ?? false)
            {
                services.AddSingleton<IJobPublisher, ServiceBusPublisher>();
                services.AddSingleton(_ => new ServiceBusClient(options.Value.ServiceBus.ConnectionString));
            }

            return services;
        }
    }
}