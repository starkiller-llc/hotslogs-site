import { Component, Input, OnChanges, OnInit, SimpleChanges } from '@angular/core';
import { Role } from '../../model';
import { Stat } from '../model';

@Component({
  selector: 'app-stats',
  templateUrl: './stats.component.html',
  styleUrls: ['./stats.component.scss']
})
export class StatsComponent implements OnInit, OnChanges {
  allColumns = ['heroImg', 'Character', 'GamesPlayed', 'GamesBanned', 'Popularity', 'WinPercent', 'WinPercentDelta'];
  columns = this.allColumns;

  @Input() stats: Stat[];
  @Input() roles: Role[];
  @Input() showBanned = true;
  @Input() showDelta = true;
  @Input() gameModeEx?: string;
  @Input() tournament?: string;
  queryParams: { GameModeEx?: string; Tournament?: string; };

  constructor() { }

  ngOnInit(): void {
  }

  ngOnChanges(changes: SimpleChanges): void {
    const exclude = [];
    if (!this.showBanned) {
      exclude.push('GamesBanned');
    }
    if (!this.showDelta) {
      exclude.push('WinPercentDelta');
    }
    if (!this.stats.some(r => r.WinPercentDelta)) {
      exclude.push('WinPercentDelta');
    }
    this.columns = this.allColumns.filter(c => !exclude.includes(c));
    if (this.gameModeEx === '0') {
      this.queryParams = {
        GameModeEx: '0',
        Tournament: this.tournament
      }
    } else if (this.gameModeEx !== '8') {
      this.queryParams = {
        GameModeEx: this.gameModeEx,
      }
    } else {
      this.queryParams = {};
    }
  }

  getQueryParams(hero: string) {
    return {
      ...this.queryParams,
      Hero: hero,
    };
  }
}
