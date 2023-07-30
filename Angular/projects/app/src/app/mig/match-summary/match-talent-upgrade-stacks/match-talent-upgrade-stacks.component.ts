import { Component, Input, OnInit, SimpleChanges } from '@angular/core';
import { TalentUpgradesStacksRow } from '../model';

@Component({
  selector: 'app-match-talent-upgrade-stacks',
  templateUrl: './match-talent-upgrade-stacks.component.html',
  styleUrls: ['./match-talent-upgrade-stacks.component.scss']
})
export class MatchTalentUpgradeStacksComponent implements OnInit {
  allColumns = [
    'PlayerName',
    'heroImg',
    'Character',
    'talentImg',
    'TalentName',
    'Stacks',
  ];
  columns = this.allColumns;

  @Input() stats: TalentUpgradesStacksRow[];
  @Input() exclude = [];

  constructor() { }

  ngOnInit(): void {
  }

  ngOnChanges(changes: SimpleChanges): void {
    this.columns = this.allColumns.filter(c => !this.exclude.includes(c));
  }
}
