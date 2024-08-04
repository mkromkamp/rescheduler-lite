using System.Reflection;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Exporter;
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

        if (_configuration.GetSection("Telemetry:Metrics:Otlp").GetValue("Enabled", false))
        {
            if (!Uri.TryCreate(_configuration.GetSection("Telemetry:Metrics:Otlp").GetValue<string>("Endpoint"), UriKind.Absolute, out var uri))
            {
                uri = Program.DefaultOtlpEndpoint;
            }

            if (!Enum.TryParse(_configuration.GetSection("Telemetry:Metrics:Otlp").GetValue<string>("Protocol"), true, out OtlpExportProtocol protocol))
            {
                protocol = OtlpExportProtocol.Grpc;
            }
            
            services.AddOpenTelemetry()
                .ConfigureResource(resource =>
                {
                    // resource.Clear();
                    resource.AddService("Rescheduler");
                    resource.AddEnvironmentVariableDetector();
                })
                .WithMetrics(metrics => metrics
                    .AddRuntimeInstrumentation()
                    .AddProcessInstrumentation()
                    .AddMeter("Microsoft.AspNetCore.Hosting")
                    .AddMeter("Microsoft.AspNetCore.Routing")
                    .AddMeter("Rescheduler")
                    .AddView("messages.publish.duration", 
                        new ExplicitBucketHistogramConfiguration()
                        {
                            Boundaries = [0.005, 0.01, 0.3, 0.5, 1, 2, 5, 30]
                        })
                    .AddOtlpExporter(otlp =>
                    {
                        otlp.Protocol = protocol;
                        otlp.Endpoint = uri;
                    }));
        }
            
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

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}