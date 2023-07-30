import { Component, OnInit } from '@angular/core';
import { PageEvent } from '@angular/material/paginator';
import { Sort } from '@angular/material/sort';
import { Title } from '@angular/platform-browser';
import { ActivatedRoute, Router } from '@angular/router';
import { ReplaySubject, Observable, Subscription, switchMap, tap, combineLatest } from 'rxjs';
import { AppUser } from '../../models/app-user';
import { FilterDefinitions } from '../../models/filters';
import { MigrationService } from '../../services/migration.service';
import { UserService } from '../../services/user.service';
import { HeroRankingsRequest, HeroRankingsResult } from './model';

interface NameValueB<T> {
  value: T;
  key: string;
  selected?: boolean;
}

type NameValue = NameValueB<string>;
type NameValueN = NameValueB<number>;

@Component({
  selector: 'app-hero-rankings',
  templateUrl: './hero-rankings.component.html',
  styleUrls: ['./hero-rankings.component.scss']
})
export class HeroRankingsComponent implements OnInit {
  filters: FilterDefinitions;

  region: number;
  gameMode: string;
  hero: string;
  heroFilter: NameValue[];
  season: string;
  seasonFilter: NameValue[];

  req$ = new ReplaySubject<HeroRankingsRequest>(1);
  data$: Observable<HeroRankingsResult>;
  result: HeroRankingsResult;
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
    const hero = spl.find(r => r[0] === 'Hero');
    if (hero) {
      this.hero = hero[1];
    } else {
      this.hero = null;
    }
    const season = spl.find(r => r[0] === 'Season');
    if (season) {
      this.season = season[1];
    } else {
      this.season = null;
    }
  }

  constructor(svc: MigrationService, private route: ActivatedRoute, public title: Title, private usr: UserService, private router: Router) {
    this.data$ = this.req$.pipe(
      switchMap(r => svc.getHeroRankings(r)),
      tap(r => this.setupData(r))
    );
    title.setTitle(`Hero Leaderboard | HOTS Logs`);
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
      this.gameMode ||= this.user?.defaultGameMode?.toString() || '8';
      this.hero ||= 'Abathur';
      this.season ||= this.filters.Season[0].Title;
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
    this.heroFilter = r.Hero.map(z => ({
      key: z.DisplayName,
      value: z.PrimaryName,
      selected: this.hero === z.PrimaryName,
    }));

    this.seasonFilter = r.Season.map(z => ({
      key: z.Title,
      value: z.Title,
      selected: this.season === z.Title,
    }));
  }

  getData(emit = true) {
    if (!this.heroFilter) {
      return;
    }
    const GameMode = this.gameMode;
    const req: HeroRankingsRequest = {
      GameMode,
      Hero: this.hero,
      Region: this.region,
      Season: this.season,
      Filter: this.tableFilter,
      Page: this.page,
      Sort: this.sort,
    };
    this.setQueryString(req);
    if (emit) {
      this.req$.next(req);
    }
  }

  setupData(r: HeroRankingsResult): void {
    this.result = r;
  }

  private setQueryString(req: HeroRankingsRequest) {
    const qs = [];
    const queryParams = {};
    if (req.GameMode !== '8') {
      qs.push(['GameMode', `${req.GameMode}`]);
      queryParams['GameMode'] = req.GameMode;
    }
    if (req.Hero[0] !== '0') {
      qs.push(['Hero', `${req.Hero}`]);
      queryParams['Hero'] = req.Hero;
    }
    qs.push(['Season', `${req.Season}`]);
    queryParams['Season'] = req.Season;
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
