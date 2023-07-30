import { Team } from '../model';

export interface MatchAwardsRequest {
  GameMode: string;
  GameModeEx: string;
  Tournament: string;
  League: number[];
  Type: number;
  PlayerId?: number;
}

export interface Stat {
  HeroPortraitURL: string;
  Character: string;
  CharacterURL: string;
  GamesPlayedTotal: string;
  GamesPlayedWithAward: string;
  PercentMVP: string;
  PercentHighestKillStreak: string;
  PercentMostXPContribution: string;
  PercentMostHeroDamageDone: string;
  PercentMostSiegeDamageDone: string;
  PercentMostDamageTaken: string;
  PercentMostHealing: string;
  PercentMostStuns: string;
  PercentMostMercCampsCaptured: string;
  PercentMapSpecific: string;
  PercentMostKills: string;
  PercentHatTrick: string;
  PercentClutchHealer: string;
  PercentMostProtection: string;
  PercentZeroDeaths: string;
  PercentMostRoots: string;
  PercentMostDragonShrinesCaptured: string;
  PercentMostCurseDamageDone: string;
  PercentMostCoinsPaid: string;
  PercentMostSkullsCollected: string;
  PercentMostDamageToPlants: string;
  PercentMostTimeInTemple: string;
  PercentMostGemsTurnedIn: string;
  PercentMostImmortalDamage: string;
  PercentMostDamageToMinions: string;
  PercentMostAltarDamage: string;
  PercentMostDamageDoneToZerg: string;
  PercentMostNukeDamageDone: string;
  Role: string;
  AliasCSV: string;
  GameMode: string;
  Event: string;
}

export interface MatchAwardsResult {
  Teams: Team[];
  LastUpdatedText: string;
  Stats: Stat[];
  Title: string;
  MostRecentDaysVisible: boolean;
}
