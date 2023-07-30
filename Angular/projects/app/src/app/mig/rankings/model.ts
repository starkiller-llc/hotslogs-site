import { PageEvent } from '@angular/material/paginator';
import { Sort } from '@angular/material/sort';

export interface RankingsRequest {
  GameMode: string;
  League: number[];
  Region: number;
  Filter: string;
  Page: PageEvent;
  Sort?: Sort;
}

export interface Stat {
  LR: number;
  N: string;
  GP: number;
  R: number;
  PID: number;
  TSS?: Date;
}

export interface RankingsResult {
  Stats: Stat[];
  LeaderboardMMRInfoLink: string;
  CurrentPlayerLeagueRank?: string;
  LastUpdatedText: string;
  LeagueRequirement: string;
  Total: number;
}
