using System;

namespace HotsLogsApi.BL.Migration.Profile.Models;

public class ProfileSharedReplayRow
{
    public int ReplayShareID { get; set; }
    public int ReplayID { get; set; }
    public int UpvoteScore { get; set; }
    public string GameMode { get; set; }
    public string Title { get; set; }
    public string Map { get; set; }
    public TimeSpan ReplayLength { get; set; }
    public TimeSpan ReplayLengthMinutes { get; set; }
    public string Characters { get; set; }
    public double AverageCharacterLevel { get; set; }
    public double? AverageMMR { get; set; }
    public DateTime TimestampReplay { get; set; }
    public DateTime TimestampReplayDate { get; set; }
}
