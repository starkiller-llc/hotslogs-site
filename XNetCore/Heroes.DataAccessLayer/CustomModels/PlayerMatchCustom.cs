using System;

namespace Heroes.DataAccessLayer.CustomModels;

public class PlayerMatchCustom
{
    public int ReplayID { get; set; }
    public int GameMode { get; set; }
    public string Map { get; set; }
    public TimeSpan ReplayLength { get; set; }
    public string Characters { get; set; }
    public int CharacterLevel { get; set; }
    public int? MMRBefore { get; set; }
    public int? MMRChange { get; set; }
    public DateTime TimestampReplay { get; set; }
}
