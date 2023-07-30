import { Component, OnInit } from '@angular/core';
import { lastValueFrom } from 'rxjs';
import { Tournament } from '../models/tournament';
import { TournamentService } from '../services/tournament.service';

@Component({
  selector: 'app-tournament-list-page',
  templateUrl: './tournament-list-page.component.html',
  styleUrls: ['./tournament-list-page.component.scss'],
})
export class TournamentListPageComponent implements OnInit {
  tournaments: Tournament[];
  constructor(private tournamentService: TournamentService) {
  }

  private async loadTournaments() {
    const t = await lastValueFrom(this.tournamentService.getTournaments());
    this.tournaments = t;
  }

  ngOnInit(): void {
    this.loadTournaments();
  }

}
