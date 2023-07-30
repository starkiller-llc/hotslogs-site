using System;

namespace HelperCore.RedisPOCOClasses;

[Serializable]
public class GroupFinderListings
{
    public GroupFinderListingPOCO[] ReplaySharesList { get; set; }
    public int BattleNetRegionId { get; set; }
    public int GroupFinderListingTypeID { get; set; }
    public DateTime LastUpdated { get; set; }
}