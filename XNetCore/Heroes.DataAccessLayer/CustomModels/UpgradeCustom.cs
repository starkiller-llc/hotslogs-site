using System;

namespace Heroes.DataAccessLayer.CustomModels;

public class UpgradeCustom
{
    public Int64 PlayerID { get; set; }
    public int UpgradeEventType { get; set; }
    public int UpgradeEventValue { get; set; }
    public decimal ReplayLengthPercent { get; set; }
    public Int64 GamesPlayed { get; set; }
    public decimal GamesWon { get; set; }
}
