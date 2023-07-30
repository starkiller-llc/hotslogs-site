import { Component, OnInit } from '@angular/core';
import { PageEvent } from '@angular/material/paginator';
import { Sort } from '@angular/material/sort';
import { Title } from '@angular/platform-browser';
import { ActivatedRoute, Router } from '@angular/router';
import { ReplaySubject, Observable, Subscription, switchMap, tap, combineLatest, debounceTime } from 'rxjs';
import { AppUser } from '../../models/app-user';
import { FilterDefinitions } from '../../models/filters';
import { MigrationService } from '../../services/migration.service';
import { UserService } from '../../services/user.service';
import { RankingsRequest, RankingsResult } from './model';

interface NameValueB<T> {
  value: T;
  key: string;
  selected?: boolean;
}

type NameValue = NameValueB<string>;
type NameValueN = NameValueB<number>;

@Component({
  selector: 'app-rankings',
  templateUrl: './rankings.component.html',
  styleUrls: ['./rankings.component.scss']
})
export class RankingsComponent implements OnInit {
  filters: FilterDefinitions;

  region: number;
  gameMode: string;
  league: number;
  leagueFilter: NameValueN[];

  req$ = new ReplaySubject<RankingsRequest>(1);
  data$: Observable<RankingsResult>;
  result: RankingsResult;
  subs: Subscription[];
  user: AppUser;
  page: PageEvent;
  tableFilter: string;
  sort: Sort;

  public set querystring(value: string) {
    this.getQueryString(value || '');
  }

  private getQueryString(value: string) {
    const spl = value.substring(1).split('&').map(r => r.split('=').map(z => decodeURIComponent(z.replace(/\+/g, ' '))));
    const region = spl.find(r => r[0] === 'Region');
    if (region) {
      this.region = +region[1];
    } else {
      this.region = null;
    }
    const gameMode = spl.find(r => r[0] === 'GameMode');
    if (gameMode) {
      this.gameMode = gameMode[1];
    } else {
      this.gameMode = null;
    }
    const league = spl.find(r => r[0] === 'League');
    if (league) {
      this.league = +league[1];
    } else {
      this.league = null;
    }
  }

  constructor(svc: MigrationService, private route: ActivatedRoute, public title: Title, private usr: UserService, private router: Router) {
    this.data$ = this.req$.pipe(
      debounceTime(200),
      switchMap(r => svc.getRankings(r)),
      tap(r => this.setupData(r))
    );
    title.setTitle(`Heroes of the Storm Leaderboard | HOTS Logs`);
  }

  ngOnInit(): void {
    const subs = [];
    const allStreams = combineLatest([this.usr.user$, this.route.data, this.route.queryParamMap]);
    subs.push(allStreams.subscribe(([user, r]) => {
      this.user = user;
      this.filters = r.filters;
      this.querystring = window.location.search;
      if (!this.region) {
        this.region = this.user?.region || 1;
      }
      this.gameMode ||= '8';
      this.league ||= 0;
      if (!this.page) {
        this.page = {
          length: 0,
          pageIndex: 0,
          pageSize: 20,
          previousPageIndex: 0,
        };
      }
      this.setFilters(this.filters);
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
    this.leagueFilter = r.LeagueCombo.map(z => ({
      key: z.LeagueDisplayText,
      value: z.LeagueID,
      selected: this.league === z.LeagueID,
    }));
  }

  getData(emit = true) {
    if (!this.leagueFilter) {
      return;
    }
    const GameMode = this.gameMode;
    const req: RankingsRequest = {
      GameMode,
      League: [this.league],
      Region: this.region,
      Filter: this.tableFilter,
      Page: this.page,
      Sort: this.sort,
    };
    this.setQueryString(req);
    if (emit) {
      this.req$.next(req);
    }
  }

  setupData(r: RankingsResult): void {
    this.result = r;
  }

  private setQueryString(req: RankingsRequest) {
    const qs = [];
    const queryParams = {};
    if (req.GameMode !== '8') {
      qs.push(['GameMode', `${req.GameMode}`]);
      queryParams['GameMode'] = req.GameMode;
    }
    if (req.League[0] !== 0) {
      qs.push(['League', `${req.League}`]);
      queryParams['League'] = req.League;
    }
    qs.push(['Region', `${req.Region}`]);
    queryParams['Region'] = req.Region;

    const q1 = qs.map(([a, b]) => `${a}=${encodeURIComponent(b)}`).join('&');
    const q = q1 ? `?${q1}` : '';
    const basePath = window.location.pathname;
    const url = `${basePath}${q}`;
    this.router.navigate([window.location.pathname], { queryParams, replaceUrl: true });
  }

  filterChange(s: string) {
    this.tableFilter = s;
    this.getData();
  }

  pageChange(p: PageEvent) {
    this.page = {
      ...p
    };
    this.getData();
  }

  sortChange(s: Sort) {
    this.sort = {
      ...s
    };
    this.getData();
  }
}
