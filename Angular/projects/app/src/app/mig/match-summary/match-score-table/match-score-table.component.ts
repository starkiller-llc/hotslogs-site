import { Component, Input, OnInit, SimpleChanges } from '@angular/core';
import { ScoreResult } from '../model';

@Component({
  selector: 'app-match-score-table',
  templateUrl: './match-score-table.component.html',
  styleUrls: ['./match-score-table.component.scss']
})
export class MatchScoreTableComponent implements OnInit {
  allColumns = [
    'PlayerName',
    'Character',
    'ScoreTooltip',
    'Takedowns',
    'SoloKills',
    'Assists',
    'Deaths',
    'TimeSpentDead',
    'HeroDamage',
    'SiegeDamage',
    'Healing',
    'SelfHealing',
    'DamageTaken',
    'ExperienceContribution',
  ];
  columns = this.allColumns;

  @Input() stats: ScoreResult[];
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
