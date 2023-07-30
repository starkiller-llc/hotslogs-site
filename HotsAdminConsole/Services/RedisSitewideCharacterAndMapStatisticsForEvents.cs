using JetBrains.Annotations;
using System;

namespace HotsAdminConsole.Services;

[UsedImplicitly]
[HotsService("Events/Tournaments Stats", KeepRunning = true, Sort = 6.15, AutoStart = true, Port = 17104)]
public class RedisSitewideCharacterAndMapStatisticsForEvents : RedisSitewideCharacterAndMapStatisticsForGameMode
{
    public RedisSitewideCharacterAndMapStatisticsForEvents(IServiceProvider svcp) : base(svcp, 0, gameEvent: true) { }
}
