using HelperCore;
using Heroes.DataAccessLayer.Data;
using Heroes.DataAccessLayer.Models;
using Heroes.ReplayParser;
using HotsLogsApi.BL.Resources;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace HotsLogsApi.BL.Migration.Models;

public class FilterDataSources
{
    private readonly HeroesdataContext _dc;
    public const int GameTimeFilterMinuteInterval = 5;
    private readonly DateTime _march2019 = new(2019, 3, 1);

    private readonly List<GameEvent> _gameEvents;
    public List<GameVersion> GameVersions { get; }

    public FilterDataSources(HeroesdataContext dc)
    {
        _dc = dc;
        _gameEvents = DataHelper.GetGameEvents();
        var gameVersions = DataHelper.GetGameVersions(true).AsEnumerable();
        GameVersions = gameVersions.Reverse().Skip(1).Reverse().ToList();

        Time = GetTimeFilterDataSource().ToArray();
        Patch = GetPatchFilterDataSource().ToArray();
        Tournament = GetTournamentFilterDataSource();
        LeagueCombo = GetLeagueFilterComboDataSource();
        LeagueList = GetLeagueFilterListDataSource();
        MapList = GetMapFilterListDataSource();
        MapCombo = GetMapFilterComboDataSource();
        GameMode = GetGameModeDataSource().ToArray();
        GameModeEx = GameModeExDataSource();
        Hero = GetHeroesDataSource();
        Level = GetLevelDataSource();
        GameLength = GetGameLengthDataSource();
        Season = GetSeasonDataSource();
    }

    private GameLengthRow[] GetGameLengthDataSource()
    {
        var dropDownGameLengthDataSource = new List<(int, string)>();
        for (var i = 10; i + GameTimeFilterMinuteInterval <= 30; i += GameTimeFilterMinuteInterval)
        {
            dropDownGameLengthDataSource.Add((i, $"{i}-{i + GameTimeFilterMinuteInterval - 1} minute Games"));
        }

        var rc = dropDownGameLengthDataSource
            .Select(
                i => new GameLengthRow
                {
                    ReplayGameLengthDisplayText = i.Item2,
                    ReplayGameLengthValue = i.Item1,
                }).ToArray();

        return rc;
    }

    private LevelRow[] GetLevelDataSource()
    {
        var levels = new List<(string, string)>
        {
            ("1-5", "Hero Levels 1-5"),
            ("6-10", "Hero Levels 6-10"),
            ("11-15", "Hero Levels 11-15"),
            ("16-19", "Hero Levels 16-19"),
            ("20+", "Hero Levels 20+"),
        };

        var rc = levels
            .Select(
                i => new LevelRow
                {
                    CharacterLevelDisplayText = i.Item2,
                    CharacterLevelValue = i.Item1,
                }).ToArray();

        return rc;
    }

#if !LOCALDEBUG
    public DateTime Now => DateTime.UtcNow;
#else
        public DateTime Now { get; } = new DateTime(2022, 5, 3, 12, 0, 0, DateTimeKind.Utc);
#endif

    public WeekOrPatchFilterValue[] Time { get; }
    public WeekOrPatchFilterValue[] Patch { get; }
    public TournamentFilterRow[] Tournament { get; }
    public LeagueFilterRow[] LeagueList { get; }
    public LeagueFilterRow[] LeagueCombo { get; }
    public MapFilterRow[] MapList { get; }
    public HeroesDataSource[] MapCombo { get; }
    public GameModeExFilterRow[] GameModeEx { get; }
    public GameModeFilterRow[] GameMode { get; }
    public HeroesDataSource[] Hero { get; }
    public LevelRow[] Level { get; }
    public GameLengthRow[] GameLength { get; }
    public PlayerMmrReset[] Season { get; }

    private List<WeekOrPatchFilterValue> GetTimeFilterDataSource()
    {
        var dataSource = new List<(DateTime Date, string Key)>
        {
            (DateTime.MaxValue, "CurrentBuild"),
        };
        var dtStart = Now.AddDays(-90).StartOfWeek(DayOfWeek.Sunday);
        for (var dt = dtStart; dt < Now.AddDays(7); dt = dt.AddDays(7))
        {
            dataSource.Add((dt, dt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)));
        }

        return dataSource
            .OrderByDescending(i => i.Date)
            .Select(
                i => new WeekOrPatchFilterValue
                {
                    Key = i.Date != DateTime.MaxValue
                        ? string.Format(
                            LocalizedText.GenericWeekOfDate,
                            $"{i.Date.AddDays(-7).ToShortDateString()} - {i.Date.AddDays(-1).ToShortDateString()}")
                        : "Last 7 Days (Current Build)",
                    Value = i.Key,
                    Checked = false,
                })
            .ToList();
    }

    private List<WeekOrPatchFilterValue> GetPatchFilterDataSource() =>
        (from x in GameVersions
         select new WeekOrPatchFilterValue
         {
             Key = x.Title,
             Value = $"{x.BuildFirst}-{x.BuildLast}",
         }).ToList();

    private TournamentFilterRow[] GetTournamentFilterDataSource()
    {
        var ds = _gameEvents
            .Select(
                i => new TournamentFilterRow
                {
                    TournamentDisplayText = i.Name,
                    TournamentId = i.Id,
                })
            .OrderBy(i => i.TournamentId).ToArray();
        return ds;
    }

    private LeagueFilterRow[] GetLeagueFilterListDataSource()
    {
        var ds = new[]
        {
            new LeagueFilterRow
            {
                LeagueDisplayText = LocalizedText.GenericAllLeagues,
                LeagueID = -1,
            },
        }.Concat(
            Global.GetLeagues()
                .Select(
                    i => new LeagueFilterRow
                    {
                        LeagueDisplayText = SiteMaster.GetLocalizedString(
                            "GenericLeague",
                            i.LeagueId.ToString()),
                        LeagueID = i.LeagueId,
                    }).OrderBy(i => i.LeagueID)).ToArray();
        return ds;
    }

    private LeagueFilterRow[] GetLeagueFilterComboDataSource()
    {
        var ds = Global.GetLeagues()
            .Select(
                i => new LeagueFilterRow
                {
                    LeagueDisplayText =
                        SiteMaster.GetLocalizedString("GenericLeague", i.LeagueId + string.Empty),
                    LeagueID = i.LeagueId,
                })
            .OrderBy(i => i.LeagueID).ToArray();
        return ds;
    }

    private HeroesDataSource[] GetMapFilterComboDataSource()
    {
        var ds = Global.GetLocalizationAlias()
            .Where(i => i.Type == (int)DataHelper.LocalizationAliasType.Map)
            .Select(
                i => new HeroesDataSource(
                    SiteMaster.GetLocalizedString("GenericMapName", i.PrimaryName),
                    i.PrimaryName))
            .OrderBy(i => i.DisplayName).ToArray();
        return ds;
    }

    private MapFilterRow[] GetMapFilterListDataSource()
    {
        var ds = new[]
            {
                new LocalizationAlias
                {
                    IdentifierId = -1,
                    PrimaryName = "All Maps",
                },
            }
            .Union(
                Global.GetLocalizationAlias().Where(
                    i => i.Type == (int)DataHelper.LocalizationAliasType.Map &&
                         i.PrimaryName != "Haunted Mines" && i.PrimaryName != "Lost Cavern"))
            .Select(
                i => new MapFilterRow
                {
                    DisplayName = SiteMaster.GetLocalizedString("GenericMapName", i.PrimaryName),
                    IdentifierId = i.IdentifierId,
                    PrimaryName = i.PrimaryName,
                }).OrderBy(i => i.IdentifierId == -1 ? 0 : 1).ThenBy(i => i.DisplayName).ToArray();
        return ds;
    }

    private GameModeExFilterRow[] GameModeExDataSource()
    {
        var ds = DataHelper.GameModeWithStatistics
            .Select(
                i => new GameModeExFilterRow
                {
                    GameModeExDisplayText = ((GameMode)i).GetGameMode(),
                    GameModeEx = i,
                })
            .Concat(
                new[]
                {
                    new GameModeExFilterRow
                    {
                        GameModeExDisplayText = "Tournaments",
                        GameModeEx = 0,
                    },
                }).ToArray();
        return ds;
    }

    private GameModeFilterRow[] GetGameModeDataSource()
    {
        var standardGameModes = DataHelper.GameModeWithStatistics
            .Select(
                i => new GameModeFilterRow
                {
                    GameModeDisplayText = ((GameMode)i).GetGameMode(),
                    GameMode = i,
                });

        var eventGameModes = _gameEvents
            .Select(
                i => new GameModeFilterRow
                {
                    GameModeDisplayText = i.Name,
                    GameMode = i.Id,
                });

        var ds = standardGameModes.Concat(eventGameModes).ToArray();
        return ds;
    }

    private HeroesDataSource[] GetHeroesDataSource()
    {
        return Global.GetLocalizationAlias()
            .Where(i => i.Type == (int)DataHelper.LocalizationAliasType.Hero)
            .Select(
                i => new HeroesDataSource(
                    SiteMaster.GetLocalizedString("GenericHero", i.PrimaryName),
                    i.PrimaryName))
            .OrderBy(i => i.DisplayName)
            .ToArray();
    }

    private PlayerMmrReset[] GetSeasonDataSource()
    {
        var seasons = _dc.PlayerMmrResets
            .Where(x => x.ResetDate > _march2019)
            .OrderByDescending(x => x.ResetDate)
            .ToList();

        seasons.Insert(0, new PlayerMmrReset { Title = "All Seasons" });

        return seasons.ToArray();
    }
}

public class GameLengthRow
{
    public string ReplayGameLengthDisplayText { get; set; }
    public int ReplayGameLengthValue { get; set; }
}

public class LevelRow
{
    public string CharacterLevelDisplayText { get; set; }
    public string CharacterLevelValue { get; set; }
}
