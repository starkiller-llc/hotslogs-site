import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { PageEvent } from '@angular/material/paginator';
import { Sort } from '@angular/material/sort';
import { Stat } from '../model';

@Component({
  selector: 'app-hero-rankings-table',
  templateUrl: './hero-rankings-table.component.html',
  styleUrls: ['./hero-rankings-table.component.scss']
})
export class HeroRankingsTableComponent implements OnInit {
  columns = [
    'LR',
    'N',
    'GP',
    'WP',
    'R',
    'MatchHistory',
  ];

  @Input() stats: Stat[];
  @Input() total: number;
  @Output() filterChange = new EventEmitter<string>();
  @Output() page = new EventEmitter<PageEvent>();
  @Output() sortChange = new EventEmitter<Sort>();

  constructor() { }

  ngOnInit(): void {
  }

}
