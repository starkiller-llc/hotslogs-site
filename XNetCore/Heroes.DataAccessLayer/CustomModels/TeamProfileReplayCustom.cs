using System;

namespace Heroes.DataAccessLayer.CustomModels;

public class TeamProfileReplayCustom
{
    public int ReplayID { get; set; }
    public int GameMode { get; set; }
    public string Map { get; set; }
    public TimeSpan ReplayLength { get; set; }
    public DateTime TimestampReplay { get; set; }
}
