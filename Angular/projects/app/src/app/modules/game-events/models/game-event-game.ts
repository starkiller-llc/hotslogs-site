export interface GameEventGame {
  replayId: number;
  dateTime: Date;
  winningTeamId?: number;
  losingTeamId?: number;
  mapId: number;
  map: string;
}
