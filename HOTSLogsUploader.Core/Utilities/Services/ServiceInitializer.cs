using HOTSLogsUploader.Core.Models;
using HOTSLogsUploader.Core.Utilities.Services.Uploaders;
using HOTSLogsUploader.Core.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HOTSLogsUploader.Core.Utilities.Services
{
    public static class ServiceInitializer
    {
        public static IServiceCollection ConfigureServices(IServiceCollection services)
        {
            services
                .AddDbContext<ReplayDbContext>(options => options.UseInMemoryDatabase(databaseName: "Replays"))
                .AddTransient<MainWindow>()
                .AddSingleton<ICommonUploadService, CommonUploadService>()
                .AddSingleton<IReplayScannerService, ReplayScannerService>()
                .AddSingleton<IUploadService, HotsLogsUploadService>()
                .AddSingleton<IMatchSummaryService, MatchSummaryService>()
                //.AddScoped<IUploadService, HeroesProfileUploaderService>() /* disabled due to HeroesProfile shutting down their top level API. Re-add and implement at a later date if this changes */                
                ;

            return services;
        }
    }
}
