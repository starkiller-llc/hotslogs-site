import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { PlayerTournamentMatch } from '../models/player-tournament-match';
import { Tournament } from '../models/tournament';
import { TournamentMatch } from '../models/tournament-match';
import { TournamentRegistrationApplication } from '../models/tournament-registration';

@Injectable({
  providedIn: 'root'
})
export class TournamentService {

  private url(api: string): string {
    return `/api/Tournament/${api}`;
  }

  constructor(private http: HttpClient) { }

  updateTournamentRegistrationDeadline(tournamentId: number) {
    const params = new HttpParams()
      .set('tournamentId', tournamentId.toString())
    const t = this.http.get<number>(this.url('UpdateTournamentRegistrationDeadline'), { params });
    return t;
  }

  updateTournamenMatchDeadline(tournamentId: number) {
    const params = new HttpParams()
      .set('tournamentId', tournamentId.toString())
    const t = this.http.get<number>(this.url('UpdateTournamenMatchDeadline'), { params });
    return t;
  }

  createTournamentMatches() {
    const params = new HttpParams()
    const t = this.http.get<number>(this.url('CreateTournamentMatches'), { params } );
    return t;
  }

  createTournament(tournament: Tournament) {
    const headers = new HttpHeaders({
      'Content-Type': 'application/json'
    })
    const j = JSON.stringify(tournament);
    const ret = this.http.post(this.url('CreateTournament'), j, { observe: 'events', responseType: 'text', headers: headers });
    return ret;
  }

  getPlayerTournaments(playerId: number) {
    const params = new HttpParams().set('playerId', playerId.toString());
    return this.http.get<PlayerTournamentMatch[]>(this.url('GetPlayerTournaments'), { params });
  }

  setMatchWinner(matchId: number, winningTeamId: number) {
    const params = new HttpParams()
      .set('matchId', matchId.toString())
      .set('winningTeamId', winningTeamId.toString());
    const t = this.http.get<TournamentMatch>(this.url('SetMatchWinner'), { params });
    return t;
  }

  getTournaments() {
    const params = new HttpParams();
    return this.http.get<Tournament[]>(this.url('GetTournaments'), { params });
  }

  getTournamentById(tournamentId: number) {
    const params = new HttpParams().set('tournamentId', tournamentId.toString());
    const t = this.http.get<Tournament>(this.url('GetTournamentById'), { params });
    return t;
  }

  setReplayId(matchId: number, replayGuid: string) {
    const params = new HttpParams()
      .set('matchId', matchId.toString())
      .set('replayGuid', replayGuid.toString());
    const t = this.http.get<TournamentMatch>(this.url('SetReplayId'), { params });
    return t;
  }

  getMatchesForTournament(tournamentId: number) {
    const params = new HttpParams().set('tournamentId', tournamentId.toString());
    const t = this.http.get<TournamentMatch[]>(this.url('GetMatchesForTournament'), { params });
    return t;
  }

  registerForTournament(r: TournamentRegistrationApplication) {
    const headers = new HttpHeaders({
      'Content-Type': 'application/json'
    })
    const j = JSON.stringify(r);
    const ret = this.http.post(this.url('RegisterForTournament'), j, { observe: 'events', responseType: 'text', headers: headers });
    return ret;
  }
}
