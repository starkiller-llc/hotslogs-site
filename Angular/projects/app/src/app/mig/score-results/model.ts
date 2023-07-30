export interface ScoreResultsRequest {
  GameMode: string;
  GameModeEx: string;
  Tournament: string;
  League: number[];
  Map: string[];
  Time: string[];
  Patch: string[];
  Tab: number;
  Subtab: number;
}

export interface GeneralStat {
  Character: string;
  CharacterURL: string;
  HeroPortraitURL: string;
  GamesPlayed: string;
  WinPercent: string;
  TDRatio: string;
  KDRatio: string;
  Takedowns: string;
  SoloKills: string;
  Assists: string;
  Deaths: string;
  HeroDamage: string;
  SiegeDamage: string;
  Healing: string;
  SelfHealing: string;
  DamageTaken: string;
  ExperienceContribution: string;
  MercCampCaptures: number;
  AverageLength: string;
  StructureDamage: string;
  MinionDamage: string;
  CreepDamage: string;
  TownKills: string;
  GameMode: string;
  Event: string;
  Role: string;
}

export interface ScoreResultsResult {
  Teams?: any;
  LastUpdatedText: string;
  GeneralStats: GeneralStat[];
  RoleStats: Record<string, GeneralStat[]>;
}
