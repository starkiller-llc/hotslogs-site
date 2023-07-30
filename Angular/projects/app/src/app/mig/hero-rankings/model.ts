import { PageEvent } from '@angular/material/paginator';
import { Sort } from '@angular/material/sort';

export interface HeroRankingsRequest {
  GameMode: string;
  Hero: string;
  Region: number;
  Season: string;
  Filter: string;
  Page: PageEvent;
  Sort: Sort;
}

export interface Stat {
  LR: number;
  N: string;
  GP: number;
  WP: number;
  R: number;
  PID: number;
  TSS?: Date;
}

export interface HeroRankingsResult {
  Stats: Stat[];
  LeaderboardMMRInfoLink: string;
  CurrentPlayerLeagueRank?: string;
  LastUpdatedText: string;
  LeagueRequirement: string;
  Total: number;
}
