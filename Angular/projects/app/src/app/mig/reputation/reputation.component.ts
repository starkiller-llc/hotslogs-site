import { Component, OnInit } from '@angular/core';
import { PageEvent } from '@angular/material/paginator';
import { Sort } from '@angular/material/sort';
import { Title } from '@angular/platform-browser';
import { ActivatedRoute, Router } from '@angular/router';
import { ReplaySubject, Observable, Subscription, switchMap, tap, debounceTime, combineLatest } from 'rxjs';
import { AppUser } from '../../models/app-user';
import { FilterDefinitions } from '../../models/filters';
import { MigrationService } from '../../services/migration.service';
import { UserService } from '../../services/user.service';
import { ReputationRequest, ReputationResult } from './model';

interface NameValueB<T> {
  value: T;
  key: string;
  selected?: boolean;
}

type NameValue = NameValueB<string>;
type NameValueN = NameValueB<number>;

@Component({
  selector: 'app-reputation',
  templateUrl: './reputation.component.html',
  styleUrls: ['./reputation.component.scss']
})
export class ReputationComponent implements OnInit {
  filters: FilterDefinitions;

  region: number;

  req$ = new ReplaySubject<ReputationRequest>(1);
  data$: Observable<ReputationResult>;
  result: ReputationResult;
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
    }
  }

  constructor(svc: MigrationService, private route: ActivatedRoute, public title: Title, private usr: UserService, private router: Router) {
    this.data$ = this.req$.pipe(
      debounceTime(200),
      switchMap(r => svc.getReputation(r)),
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
  }

  getData(emit = true) {
    const req: ReputationRequest = {
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

  setupData(r: ReputationResult): void {
    this.result = r;
  }

  private setQueryString(req: ReputationRequest) {
    const qs = [];
    const queryParams = {};
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
