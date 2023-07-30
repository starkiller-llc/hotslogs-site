using System;
using System.Collections.Generic;

namespace HelperCore.RedisPOCOClasses;

[Serializable]
public class TeamProfile
{
    public int ID { get; set; }
    public string Name { get; set; }

    public Dictionary<int, TeamProfilePlayer> Players { get; set; } = new();
    public TeamProfileReplay[] Replays { get; set; } = Array.Empty<TeamProfileReplay>();

    public TeamProfilePlayerAverageReplayCharacterUpgradeEventReplayLengthPercents[]
        PlayerAverageReplayCharacterUpgradeEventReplayLengthPercents { get; set; } =
        Array.Empty<TeamProfilePlayerAverageReplayCharacterUpgradeEventReplayLengthPercents>();

    public int[] FilterGameModes { get; set; }
    public DateTime FilterDateTimeStart { get; set; }
    public DateTime FilterDateTimeEnd { get; set; }
    public int? GamesPlayedRequired { get; set; }
    public int? TeamGamePartySize { get; set; }
    public DateTime LastUpdated { get; set; }

    public bool IsTruncated { get; set; }
}