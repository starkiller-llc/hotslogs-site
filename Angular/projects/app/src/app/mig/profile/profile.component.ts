import { Component, OnInit } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { ActivatedRoute, Router } from '@angular/router';
import { ReplaySubject, Observable, Subscription, switchMap, tap, combineLatest, map, finalize } from 'rxjs';
import { AppUser } from '../../models/app-user';
import { FilterDefinitions } from '../../models/filters';
import { OverlayService } from '../../modules/shared/services/overlay.service';
import { MigrationService } from '../../services/migration.service';
import { UserService } from '../../services/user.service';
import { MapStatisticsRow, ProfileCharacterStatisticsRow, ProfileRequest, ProfileResult } from './model';

interface NameValueB<T> {
  value: T;
  key: string;
  selected?: boolean;
}

type NameValue = NameValueB<string>;
type NameValueN = NameValueB<number>;

interface NameValueN2 extends NameValueN {
  tabs?: NameValueN2[];
}

@Component({
  selector: 'app-profile',
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.scss']
})
export class ProfileComponent implements OnInit {
  filters: FilterDefinitions;

  playerId?: number;
  gameMode: string;
  gameModeFilter: NameValue[];
  gameMode2: string;
  gameModeFilter2: NameValue[];
  time: string;
  timeFilter: NameValue[];
  tabs: string[] = [
    'Hero Statistics',
    'Map Statistics',
    'Matchups',
    'Duos',
    'MMR Milestones',
    'Win Rate by Game Time',
    'Friends',
    'Rivals',
    'Shared Replays',
  ];
  activeTab = 0;
  nextActiveTab = 0;
  req$ = new ReplaySubject<ProfileRequest>(1);
  data$: Observable<ProfileResult>;
  result: ProfileResult;
  subs: Subscription[];
  user: AppUser;

  public set querystring(value: string) {
    this.getQueryString(value || '');
  }

  private getQueryString(value: string) {
    const spl = value.substring(1).split('&').map(r => r.split('=').map(z => decodeURIComponent(z.replace(/\+/g, ' '))));
    const tab = spl.find(r => r[0] === 'Tab');
    if (tab) {
      this.activeTab = +tab[1];
    } else {
      this.activeTab = 0;
    }
    this.nextActiveTab = this.activeTab;

    const gameMode = spl.find(r => r[0] === 'GameMode');
    if (gameMode) {
      this.gameMode = gameMode[1];
    } else {
      this.gameMode = null;
    }
    const time = spl.find(r => r[0] === 'Time');
    if (time) {
      this.time = time[1];
    } else {
      this.time = null;
    }
    const playerId = spl.find(r => r[0] === 'PlayerID');
    if (playerId) {
      this.playerId = +playerId[1];
    } else {
      this.playerId = null;
    }
  }

  constructor(
    private svc: MigrationService,
    ovl: OverlayService,
    private route: ActivatedRoute,
    public title: Title,
    private router: Router,
    private usr: UserService) {

    this.data$ = this.req$.pipe(
      tap(r => ovl.setOverlay('profile', true)),
      switchMap(r => svc.getProfile(r).pipe(map(resp => [resp, r] as const))),
      tap(([r, req]) => this.setupData(r, req)),
      map(([r, req]) => r),
      tap(r => ovl.setOverlay('profile', false)),
      finalize(() => ovl.setOverlay('profile', false)),
    );
    title.setTitle(`Profile`);
  }

  ngOnInit(): void {
    const subs = [];
    const allStreams = combineLatest([this.usr.user$, this.route.data, this.route.queryParamMap]);
    subs.push(allStreams.subscribe(([user, r]) => {
      this.filters = r.filters;
      this.user = user;
      this.querystring = window.location.search;
      this.gameMode ||= '0';
      this.gameMode2 ||= user?.defaultGameMode.toString() || '8';
      this.time ||= '1';
      if (!this.playerId && this.playerId !== 0) {
        this.playerId = this.user?.mainPlayerId;
      }
      this.setFilters(this.filters);
      if (!this.playerId) {
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
    this.gameModeFilter = r.GameMode
      .filter(r => r.GameMode < 1000)
      .map(z => ({
        key: z.GameModeDisplayText,
        value: z.GameMode.toString(),
        selected: z.GameMode.toString() === this.gameMode,
      }));
    this.gameModeFilter.splice(0, 0, {
      key: 'All Game Modes',
      value: '0',
      selected: this.gameMode === '0',
    });
    if (this.playerId === this.user?.mainPlayerId) {
      this.gameModeFilter.push({
        key: 'Custom',
        value: '-1',
        selected: this.gameMode === '-1',
      });
    }

    const mmrGameModes = ['8', '6', '3'];
    this.gameModeFilter2 = this.gameModeFilter.filter(r => mmrGameModes.includes(r.value));
    this.gameModeFilter2.find(r => r.value === this.gameMode2).selected = true;

    this.timeFilter = [
      { key: 'All Days', value: '1' },
      { key: 'Last 30 Days', value: '-30' },
      { key: 'Last 60 Days', value: '-60' },
      { key: 'Last 90 Days', value: '-90' },
      { key: 'Last 180 Days', value: '-180' },
      { key: 'Last 360 Days', value: '-360' },
      ...this.filters.Season.map(r => ({
        key: r.Title,
        value: r.Title,
      }))
    ];
    this.timeFilter.find(r => r.value === this.time).selected = true;
  }

  tabClick(t: number) {
    this.nextActiveTab = t;
    this.getData(false);
  }

  getData(emit = true) {
    if (!this.gameModeFilter) {
      return;
    }
    const req = this.getRequestObject();
    this.setQueryString(req);
    if (emit) {
      this.req$.next(req);
    }
  }

  private getRequestObject(): ProfileRequest {
    const GameMode = this.gameMode;
    const Time = this.time;
    const Tab = this.nextActiveTab;
    const req: ProfileRequest = {
      PlayerId: this.playerId,
      GameMode,
      GameModeForMmr: this.gameMode2,
      Time,
      Tab,
    };
    return req;
  }

  setupData(r: ProfileResult, req: ProfileRequest): void {
    this.result = r;
    this.title.setTitle(r.Title);
    this.activeTab = req.Tab;
  }

  private setQueryString(req: ProfileRequest) {
    const qs = [];
    const queryParams = {};
    if (req.GameMode !== '0') {
      qs.push(['GameMode', `${req.GameMode}`]);
      queryParams['GameMode'] = req.GameMode;
    }
    if (req.Time !== '1') {
      qs.push(['Time', `${req.Time}`]);
      queryParams['Time'] = req.Time;
    }
    if (req.Tab !== 0) {
      qs.push(['Tab', `${req.Tab}`]);
      queryParams['Tab'] = req.Tab;
    }
    if (this.playerId) {
      qs.push([`PlayerID`, `${this.playerId}`]);
      queryParams['PlayerID'] = this.playerId;
    }

    const q1 = qs.map(([a, b]) => `${a}=${encodeURIComponent(b)}`).join('&');
    const q = q1 ? `?${q1}` : '';
    const basePath = window.location.pathname;
    const url = `${basePath}${q}`;
    this.router.navigate([window.location.pathname], { queryParams, replaceUrl: true });
  }

  expandHero(hero: ProfileCharacterStatisticsRow) {
    const req = this.getRequestObject();
    req.HeroDetails = hero.CharacterURL;
    hero.SummaryLoading = true;
    this.svc.getProfile(req).subscribe(r => {
      hero.SummaryLoading = false;
      hero.Summary = r.HeroDetails;
    });
  }

  expandMap(map: MapStatisticsRow) {
    const req = this.getRequestObject();
    req.MapDetails = map.Map;
    map.SummaryLoading = true;
    this.svc.getProfile(req).subscribe(r => {
      map.SummaryLoading = false;
      map.Summary = r.MapDetails;
    });
  }
}
