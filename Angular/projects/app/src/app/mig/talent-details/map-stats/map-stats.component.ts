import { Component, Input, OnInit } from '@angular/core';
import { MapStatistic } from '../model';

@Component({
  selector: 'app-map-stats',
  templateUrl: './map-stats.component.html',
  styleUrls: ['./map-stats.component.scss']
})
export class MapStatsComponent implements OnInit {
  @Input() stats: MapStatistic[];

  constructor() { }

  ngOnInit(): void {
  }

}
