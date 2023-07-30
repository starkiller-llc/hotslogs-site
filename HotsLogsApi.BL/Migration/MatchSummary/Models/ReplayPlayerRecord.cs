using HotsLogsApi.BL.Migration.Models;

namespace HotsLogsApi.BL.Migration.MatchSummary.Models;

public class ReplayPlayerRecord : IHasHeaderRow
{
    public string MatchAwards { get; set; }
    public AwardRow[] MatchAwards2 { get; set; }
    public int? PlayerID { get; set; }
    public int RealPlayerID { get; set; }
    public string PlayerName { get; set; }
    public bool VoteUp { get; set; }
    public bool VoteDown { get; set; }
    public bool ShowVoteIcons { get; set; }
    public string Character { get; set; }
    public string CharacterURL { get; set; }
    public string HeroPortraitImageURL { get; set; }
    public int CharacterLevel { get; set; }
    public double? CharacterLevelNumber { get; set; }
    public string TalentImageURL01 { get; set; }
    public string TalentImageURL04 { get; set; }
    public string TalentImageURL07 { get; set; }
    public string TalentImageURL10 { get; set; }
    public string TalentImageURL13 { get; set; }
    public string TalentImageURL16 { get; set; }
    public string TalentImageURL20 { get; set; }
    public string TalentNameDescription01 { get; set; }
    public string TalentNameDescription04 { get; set; }
    public string TalentNameDescription07 { get; set; }
    public string TalentNameDescription10 { get; set; }
    public string TalentNameDescription13 { get; set; }
    public string TalentNameDescription16 { get; set; }
    public string TalentNameDescription20 { get; set; }
    public bool Team { get; set; }
    public int MMRBefore { get; set; }
    public int? MMRChange { get; set; }
    public int Reputation { get; set; }
    public string TalentName01 { get; set; }
    public string TalentName04 { get; set; }
    public string TalentName07 { get; set; }
    public string TalentName10 { get; set; }
    public string TalentName13 { get; set; }
    public string TalentName16 { get; set; }
    public string TalentName20 { get; set; }
    public string HeaderStart { get; set; }
}
