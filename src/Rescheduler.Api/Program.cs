using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;

namespace Rescheduler.Api;
#pragma warning disable CS1591
public class Program
{
    public static void Main(string[] args)
    {
        try
        {
            CreateHostBuilder(args).Build().Run();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            throw;
        }
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureLogging((ctx, logging) =>
            {
                logging.ClearProviders();

                if (ctx.Configuration.GetSection("Telemetry:Logs:Console").GetValue("Enabled", false))
                {
                    var loggingFormat = ctx.Configuration.GetSection("Telemetry:Logs:Console:Format").Value ?? "json";
                    logging.AddConsole(opts => opts.FormatterName = loggingFormat);  
                }
                
                if (ctx.Configuration.GetSection("Telemetry:Logs:Otlp").GetValue("Enabled", false))
                {
                    if (!Uri.TryCreate(ctx.Configuration.GetSection("Telemetry:Logs:Otlp").GetValue<string>("Endpoint"), UriKind.Absolute, out var uri))
                    {
                        uri = new Uri("http://localhost:4317");
                    }

                    if (!Enum.TryParse(ctx.Configuration.GetSection("Telemetry:Logs:Otlp").GetValue<string>("Protocol"), true, out OtlpExportProtocol protocol))
                    {
                        protocol = OtlpExportProtocol.Grpc;
                    }
                    
                    logging.AddOpenTelemetry(options =>
                    {
                        options
                            .SetResourceBuilder(ResourceBuilder.CreateDefault()
                                .AddService("Rescheduler"))
                            .AddOtlpExporter(otlp =>
                            {
                                otlp.Protocol = protocol;
                                otlp.Endpoint = uri;
                            });
                    });
                }
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
}
#pragma warning restore CS1591