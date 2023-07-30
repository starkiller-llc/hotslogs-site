import { Component, Input, OnInit, SimpleChanges } from '@angular/core';
import { TeamObjective } from '../model';

@Component({
  selector: 'app-team-objectives-table',
  templateUrl: './team-objectives-table.component.html',
  styleUrls: ['./team-objectives-table.component.scss']
})
export class TeamObjectivesTableComponent implements OnInit {
  allColumns = [
    'TimeSpan',
    'Team',
    'heroImg',
    'TeamObjectiveType',
    'Value',
  ];
  columns = this.allColumns;

  @Input() stats: TeamObjective[];
  @Input() exclude = [];

  constructor() { }

  ngOnInit(): void {
  }

  ngOnChanges(changes: SimpleChanges): void {
    this.columns = this.allColumns.filter(c => !this.exclude.includes(c));
  }
}
