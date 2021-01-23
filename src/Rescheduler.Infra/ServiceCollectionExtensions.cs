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
            services.AddDbContext<JobContext>(ctx => ctx.UseSqlite("DataSource=rescheduler.db"));

            services.AddScoped<IRepository<Job>, Repository<Job>>();
            services.AddScoped<IRepository<ScheduledJob>, Repository<ScheduledJob>>();
            services.AddScoped<IScheduledJobsRepository, Repository<ScheduledJob>>();
            services.AddSingleton<IJobPublisher, RabbitJobPublisher>();

            services.AddSingleton<IConnectionFactory>(svc => new ConnectionFactory()
            {
                Uri = new Uri("amqp://rabbitmq:rabbitmq@127.0.0.1:5672/"),
                AutomaticRecoveryEnabled = true,
                TopologyRecoveryEnabled = true,
            });

            return services;
        }
    }
}