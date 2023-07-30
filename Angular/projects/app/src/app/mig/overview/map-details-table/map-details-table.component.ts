import { Component, Input, OnChanges, OnInit, SimpleChanges } from '@angular/core';
import { Role } from '../../model';
import { MapDetailsStat } from '../model';

@Component({
  selector: 'app-map-details-table',
  templateUrl: './map-details-table.component.html',
  styleUrls: ['./map-details-table.component.scss']
})
export class MapDetailsTableComponent implements OnInit, OnChanges {
  allColumns = [
    'heroImg',
    'Character',
    'GamesPlayed',
    'AverageLength',
    'WinPercent',
  ];
  columns = this.allColumns;

  @Input() stats: MapDetailsStat[];
  @Input() roles: Role[];

  constructor() { }

  ngOnInit(): void {
  }

  ngOnChanges(changes: SimpleChanges): void {
    const exclude = [];
    this.columns = this.allColumns.filter(c => !exclude.includes(c));
  }
}
