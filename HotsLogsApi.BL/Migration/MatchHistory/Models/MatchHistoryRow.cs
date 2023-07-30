using System;

namespace HotsLogsApi.BL.Migration.MatchHistory.Models;

public class MatchHistoryRow
{
    public int ReplayID { get; set; }
    public string Map { get; set; }
    public TimeSpan ReplayLength { get; set; }
    public TimeSpan ReplayLengthMinutes { get; set; }
    public string Character { get; set; }
    public string CharacterURL { get; set; }
    public int CharacterLevel { get; set; }
    public int Result { get; set; }
    public int? MMRBefore { get; set; }
    public int? MMRChange { get; set; }
    public int? ReplayShare { get; set; }
    public DateTime TimestampReplay { get; set; }
    public DateTime TimestampReplayDate { get; set; }
    public string Role { get; set; }
    public long TimestampReplayTicks { get; set; }

    public bool Season { get; set; }
    public string MapURL { get; set; }
}
