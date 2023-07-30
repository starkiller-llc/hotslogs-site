using HotsLogsApi.BL.Migration.Models;

namespace HotsLogsApi.BL.Migration.Overview.Models;

public class CharacterStatisticsDetailsRow : IHasHeaderRow
{
    public int? TalentTier { get; set; }
    public string TalentImageURL { get; set; }
    public string TalentName { get; set; }
    public string TalentDescription { get; set; }
    public string GamesPlayed { get; set; }
    public string WinPercent { get; set; }
    public string HeaderStart { get; set; }
}
