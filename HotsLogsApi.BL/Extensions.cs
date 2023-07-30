using Amazon;
using Amazon.S3;
using HelperCore;
using Heroes.DataAccessLayer.Data;
using HotsLogsApi.BL.Migration.Helpers;
using HotsLogsApi.BL.Migration.MatchHistory;
using HotsLogsApi.BL.Migration.Profile;
using HotsLogsApi.BL.Migration.ProfileImage;
using HotsLogsApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ServiceStackReplacement;
using StackExchange.Redis;

namespace HotsLogsApi.BL;

public static class Extensions
{
    public static void AddHotsLogsApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<HeroesdataContext>(
            opts =>
            {
                var connStr = configuration.GetConnectionString("DefaultConnection");
                opts.UseMySql(connStr, ServerVersion.AutoDetect(connStr),
                    o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery));
            });

        services.AddScoped(
            svcp =>
            {
                var connStr = configuration.GetConnectionString("RedisHost");
                var mux = ConnectionMultiplexer.Connect(connStr);
                return mux.GetClient();
            });

        services.AddScoped<UserRepository>();
        services.AddScoped<PlayerRepository>();
        services.AddScoped<TournamentRepository>();
        services.AddScoped<GameEventsRepository>();

        // Explicitly requested services
        services.AddScoped<EventHelper>();
        services.AddSingleton<MMR>();
        services.AddScoped<MatchHistoryHelper>();
        services.AddScoped<PlayerNameDictionary>();
        services.AddScoped<ProfileHelper>();
        services.AddScoped<ReplayCharacterHelper>();

        // Services requested by constructors
        services.AddScoped<PayPalHelper2>();
        services.AddScoped<BnetHelper>();
        services.AddScoped<PayPalHelper>();
        services.AddScoped<PlayerProfileImage>();
        services.AddSingleton<UploadHelper>();
        services.AddTransient(
            svcp =>
            {
                var opts = svcp.GetRequiredService<IOptions<HotsLogsOptions>>();
                return new AmazonS3Client(
                    opts.Value.AwsAccessKeyID,
                    opts.Value.AwsSecretAccessKey,
                    RegionEndpoint.USWest2);
            });
    }
}
