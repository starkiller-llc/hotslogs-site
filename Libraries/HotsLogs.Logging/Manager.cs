using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Linq;
using System.Reflection;

namespace HotsLogs.Logging;

public static class Manager
{
    private static Logger _logger;

    static Manager()
    {
        Init();
    }

    public static void Deinit()
    {
        _logger?.Dispose();
    }

    public static void InternalLog(Action<string, string, string> log, string source, string msg)
    {
        var lines = msg.Split('\n').Select(x => x.TrimEnd()).ToArray();
        log("[SOURCE:{source}] {msg}", source, lines[0]);
        foreach (var line in lines.Skip(1))
        {
            log("[SOURCE:{source}] >>> {msg}", source, line);
        }
    }

    private static void Init()
    {
        var logName = Assembly.GetEntryAssembly()?.GetName().Name ?? "SiteLog";
#if DEBUG
        logName += "_Dev";
#endif
        _logger ??= CreateLogger(logName);
    }

    public static Logger CreateLogger(string logName)
    {
        var loggerConfig = new LoggerConfiguration();
        return ConfigureLogger(logName, loggerConfig)
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
            .CreateLogger();
    }

    public static LoggerConfiguration ConfigureLogger(string logName, LoggerConfiguration loggerConfig)
    {
        return loggerConfig
            .WriteTo.Console()
            .WriteTo.File(
                @"C:\HotsLogs\log.txt",
                rollOnFileSizeLimit: true,
                fileSizeLimitBytes: 536870912 /* 0.5GB */,
                retainedFileCountLimit: 5,
                outputTemplate: logName + " {Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                shared: true);
    }
}
