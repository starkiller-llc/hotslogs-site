namespace HotsLogsApi.BL.Migration.MatchSummary.Models;

public class TalentUpgradesRow
{
    public int PlayerId { get; set; }
    public string PlayerName { get; set; }
    public string HeroPortraitImageURL { get; set; }
    public string Character { get; set; }
    public string CharacterURL { get; set; }
    public bool Team { get; set; }
    public string TalentImageURL { get; set; }
    public string TalentName { get; set; }
    public decimal ReplayLengthPercentAtValue0 { get; set; }
    public decimal ReplayLengthPercentAtValue1 { get; set; }
    public decimal ReplayLengthPercentAtValue2 { get; set; }
    public decimal ReplayLengthPercentAtValue3 { get; set; }
    public decimal ReplayLengthPercentAtValue4 { get; set; }
    public decimal? ReplayLengthPercentAtValue5 { get; set; }
}
