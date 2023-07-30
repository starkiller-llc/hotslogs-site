import { Component, OnInit } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { ActivatedRoute, Router } from '@angular/router';
import { ReplaySubject, Observable, Subscription, switchMap, tap, combineLatest } from 'rxjs';
import { FilterDefinitions } from '../../models/filters';
import { MigrationService } from '../../services/migration.service';
import { ScoreResultsRequest, ScoreResultsResult } from './model';

interface NameValueB<T> {
  value: T;
  key: string;
  selected?: boolean;
}

type NameValue = NameValueB<string>;
type NameValueN = NameValueB<number>;

interface NameValueN2 extends NameValueN {
  exclude: string[];
}

@Component({
  selector: 'app-score-results',
  templateUrl: './score-results.component.html',
  styleUrls: ['./score-results.component.scss']
})
export class ScoreResultsComponent implements OnInit {
  filters: FilterDefinitions;

  gameModeEx: string;
  gameModeExFilter: NameValue[];
  tournament: string;
  tournamentFilter: NameValue[];
  league: number[];
  leagueFilter: NameValueN[];
  map: string[];
  mapFilter: NameValue[];
  time: string[];
  timeFilter: NameValue[];
  patch: string[];
  patchFilter: NameValue[];
  tabs: NameValueN[] = [
    { key: "Average Match Statistics", value: 0 },
    { key: "Average Role Statistics", value: 1 },
  ]
  activeTab = 0;
  subTabs: NameValueN2[] = [
    { key: "Tank", value: 0, exclude: ['KDRatio', 'SoloKills', 'Assists', 'Healing'] },
    { key: "Bruiser", value: 1, exclude: ['KDRatio', 'SoloKills', 'Assists', 'Healing'] },
    { key: "Healer", value: 2, exclude: ['KDRatio', 'SoloKills', 'Assists', 'DamageTaken'] },
    { key: "Support", value: 3, exclude: ['KDRatio', 'SoloKills', 'Assists', 'DamageTaken'] },
    { key: "Melee Assassin", value: 4, exclude: ['Healing', 'DamageTaken'] },
    { key: "Ranged Assassin", value: 5, exclude: ['Healing', 'DamageTaken'] },
  ]
  activeSubtab = 0;
  req$ = new ReplaySubject<ScoreResultsRequest>(1);
  data$: Observable<ScoreResultsResult>;
  result: ScoreResultsResult;
  subs: Subscription[];

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
    const subTab = spl.find(r => r[0] === 'Subtab');
    if (subTab) {
      this.activeSubtab = +subTab[1];
    } else {
      this.activeSubtab = 0;
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

  constructor(svc: MigrationService, private route: ActivatedRoute, private title: Title, private router: Router) {
    this.data$ = this.req$.pipe(
      switchMap(r => svc.getScoreResults(r)),
      tap(r => this.setupData(r))
    );
    title.setTitle(`Heroes of the Storm Average Hero & Role Scores | HOTS Logs`);
  }

  ngOnInit(): void {
    const subs = [];
    const allStreams = combineLatest([this.route.data, this.route.queryParamMap]);
    subs.push(allStreams.subscribe(([r, params]) => {
      this.filters = r.filters;
      this.querystring = window.location.search;
      this.gameModeEx ||= '8';
      this.tournament ||= this.filters.Tournament[this.filters.Tournament.length - 1].TournamentId.toString();
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
    this.activeTab = t.value;
    this.getData(false);
  }

  subTabClick(t: NameValueN) {
    this.activeSubtab = t.value;
    this.getData(false);
  }

  getData(emit = true) {
    if (!this.gameModeExFilter) {
      return;
    }
    const GameModeEx = this.gameModeEx;
    const Tournament = this.tournament;
    const GameMode = GameModeEx === '0' ? Tournament : GameModeEx;
    const Subtab = this.activeTab === 1 ? this.activeSubtab : 0;
    const req: ScoreResultsRequest = {
      Tab: this.activeTab,
      Subtab,
      GameMode,
      GameModeEx,
      Tournament,
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

  setupData(r: ScoreResultsResult): void {
    this.result = r;
  }

  private setQueryString(req: ScoreResultsRequest) {
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
    if (this.activeTab !== 0) {
      qs.push(['Tab', `${this.activeTab}`]);
      queryParams['Tab'] = this.activeTab;
    }
    if (this.activeSubtab !== 0) {
      qs.push(['Subtab', `${this.activeSubtab}`]);
      queryParams['Subtab'] = this.activeSubtab;
    }

    const q1 = qs.map(([a, b]) => `${a}=${encodeURIComponent(b)}`).join('&');
    const q = q1 ? `?${q1}` : '';
    const basePath = window.location.pathname;
    const url = `${basePath}${q}`;
    this.router.navigate([window.location.pathname], { queryParams, replaceUrl: true });
  }
}
