import { Component, Input, OnInit } from '@angular/core';
import { Datum } from '../model';

@Component({
  selector: 'app-generic-table',
  templateUrl: './generic-table.component.html',
  styleUrls: ['./generic-table.component.scss']
})
export class GenericTableComponent implements OnInit {
  columns = ['rowTitle', 'GamesPlayed', 'Value'];

  @Input() stats: Datum[];
  @Input() fieldName: string;
  @Input() showBanned = true;
  @Input() showDelta = true;

  constructor() { }

  ngOnInit(): void {
  }
}
