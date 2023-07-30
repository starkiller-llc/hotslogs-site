import { Component, Input, OnInit, SimpleChanges } from '@angular/core';
import { CharacterScoreResultsTotal } from '../model';

@Component({
  selector: 'app-score-totals-table',
  templateUrl: './score-totals-table.component.html',
  styleUrls: ['./score-totals-table.component.scss']
})
export class ScoreTotalsTableComponent implements OnInit {
  allColumns = [
    'Team',
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

  @Input() stats: CharacterScoreResultsTotal[];
  @Input() exclude = [];

  constructor() { }

  ngOnInit(): void {
  }

  ngOnChanges(changes: SimpleChanges): void {
    this.columns = this.allColumns.filter(c => !this.exclude.includes(c));
  }
}
