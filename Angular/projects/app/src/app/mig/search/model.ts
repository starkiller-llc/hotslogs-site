import { PageEvent } from '@angular/material/paginator';

export interface PlayerSearchRequest {
  Name: string;
  Page: PageEvent;
}

export interface SearchResultEntry {
  PlayerID: number;
  Region: string;
  Name: string;
  LeageName: string;
  CurrentMMR?: number;
  GamesPlayed?: number;
  PremiumSupporterSince?: Date;
}

export interface PlayerSearchResult {
  Results?: SearchResultEntry[];
  Total: number;
}
