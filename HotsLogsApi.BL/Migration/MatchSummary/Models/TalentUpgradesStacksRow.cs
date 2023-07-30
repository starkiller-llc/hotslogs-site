namespace HotsLogsApi.BL.Migration.MatchSummary.Models;

public class TalentUpgradesStacksRow
{
    public int PlayerId { get; set; }
    public string PlayerName { get; set; }
    public string HeroPortraitImageURL { get; set; }
    public string Character { get; set; }
    public string CharacterURL { get; set; }
    public bool Team { get; set; }
    public string TalentImageURL { get; set; }
    public string TalentName { get; set; }
    public int Stacks { get; set; }
}
