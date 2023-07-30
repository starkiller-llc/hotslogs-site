import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from "@angular/router";
import { lastValueFrom } from 'rxjs';
import { Tournament } from '../models/tournament';
import { TournamentService } from '../services/tournament.service';

@Component({
  selector: 'app-tournament-page',
  templateUrl: './tournament-page.component.html',
  styleUrls: ['./tournament-page.component.scss']
})
export class TournamentPageComponent implements OnInit {
  private _tournamentId: number;
  tournament: Tournament;
  registrationActive: boolean = true;

  constructor(private route: ActivatedRoute, private tournamentService: TournamentService) {
  }

  private async loadTournament() {
    const t = await lastValueFrom(this.tournamentService.getTournamentById(this._tournamentId));
    this.registrationActive = (t.registrationDeadline >= new Date()) && (t.numTeams < t.maxNumTeams || !t.maxNumTeams);
    this.tournament = t;
  }

  ngOnInit(): void {
    const routeParams = this.route.snapshot.paramMap;
    this._tournamentId = Number(routeParams.get('tournament_id'));
    this.loadTournament();
  }

}
