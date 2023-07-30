using HotsLogsApi.BL.Migration.Models;

namespace HotsLogsApi.BL.Migration.TalentDetails.Models;

public class TalentDetailsRow<T> : IHasHeaderRow
{
    public int? TalentTier { get; set; }
    public string TalentImageURL { get; set; }
    public string TalentName { get; set; }
    public string TalentDescription { get; set; }
    public T GamesPlayed { get; set; }
    public string Popularity { get; set; }
    public string WinPercent { get; set; }
    public string HeaderStart { get; set; }
}
