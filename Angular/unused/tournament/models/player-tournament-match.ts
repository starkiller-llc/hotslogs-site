export interface PlayerTournamentMatch {
    tournamentId: number;
    tournamentName: string;
    matchId: number;
    matchDeadline: Date;
    roundNum: number;
    replayId: number;
    teamId: number;
    teamName: string;
    oppTeamId: number;
    oppTeamName: string;
    wonMatch?: number;
}
