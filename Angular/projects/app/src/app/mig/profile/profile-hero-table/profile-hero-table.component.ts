import { Component, EventEmitter, Input, OnInit, Output, SimpleChanges } from '@angular/core';
import { Role } from '../../model';
import { ProfileCharacterStatisticsRow } from '../model';

@Component({
  selector: 'app-profile-hero-table',
  templateUrl: './profile-hero-table.component.html',
  styleUrls: ['./profile-hero-table.component.scss']
})
export class ProfileHeroTableComponent implements OnInit {
  allColumns = [
    'select',
    'heroImg',
    'Character',
    'GamesPlayed',
    'AverageLength',
    'WinPercent'
  ];
  columns = this.allColumns;

  @Input() stats: ProfileCharacterStatisticsRow[];
  @Input() roles: Role[];
  @Input() showBanned = true;
  @Input() showDelta = true;
  @Output() detailsRequested = new EventEmitter<ProfileCharacterStatisticsRow>();

  constructor() { }

  ngOnInit(): void {
  }

  ngOnChanges(changes: SimpleChanges): void {
    const exclude = [];
    this.columns = this.allColumns.filter(c => !exclude.includes(c));
  }

  expand(stat: ProfileCharacterStatisticsRow) {
    if (stat.Summary) {
      stat.Summary = null;
      return;
    }
    this.detailsRequested.emit(stat);
  }
}
