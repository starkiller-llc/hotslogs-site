import { Component, OnInit } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { ActivatedRoute, Router } from '@angular/router';
import { ReplaySubject, Observable, Subscription, switchMap, tap, combineLatest } from 'rxjs';
import { AppUser } from '../../models/app-user';
import { FilterDefinitions } from '../../models/filters';
import { MigrationService } from '../../services/migration.service';
import { UserService } from '../../services/user.service';
import { MatchAwardsRequest, MatchAwardsResult } from './model';

interface NameValueB<T> {
  value: T;
  key: string;
  selected?: boolean;
}

type NameValue = NameValueB<string>;
type NameValueN = NameValueB<number>;

@Component({
  selector: 'app-match-awards',
  templateUrl: './match-awards.component.html',
  styleUrls: ['./match-awards.component.scss']
})
export class MatchAwardsComponent implements OnInit {
  filters: FilterDefinitions;

  gameModeEx: string;
  gameModeExFilter: NameValue[];
  tournament: string;
  tournamentFilter: NameValue[];
  league: number;
  leagueFilter: NameValueN[];
  type: number;
  playerId: number;

  req$ = new ReplaySubject<MatchAwardsRequest>(1);
  data$: Observable<MatchAwardsResult>;
  result: MatchAwardsResult;
  subs: Subscription[];
  user: AppUser;

  public set querystring(value: string) {
    this.getQueryString(value || '');
  }

  private getQueryString(value: string) {
    const spl = value.substring(1).split('&').map(r => r.split('=').map(z => decodeURIComponent(z.replace(/\+/g, ' '))));
    const playerId = spl.find(r => r[0] === 'PlayerID');
    if (playerId) {
      this.playerId = +playerId[1];
    } else {
      this.playerId = null;
    }
    const gameModeEx = spl.find(r => r[0] === 'GameModeEx');
    if (gameModeEx) {
      this.gameModeEx = gameModeEx[1];
    } else {
      this.gameModeEx = null;
    }
    const tournament = spl.find(r => r[0] === 'Event');
    if (tournament) {
      this.tournament = tournament[1];
    } else {
      this.tournament = null;
    }
    const league = spl.find(r => r[0] === 'League');
    if (league) {
      this.league = +league[1];
    } else {
      this.league = null;
    }
    const type = spl.find(r => r[0] === 'Type');
    if (type) {
      this.type = +type[1];
    } else {
      this.type = null;
    }
  }

  constructor(svc: MigrationService, private route: ActivatedRoute, public title: Title, private usr: UserService, private router: Router) {
    this.data$ = this.req$.pipe(
      switchMap(r => svc.getMatchAwards(r)),
      tap(r => this.setupData(r))
    );
    title.setTitle(`Heroes of the Storm Match Awards Averages | HOTS Logs`);
  }

  ngOnInit(): void {
    const subs = [];
    const allStreams = combineLatest([this.usr.user$, this.route.data, this.route.queryParamMap]);
    subs.push(allStreams.subscribe(([user, r]) => {
      this.user = user;
      this.filters = r.filters;
      this.querystring = window.location.search;
      this.gameModeEx ||= '8';
      this.tournament ||= this.filters.Tournament[this.filters.Tournament.length - 1].TournamentId.toString();
      if (!this.league && this.league !== 0) {
        this.league = -1;
      }
      this.type ||= 0;
      this.playerId ||= 0;

      if (r.player && !this.playerId) {
        this.playerId = this.user?.mainPlayerId || 0;
      }

      this.setFilters(this.filters);
      if (!this.playerId && r.player) {
        if (!this.user) {
          this.router.navigate(['/Login'], { queryParams: { returnUrl: window.location.pathname + window.location.search } });
        } else {
          this.router.navigate(['/Account/Manage']);
        }
        return;
      }
      this.getData();
    }));
    this.subs = subs;
  }

  ngOnDestroy() {
    this.subs.forEach(r => r.unsubscribe());
  }

  sel() {
    this.getData(false);
  }

  setFilters(r: FilterDefinitions): void {
    this.gameModeExFilter = r.GameModeEx.map(z => ({
      key: z.GameModeExDisplayText,
      value: z.GameModeEx.toString(),
      selected: z.GameModeEx.toString() === (this.gameModeEx || '8'),
    }));

    this.tournamentFilter = r.Tournament.map(z => ({
      key: z.TournamentDisplayText,
      value: z.TournamentId.toString(),
      selected: z.TournamentId.toString() === this.tournament,
    }));

    this.leagueFilter = r.LeagueCombo.map(z => ({
      key: z.LeagueDisplayText,
      value: z.LeagueID,
      selected: this.league === z.LeagueID,
    }));
    this.leagueFilter.splice(0, 0, {
      key: 'All Leagues',
      value: -1,
      selected: this.league === -1,
    });
  }

  getData(emit = true) {
    if (!this.gameModeExFilter) {
      return;
    }
    const GameModeEx = this.gameModeEx;
    const Tournament = this.tournament;
    const GameMode = GameModeEx === '0' ? Tournament : GameModeEx;
    const req: MatchAwardsRequest = {
      GameMode,
      GameModeEx,
      Tournament,
      League: [this.league],
      Type: this.type,
      PlayerId: this.playerId,
    };
    this.setQueryString(req);
    if (emit) {
      this.req$.next(req);
    }
  }

  setupData(r: MatchAwardsResult): void {
    this.result = r;
    this.title.setTitle(r.Title);
  }

  private setQueryString(req: MatchAwardsRequest) {
    const qs = [];
    const queryParams = {};
    if (req.GameModeEx !== '8') {
      qs.push(['GameModeEx', `${req.GameModeEx}`]);
      queryParams['GameModeEx'] = req.GameModeEx;
    }
    if (req.GameModeEx === '0') {
      qs.push(['Event', `${req.Tournament}`]);
      queryParams['Event'] = req.Tournament;
    }
    if (req.League[0] !== -1) {
      qs.push(['League', `${req.League}`]);
      queryParams['League'] = req.League;
    }
    if (req.Type !== 0) {
      qs.push(['Type', `${req.Type}`]);
      queryParams['Type'] = req.Type;
    }
    if ((req.PlayerId || 0) !== 0) {
      qs.push(['PlayerID', `${req.PlayerId}`]);
      queryParams['PlayerID'] = req.PlayerId;
    }

    const q1 = qs.map(([a, b]) => `${a}=${encodeURIComponent(b)}`).join('&');
    const q = q1 ? `?${q1}` : '';
    const basePath = window.location.pathname;
    const url = `${basePath}${q}`;
    this.router.navigate([window.location.pathname], { queryParams, replaceUrl: true });
  }
}
