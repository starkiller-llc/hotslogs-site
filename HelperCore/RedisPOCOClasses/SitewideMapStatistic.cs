using System;

namespace HelperCore.RedisPOCOClasses;

[Serializable]
public class SitewideMapStatistic
{
    public string Map { get; set; }
    public string Character { get; set; }
    public int GamesPlayed { get; set; }
    public int GamesBanned { get; set; }
    public TimeSpan AverageLength { get; set; }
    public decimal WinPercent { get; set; }

    public AverageTeamProfileReplayCharacterScoreResult AverageScoreResult { get; set; } = new();
}