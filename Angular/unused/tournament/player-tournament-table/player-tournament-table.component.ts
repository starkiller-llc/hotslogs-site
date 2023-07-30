import { Component, OnInit, Input } from '@angular/core';
import { Router } from '@angular/router';
import { lastValueFrom } from 'rxjs';
import { PlayerTournamentMatch } from '../../models/player-tournament-match';
import { TournamentService } from '../services/tournament.service';

@Component({
  selector: 'app-player-tournament-table',
  templateUrl: './player-tournament-table.component.html',
  styleUrls: ['./player-tournament-table.component.scss']
})
export class PlayerTournamentTableComponent implements OnInit {
  @Input() playerTournaments: PlayerTournamentMatch[];

  constructor(private router: Router, private tournamentService: TournamentService) { }

  navigate($event: any, tournamentId: number) {
    if ($event.srcElement.type != "button" && $event.srcElement.type != "file" && $event.srcElement.type != "select-one") {
      this.router.navigate(['/tournament', tournamentId])
    }
  }

  public async setMatchWinner($event: any, matchId: number) {
    if ($event.target.selectedOptions && $event.target.selectedOptions.length) {
      const winningTeamId = parseInt($event.target.selectedOptions[0].value);
      const match = await lastValueFrom(this.tournamentService.setMatchWinner(matchId, winningTeamId));
      if (this.playerTournaments && this.playerTournaments.length) {
        for (let i = 0; i < this.playerTournaments.length; i++) {
          const t = this.playerTournaments[i];
          if (t.matchId === matchId) {
            this.playerTournaments[i].wonMatch = t.teamId === winningTeamId ? 1 : 0;
          }
        }
      }
    }
  }

  ngOnInit(): void {
  }

}
