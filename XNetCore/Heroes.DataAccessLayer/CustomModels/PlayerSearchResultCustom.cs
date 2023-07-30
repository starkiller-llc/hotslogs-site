using System;

namespace Heroes.DataAccessLayer.CustomModels;

public class PlayerSearchResultCustom
{
    public int BattleNetRegionId { get; set; }
    public int PlayerID { get; set; }
    public string Name { get; set; }
    public int? CurrentMMR { get; set; }
    public decimal? GamesPlayed { get; set; }
    public DateTime? PremiumSupporterSince { get; set; }
}
