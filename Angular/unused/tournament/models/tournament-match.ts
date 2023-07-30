export interface TournamentMatch {
    matchId: number;
    tournamentId: number;
    replayId: number;
    roundNum: number;
    matchCreated: Date;
    matchDeadline: Date;
    matchTime?: Date;
    team1Id: number;
    team2Id: number;
    team1Name: string;
    team2Name: string;
    winningTeamId?: number;
}
