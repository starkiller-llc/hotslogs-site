import { Component, OnInit, Input } from '@angular/core';
import { TournamentMatch } from '../models/tournament-match';

@Component({
  selector: 'app-tournament-bracket-round',
  templateUrl: './tournament-bracket-round.component.html',
  styleUrls: ['./tournament-bracket-round.component.scss']
})
export class TournamentBracketRoundComponent implements OnInit {
  @Input() matches: TournamentMatch[];

  constructor() { }

  ngOnInit(): void {
  }
}
