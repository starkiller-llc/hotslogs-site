import { Team } from '../model';

export interface TalentDetailsRequest {
  GameMode: string;
  GameModeEx: string;
  Tournament: string;
  Hero: string;
  League: number[];
  Map: string[];
  Patch: string[];
  Time: string[];
  Tab: number;
}

export interface TalentStatistic {
  TalentTier: number;
  TalentImageURL: string;
  TalentName: string;
  TalentDescription: string;
  GamesPlayed: string | number;
  Popularity?: string;
  WinPercent: string;
  HeaderStart: string;
}

export interface PopularTalentBuild {
  GamesPlayed: string;
  WinPercent: string;
  TalentNameDescription01: string;
  TalentNameDescription04: string;
  TalentNameDescription07: string;
  TalentNameDescription10: string;
  TalentNameDescription13: string;
  TalentNameDescription16: string;
  TalentNameDescription20?: string;
  TalentName01: string;
  TalentName04: string;
  TalentName07: string;
  TalentName10: string;
  TalentName13: string;
  TalentName16: string;
  TalentName20?: string;
  TalentImageURL01: string;
  TalentImageURL04: string;
  TalentImageURL07: string;
  TalentImageURL10: string;
  TalentImageURL13: string;
  TalentImageURL16: string;
  TalentImageURL20: string;
  Export: string;
}

export interface TalentBuildStatistic {
  GamesPlayed: number;
  WinPercent: number;
  GamesWon: number;
  CharacterID: number;
  LeagueID?: any;
  MapID: number;
  TalentNameDescription: string[];
  TalentName: string[];
  TalentImageURL: string[];
  TalentId: number[];
}

export interface WinRateWithVs {
  HeroPortraitURL: string;
  Character: string;
  CharacterURL: string;
  GamesPlayed: string;
  WinPercent: string;
  RelativeWinPercent: number;
  Role: string;
  AliasCSV: string;
}

export interface MapStatistic {
  MapImageURL: string;
  MapNameLocalized: string;
  GamesPlayed: string;
  WinPercent: string;
}

export interface TalentUpgradeType {
  Text: string;
  Value: number;
}

export interface TalentDetailResult {
  Teams: Team[];
  TalentStatistics: TalentStatistic[];
  PopularTalentBuilds: PopularTalentBuild[];
  TalentBuildStatistics: TalentBuildStatistic[];
  RecentPatchNotesVisible: boolean;
  WinRatesByDate: string;
  WinRatesByGameLength: string;
  WinRateVs: WinRateWithVs[];
  WinRateWith: WinRateWithVs[];
  MapStatistics: MapStatistic[];
  WinRateByHeroLevel: string;
  WinRateByTalentUpgrade: string;
  TalentUpgradeTypes: TalentUpgradeType[];
}
