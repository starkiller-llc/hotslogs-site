using System;

namespace HelperCore.RedisPOCOClasses;

[Serializable]
public class GroupFinderListingPOCO
{
    public int PlayerID { get; set; }
    public string Name { get; set; }
    public int BattleTag { get; set; }
    public string Information { get; set; }
    public int MMRRatingSearchRangeBegin { get; set; }
    public int MMRRatingSearchRangeEnd { get; set; }
    public int MMRRating { get; set; }
}
