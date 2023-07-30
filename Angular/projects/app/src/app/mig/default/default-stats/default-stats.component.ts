import { Component, EventEmitter, Input, OnInit, Output, SimpleChanges, ViewChild } from '@angular/core';
import { MigSortDirective } from '../../../directives/mig-sort.directive';
import { Role } from '../../model';
import { TableFilterComponent } from '../../../widgets/table-filter/table-filter.component';
import { DataSource } from '../model';

@Component({
  selector: 'app-default-stats',
  templateUrl: './default-stats.component.html',
  styleUrls: ['./default-stats.component.scss']
})
export class DefaultStatsComponent implements OnInit {
  allColumns = ['heroImg', 'Character', 'GamesPlayed', 'GamesBanned', 'Popularity', 'WinPercent', 'WinPercentDelta'];
  columns = this.allColumns;

  @Input() stats: DataSource[];
  @Input() roles: Role[];
  @Input() showBanned = true;
  @Input() showDelta = true;
  @Output() filterChange = new EventEmitter<string>();
  @Output() sortChange = new EventEmitter<string>();
  @ViewChild('tbl') tbl: MigSortDirective<DataSource>;
  @ViewChild('flt') flt: TableFilterComponent;

  constructor() { }

  ngOnInit(): void {
  }

  ngOnChanges(changes: SimpleChanges): void {
    const exclude = [];
    if (!this.showBanned) {
      exclude.push('GamesBanned');
    }
    if (!this.showDelta) {
      exclude.push('WinPercentDelta');
    }
    if (!this.stats.some(r => r.WinPercentDelta)) {
      exclude.push('WinPercentDelta');
    }
    this.columns = this.allColumns.filter(c => !exclude.includes(c));
  }

  // Public API
  get sort(): string {
    const prop = this.tbl.ds.sort.active;
    const desc = this.tbl.ds.sort.direction;
    return desc === 'desc'
      ? `${prop} DESC`
      : prop;
  }
  set sort(s: string) {
    let prop = s;
    let desc = false;
    if (prop.endsWith(' DESC')) {
      prop = prop.replace(' DESC', '');
      desc = true;
    }
    this.tbl.ds.sort.active = prop;
    this.tbl.ds.sort.direction = desc ? 'desc' : 'asc';
    this.tbl.ds.sort.sortChange.emit({ active: prop, direction: desc ? 'desc' : 'asc' });
  }

  // Public API
  get filter(): string {
    return this.tbl.ds.filter;
  }
  set filter(f: string) {
    this.tbl.ds.filter = f;
    this.flt.query = f;
  }
}
