import { Component, OnInit } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { ActivatedRoute, Router } from '@angular/router';
import { ReplaySubject, Observable, Subscription, switchMap, tap, combineLatest } from 'rxjs';
import { FilterDefinitions } from '../../models/filters';
import { MigrationService } from '../../services/migration.service';
import { TeamCompositionsRequest, TeamCompositionsResult } from './model';

interface NameValueB<T> {
  value: T;
  key: string;
  selected?: boolean;
}

type NameValue = NameValueB<string>;
type NameValueN = NameValueB<number>;

@Component({
  selector: 'app-team-compositions',
  templateUrl: './team-compositions.component.html',
  styleUrls: ['./team-compositions.component.scss']
})
export class TeamCompositionsComponent implements OnInit {
  filters: FilterDefinitions;

  map: string;
  mapFilter: NameValue[];
  hero: string;
  heroFilter: NameValue[];
  grouping: number;

  req$ = new ReplaySubject<TeamCompositionsRequest>(1);
  data$: Observable<TeamCompositionsResult>;
  result: TeamCompositionsResult;
  subs: Subscription[];

  public set querystring(value: string) {
    this.getQueryString(value || '');
  }

  private getQueryString(value: string) {
    const spl = value.substring(1).split('&').map(r => r.split('=').map(z => decodeURIComponent(z.replace(/\+/g, ' '))));
    const map = spl.find(r => r[0] === 'Map');
    if (map) {
      this.map = map[1];
    } else {
      this.map = null;
    }
    const hero = spl.find(r => r[0] === 'Hero');
    if (hero) {
      this.hero = hero[1];
    } else {
      this.hero = null;
    }
    const grouping = spl.find(r => r[0] === 'Grouping');
    if (grouping) {
      this.grouping = +grouping[1];
    } else {
      this.grouping = null;
    }
  }

  constructor(svc: MigrationService, private route: ActivatedRoute, private title: Title, private router: Router) {
    this.data$ = this.req$.pipe(
      switchMap(r => svc.getTeamCompositions(r)),
      tap(r => this.setupData(r))
    );
    title.setTitle(`Heroes of the Storm Popular Team Compositions | HOTS Logs`);
  }

  ngOnInit(): void {
    const subs = [];
    const allStreams = combineLatest([this.route.data, this.route.queryParamMap]);
    subs.push(allStreams.subscribe(([r, params]) => {
      this.filters = r.filters;
      this.querystring = window.location.search;
      this.map ||= '-1';
      this.hero ||= '-1';
      this.grouping ||= 1;
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
    this.mapFilter = r.MapList.map(z => ({
      key: z.DisplayName,
      value: z.IdentifierId === -1 ? '-1' : z.PrimaryName,
      selected: this.map === (z.IdentifierId === -1 ? '-1' : z.PrimaryName),
    }));

    this.heroFilter = r.Hero.map(z => ({
      key: z.DisplayName,
      value: z.PrimaryName,
      selected: z.PrimaryName === this.hero,
    }));

    this.heroFilter.splice(0, 0, {
      key: 'All Heroes',
      value: '-1',
      selected: this.hero === '-1',
    });
  }

  getData(emit = true) {
    const req: TeamCompositionsRequest = {
      Grouping: this.grouping,
      Map: [`${this.map}`],
      Hero: this.hero,
    };
    this.setQueryString(req);
    if (emit) {
      this.req$.next(req);
    }
  }

  setupData(r: TeamCompositionsResult): void {
    this.result = r;
  }

  private setQueryString(req: TeamCompositionsRequest) {
    const qs = [];
    const queryParams = {};
    if (req.Grouping !== 1) {
      qs.push(['Grouping', `${req.Grouping}`]);
      queryParams['Grouping'] = `${req.Grouping}`;
    }
    if (req.Map[0] !== '-1') {
      qs.push(['Map', `${req.Map[0]}`]);
      queryParams['Map'] = `${req.Map[0]}`;
    }
    if (req.Hero !== '-1') {
      qs.push(['Hero', `${req.Hero}`]);
      queryParams['Hero'] = `${req.Hero}`;
    }

    const q1 = qs.map(([a, b]) => `${a}=${encodeURIComponent(b)}`).join('&');
    const q = q1 ? `?${q1}` : '';
    const basePath = window.location.pathname;
    const url = `${basePath}${q}`;
    this.router.navigate([window.location.pathname], { queryParams, replaceUrl: true });
  }
}
