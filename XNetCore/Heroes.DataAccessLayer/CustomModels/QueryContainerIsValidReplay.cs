// ReSharper disable InconsistentNaming

using System;
using System.Linq;

namespace Heroes.DataAccessLayer.CustomModels;

public class QueryContainerIsValidReplay
{
    private int[] _playerIDs;
    public int ReplayID { get; set; }
    public int GameMode { get; set; }
    public int MapID { get; set; }
    public bool IsReplayShared { get; set; }

    public string PlayerIDsStr { get; set; }

    public int[] PlayerIDs => _playerIDs ??= PlayerIDsStr.Split(',').Select(int.Parse).ToArray();

    public bool IsEventReplayAndEnabled { get; set; }
    public bool IsAnyReplayTeamHeroBans { get; set; }
    public bool IsAnyReplayCharacterUpgradeEventReplayLengthPercents { get; set; }
    public bool IsAnyReplayPeriodicXPBreakdowns { get; set; }
    public bool IsAnyReplayTeamObjectives { get; set; }
    public TimeSpan ReplayLength { get; set; }
    public DateTime TimestampReplay { get; set; }
    public DateTime TimestampCreated { get; set; }
}
