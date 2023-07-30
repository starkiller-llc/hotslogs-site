using Heroes.ReplayParser;
using JetBrains.Annotations;
using System;

namespace HotsAdminConsole.Services;

[UsedImplicitly]
[HotsService("Weekly Stats (QM)", KeepRunning = true, Sort = 6.2, AutoStart = true, Port = 17020)]
public class RedisSitewideCharacterAndMapStatisticsForQuickMatch : RedisSitewideCharacterAndMapStatisticsForGameMode
{
    public RedisSitewideCharacterAndMapStatisticsForQuickMatch(IServiceProvider svcp) : base(svcp, GameMode.QuickMatch) { }
}
