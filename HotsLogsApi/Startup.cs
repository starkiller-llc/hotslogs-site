using HelperCore;
using HotsLogsApi.Auth;
using HotsLogsApi.Auth.IsAdmin;
using HotsLogsApi.BL;
using HotsLogsApi.BL.Migration;
using HotsLogsApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace HotsLogsApi;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers(
            opts =>
            {
                var jopts = new JsonSerializerOptions { PropertyNamingPolicy = new MyNamingPolicy() };
                opts.OutputFormatters.Insert(0, new MyJsonOutputFormatter(jopts));
            });

        services.AddSwaggerGen(
            c =>
            {
                c.SwaggerDoc(
                    "v1",
                    new OpenApiInfo
                    {
                        Title = "HotsLogsApi",
                        Version = "v1",
                    });
            });

        services.AddDataProtection()
            .PersistKeysToFileSystem(new DirectoryInfo(@"c:\CookieDataProtection"))
            .SetApplicationName("HOTSLogsAviad");

        services.AddHttpContextAccessor();
        services.AddScoped<IPasswordHasher<ApplicationUser>, MyPasswordHasher2>();
        services.AddIdentityCore<ApplicationUser>()
            .AddClaimsPrincipalFactory<MyUserClaimsPrincipalFactory>()
            .AddUserStore<HotsLogsUserStore>()
            .AddDefaultTokenProviders()
            .AddSignInManager<MySigninManager>();

        services.Configure<HotsLogsOptions>(Configuration.GetSection("HotsLogsOptions"));

#if LOCALDEBUG
        services.Configure<PayPalOptions>(Configuration.GetSection("PayPalSandbox"));
        services.Configure<BnetOptions>(Configuration.GetSection("BattleNetLocalDebug"));
#elif DEBUG
        services.Configure<PayPalOptions>(Configuration.GetSection("PayPalSandbox"));
        services.Configure<BnetOptions>(Configuration.GetSection("BattleNetDebug"));
#else
        services.Configure<PayPalOptions>(Configuration.GetSection("PayPal"));
        services.Configure<BnetOptions>(Configuration.GetSection("BattleNet"));
#endif

        services.Configure<IdentityOptions>(
            opts =>
            {
                // TODO: quick hack to allow users to register because password requirement errors are not displayed in the register process -- Aviad, 6-Jun-2023
                opts.Password.RequireDigit = false;
                opts.Password.RequireLowercase = false;
                opts.Password.RequireNonAlphanumeric = false;
                opts.Password.RequireUppercase = false;
                opts.Password.RequiredLength = 1;
                opts.Password.RequiredUniqueChars = 0;
            });

        services.AddAuthentication("Identity.Application")
            .AddCookie(
                "Identity.Application",
                options =>
                {
                    options.Cookie.Name = ".AspNet.HOTSLogsAviad";
                    options.Cookie.Path = "/";
#if !LOCALDEBUG
                    options.Cookie.Domain = ".hotslogs.com";
#endif
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

                    options.Events.OnValidatePrincipal = async c =>
                    {
                        await SecurityStampValidator.ValidatePrincipalAsync(c);
                    };

                    options.Events.OnRedirectToAccessDenied =
                        options.Events.OnRedirectToLogin =
                            c =>
                            {
                                c.Response.StatusCode = StatusCodes.Status401Unauthorized;
                                return Task.FromResult<object>(null);
                            };
                });

        services.AddAuthorization(
            options =>
            {
                options.AddPolicy(
                    "IsAdmin",
                    policy =>
                        policy.Requirements.Add(new IsAdminRequirement()));
            });
        services.AddSingleton<IAuthorizationHandler, IsAdminHandler>();

        /* HotsLogs Domain Services */
        services.AddHotsLogsApiServices(Configuration);

        services.AddResponseCompression(options => options.EnableForHttps = true);

        services.AddCors(
            options =>
            {
                options.AddPolicy(
                    "CorsPolicy",
                    builder => builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
            });

        services.Configure<FormOptions>(
            o =>
            {
                o.ValueLengthLimit = int.MaxValue;
                o.MultipartBodyLengthLimit = int.MaxValue;
                o.MemoryBufferThreshold = int.MaxValue;
            });
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        Global.SetServiceProvider(app.ApplicationServices);
        DataHelper.SetServiceProvider(app.ApplicationServices);

        var tmpLogger = app.ApplicationServices.GetRequiredService<ILogger<Startup>>();
        tmpLogger.LogInformation("ZZZZZZZZZZZZZZ STARTUP");

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "HotsLogsApi v1"));
        }

        app.UseForwardedHeaders(
            new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor |
                                   ForwardedHeaders.XForwardedProto,
            });

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseResponseCompression();

        app.Use(
            (context, next) =>
            {
                if (!context.Request.Cookies.ContainsKey("CultureInfo"))
                {
                    return next(context);
                }

                var lang = context.Request.Cookies["CultureInfo"]!;
                Thread.CurrentThread.CurrentCulture = new CultureInfo(lang);
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(lang);

                return next(context);
            });

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(
            endpoints =>
            {
                endpoints.MapControllers();
            });

        try
        {
            var uploaderPath = Path.Combine("wwwroot", "HOTSLogsUploader", "HOTSLogsUploader.exe");
            var serverVersionInfo = FileVersionInfo.GetVersionInfo(uploaderPath);

            UploaderVersionHelper.Version = serverVersionInfo;
        }
        catch
        {
            /* ignored */
        }
    }
}
