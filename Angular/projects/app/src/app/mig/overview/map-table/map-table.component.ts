import { Component, EventEmitter, Input, OnInit, Output, SimpleChanges } from '@angular/core';
import { Role } from '../../model';
import { MapStat } from '../model';

@Component({
  selector: 'app-map-table',
  templateUrl: './map-table.component.html',
  styleUrls: ['./map-table.component.scss']
})
export class MapTableComponent implements OnInit {
  allColumns = [
    'select',
    'mapImg',
    'MapNameLocalized',
    'GamesPlayed',
    'AverageLength',
    'WinPercent'
  ];
  columns = this.allColumns;

  @Input() stats: MapStat[];
  @Input() roles: Role[];
  @Input() showBanned = true;
  @Input() showDelta = true;
  @Output() detailsRequested = new EventEmitter<MapStat>();

  constructor() { }

  ngOnInit(): void {
  }

  ngOnChanges(changes: SimpleChanges): void {
    const exclude = [];
    this.columns = this.allColumns.filter(c => !exclude.includes(c));
  }

  expand(stat: MapStat) {
    if (stat.Summary) {
      stat.Summary = null;
      return;
    }
    this.detailsRequested.emit(stat);
  }
}
