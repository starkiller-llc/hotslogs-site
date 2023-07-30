import { Component, EventEmitter, Input, OnInit, Output, SimpleChanges } from '@angular/core';
import { MatchDetail } from '../model';

@Component({
  selector: 'app-details-table',
  templateUrl: './details-table.component.html',
  styleUrls: ['./details-table.component.scss']
})
export class DetailsTableComponent implements OnInit {
  allColumns = [
    'PlayerName',
    'Character',
    'CharacterLevel',
    '1',
    '4',
    '7',
    '10',
    '13',
    '16',
    '20',
    'MMRBefore',
    'MMRChange',
    'MatchHistory',
  ];
  columns = this.allColumns;

  @Input() stats: MatchDetail[];
  @Input() exclude = [];
  @Output() voteDown = new EventEmitter<[number, boolean]>();
  @Output() voteUp = new EventEmitter<[number, boolean]>();

  constructor() { }

  ngOnInit(): void {
  }

  ngOnChanges(changes: SimpleChanges): void {
    this.columns = this.allColumns.filter(c => !this.exclude.includes(c));
  }
}
