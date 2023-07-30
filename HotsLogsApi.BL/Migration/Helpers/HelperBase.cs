using HelperCore;
using HelperCore.RedisPOCOClasses;
using Heroes.DataAccessLayer.Data;
using Heroes.ReplayParser;
using HotsLogsApi.BL.Migration.HeroAndMap.Models;
using HotsLogsApi.BL.Migration.Models;
using HotsLogsApi.Models;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HotsLogsApi.BL.Migration.Helpers;

public abstract class HelperBase<T>
{
    private static readonly Dictionary<string, (int Start, int Count)> LevelToRange = new()
    {
        ["1-5"] = (1, 5),
        ["6-10"] = (6, 5),
        ["11-15"] = (11, 5),
        ["16-19"] = (16, 4),
        ["20+"] = (20, 1),
    };

    private readonly string[] _allPatches = { "-1" };
    private readonly HelperArgsBase _args;
    protected readonly FilterDataSources Flt;
    protected readonly KeyValuePair<string, string>[] Roles;
    protected readonly string SupportColor = TeamCompHelper.HeroRoleColorsDictionary["Support"];

    protected HelperBase(IServiceProvider svcp, HelperArgsBase args)
    {
        Svcp = svcp;
        _args = args;
        Roles = TeamCompHelper.HeroRoleColorsDictionary
            .Where(i => HeroRole.HeroRoleOrderDictionary.ContainsKey(i.Key))
            .OrderByDescending(i => HeroRole.HeroRoleOrderDictionary[i.Key])
            .ToArray();

        using var scope = Svcp.CreateScope();
        var dc = HeroesdataContext.Create(scope);
        Flt = new FilterDataSources(dc);
    }

    [PublicAPI]
    public string PatchesEmptyMessage { get; set; }

    protected IServiceProvider Svcp { get; }

    public abstract T MainCalculation();

    protected string ExportBuild(HeroTalentBuildStatistic b)
    {
        return string.Join(string.Empty, b.TalentId.Select(x => $"{x}"));
    }

    protected HeroTalentBuildStatistic[] FillTalentNames(HeroTalentBuildStatistic[] stats)
    {
        if (stats is null)
        {
            return stats;
        }

        foreach (var stat in stats)
        {
            for (var i = 0; i < 7; i++)
            {
                stat.TalentName[i] ??= GetTalentName(stat.TalentNameDescription[i]);
            }
        }

        return stats;
    }

    protected PopularTalentsRow[] FillTalentNames(PopularTalentsRow[] stats)
    {
        if (stats is null)
        {
            return stats;
        }

        foreach (var stat in stats)
        {
            stat.TalentName01 ??= GetTalentName(stat.TalentNameDescription01);
            stat.TalentName04 ??= GetTalentName(stat.TalentNameDescription04);
            stat.TalentName07 ??= GetTalentName(stat.TalentNameDescription07);
            stat.TalentName10 ??= GetTalentName(stat.TalentNameDescription10);
            stat.TalentName13 ??= GetTalentName(stat.TalentNameDescription13);
            stat.TalentName16 ??= GetTalentName(stat.TalentNameDescription16);
            stat.TalentName20 ??= GetTalentName(stat.TalentNameDescription20);
        }

        return stats;
    }

    protected List<int> GetGameModes()
    {
        if (!int.TryParse(_args.GameMode, out var selectedGameMode))
        {
            return null;
        }

        var allGameModes = new List<int>
        {
            (int)GameMode.HeroLeague,
            (int)GameMode.TeamLeague,
            (int)GameMode.StormLeague,
        };
        var gameModes = selectedGameMode == -99
            ? allGameModes
            : new List<int> { selectedGameMode };
        return gameModes;
    }

    protected string GetPreviousKey()
    {
        var selected = SelectedPatchesOrWeeks();
        var df = Flt.Time.Select(x => x.Value).ToArray();
        var pf = Flt.Patch.Select(x => x.Value).ToArray();
        var idxWeeks = selected.Select(x => Array.IndexOf(df, x)).Where(x => x != -1).ToList();
        var idxPatches = selected.Select(x => Array.IndexOf(pf, x)).Where(x => x != -1).ToList();
        if (idxPatches.Any())
        {
            var maxIdx = idxPatches.Max();
            return maxIdx == pf.Length - 1 ? "nodata" : pf[maxIdx + 1];
        }

        if (idxWeeks.Any())
        {
            var maxIdx = idxWeeks.Max();
            return maxIdx == df.Length - 1 ? "nodata" : df[maxIdx + 1];
        }

        return pf[2];
    }

    protected int[] GetSelectedGameLengths()
    {
        return _args.GameLength.Count == 0 || _args.GameLength.Count == Flt.GameLength.Length
            ? new[] { -1 }
            : _args.GameLength.ToArray();
    }

    protected int GetSelectedGameMode()
    {
        var rc = int.Parse(_args.GameMode);
        return rc;
    }

    protected int[] GetSelectedLeagues()
    {
        return _args.League.Count == 0 || _args.League.Count == Flt.LeagueCombo.Length
            ? new[] { -1 }
            : _args.League.ToArray();
    }

    protected int[] GetSelectedLevels()
    {
        return _args.Level.Count == 0 || _args.Level.Count == Flt.Level.Length
            ? new[] { -1 }
            : _args.Level.SelectMany(x => Enumerable.Range(LevelToRange[x].Start, LevelToRange[x].Count)).ToArray();
    }

    protected string[] GetSelectedMaps()
    {
        return _args.Map.Count == 0 || _args.Map.Count == Flt.MapCombo.Length
            ? new[] { "-1" }
            : _args.Map.ToArray();
    }

    protected string GetTalentName(string desc)
    {
        var p = desc?.Split(':');
        if (p?[0].EndsWith("Word") ?? false)
        {
            return $"{p[0]}:{p[1]}";
        }

        return p?[0];
    }

    protected GameEventTeam[] GetTeamsIfTournamentSelected(IServiceScope scope)
    {
        if (_args.GameModeEx != "0")
        {
            return null;
        }

        var eventId = int.Parse(_args.Tournament);
        var teams = GetTeams(scope, eventId);
        return teams;
    }

    protected int MinRequiredGames(string gameModeZ) => Global.IsEventGameMode(gameModeZ)
        ? 0
        : DataHelper.GamesPlayedRequirementForWinPercentDisplay;

    protected int MinRequiredGamesForWinRate(string gameModeZ) => Global.IsEventGameMode(gameModeZ) ? 0 : 5;

    protected string[] SelectedPatchesOrWeeks()
    {
        if (Global.IsEventGameMode(_args.GameMode))
        {
            return _allPatches;
        }

        var weeks = _args.Time.ToArray();
        var patches = _args.Patch.ToArray();

        PatchesEmptyMessage = @"Patches";

        if (patches.Any())
        {
            return patches;
        }

        if (weeks.Any())
        {
            return weeks;
        }

        var lastNPatches = GetDefaultNumberOfPatchesByNumberOfGamesOnRecord();
        var patchDescription = lastNPatches == 1 ? "Patch" : $"{lastNPatches} Patches";

        PatchesEmptyMessage = $@"Last {patchDescription}";

        var patchKeys = Enumerable.Range(0, 2)
            .Take(lastNPatches)
            .Reverse()
            .Select(x => $"{Flt.GameVersions[x].BuildFirst}-{Flt.GameVersions[x].BuildLast}")
            .ToArray();

        return patchKeys;
    }

    private int GetDefaultNumberOfPatchesByNumberOfGamesOnRecord()
    {
        var gameMode = int.Parse(_args.GameMode);

        var latestGameVersions = DataHelper.GetGameVersions();

        var latestPatch = latestGameVersions.Select(x => $"{x.BuildFirst}-{x.BuildLast}").First();
        var ratedGamesLatestPatchKey =
            $"HOTSLogs:NumberOfRatedGamesV1:{latestPatch}:-1:-1:-1:{gameMode}";
        var numberOfRatedGamesLatestPatch = DataHelper.RedisCacheGetInt(ratedGamesLatestPatchKey);

        var lastNPatches = numberOfRatedGamesLatestPatch < 10000 ? 2 : 1;
        return lastNPatches;
    }

    private GameEventTeam[] GetTeams(IServiceScope scope, int eventId)
    {
        var dc = HeroesdataContext.Create(scope);
        var teams = dc.EventTeams
            .Include(x => x.ReplayCharacterProAssociations)
            .Where(x => x.EventId == eventId)
            .ToList();

        var rc = teams.Select(GameEventsRepository.ConvertFromDb).ToArray();
        return rc;
    }
}
