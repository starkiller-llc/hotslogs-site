export interface Tournament {
  tournamentId: number;
  tournamentName: string;
  tournamentDescription: string;
  registrationDeadline: Date;
  endDate?: Date;
  isPublic: number;
  maxNumTeams?: number;
  entryFee: number;
  numTeams: number;
}
