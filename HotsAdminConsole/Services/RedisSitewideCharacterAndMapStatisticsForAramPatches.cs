using Heroes.ReplayParser;
using JetBrains.Annotations;
using System;

namespace HotsAdminConsole.Services;

[UsedImplicitly]
[HotsService("Patch Stats (ARAM)", KeepRunning = true, Sort = 6.35, AutoStart = true, Port = 17102)]
public class RedisSitewideCharacterAndMapStatisticsForAramPatches : RedisSitewideCharacterAndMapStatisticsForGameMode
{
    public RedisSitewideCharacterAndMapStatisticsForAramPatches(IServiceProvider svcp) : base(svcp, GameMode.ARAM, true) { }
}
