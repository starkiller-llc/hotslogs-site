import { Component, EventEmitter, Input, OnInit, Output, SimpleChanges } from '@angular/core';
import { MapStatisticsRow } from '../model';

@Component({
  selector: 'app-profile-map-table',
  templateUrl: './profile-map-table.component.html',
  styleUrls: ['./profile-map-table.component.scss']
})
export class ProfileMapTableComponent implements OnInit {
  allColumns = [
    'select',
    'mapImg',
    'MapNameLocalized',
    'GamesPlayed',
    'AverageLength',
    'WinPercent'
  ];
  columns = this.allColumns;

  @Input() stats: MapStatisticsRow[];
  @Input() showBanned = true;
  @Input() showDelta = true;
  @Output() detailsRequested = new EventEmitter<MapStatisticsRow>();

  constructor() { }

  ngOnInit(): void {
  }

  ngOnChanges(changes: SimpleChanges): void {
    const exclude = [];
    this.columns = this.allColumns.filter(c => !exclude.includes(c));
  }

  expand(stat: MapStatisticsRow) {
    if (stat.Summary) {
      stat.Summary = null;
      return;
    }
    this.detailsRequested.emit(stat);
  }
}
