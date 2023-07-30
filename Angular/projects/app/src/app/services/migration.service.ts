import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { catchError, Observable, tap } from 'rxjs';
import { HeroAndMapResult } from '../mig/hero-and-map/model';
import { HeroRankingsResult } from '../mig/hero-rankings/model';
import { AccountData } from '../mig/manage/model';
import { MapObjectivesResult } from '../mig/map-objectives/model';
import { MatchAwardsResult } from '../mig/match-awards/model';
import { MatchHistoryResult } from '../mig/match-history/model';
import { MatchSummaryResult, VoteRequest, VoteResponse } from '../mig/match-summary/model';
import { OverviewResult } from '../mig/overview/model';
import { ProfileResult } from '../mig/profile/model';
import { RankingsResult } from '../mig/rankings/model';
import { ReputationResult } from '../mig/reputation/model';
import { ScoreResultsResult } from '../mig/score-results/model';
import { PlayerSearchResult } from '../mig/search/model';
import { TalentDetailResult } from '../mig/talent-details/model';
import { TeamCompositionsResult } from '../mig/team-compositions/model';
import { FilterDefinitions } from '../models/filters';
import { LanguageDescription } from '../models/language-description';
import { UserService } from './user.service';

@Injectable({
  providedIn: 'root'
})
export class MigrationService {
  private _url = `/api/mig/Default`;
  private _auth = `/api/mig/Auth`;

  constructor(private http: HttpClient, private auth: UserService) { }

  login(creds) {
    return this.http.post<any>(`${this._auth}/login`, creds).pipe(
      tap(r => this.auth.refresh()),
      catchError(err => {
        throw JSON.parse(err.error);
      }),
    );
  }

  logout() {
    return this.http.post(`/api/mig/Auth/logout`, null, { responseType: 'text' }).pipe(
      tap(r => this.auth.refresh()),
    );
  }

  changePassword(req): Observable<any> {
    return this.http.post(`${this._auth}/passwd`, req, { responseType: 'text' }).pipe(
      catchError(err => {
        throw JSON.parse(err.error);
      }),
    );
  }

  changeOptOut(req): Observable<any> {
    const params = new HttpParams()
      .set('optout', req ? 'True' : 'False');
    return this.http.post(`${this._auth}/optout`, null, { params, responseType: 'text' }).pipe(
      catchError(err => {
        throw JSON.parse(err.error);
      }),
    );
  }

  changeGameMode(req: number): Observable<any> {
    const params = new HttpParams()
      .set('gm', req.toString());
    return this.http.post(`${this._auth}/gamemode`, null, { params, responseType: 'text' }).pipe(
      catchError(err => {
        throw JSON.parse(err.error);
      }),
    );
  }

  makeMain(req: number): Observable<any> {
    const params = new HttpParams()
      .set('id', req.toString());
    return this.http.post(`${this._auth}/makemain`, null, { params, responseType: 'text' }).pipe(
      catchError(err => {
        throw JSON.parse(err.error);
      }),
    );
  }

  removeAlt(req: number): Observable<any> {
    const params = new HttpParams()
      .set('id', req.toString());
    return this.http.post(`${this._auth}/removealt`, null, { params, responseType: 'text' }).pipe(
      catchError(err => {
        throw JSON.parse(err.error);
      }),
    );
  }

  resetPassword(req): Observable<any> {
    return this.http.post(`${this._auth}/resetpasswd`, req, { responseType: 'text' }).pipe(
      catchError(err => {
        throw JSON.parse(err.error);
      }),
    );
  }

  resetPasswordConfirm(req): Observable<any> {
    return this.http.post(`${this._auth}/resetpasswdconfirm`, req, { responseType: 'text' }).pipe(
      catchError(err => {
        throw JSON.parse(err.error);
      }),
    );
  }

  register(req): Observable<any> {
    return this.http.post(`${this._auth}/register`, req, { responseType: 'text' }).pipe(
      catchError(err => {
        throw JSON.parse(err.error);
      }),
    );
  }

  get(): Observable<any> {
    const params = new HttpParams()
      .set('ReferenceDate', new Date().toJSON())
      .set('IsMobileDevice', 'false')
      .set('IsPostBack', 'false')
      .set('MonkeyBrokerScriptVisible', 'false')
      ;
    return this.http.get<any>(this._url, { params });
  }

  getFilters(): Observable<FilterDefinitions> {
    return this.http.get<FilterDefinitions>(`${this._url}/filters`);
  }

  getTalentDetails(req): Observable<TalentDetailResult> {
    return this.http.post<TalentDetailResult>(`${this._url}/talentdetails`, req);
  }

  getHeroAndMap(req): Observable<HeroAndMapResult> {
    return this.http.post<HeroAndMapResult>(`${this._url}/heroandmap`, req);
  }

  getMapObjectives(req): Observable<MapObjectivesResult> {
    return this.http.post<MapObjectivesResult>(`${this._url}/mapobjectives`, req);
  }

  getScoreResults(req): Observable<ScoreResultsResult> {
    return this.http.post<ScoreResultsResult>(`${this._url}/scoreresults`, req);
  }

  getTeamCompositions(req): Observable<TeamCompositionsResult> {
    return this.http.post<TeamCompositionsResult>(`${this._url}/teamcompositions`, req);
  }

  getMatchAwards(req): Observable<MatchAwardsResult> {
    return this.http.post<MatchAwardsResult>(`${this._url}/matchawards`, req);
  }

  getRankings(req): Observable<RankingsResult> {
    return this.http.post<RankingsResult>(`${this._url}/rankings`, req);
  }

  getHeroRankings(req): Observable<HeroRankingsResult> {
    return this.http.post<HeroRankingsResult>(`${this._url}/herorankings`, req);
  }

  getReputation(req): Observable<ReputationResult> {
    return this.http.post<ReputationResult>(`${this._url}/reputation`, req);
  }

  getMatchSummary(req): Observable<MatchSummaryResult> {
    return this.http.post<MatchSummaryResult>(`${this._url}/matchsummary`, req);
  }

  getMatchHistory(req): Observable<MatchHistoryResult> {
    return this.http.post<MatchHistoryResult>(`${this._url}/matchhistory`, req);
  }

  getOverview(req): Observable<OverviewResult> {
    return this.http.post<OverviewResult>(`${this._url}/overview`, req);
  }

  getProfile(req): Observable<ProfileResult> {
    return this.http.post<ProfileResult>(`${this._url}/profile`, req);
  }

  getPlayerSearch(req): Observable<PlayerSearchResult> {
    return this.http.post<PlayerSearchResult>(`${this._url}/search`, req);
  }

  getAccountData(req): Observable<AccountData> {
    return this.http.post<AccountData>(`${this._auth}/account`, req);
  }

  confirmSubscription(req): Observable<any> {
    const params = new HttpParams()
      .set('subId', req);
    return this.http.post(`${this._auth}/confirmsub`, null, { params, responseType: 'text' });
  }

  cancelSubscription(req): Observable<any> {
    const params = new HttpParams()
      .set('subId', req);
    return this.http.post(`${this._auth}/cancelsub`, null, { params, responseType: 'text' });
  }

  bnetAuth(btag: string, region: number): Observable<any> {
    var params = new HttpParams()
      .set('battleTag', btag)
      .set('region', region.toString());
    return this.http.post(`${this._url}/bnetid`, null, { params, responseType: 'text' }).pipe(
      catchError(err => {
        throw JSON.parse(err.error);
      }),
    );
  }

  vote(up: boolean, pid: number, rid: number, flag: boolean): Observable<VoteResponse> {
    const body: VoteRequest = {
      Up: up,
      PlayerId: pid,
      ReplayId: rid,
      Perform: flag,
    };
    return this.http.post<any>(`${this._url}/vote`, body);
  }

  changeLanguage(lang: string): Observable<LanguageDescription> {
    return this.http.post<LanguageDescription>(`${this._url}/lang/${lang}`, null);
  }

  getLanguage(): Observable<LanguageDescription> {
    return this.http.get<LanguageDescription>(`${this._url}/lang`);
  }
}
