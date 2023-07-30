import { Component, OnInit } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { ActivatedRoute, Router } from '@angular/router';
import { combineLatest, finalize, map, Observable, ReplaySubject, Subscription, switchMap, tap } from 'rxjs';
import { FilterDefinitions } from '../../models/filters';
import { OverlayService } from '../../modules/shared/services/overlay.service';
import { MigrationService } from '../../services/migration.service';
import { TalentDetailResult, TalentDetailsRequest } from './model';

interface NameValueB<T> {
  value: T;
  key: string;
  selected?: boolean;
}

type NameValue = NameValueB<string>;
type NameValueN = NameValueB<number>;

@Component({
  selector: 'app-talent-details',
  templateUrl: './talent-details.component.html',
  styleUrls: ['./talent-details.component.scss']
})
export class TalentDetailsComponent implements OnInit {
  tmp: NameValue[] = [
    { key: 'Extra cheese', value: '0' },
    { key: 'Mushroom', value: '1' },
    { key: 'Onion', value: '2' },
    { key: 'Pepperoni', value: '3' },
  ];
  filters: FilterDefinitions;

  gameModeEx: string;
  gameModeExFilter: NameValue[];
  tournament: string;
  tournamentFilter: NameValue[];
  hero: string;
  heroFilter: NameValue[];
  league: number[];
  leagueFilter: NameValueN[];
  map: string[];
  mapFilter: NameValue[];
  time: string[];
  timeFilter: NameValue[];
  patch: string[];
  patchFilter: NameValue[];
  tabs: NameValueN[] = [
    { key: "Talent Details", value: 0 },
    { key: "Wins Over Time", value: 1 },
    { key: "Wins by Game Time", value: 2 },
    { key: "Matchups", value: 3 },
    { key: "Duos", value: 4 },
    { key: "Wins by Map", value: 5 },
    { key: "Wins by Hero Level", value: 6 },
    { key: "Wins by Talent Upgrades", value: 7 },
  ]
  activeTab = 0;
  req$ = new ReplaySubject<TalentDetailsRequest>(1);
  data$: Observable<TalentDetailResult>;
  result: TalentDetailResult;
  subs: Subscription[];
  nextActiveTab = 0;

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

    const gameModeEx = spl.find(r => r[0] === 'GameModeEx');
    if (gameModeEx) {
      this.gameModeEx = gameModeEx[1];
    } else {
      this.gameModeEx = null;
    }
    const hero = spl.find(r => r[0] === 'Hero');
    if (hero) {
      this.hero = hero[1];
    } else {
      this.hero = null;
    }
    const tournament = spl.find(r => r[0] === 'Event');
    if (tournament) {
      this.tournament = tournament[1];
    } else {
      this.tournament = null;
    }
    const league = spl.find(r => r[0] === 'League');
    if (league) {
      this.league = league[1].split(',').map(r => +r).filter(r => !isNaN(r));
    } else {
      this.league = null;
    }
    const map = spl.find(r => r[0] === 'Map');
    if (map) {
      this.map = map[1].split(',');
    } else {
      this.map = null;
    }
    const time = spl.find(r => r[0] === 'Time');
    if (time) {
      this.time = time[1].split(',');
    } else {
      this.time = null;
    }
    const patch = spl.find(r => r[0] === 'Patch');
    if (patch) {
      this.patch = patch[1].split(',');
    } else {
      this.patch = null;
    }
  }

  constructor(
    svc: MigrationService,
    ovl: OverlayService,
    private route: ActivatedRoute,
    private router: Router,
    private title: Title) {

    this.data$ = this.req$.pipe(
      tap(r => ovl.setOverlay('talent-details', true)),
      switchMap(r => svc.getTalentDetails(r).pipe(map(resp => [resp, r] as const))),
      tap(([r, req]) => this.setupData(r, req)),
      map(([r, req]) => r),
      tap(r => ovl.setOverlay('talent-details', false)),
      finalize(() => ovl.setOverlay('talent-details', false)),
    );
  }

  ngOnInit(): void {
    const subs = [];
    const allStreams = combineLatest([this.route.data, this.route.queryParamMap]);
    subs.push(allStreams.subscribe(([r, params]) => {
      this.filters = r.filters;
      this.querystring = window.location.search;
      this.gameModeEx ||= '8';
      this.tournament ||= this.filters.Tournament[this.filters.Tournament.length - 1].TournamentId.toString();
      this.hero ||= this.filters.Hero[0].PrimaryName;
      this.league ||= [];
      this.map ||= [];
      this.time ||= [];
      this.patch ||= [];
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
    this.gameModeExFilter = r.GameModeEx.map(z => ({
      key: z.GameModeExDisplayText,
      value: z.GameModeEx.toString(),
      selected: z.GameModeEx.toString() === (this.gameModeEx || '8'),
    }))

    this.tournamentFilter = r.Tournament.map(z => ({
      key: z.TournamentDisplayText,
      value: z.TournamentId.toString(),
      selected: z.TournamentId.toString() === this.tournament,
    }))

    this.heroFilter = r.Hero.map(z => ({
      key: z.DisplayName,
      value: z.PrimaryName,
      selected: z.PrimaryName === this.hero,
    }))

    this.leagueFilter = r.LeagueCombo.map(z => ({
      key: z.LeagueDisplayText,
      value: z.LeagueID,
      selected: this.league.includes(z.LeagueID),
    }))

    this.mapFilter = r.MapCombo.map(z => ({
      key: z.DisplayName,
      value: z.PrimaryName,
      selected: this.map.includes(z.PrimaryName),
    }))

    this.timeFilter = r.Time.map(z => ({
      key: z.Key,
      value: z.Value,
      selected: this.time.includes(z.Value),
    }))

    this.patchFilter = r.Patch.map(z => ({
      key: z.Key,
      value: z.Value,
      selected: this.patch.includes(z.Value),
    }))
  }

  tabClick(t: NameValueN) {
    this.nextActiveTab = t.value;
    this.getData(false);
  }

  getData(emit = true) {
    if (!this.gameModeExFilter) {
      return;
    }
    const GameModeEx = this.gameModeEx;
    const Tournament = this.tournament;
    const GameMode = GameModeEx === '0' ? Tournament : GameModeEx;
    const req: TalentDetailsRequest = {
      Tab: this.nextActiveTab,
      GameMode,
      GameModeEx,
      Tournament,
      Hero: this.hero,
      League: this.league,
      Map: this.map,
      Time: this.time,
      Patch: this.patch,
    };
    this.setQueryString(req);
    if (emit) {
      this.req$.next(req);
    }
  }

  setupData(r: TalentDetailResult, req: TalentDetailsRequest): void {
    this.result = r;
    const title = `${this.hero} Build & Guide - Heroes of the Storm | hotslogs.com: ${this.hero}`;
    this.title.setTitle(title);
    this.activeTab = req.Tab;
  }

  private setQueryString(req: TalentDetailsRequest) {
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
    if (req.Hero !== 'Abathur') {
      qs.push(['Hero', `${req.Hero}`]);
      queryParams['Hero'] = req.Hero;
    }
    if (req.League.length) {
      qs.push(['League', `${req.League.join(',')}`]);
      queryParams['League'] = req.League.join(',');
    }
    if (req.Map.length) {
      qs.push(['Map', `${req.Map.join(',')}`]);
      queryParams['Map'] = req.Map.join(',');
    }
    if (req.Time.length) {
      qs.push(['Time', `${req.Time.join(',')}`]);
      queryParams['Time'] = req.Time.join(',');
    }
    if (req.Patch.length) {
      qs.push(['Patch', `${req.Patch.join(',')}`]);
      queryParams['Patch'] = req.Patch.join(',');
    }
    if (req.Tab !== 0) {
      qs.push(['Tab', `${req.Tab}`]);
      queryParams['Tab'] = req.Tab;
    }

    const q1 = qs.map(([a, b]) => `${a}=${encodeURIComponent(b)}`).join('&');
    const q = q1 ? `?${q1}` : '';
    const basePath = window.location.pathname;
    const url = `${basePath}${q}`;
    this.router.navigate([window.location.pathname], { queryParams, replaceUrl: true });
  }
}
