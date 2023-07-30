import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { MatchDetails } from './models/match-details';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class MatchDetailsService {

  constructor(private http: HttpClient) { }

  get(replayId: number): Observable<MatchDetails> {
    const params = new HttpParams().set('replayId', replayId.toString());
    return this.http.get<MatchDetails>('/api/matchdetails', { params });
  }
}
