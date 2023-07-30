import { Component, OnDestroy, OnInit } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { ActivatedRoute, Router } from '@angular/router';
import { combineLatest, Observable, ReplaySubject, Subscription, switchMap, tap } from 'rxjs';
import { FilterDefinitions } from '../../models/filters';
import { MigrationService } from '../../services/migration.service';
import { HeroAndMapRequest, HeroAndMapResult } from './model';

interface NameValueB<T> {
  value: T;
  key: string;
  selected?: boolean;
}

type NameValue = NameValueB<string>;
type NameValueN = NameValueB<number>;


@Component({
  selector: 'app-hero-and-map',
  templateUrl: './hero-and-map.component.html',
  styleUrls: ['./hero-and-map.component.scss']
})
export class HeroAndMapComponent implements OnInit, OnDestroy {
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
  gameLength: number[];
  gameLengthFilter: NameValueN[];
  level: string[];
  levelFilter: NameValue[];
  talent: string;
  subs: Subscription[];

  public set querystring(value: string) {
    this.getQueryString(value || '');
  }

  req$ = new ReplaySubject<HeroAndMapRequest>(1);
  data$: Observable<HeroAndMapResult>;
  result: HeroAndMapResult;

  constructor(svc: MigrationService, private route: ActivatedRoute, title: Title, private router: Router) {
    this.data$ = this.req$.pipe(
      switchMap(r => svc.getHeroAndMap(r)),
      tap(r => this.setupData(r))
    );
    title.setTitle(`Heroes of the Storm Hero & Map Statistics | HOTS Logs`);
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
      this.gameLength ||= [];
      this.level ||= [];
      this.talent = 'AllTalents';
      this.setFilters(this.filters);
      this.getData();
    }));
    this.subs = subs;
  }

  ngOnDestroy(): void {
    this.subs.forEach(r => r.unsubscribe());
  }

  private getQueryString(value: string) {
    const spl = value.substring(1).split('&').map(r => r.split('=').map(z => decodeURIComponent(z.replace(/\+/g, ' '))));
    const talent = spl.find(r => r[0] === 'Talent');
    if (talent) {
      this.talent = talent[1];
    } else {
      this.talent = null;
    }
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
    const gameLength = spl.find(r => r[0] === 'GameLength');
    if (gameLength) {
      this.gameLength = gameLength[1].split(',').map(r => +r);
    } else {
      this.gameLength = null;
    }
    const level = spl.find(r => r[0] === 'Level');
    if (level) {
      this.level = level[1].split(',');
    } else {
      this.level = null;
    }
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

    this.heroFilter = r.Hero.map(z => ({
      key: z.DisplayName,
      value: z.PrimaryName,
      selected: z.PrimaryName === this.hero,
    }));

    this.leagueFilter = r.LeagueCombo.map(z => ({
      key: z.LeagueDisplayText,
      value: z.LeagueID,
      selected: this.league.includes(z.LeagueID),
    }));

    this.mapFilter = r.MapCombo.map(z => ({
      key: z.DisplayName,
      value: z.PrimaryName,
      selected: this.map.includes(z.PrimaryName),
    }));

    this.timeFilter = r.Time.map(z => ({
      key: z.Key,
      value: z.Value,
      selected: this.time.includes(z.Value),
    }));

    this.patchFilter = r.Patch.map(z => ({
      key: z.Key,
      value: z.Value,
      selected: this.patch.includes(z.Value),
    }));

    this.gameLengthFilter = r.GameLength.map(z => ({
      key: z.ReplayGameLengthDisplayText,
      value: z.ReplayGameLengthValue,
      selected: this.gameLength.includes(z.ReplayGameLengthValue),
    }));

    this.levelFilter = r.Level.map(z => ({
      key: z.CharacterLevelDisplayText,
      value: z.CharacterLevelValue,
      selected: this.level.includes(z.CharacterLevelValue),
    }));
  }

  getData(emit = true) {
    if (!this.gameModeExFilter) {
      return;
    }
    const GameModeEx = this.gameModeEx;
    const Tournament = this.tournament;
    const GameMode = GameModeEx === '0' ? Tournament : GameModeEx;
    const GameLength = this.gameLength;
    const Level = this.level;
    const Talent = GameLength.length || Level.length ? 'AllTalents' : this.talent;
    const req: HeroAndMapRequest = {
      GameMode,
      GameModeEx,
      Tournament,
      Hero: this.hero,
      League: this.league,
      Map: this.map,
      Time: this.time,
      Patch: this.patch,
      GameLength,
      Level,
      Talent,
    };
    this.setQueryString(req);
    if (emit) {
      this.req$.next(req);
    }
  }

  setupData(r: HeroAndMapResult): void {
    this.result = r;
  }

  sel() {
    this.getData(false);
  }

  private setQueryString(req: HeroAndMapRequest) {
    const qs = [];
    const queryParams = {};
    if (req.Talent !== 'AllTalents') {
      qs.push(['Talent', `${req.Talent}`]);
      queryParams['Talent'] = req.Talent;
    }
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
    if (req.GameLength.length) {
      qs.push(['GameLength', `${req.GameLength.join(',')}`]);
      queryParams['GameLength'] = req.GameLength.join(',');
    }
    if (req.Level.length) {
      qs.push(['Level', `${req.Level.join(',')}`]);
      queryParams['Level'] = req.Level.join(',');
    }

    const q1 = qs.map(([a, b]) => `${a}=${encodeURIComponent(b)}`).join('&');
    const q = q1 ? `?${q1}` : '';
    const basePath = window.location.pathname;
    const url = `${basePath}${q}`;
    this.router.navigate([window.location.pathname], { queryParams, replaceUrl: true });
  }
}
