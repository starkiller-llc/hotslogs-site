using HOTSLogsUploader.Core.Models;
using HOTSLogsUploader.Core.Models.Db.Entity;
using HOTSLogsUploader.Core.Models.Deprecated;
using HOTSLogsUploader.Core.Utilities.Services;
using HOTSLogsUploader.Core.Utilities.Services.Uploaders;
using HOTSLogsUploader.Core.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace HOTSLogsUploader.Core
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public IServiceProvider ServiceProvider { get; set; }
        public IConfiguration Configuration { get; set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            Configuration = new ConfigurationBuilder().Build();
            ServiceProvider = ConfigureServices(new ServiceCollection())
                .BuildServiceProvider();
            ConfigureServiceProvider();

            ServiceProvider.GetRequiredService<MainWindow>().Show();
        }

        private void ConfigureServiceProvider()
        {
            var context = ServiceProvider.GetRequiredService<ReplayDbContext>();
            context.FileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{AppDomain.CurrentDomain.FriendlyName}.db");
            context.LoadFromFile();

            var jsonDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ReplayData.json");
            if (File.Exists(jsonDataPath) && !context.Replays.Any())
            {
                var jsonData = JsonSerializer.Deserialize<ReplayFileCollection>(File.ReadAllText(jsonDataPath));
                
                context.AddRange(jsonData.Data.Select(i => new Replay
                {
                    DateUploaded = !string.IsNullOrEmpty(i.DateUploaded) ? DateTime.Parse(i.DateUploaded) : null,
                    FilePath = i.FilePath,
                    Fingerprint = i.Fingerprint,
                    ParseResult = i.ParseResult,
                    UploadStatus = i.UploadStatus
                }));
                context.SaveChanges();

                File.Move(jsonDataPath, Path.ChangeExtension(jsonDataPath, ".deprecated.json"));
            }
        }

        private IServiceCollection ConfigureServices(IServiceCollection services)
        {
            return ServiceInitializer.ConfigureServices(services);
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            ServiceProvider
                .GetRequiredService<ReplayDbContext>()
                .SaveToFile();
        }
    }
}
