import { Component, Input, OnInit, SimpleChanges } from '@angular/core';
import { MatchStat } from '../model';

@Component({
  selector: 'app-overview-score-table',
  templateUrl: './overview-score-table.component.html',
  styleUrls: ['./overview-score-table.component.scss']
})
export class OverviewScoreTableComponent implements OnInit {
  allColumns = [
    'Character',
    'heroImg',
    'GamesPlayed',
    'WinPercent',
    'Length',
    'TDRatio',
    'KDRatio',
    'Takedowns',
    'SoloKills',
    'Assists',
    'Deaths',
    'HeroDamage',
    'SiegeDamage',
    'Healing',
    'SelfHealing',
    'DamageTaken',
    'ExperienceContribution',
  ];
  columns = this.allColumns;

  @Input() stats: MatchStat[];
  @Input() showBanned = true;
  @Input() showDelta = true;
  @Input() exclude = [];

  constructor() { }

  ngOnInit(): void {
  }

  ngOnChanges(changes: SimpleChanges): void {
    this.columns = this.allColumns.filter(c => !this.exclude.includes(c));
  }
}
