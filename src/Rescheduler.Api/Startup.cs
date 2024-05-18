using System.Reflection;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using Rescheduler.Core;
using Rescheduler.Infra;
using Rescheduler.Infra.Data;
using Rescheduler.Worker;

namespace Rescheduler.Api;

public class Startup
{
    private readonly IConfiguration _configuration;
    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddOptions();
        services.AddMetrics();
        services.AddControllers(options =>
            {
                options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
            })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });
        
        services.AddOpenTelemetry()
            .ConfigureResource(resource =>
            {
                resource.AddService("Rescheduler");
            })
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddRuntimeInstrumentation()
                .AddProcessInstrumentation()
                .AddMeter("Rescheduler")
                // .AddEventCountersInstrumentation(ec =>
                // {
                //     ec.AddEventSources("Microsoft.EntityFrameworkCore");
                //     ec.AddEventSources("Microsoft.Data.SqlClient.EventSource");
                // })
                .AddPrometheusExporter());
            
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Rescheduler.Api", Version = "v1" });
                
            // Set the comments path for the Swagger JSON and UI.
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            c.IncludeXmlComments(xmlPath);
        });

        services.AddCore()
            .AddInfra(_configuration)
            .AddWorker();

        services.Configure<HostOptions>(opts =>
            opts.ShutdownTimeout = TimeSpan.FromMinutes(2));
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, JobContext jobContext)
    {
        // Migrate database at startup, ensures creation if no database file found
        jobContext.Database.Migrate();
            
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
            
        app.UseSwagger(c =>
        {
            c.RouteTemplate = "api/docs/{documentName}/openapi.json";
        });
        app.UseSwaggerUI(c =>
        {
            c.RoutePrefix = "api/docs";
            c.SwaggerEndpoint("/api/docs/v1/openapi.json", "Rescheduler.Api v1");
        });

        app.UseRouting();
        app.UseAuthorization();
        app.UseOpenTelemetryPrometheusScrapingEndpoint();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}