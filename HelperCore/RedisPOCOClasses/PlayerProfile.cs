using System;

namespace HelperCore.RedisPOCOClasses;

[Serializable]
public class PlayerProfile
{
    public PlayerProfileCharacterStatistic[] PlayerProfileCharacterStatistics { get; set; }
    public PlayerProfileCharacterRoleStatistic[] PlayerProfileCharacterRoleStatistics { get; set; }
    public PlayerProfileMapStatistic[] PlayerProfileMapStatistics { get; set; }
    public PlayerProfileCharacterStatistic[] PlayerProfileCharacterWinPercentVsOtherCharacters { get; set; }
    public PlayerProfileCharacterStatistic[] PlayerProfileCharacterWinPercentWithOtherCharacters { get; set; }
    public PlayerProfileMMRMilestoneV3[] PlayerProfileMMRMilestonesV3 { get; set; }
    public SitewideCharacterGameTimeWinRate[] PlayerProfileGameTimeWinRate { get; set; }
    public PlayerProfilePlayerRelationship[] PlayerProfileFriends { get; set; }
    public PlayerProfilePlayerRelationship[] PlayerProfileRivals { get; set; }
    public ReplaySharePOCO[] PlayerProfileSharedReplays { get; set; }
    public int PlayerID { get; set; }
    public int BattleNetRegionId { get; set; }
    public string PlayerName { get; set; }
    public PlayerLeaderboardRanking[] LeaderboardRankings { get; set; }
    public int TotalGamesPlayed { get; set; }
    public decimal OverallMVPPercent { get; set; }
    public decimal OverallWinPercent { get; set; }
    public TimeSpan TotalTimePlayed { get; set; }
    public int? FilterGameMode { get; set; }
    public DateTime FilterDateTimeStart { get; set; }
    public DateTime FilterDateTimeEnd { get; set; }
    public DateTime LastUpdated { get; set; }
    public int Reputation { get; set; }
}