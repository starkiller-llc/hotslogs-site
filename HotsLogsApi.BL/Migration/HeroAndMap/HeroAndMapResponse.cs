using HotsLogsApi.BL.Migration.HeroAndMap.Models;
using HotsLogsApi.Models;
using System.Collections.Generic;

namespace HotsLogsApi.BL.Migration.HeroAndMap;

public class HeroAndMapResponse
{
    public GameEventTeam[] Teams { get; set; }
    public string LastUpdatedText { get; set; }
    public StatisticsGridRow[] Stats { get; set; }
    public PopularTalentsRow[] PopularTalentBuilds { get; set; }
    public bool RecentPatchNoteVisible { get; set; }
    public bool PopularityNotice { get; set; }
    public bool GameLengthFilterNotice { get; set; }
    public bool CharacterLevelFilterNotice { get; set; }
    public bool BanDataAvailable { get; set; }
    public KeyValuePair<string, string>[] Roles { get; set; }
}
