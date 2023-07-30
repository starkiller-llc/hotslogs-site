import { PageEvent } from '@angular/material/paginator';
import { Sort } from '@angular/material/sort';

export interface ReputationRequest {
  Region: number;
  Filter: string;
  Page: PageEvent;
  Sort: Sort;
}

export interface Stat {
  PlayerId: number;
  Name: string;
  Reputation: number;
  TSS?: Date;
}

export interface ReputationResult {
  Stats: Stat[];
  Total: number;
}
