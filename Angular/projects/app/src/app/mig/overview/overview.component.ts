import { Component, OnInit } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { ActivatedRoute, Router } from '@angular/router';
import { combineLatest, finalize, map, Observable, ReplaySubject, Subscription, switchMap, tap } from 'rxjs';
import { AppUser } from '../../models/app-user';
import { FilterDefinitions } from '../../models/filters';
import { OverlayService } from '../../modules/shared/services/overlay.service';
import { MigrationService } from '../../services/migration.service';
import { UserService } from '../../services/user.service';
import { HeroStat, MapStat, OverviewRequest, OverviewResult } from './model';

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
  selector: 'app-overview',
  templateUrl: './overview.component.html',
  styleUrls: ['./overview.component.scss']
})
export class OverviewComponent implements OnInit {
  filters: FilterDefinitions;

  playerId?: number;
  gameMode: string;
  gameModeFilter: NameValue[];
  time: string;
  timeFilter: NameValue[];
  gamesTogether: string;
  gamesTogetherFilter: NameValue[] = [
    { key: '5+ Games Played', value: '5' },
    { key: '10+ Games Played', value: '10' },
    { key: '20+ Games Played', value: '20' },
    { key: '50+ Games Played', value: '50' },
    { key: '100+ Games Played', value: '100' },
  ];
  partySizeFilter: NameValue[] = [
    { key: '2+ Player Party', value: '2' },
    { key: '3+ Player Party', value: '3' },
    { key: '4+ Player Party', value: '4' },
    { key: '5 Player Party', value: '5' },
  ];
  partySize: string;
  allTabs: NameValueN2[] = [
    { key: "Average Match Statistics", value: 0 },
    {
      key: "Average Role Statistics", value: 1, tabs: [
        { key: "Tank", value: 0 },
        { key: "Bruiser", value: 1 },
        { key: "Healer", value: 2 },
        { key: "Support", value: 3 },
        { key: "Melee Assassin", value: 4 },
        { key: "Ranged Assassin", value: 5 },
      ]
    },
    {
      key: "Hero and Map Summary", value: 2, tabs: [
        { key: 'Hero Statistics', value: 0 },
        { key: 'Map Statistics', value: 1 },
      ]
    },
    {
      key: "Talent Upgrades", value: 3, tabs: [
        { key: 'Nova - Sniper Master', value: 0 },
        { key: 'Gall - Dark Descent', value: 1 },
      ]
    },
  ];
  tabs: NameValueN2[];
  activeTab: [number, number | null] = [0, null];
  nextActiveTab: [number, number | null] = [...this.activeTab];
  req$ = new ReplaySubject<OverviewRequest>(1);
  data$: Observable<OverviewResult>;
  result: OverviewResult;
  subs: Subscription[];
  user: AppUser;
  heroOverview: boolean;

  public set querystring(value: string) {
    this.getQueryString(value || '');
  }

  private getQueryString(value: string) {
    const spl = value.substring(1).split('&').map(r => r.split('=').map(z => decodeURIComponent(z.replace(/\+/g, ' '))));
    const tab = spl.find(r => r[0] === 'Tab');
    if (tab) {
      const tabs = tab[1].split(',').map(r => +r);
      this.activeTab = [0, 0];
      tabs.slice(0, 2).forEach((x, i) => this.activeTab[i] = x || 0);
    } else {
      this.activeTab = [0, 0];
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
    const gamesTogether = spl.find(r => r[0] === 'GamesTogether');
    if (gamesTogether) {
      this.gamesTogether = gamesTogether[1];
    } else {
      this.gamesTogether = null;
    }
    const partySize = spl.find(r => r[0] === 'PartySize');
    if (partySize) {
      this.partySize = partySize[1];
    } else {
      this.partySize = null;
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
      tap(r => ovl.setOverlay('overview', true)),
      switchMap(r => svc.getOverview(r).pipe(map(resp => [resp, r] as const))),
      tap(([r, req]) => this.setupData(r, req)),
      map(([r, req]) => r),
      tap(r => ovl.setOverlay('overview', false)),
      finalize(() => ovl.setOverlay('overview', false)),
    );
    title.setTitle(`Team Overview`);
  }

  ngOnInit(): void {
    const subs = [];
    const allStreams = combineLatest([this.usr.user$, this.route.data, this.route.queryParamMap]);
    subs.push(allStreams.subscribe(([user, r]) => {
      this.filters = r.filters;
      this.heroOverview = r.player;
      this.user = user;
      if (this.heroOverview) {
        this.title.setTitle('Hero Overview');
      }
      this.querystring = window.location.search;
      this.gameMode ||= '0';
      this.time ||= '1';
      this.gamesTogether ||= '10';
      this.partySize ||= '3';
      if (!this.playerId && this.playerId !== 0) {
        this.playerId = this.user?.mainPlayerId;
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

    this.gamesTogetherFilter.find(r => r.value === this.gamesTogether).selected = true;

    this.partySizeFilter.find(r => r.value === this.partySize).selected = true;

    if (this.heroOverview) {
      this.tabs = this.allTabs.filter(r => r.value !== 2);
    } else {
      this.tabs = this.allTabs;
    }
  }

  tabClick(level: number, t: NameValueN2) {
    this.nextActiveTab = [...this.activeTab];
    this.nextActiveTab[level] = t.value;
    if (t.tabs) {
      if (!this.nextActiveTab[level + 1]) {
        this.nextActiveTab[level + 1] = 0;
      } else if (this.nextActiveTab[level + 1] >= t.tabs.length) {
        this.nextActiveTab[level + 1] = 0;
      }
    }
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

  private getRequestObject(): OverviewRequest {
    const GameMode = this.gameMode;
    const Time = this.time;
    const GamesTogether = this.gamesTogether;
    const PartySize = this.partySize;
    const Tab = this.nextActiveTab;
    const req: OverviewRequest = {
      TeamOverview: !this.heroOverview,
      PlayerId: this.playerId,
      GameMode,
      Time,
      GamesTogether,
      PartySize,
      Tab,
    };
    return req;
  }

  setupData(r: OverviewResult, req: OverviewRequest): void {
    this.result = r;
    this.title.setTitle(r.Title);
    this.activeTab = req.Tab;
  }

  private setQueryString(req: OverviewRequest) {
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
    if (req.GamesTogether !== '10') {
      qs.push(['GamesTogether', `${req.GamesTogether}`]);
      queryParams['GamesTogether'] = req.GamesTogether;
    }
    if (req.PartySize !== '3') {
      qs.push(['PartySize', `${req.PartySize}`]);
      queryParams['PartySize'] = req.PartySize;
    }
    if (req.Tab[0] !== 0) {
      qs.push(['Tab', `${req.Tab.filter(r => r).join(',')}`]);
      queryParams['Tab'] = req.Tab.filter(r => r).join(',');
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

  expandHero(hero: HeroStat) {
    const req = this.getRequestObject();
    req.HeroDetails = hero.CharacterURL;
    hero.SummaryLoading = true;
    this.svc.getOverview(req).subscribe(r => {
      hero.SummaryLoading = false;
      hero.Summary = r.HeroDetails;
    });
  }

  expandMap(map: MapStat) {
    const req = this.getRequestObject();
    req.MapDetails = map.Map;
    map.SummaryLoading = true;
    this.svc.getOverview(req).subscribe(r => {
      map.SummaryLoading = false;
      map.Summary = r.MapDetails;
    });
  }

  getTab(i: number) {
    return this.tabs.find(r => r.value === i);
  }
}
