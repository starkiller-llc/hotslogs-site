import { PageEvent } from '@angular/material/paginator';
import { Sort } from '@angular/material/sort';
import { MatchSummaryResult } from '../match-summary/model';

export interface MatchHistoryRequest {
  PlayerId?: number;
  OtherPlayerIds?: number[];
  EventId?: number;
  GameMode?: string;
  Filter: string;
  Page: PageEvent;
  Sort: Sort;
}

export interface Stat {
  ReplayID: number;
  Map: string;
  ReplayLength: Date;
  ReplayLengthMinutes: string;
  Character: string;
  CharacterURL: string;
  CharacterLevel: number;
  Result: number;
  MMRBefore?: number;
  MMRChange?: number;
  ReplayShare?: number;
  TimestampReplay: Date;
  TimestampReplayDate: Date;
  Role: string;
  TimestampReplayTicks: any;
  Season: boolean;
  MapURL: string;

  // client properties
  Summary?: MatchSummaryResult;
  SummaryLoading?: boolean;
}

export interface MatchHistoryResult {
  Stats: Stat[];
  LiteralHeaderLinks?: any;
  Unauthorized: boolean;
  PremiumSupporterSince?: any;
  ShowShareColumn: boolean;
  OtherPlayerIds?: any;
  Total: number;
  Title: string;
  HideMessageLineVersion2: boolean;
}
