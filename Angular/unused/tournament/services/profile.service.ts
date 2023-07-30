import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { PlayerProfile } from '../models/player-profile';
import { PlayerTournamentMatch } from '../models/player-tournament-match';

@Injectable({
  providedIn: 'root'
})
export class ProfileService {

  private url(api: string): string {
    return `/api/Player/${api}`;
  }

  constructor(private http: HttpClient) { }

  getProfile(playerId: number) {
    const params = new HttpParams().set('playerId', playerId.toString());
    return this.http.get<PlayerProfile>(this.url('GetProfile'), { params });
  }
}
