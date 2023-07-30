import { Component, Input, OnInit, SimpleChanges } from '@angular/core';
import { HeroBanRow } from '../model';

@Component({
  selector: 'app-hero-bans-table',
  templateUrl: './hero-bans-table.component.html',
  styleUrls: ['./hero-bans-table.component.scss']
})
export class HeroBansTableComponent implements OnInit {
  allColumns = [
    'BanPhase',
    'heroImg',
    'Character',
  ];
  columns = this.allColumns;

  @Input() stats: HeroBanRow[];
  @Input() exclude = [];

  constructor() { }

  ngOnInit(): void {
  }

  ngOnChanges(changes: SimpleChanges): void {
    this.columns = this.allColumns.filter(c => !this.exclude.includes(c));
  }
}
