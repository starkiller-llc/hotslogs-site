using Heroes.ReplayParser;
using JetBrains.Annotations;
using System;

namespace HotsAdminConsole.Services;

[UsedImplicitly]
[HotsService("Weekly Stats (UD)", KeepRunning = true, Sort = 6.3, AutoStart = true, Port = 17028)]
public class RedisSitewideCharacterAndMapStatisticsForUnrankedDraft : RedisSitewideCharacterAndMapStatisticsForGameMode
{
    public RedisSitewideCharacterAndMapStatisticsForUnrankedDraft(IServiceProvider svcp) : base(svcp, GameMode.UnrankedDraft) { }
}
