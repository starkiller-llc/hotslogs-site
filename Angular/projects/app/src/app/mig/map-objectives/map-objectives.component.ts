import { Component, OnDestroy, OnInit } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { ActivatedRoute, Router } from '@angular/router';
import { combineLatest, Observable, ReplaySubject, Subscription, switchMap, tap } from 'rxjs';
import { FilterDefinitions } from '../../models/filters';
import { MigrationService } from '../../services/migration.service';
import { MapObjectivesRequest, MapObjectivesResult } from './model';

interface NameValueB<T> {
  value: T;
  key: string;
  selected?: boolean;
}

type NameValue = NameValueB<string>;
type NameValueN = NameValueB<number>;

@Component({
  selector: 'app-map-objectives',
  templateUrl: './map-objectives.component.html',
  styleUrls: ['./map-objectives.component.scss']
})
export class MapObjectivesComponent implements OnInit, OnDestroy {
  filters: FilterDefinitions;

  gameModeEx: string;
  gameModeExFilter: NameValue[];
  tournament: string;
  tournamentFilter: NameValue[];
  map: number;
  mapFilter: NameValueN[];
  subs: Subscription[];

  public set querystring(value: string) {
    this.getQueryString(value || '');
  }

  req$ = new ReplaySubject<MapObjectivesRequest>(1);
  data$: Observable<MapObjectivesResult>;
  result: MapObjectivesResult;

  constructor(svc: MigrationService, private route: ActivatedRoute, title: Title, private router: Router) {
    this.data$ = this.req$.pipe(
      switchMap(r => svc.getMapObjectives(r)),
      tap(r => this.setupData(r))
    );
    title.setTitle(`Heroes of the Storm Map Objectives & Statistics | HOTS Logs`);
  }

  ngOnInit(): void {
    const subs = [];
    const allStreams = combineLatest([this.route.data, this.route.queryParamMap]);
    subs.push(allStreams.subscribe(([r, params]) => {
      this.filters = r.filters;
      this.querystring = window.location.search;
      this.gameModeEx ||= '8';
      this.tournament ||= this.filters.Tournament[this.filters.Tournament.length - 1].TournamentId.toString();
      this.map ||= -1;
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
    const map = spl.find(r => r[0] === 'Map');
    if (map) {
      this.map = +map[1];
    } else {
      this.map = null;
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

    this.mapFilter = r.MapList.map(z => ({
      key: z.DisplayName,
      value: z.IdentifierId,
      selected: this.map === z.IdentifierId,
    }));
  }

  getData(emit = true) {
    if (!this.gameModeExFilter) {
      return;
    }
    const GameModeEx = this.gameModeEx;
    const Tournament = this.tournament;
    const GameMode = GameModeEx === '0' ? Tournament : GameModeEx;
    const req: MapObjectivesRequest = {
      GameMode,
      GameModeEx,
      Tournament,
      Map: [`${this.map}`],
    };
    this.setQueryString(req);
    if (emit) {
      this.req$.next(req);
    }
  }

  setupData(r: MapObjectivesResult): void {
    this.result = r;
  }

  sel() {
    this.getData(false);
  }

  private setQueryString(req: MapObjectivesRequest) {
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
    if (req.Map[0] !== '-1') {
      qs.push(['Map', `${req.Map[0]}`]);
      queryParams['Map'] = req.Map[0];
    }

    const q1 = qs.map(([a, b]) => `${a}=${encodeURIComponent(b)}`).join('&');
    const q = q1 ? `?${q1}` : '';
    const basePath = window.location.pathname;
    const url = `${basePath}${q}`;
    this.router.navigate([window.location.pathname], { queryParams, replaceUrl: true });
  }
}
