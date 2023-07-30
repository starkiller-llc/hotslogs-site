import { Component, Input, OnInit, SimpleChanges } from '@angular/core';
import { GeneralStat } from '../model';

@Component({
  selector: 'app-score-table',
  templateUrl: './score-table.component.html',
  styleUrls: ['./score-table.component.scss']
})
export class ScoreTableComponent implements OnInit {
  allColumns = ['heroImg', 'Character', 'GamesPlayed', 'WinPercent',
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

  @Input() stats: GeneralStat[];
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
