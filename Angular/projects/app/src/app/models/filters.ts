export interface Version {
}

export interface GameVersion {
  Version: Version;
  BuildFirst: number;
  BuildLast: number;
  Builds: number[];
  Title: string;
}

export interface Time {
  Key: string;
  Value: string;
  Checked: boolean;
}

export interface Patch {
  Key: string;
  Value: string;
  Checked: boolean;
}

export interface Tournament {
  TournamentDisplayText: string;
  TournamentId: number;
}

export interface LeagueList {
  LeagueDisplayText: string;
  LeagueID: number;
}

export interface LeagueCombo {
  LeagueDisplayText: string;
  LeagueID: number;
}

export interface MapList {
  DisplayName: string;
  IdentifierId: number;
  PrimaryName: string;
}

export interface Hero {
  DisplayName: string;
  PrimaryName: string;
}

export interface MapCombo {
  DisplayName: string;
  PrimaryName: string;
}

export interface GameModeEx {
  GameModeExDisplayText: string;
  GameModeEx: number;
}

export interface GameMode {
  GameModeDisplayText: string;
  GameMode: number;
}

export interface GameLength {
  ReplayGameLengthDisplayText: string;
  ReplayGameLengthValue: number;
}

export interface Level {
  CharacterLevelDisplayText: string;
  CharacterLevelValue: string;
}

export interface Season {
  ResetDate: Date;
  Title: string;
}

export interface FilterDefinitions {
  GameVersions: GameVersion[];
  Now: Date;
  Time: Time[];
  Patch: Patch[];
  Tournament: Tournament[];
  LeagueList: LeagueList[];
  LeagueCombo: LeagueCombo[];
  MapList: MapList[];
  MapCombo: MapCombo[];
  GameModeEx: GameModeEx[];
  GameMode: GameMode[];
  Hero: Hero[];
  GameLength: GameLength[];
  Level: Level[];
  Season: Season[];
}
