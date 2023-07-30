using HelperCore;
using Heroes.DataAccessLayer.Data;
using HotsLogs.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog.Extensions.Logging;
using ServiceStackReplacement;
using StackExchange.Redis;
using System.Windows;

namespace HotsAdminConsole;

/// <summary>
///     Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var builder = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddUserSecrets<App>();

        var configuration = builder.Build();

        var services = new ServiceCollection();

        services.AddScoped<IConfiguration>(_ => configuration);

        services.AddLogging(
            conf =>
                conf.AddConsole());

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<HeroesdataContext>(
            opts =>
                opts.UseMySql(
                    connectionString,
                    ServerVersion.AutoDetect(connectionString)));

        services.AddScoped(
            _ =>
            {
                var redisHost = configuration.GetConnectionString("RedisHost");
                var redisManagerPool = ConnectionMultiplexer.Connect(redisHost);

                var client = redisManagerPool.GetClient();
                return client;
            });

        services.AddScoped<MMR>();
        services.AddSingleton<MainWindow>();

        var logger = Manager.CreateLogger("CONSOLE");
        services.AddSingleton<ILoggerFactory>(_ => new SerilogLoggerFactory(logger));

        var svcp = services.BuildServiceProvider();

        DataHelper.SetServiceProvider(svcp);

        var wnd = svcp.GetRequiredService<MainWindow>();

        wnd.Show();
    }
}
