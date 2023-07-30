using System;

namespace HelperCore.RedisPOCOClasses;

[Serializable]
public class SitewideHeroTalentStatistic
{
    public string Character { get; set; }
    public int? TalentTier { get; set; }
    public int TalentID { get; set; }
    public string TalentName { get; set; }
    public string TalentDescription { get; set; }
    public int GamesPlayed { get; set; }
    public decimal WinPercent { get; set; }
}