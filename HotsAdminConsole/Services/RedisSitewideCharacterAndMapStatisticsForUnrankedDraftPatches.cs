using Heroes.ReplayParser;
using JetBrains.Annotations;
using System;

namespace HotsAdminConsole.Services;

[UsedImplicitly]
[HotsService("Patch Stats (UD)", KeepRunning = true, Sort = 6.35, AutoStart = true, Port = 17030)]
public class
    RedisSitewideCharacterAndMapStatisticsForUnrankedDraftPatches : RedisSitewideCharacterAndMapStatisticsForGameMode
{
    public RedisSitewideCharacterAndMapStatisticsForUnrankedDraftPatches(IServiceProvider svcp) : base(svcp, GameMode.UnrankedDraft, true) { }
}
