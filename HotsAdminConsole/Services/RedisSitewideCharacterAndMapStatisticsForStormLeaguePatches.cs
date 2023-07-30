using Heroes.ReplayParser;
using JetBrains.Annotations;
using System;

namespace HotsAdminConsole.Services;

[UsedImplicitly]
[HotsService("Patch Stats (SL)", KeepRunning = true, Sort = 6.15, AutoStart = true, Port = 17026)]
public class
    RedisSitewideCharacterAndMapStatisticsForStormLeaguePatches : RedisSitewideCharacterAndMapStatisticsForGameMode
{
    public RedisSitewideCharacterAndMapStatisticsForStormLeaguePatches(IServiceProvider svcp) : base(svcp, GameMode.StormLeague, true) { }
}
