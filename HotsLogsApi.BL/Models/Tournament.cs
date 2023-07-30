using System;

namespace HotsLogsApi.Models;

public class Tournament
{
    public int TournamentId { get; set; }
    public string TournamentName { get; set; }
    public string TournamentDescription { get; set; }
    public DateTime RegistrationDeadline { get; set; }
    public DateTime? EndDate { get; set; }
    public int IsPublic { get; set; }
    public int? MaxNumTeams { get; set; }
    public decimal EntryFee { get; set; }
    public int NumTeams { get; set; }
}

public class TournamentRegistrationApplication
{
    public int TournamentId { get; set; }
    public string TeamName { get; set; }
    public string CaptainEmail { get; set; }
    public string Battletag1 { get; set; }
    public string Battletag2 { get; set; }
    public string Battletag3 { get; set; }
    public string Battletag4 { get; set; }
    public string Battletag5 { get; set; }
    public string Battletag6 { get; set; }
    public string Battletag7 { get; set; }
    public string PaypalEmail { get; set; }
    public int IsPaid { get; set; }
}

public class TournamentRegistration : TournamentRegistrationApplication
{
    public int TeamId { get; set; }
}

public class TournamentTeam
{
    public int TeamId { get; set; }
    public int TournamentId { get; set; }
    public string TeamName { get; set; }
    public string CaptainEmail { get; set; }
    public string PaypalEmail { get; set; }
    public int IsPaid { get; set; }
}

public class TournamentParticipant
{
    public int ParticipantId { get; set; }
    public int TournamentId { get; set; }
    public int TeamId { get; set; }
    public string Battletag { get; set; }
}

public class TournamentMatch
{
    public int MatchId { get; set; }
    public int TournamentId { get; set; }
    public int? ReplayId { get; set; }
    public int RoundNum { get; set; }
    public DateTime MatchCreated { get; set; }
    public DateTime MatchDeadline { get; set; }
    public DateTime? MatchTime { get; set; }
    public int Team1Id { get; set; }
    public int Team2Id { get; set; }
    public string Team1Name { get; set; }
    public string Team2Name { get; set; }
    public int? WinningTeamId { get; set; }
}
