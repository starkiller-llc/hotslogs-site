import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { PageEvent } from '@angular/material/paginator';
import { Sort } from '@angular/material/sort';
import { Stat } from '../model';

@Component({
  selector: 'app-reputation-table',
  templateUrl: './reputation-table.component.html',
  styleUrls: ['./reputation-table.component.scss']
})
export class ReputationTableComponent implements OnInit {
  columns = [
    'Name',
    'Reputation',
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
