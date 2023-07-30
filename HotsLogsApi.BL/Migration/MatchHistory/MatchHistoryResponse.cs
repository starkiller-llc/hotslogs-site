using HotsLogsApi.BL.Migration.MatchHistory.Models;
using System;

namespace HotsLogsApi.BL.Migration.MatchHistory;

public class MatchHistoryResponse
{
    public MatchHistoryRow[] Stats { get; set; }
    public string LiteralHeaderLinks { get; set; }
    public bool Unauthorized { get; set; }
    public DateTime? PremiumSupporterSince { get; set; }
    public bool? ShowShareColumn { get; set; }
    public int[] OtherPlayerIds { get; set; }
    public int Total { get; set; }
    public string Title { get; set; }
    public bool HideMessageLineVersion2 { get; set; }
}
