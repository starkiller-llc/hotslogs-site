using System;

namespace HelperCore.RedisPOCOClasses;

[Serializable]
public class ReplayShares
{
    public ReplaySharePOCO[] ReplaySharesList { get; set; }
    public int BattleNetRegionId { get; set; }
    public DateTime LastUpdated { get; set; }
}