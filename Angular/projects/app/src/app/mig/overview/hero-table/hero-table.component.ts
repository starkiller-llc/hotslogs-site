import { Component, EventEmitter, Input, OnInit, Output, SimpleChanges } from '@angular/core';
import { Role } from '../../model';
import { HeroStat } from '../model';

@Component({
  selector: 'app-hero-table',
  templateUrl: './hero-table.component.html',
  styleUrls: ['./hero-table.component.scss']
})
export class HeroTableComponent implements OnInit {
  allColumns = [
    'select',
    'heroImg',
    'Character',
    'GamesPlayed',
    'AverageLength',
    'WinPercent'
  ];
  columns = this.allColumns;

  @Input() stats: HeroStat[];
  @Input() roles: Role[];
  @Input() showBanned = true;
  @Input() showDelta = true;
  @Output() detailsRequested = new EventEmitter<HeroStat>();

  constructor() { }

  ngOnInit(): void {
  }

  ngOnChanges(changes: SimpleChanges): void {
    const exclude = [];
    this.columns = this.allColumns.filter(c => !exclude.includes(c));
  }

  expand(stat: HeroStat) {
    if (stat.Summary) {
      stat.Summary = null;
      return;
    }
    this.detailsRequested.emit(stat);
  }
}
