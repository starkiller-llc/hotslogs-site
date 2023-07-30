import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { PageEvent } from '@angular/material/paginator';
import { SearchResultEntry } from '../model';

@Component({
  selector: 'app-results-table',
  templateUrl: './results-table.component.html',
  styleUrls: ['./results-table.component.scss']
})
export class ResultsTableComponent implements OnInit {
  columns = [
    'Region',
    'Name',
    'CurrentMMR',
    'GamesPlayed',
    'MatchHistory',
  ];

  @Input() stats: SearchResultEntry[];
  @Input() total: number;
  @Output() page = new EventEmitter<PageEvent>();

  constructor() { }

  ngOnInit(): void {
  }

}
