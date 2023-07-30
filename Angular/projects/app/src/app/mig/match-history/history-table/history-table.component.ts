import { Component, EventEmitter, Input, OnChanges, OnDestroy, OnInit, Output, SimpleChanges } from '@angular/core';
import { PageEvent } from '@angular/material/paginator';
import { Sort } from '@angular/material/sort';
import { Subscription } from 'rxjs';
import { IsMobileService } from '../../../modules/shared/services/is-mobile.service';
import { MigrationService } from '../../../services/migration.service';
import { MatchSummaryRequest } from '../../match-summary/model';
import { Stat } from '../model';

@Component({
  selector: 'app-history-table',
  templateUrl: './history-table.component.html',
  styleUrls: ['./history-table.component.scss']
})
export class HistoryTableComponent implements OnInit, OnChanges, OnDestroy {
  allColumns = [
    'expand',
    'Map',
    'ReplayLength',
    'Character',
    'CharacterLevel',
    'MMRBefore',
    'MMRChange',
    'TimestampReplay',
    'share',
  ];
  columns = this.allColumns;

  exclude = [
    'Character',
    'MMRBefore',
    'MMRChange',
  ];

  excludeMobile = [
    'ReplayLength',
    'CharacterLevel',
    'MMRChange',
    'TimestampReplay',
  ];

  @Input() stats: Stat[];
  @Input() total: number;
  @Input() event = false;
  @Output() filterChange = new EventEmitter<string>();
  @Output() page = new EventEmitter<PageEvent>();
  @Output() voteDown = new EventEmitter<[number, number, boolean]>();
  @Output() voteUp = new EventEmitter<[number, number, boolean]>();
  @Output() sortChange = new EventEmitter<Sort>();

  stats2: Stat[];
  sub: Subscription;
  mobile: boolean;
  sorted = false;

  constructor(private svc: MigrationService, mob: IsMobileService) {
    this.sub = mob.mobile$.subscribe(r => this.mobile = r);
  }

  ngOnInit(): void {
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (this.event) {
      this.columns = this.allColumns.filter(c => !this.exclude.includes(c));
    }
    if (this.mobile) {
      this.columns = this.columns.filter(c => !this.excludeMobile.includes(c));
    }
    if (changes.stats) {
      if (this.sorted) {
        this.stats2 = this.stats.filter(r => !r.Season);
      } else {
        this.stats2 = this.stats;
      }
    }
  }

  ngOnDestroy(): void {
    this.sub.unsubscribe();
  }

  expand(stat: Stat) {
    if (stat.Summary) {
      stat.Summary = null;
      return;
    }
    stat.SummaryLoading = true;
    const req: MatchSummaryRequest = {
      ReplayId: stat.ReplayID,
    };
    this.svc.getMatchSummary(req).subscribe(r => {
      stat.Summary = r;
      stat.SummaryLoading = false;
    })
  }

  onSortChange(e: Sort) {
    this.sorted = !!e.direction;
    this.sortChange.emit(e);
  }
}
