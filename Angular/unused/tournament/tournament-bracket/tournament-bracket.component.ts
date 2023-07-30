import { Component, OnInit, Input, ViewEncapsulation } from '@angular/core';
import { TournamentMatch } from '../models/tournament-match';
import { TournamentService } from '../services/tournament.service';
import { lastValueFrom } from 'rxjs';

@Component({
  selector: 'app-tournament-bracket',
  templateUrl: './tournament-bracket.component.html',
  styleUrls: ['./tournament-bracket.component.scss'],
  encapsulation: ViewEncapsulation.None
})
export class TournamentBracketComponent implements OnInit {
  @Input() tournament_id: number;
  matches: TournamentMatch[][] = [];


  constructor(private tournamentService: TournamentService) { }

  private async loadMatches() {
    const m = (await lastValueFrom(this.tournamentService.getMatchesForTournament(this.tournament_id))).sort((a, b) => a.team1Id > b.team1Id ? 1 : -1);
    const curRound = m.map(x => x.roundNum).reduce((agg, cur) => Math.max(agg, cur), 0);
    const curMatches = m.filter(x => x.roundNum === curRound);
    const roundsToAdd = Math.log2(curMatches.length);

    const matches = [];
    for (let i = 1; i <= curRound; i++) {
      const cur = m.filter(x => x.roundNum === i);
      matches.push(cur);
    }
    for (let i = 0; i < roundsToAdd; i++) {
      const cur = [];
      const nMatchesToAdd = Math.pow(2, roundsToAdd - i - 1);
      for (let j = 0; j < nMatchesToAdd; j++) {
        const newMatch = {
          'match_id': 0,
          'tournament_id': this.tournament_id,
          'round_num': curRound + 1 + i,
          'match_created': new Date(),
          'match_deadline': new Date(),
          'team1_id': null,
          'team2_id': null,
          'team1_name': null,
          'team2_name': null,
        }
        cur.push(newMatch);
      }
      matches.push(cur);
    }
    this.matches = matches;
  }

  ngOnInit(): void {
    this.loadMatches();
  }
}
