import { Component, Input, OnInit, SimpleChanges } from '@angular/core';
import { ProfileFriendsRow } from '../model';

@Component({
  selector: 'app-friends-table',
  templateUrl: './friends-table.component.html',
  styleUrls: ['./friends-table.component.scss']
})
export class FriendsTableComponent implements OnInit {
  allColumns = [
    'heroImg',
    'PlayerName',
    'FavoriteHero',
    'GamesPlayedWith',
    'WinPercent',
    'MMRBefore',
    'MatchHistory',
  ];
  columns = this.allColumns;

  @Input() stats: ProfileFriendsRow[];
  @Input() rivals = false;

  constructor() { }

  ngOnInit(): void {
  }

  ngOnChanges(changes: SimpleChanges): void {
    const exclude = [];
    this.columns = this.allColumns.filter(c => !exclude.includes(c));
  }
}
