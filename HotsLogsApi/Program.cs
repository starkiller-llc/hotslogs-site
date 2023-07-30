using HotsLogs.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace HotsLogsApi;

public class Program
{
    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .UseSerilog(
                (c, log) =>
                {
                    log.ReadFrom.Configuration(c.Configuration);
                    Manager.ConfigureLogger("API", log);
                })
            .ConfigureWebHostDefaults(
                webBuilder =>
                {
                    webBuilder
                        .UseStartup<Startup>()
                        .UseWebRoot("wwwroot");
                });

    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }
}
