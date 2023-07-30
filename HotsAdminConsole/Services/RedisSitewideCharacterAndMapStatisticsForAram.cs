using Heroes.ReplayParser;
using JetBrains.Annotations;
using System;

namespace HotsAdminConsole.Services;

[UsedImplicitly]
[HotsService("Weekly Stats (ARAM)", KeepRunning = true, Sort = 6.3, AutoStart = true, Port = 17100)]
public class RedisSitewideCharacterAndMapStatisticsForAram : RedisSitewideCharacterAndMapStatisticsForGameMode
{
    public RedisSitewideCharacterAndMapStatisticsForAram(IServiceProvider svcp) : base(svcp, GameMode.ARAM) { }
}
