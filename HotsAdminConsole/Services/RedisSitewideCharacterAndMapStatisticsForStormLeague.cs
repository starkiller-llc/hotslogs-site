using Heroes.ReplayParser;
using JetBrains.Annotations;
using System;

namespace HotsAdminConsole.Services;

[UsedImplicitly]
[HotsService("Weekly Stats (SL)", KeepRunning = true, Sort = 6.1, AutoStart = true, Port = 17024)]
public class RedisSitewideCharacterAndMapStatisticsForStormLeague : RedisSitewideCharacterAndMapStatisticsForGameMode
{
    public RedisSitewideCharacterAndMapStatisticsForStormLeague(IServiceProvider svcp) : base(svcp, GameMode.StormLeague) { }
}
