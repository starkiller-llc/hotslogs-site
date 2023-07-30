import { TalentStatistic } from '../talent-details/model';

export interface OverviewRequest {
  TeamOverview: boolean;
  PlayerId?: number;
  TeamId?: number;
  GameMode: string;
  Time: string;
  GamesTogether: string;
  PartySize: string;
  Tab: [number, number | null];
  HeroDetails?: string;
  MapDetails?: string;
}

export interface FavHero {
  CharacterURL: string;
  Character: string;
  GamesPlayed: number;
}

export interface MatchStat {
  PlayerID?: number;
  PlayerName: string;
  GamesPlayed: string;
  WinPercent: string;
  AverageLength: Date;
  FavoriteHeroes: FavHero[];
  TDRatio: string;
  Takedowns: string;
  SoloKills: string;
  Deaths: string;
  HeroDamage: string;
  SiegeDamage: string;
  Healing: string;
  SelfHealing: string;
  DamageTaken: string;
  MercCampCaptures: string;
  ExperienceContribution: string;
  KDRatio: string;
  Assists: string;
  StructureDamage: string;
  MinionDamage: string;
  CreepDamage: string;
  TownKills: string;
}

export interface MapStat {
  MapImageURL: string;
  Map: string;
  MapNameLocalized: string;
  GamesPlayed: string;
  GamesPlayedValue: number;
  AverageLength: Date;
  WinPercent: string;
  WinPercentValue: number;

  // client properties
  Summary?: any;
  SummaryLoading?: boolean;
}

export interface HeroStat {
  HeroPortraitURL: string;
  PrimaryName: string;
  Character: string;
  CharacterURL: string;
  CharacterLevel: number;
  GamesPlayed: string;
  GamesPlayedValue: number;
  AverageLength: Date;
  WinPercent: string;
  WinPercentValue: number;

  // client properties
  Summary?: any;
  SummaryLoading?: boolean;
}

export interface UpgStat {
  PlayerID?: number;
  PlayerName: string;
  GamesPlayed: number;
  WinPercent: number;
  ReplayLengthPercentAtValue0: number;
  ReplayLengthPercentAtValue1: number;
  ReplayLengthPercentAtValue2: number;
  ReplayLengthPercentAtValue3: number;
  ReplayLengthPercentAtValue4: number;
  ReplayLengthPercentAtValue5: number;
}

export interface MapDetailsStat {
  Character: string;
  CharacterURL: string;
  GamesPlayed: string;
  AverageLength: Date;
  WinPercent: string;
  Role?: string;
  AliasCSV?: string;
}

export interface OverviewResult {
  MatchStats: MatchStat[];
  Title: string;
  RoleStats: Record<string, MatchStat[]>;
  MapStats: MapStat[];
  HeroStats: HeroStat[];
  NovaStats: UpgStat[];
  GallStats: UpgStat[];
  TeamMembers: string;
  HeroDetails?: TalentStatistic[];
  MapDetails?: MapDetailsStat[];
  IsTruncated: boolean;
}
