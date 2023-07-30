import { AfterViewInit, Component, Input, OnDestroy, OnInit, QueryList, ViewChildren } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { ActivatedRoute, Router } from '@angular/router';
import { combineLatest, Observable, Subscription, tap } from 'rxjs';
import { MigrationService } from '../../services/migration.service';
import { DefaultStatsComponent } from './default-stats/default-stats.component';
import { RootObject } from './model';

@Component({
  selector: 'app-default',
  templateUrl: './default.component.html',
  styleUrls: ['./default.component.scss']
})
export class DefaultComponent implements OnInit, AfterViewInit, OnDestroy {
  data$: Observable<any>;

  private _querystring: string;

  data: RootObject;// = sample;
  newsReady = false;

  @ViewChildren('tbl') tbl: QueryList<DefaultStatsComponent>;
  subs: Subscription[] = [];

  @Input()
  public get querystring(): string {
    return this._querystring;
  }
  public set querystring(value: string) {
    this._querystring = value;
    this.getQueryString(value || '');
  }

  constructor(svc: MigrationService, private title: Title, private route: ActivatedRoute, private router: Router) {
    this.data$ = svc.get().pipe(tap(r => {
      this.data = r;
    }));
    title.setTitle('HOTS Logs - Heroes of the Storm Stats, Builds, & More');
  }

  ngOnInit(): void {
    const subs = [];
    const allStreams = combineLatest([this.route.queryParamMap]);
    subs.push(allStreams.subscribe(([params]) => {
      this.querystring = window.location.search;
    }));
  }

  ngAfterViewInit(): void {
    this.subs.push(this.tbl.changes.subscribe(r => {
      if (this.tbl.length) {
        this.querystring = window.location.search;
      }
    }));
  }

  ngOnDestroy(): void {
    this.subs.forEach(r => r.unsubscribe());
  }

  filterChange(f: string) {
    this.setQueryString();
  }

  sorted() {
    this.setQueryString();
  }

  private getQueryString(value: string) {
    const spl = value.substring(1).split('&').map(r => r.split('=').map(z => decodeURIComponent(z.replace(/\+/g, ' '))));
    const q = spl.find(r => r[0] === 'q');
    if (q) {
      this.tbl.first.filter = q[1];
    } else if (this.tbl) {
      this.tbl.first.filter = '';
    }
    const s = spl.find(r => r[0] === 's');
    if (s) {
      this.tbl.first.sort = s[1];
    } else if (this.tbl) {
      this.tbl.first.sort = '';
    }
  }

  setQueryString() {
    const qs = [];
    const queryParams = {};
    if (this.tbl.first.filter) {
      qs.push(`q=${encodeURIComponent(this.tbl.first.filter)}`);
      queryParams['q'] = this.tbl.first.filter;
    }
    if (this.tbl.first.sort) {
      qs.push(`s=${encodeURIComponent(this.tbl.first.sort)}`);
      queryParams['s'] = this.tbl.first.sort;
    }
    const q1 = qs.join('&');
    const q = q1 ? `?${q1}` : '';
    const basePath = window.location.pathname;
    const url = `${basePath}${q}`;
    this.router.navigate([window.location.pathname], { queryParams, replaceUrl: true });
  }
}
