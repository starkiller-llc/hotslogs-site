using System;

namespace Heroes.DataAccessLayer.CustomModels;

public class PlayerMatchCustom2
{
    public int ReplayID { get; set; }
    public int GameMode { get; set; }
    public int MapID { get; set; }
    public TimeSpan ReplayLength { get; set; }
    public int CharacterID { get; set; }
    public int CharacterLevel { get; set; }
    public UInt64 IsWinner { get; set; }
    public int? MMRBefore { get; set; }
    public int? MMRChange { get; set; }
    public UInt64 IsReplayShareable { get; set; }
    public DateTime TimestampReplay { get; set; }
}
