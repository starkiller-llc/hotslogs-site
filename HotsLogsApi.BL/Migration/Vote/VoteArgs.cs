namespace HotsLogsApi.BL.Migration.Vote;

public class VoteArgs
{
    public bool Up { get; set; }
    public int PlayerId { get; set; }
    public int ReplayId { get; set; }
    public bool Perform { get; set; } // true means do, false means undo
}
