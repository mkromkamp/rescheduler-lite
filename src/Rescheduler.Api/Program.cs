using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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
                    
                var loggingFormat = ctx.Configuration.GetSection("LoggingFormat").Value ?? "json";
                logging.AddConsole(opts => opts.FormatterName = loggingFormat);
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
}
#pragma warning restore CS1591