namespace HotsLogsApi.BL.Migration.Vote;

public class VoteResponse
{
    public bool Success { get; set; }
    public string ErrorMessage { get; set; }
    public bool? Up { get; set; }
    public bool? Down { get; set; }
    public int VotingPlayer { get; set; }
    public int TargetPlayer { get; set; }
    public int SelfRep { get; set; }
    public int TargetRep { get; set; }
    public int NeededRep { get; set; }
    public int Rep { get; set; }
    public int ReplayId { get; set; }
}
