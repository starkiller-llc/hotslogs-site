import { Role } from '../model';

export interface DataSource {
  HeroPortraitURL: string;
  Character: string;
  CharacterURL: string;
  GamesPlayed: number;
  GamesBanned: number;
  Popularity: string;
  WinPercent: string;
  WinPercentDelta: number;
  Role: string;
  AliasCSV: string;
}

export interface Constants {
  Header: string;
  Intro: string;
  SitewideHeroStatistics: string;
  GenericWinPercent: string;
  MonkeyBrokerScript: string;
  MonkeyBrokerScriptVisible: boolean;
}

export interface RootObject {
  Constants: Constants;
  ImportantNoteVisibleSet: boolean;
  ImportantNoteVisible: boolean;
  ImportantNoteSet: boolean;
  ImportantNote?: any;
  TotalGamesPlayedMessageSet: boolean;
  TotalGamesPlayedMessage: string;
  DataSourceSet: boolean;
  DataSource: DataSource[];
  NumberOfPatchesShownSet: boolean;
  NumberOfPatchesShown: number;
  LastUpdatedSet: boolean;
  LastUpdated: string;
  RolesSet: boolean;
  Roles: Role[];
  GamesPlayedVisibleSet: boolean;
  GamesPlayedVisible: boolean;
  GamesBannedVisibleSet: boolean;
  GamesBannedVisible: boolean;
  WinPercentDeltaVisibleSet: boolean;
  WinPercentDeltaVisible: boolean;
}
