import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { GameEvent } from '../models/game-event';
import { GameEventGamesAndInfo } from '../models/game-event-games-and-info';
import { GameEventTeam } from '../models/game-event-team';

@Injectable({
  providedIn: 'root'
})
export class GameEventsService {
  private url(api: string): string {
    return `/api/GameEvents/${api}`;
  }

  constructor(private http: HttpClient) { }

  getGameEvents(): Observable<GameEvent[]> {
    return this.http.get<GameEvent[]>(this.url('GetGameEvents'));
  }

  getGameEvent(id: number): Observable<GameEventGamesAndInfo> {
    const params = new HttpParams().set('id', id.toString());
    return this.http.get<GameEventGamesAndInfo>(this.url('GetGameEvent'), { params });
  }

  assignTeam(replayId: number, winningTeam: boolean, teamId: number, teamName: string): Observable<GameEventTeam> {
    const params = new HttpParams()
      .set('replayId', replayId.toString())
      .set('winningTeam', winningTeam ? 'True' : 'False')
      .set('teamId', teamId.toString())
      .set('teamName', teamName);
    return this.http.post<GameEventTeam>(this.url('AssociateGameEventTeam'), null, { params });
  }
}
