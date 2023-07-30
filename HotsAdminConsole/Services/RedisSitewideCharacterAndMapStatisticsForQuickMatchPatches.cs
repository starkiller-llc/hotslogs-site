using Heroes.ReplayParser;
using JetBrains.Annotations;
using System;

namespace HotsAdminConsole.Services;

[UsedImplicitly]
[HotsService("Patch Stats (QM)", KeepRunning = true, Sort = 6.25, AutoStart = true, Port = 17022)]
public class
    RedisSitewideCharacterAndMapStatisticsForQuickMatchPatches : RedisSitewideCharacterAndMapStatisticsForGameMode
{
    public RedisSitewideCharacterAndMapStatisticsForQuickMatchPatches(IServiceProvider svcp) : base(svcp, GameMode.QuickMatch, true) { }
}
