import { Role, Team } from '../model';

export interface HeroAndMapRequest {
  GameMode: string;
  GameModeEx: string;
  Tournament: string;
  Hero: string;
  League: number[];
  Map: string[];
  Patch: string[];
  Time: string[];
  GameLength: number[];
  Level: string[];
  Talent: string;
}

export interface AverageScoreResult {
  T: number;
  S: number;
  A: number;
  D: number;
  HD: number;
  SiD: number;
  StD: number;
  MD: number;
  CD: number;
  SuD: number;
  TCCdEH?: Date;
  H?: number;
  SH: number;
  DT?: number;
  EC: number;
  TK: number;
  TSD: Date;
  MCC: number;
  WTC: number;
  ME: number;
}

export interface SitewideCharacterStatisticArray {
  HeroPortraitURL: string;
  Character: string;
  GamesPlayed: number;
  GamesBanned: number;
  AverageLength: Date;
  WinPercent: number;
  AverageScoreResult: AverageScoreResult;
}

export interface Stat {
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
  GameMode: string;
  Event: string;
}

export interface PopularTalentBuild {
  HeroPortraitURL: string;
  Character: string;
  CharacterURL: string;
  GamesPlayed: string;
  WinPercent: string;
  TalentNameDescription01: string;
  TalentNameDescription04: string;
  TalentNameDescription07: string;
  TalentNameDescription10: string;
  TalentNameDescription13: string;
  TalentNameDescription16: string;
  TalentNameDescription20?: string;
  TalentImageURL01: string;
  TalentImageURL04: string;
  TalentImageURL07: string;
  TalentImageURL10: string;
  TalentImageURL13: string;
  TalentImageURL16: string;
  TalentImageURL20: string;
  Role: string;
  AliasCSV: string;
  Export: string;
}

export interface HeroAndMapResult {
  Teams: Team[];
  LastUpdatedText: string;
  Stats: Stat[];
  PopularTalentBuilds: PopularTalentBuild[];
  RecentPatchNoteVisible: boolean;
  BanDataAvailable: boolean;
  PopularityNotice: boolean;
  GameLengthFilterNotice: boolean;
  CharacterLevelFilterNotice: boolean;
  Roles: Role[];
}
