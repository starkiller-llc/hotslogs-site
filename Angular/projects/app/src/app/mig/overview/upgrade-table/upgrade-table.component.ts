import { Component, EventEmitter, Input, OnInit, Output, SimpleChanges } from '@angular/core';
import { Role } from '../../model';
import { UpgStat } from '../model';

@Component({
  selector: 'app-upgrade-table',
  templateUrl: './upgrade-table.component.html',
  styleUrls: ['./upgrade-table.component.scss']
})
export class UpgradeTableComponent implements OnInit {
  allColumns = [
    'PlayerName',
    'WinPercent',
    'ReplayLengthPercentAtValue0',
    'ReplayLengthPercentAtValue1',
    'ReplayLengthPercentAtValue2',
    'ReplayLengthPercentAtValue3',
    'ReplayLengthPercentAtValue4',
    'ReplayLengthPercentAtValue5',
  ];
  columns = this.allColumns;

  @Input() stats: UpgStat[];
  @Input() roles: Role[];
  @Input() showBanned = true;
  @Input() showDelta = true;

  constructor() { }

  ngOnInit(): void {
  }

  ngOnChanges(changes: SimpleChanges): void {
    const exclude = [];
    this.columns = this.allColumns.filter(c => !exclude.includes(c));
  }
}
