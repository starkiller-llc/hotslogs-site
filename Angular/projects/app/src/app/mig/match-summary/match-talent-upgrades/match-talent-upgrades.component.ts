import { Component, Input, OnInit, SimpleChanges } from '@angular/core';
import { TalentUpgradeRow } from '../model';

@Component({
  selector: 'app-match-talent-upgrades',
  templateUrl: './match-talent-upgrades.component.html',
  styleUrls: ['./match-talent-upgrades.component.scss']
})
export class MatchTalentUpgradesComponent implements OnInit {
  allColumns = [
    'PlayerName',
    'heroImg',
    'Character',
    'talentImg',
    'TalentName',
    '0',
    '1',
    '2',
    '3',
    '4',
    '5',
  ];
  columns = this.allColumns;

  @Input() stats: TalentUpgradeRow[];
  @Input() exclude = [];

  constructor() { }

  ngOnInit(): void {
  }

  ngOnChanges(changes: SimpleChanges): void {
    this.columns = this.allColumns.filter(c => !this.exclude.includes(c));
  }
}
