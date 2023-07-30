export interface Role {
  Key: string;
  Value: string;
}

export interface Player {
  PlayerId: number;
  Name?: string;
  BattleTag?: string;
}

export interface Team {
  Id: number;
  EventId: number;
  Name: string;
  Players: Player[];
  LogoUrl: string;
}
