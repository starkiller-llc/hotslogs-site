import { Component, Input, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { TournamentMatch } from '../models/tournament-match';

@Component({
  selector: 'app-tournament-bracket-match',
  templateUrl: './tournament-bracket-match.component.html',
  styleUrls: ['./tournament-bracket-match.component.scss', '../tournament-bracket-round/tournament-bracket-round.component.scss', '../tournament-bracket/tournament-bracket.component.scss']
})
export class TournamentBracketMatchComponent implements OnInit {
  @Input() match: TournamentMatch;
  @Input() odd: boolean;

  constructor(private router: Router) { }

  ngOnInit(): void {
  }

}
