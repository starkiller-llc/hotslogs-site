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
import { VoteResponse } from '../match-summary/model';
import { MatchHistoryRequest, MatchHistoryResult } from './model';

@Component({
  selector: 'app-match-history',
  templateUrl: './match-history.component.html',
  styleUrls: ['./match-history.component.scss']
})
export class MatchHistoryComponent implements OnInit {
  filters: FilterDefinitions;

  gameMode: string;
  playerId?: number;
  eventId?: string;
  otherPlayerIds?: number[];

  req$ = new ReplaySubject<MatchHistoryRequest>(1);
  data$: Observable<MatchHistoryResult>;
  result: MatchHistoryResult;
  subs: Subscription[] = [];
  user: AppUser;
  page: PageEvent;
  tableFilter: string;
  sort: Sort;

  public set querystring(value: string) {
    this.getQueryString(value || '');
  }

  constructor(
    private svc: MigrationService,
    private route: ActivatedRoute,
    public title: Title,
    private usr: UserService,
    private router: Router) {

    this.data$ = this.req$.pipe(
      debounceTime(200),
      switchMap(r => svc.getMatchHistory(r)),
      tap(r => this.setupData(r))
    );
    title.setTitle(`Match Summary | HOTS Logs`);
  }

  ngOnInit(): void {
    const subs = [];
    const allStreams = combineLatest([this.usr.user$, this.route.data, this.route.queryParamMap]);
    subs.push(allStreams.subscribe(([user, r]) => {
      this.user = user;
      this.filters = r.filters;
      this.querystring = window.location.search;
      this.gameMode ||= this.user?.defaultGameMode?.toString() || '8';
      if (!this.playerId && this.playerId !== 0) {
        this.playerId = this.user?.mainPlayerId;
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

  private getQueryString(value: string) {
    const spl = value.substring(1).split('&').map(r => r.split('=').map(z => decodeURIComponent(z.replace(/\+/g, ' '))));
    const playerId = spl.find(r => r[0] === 'PlayerID');
    if (playerId) {
      this.playerId = +playerId[1];
    } else if (this.user) {
      this.playerId = this.user?.mainPlayerId;
    } else {
      this.playerId = null;
    }

    const otherPlayerIds = spl.filter(r => r[0] === 'OtherPlayerIDs').map(r => r[1].split(',')).flatMap(r => +r);
    if (otherPlayerIds?.length) {
      this.otherPlayerIds = otherPlayerIds;
    } else {
      this.otherPlayerIds = null;
    }

    const eventId = spl.find(r => r[0] === 'EventID');
    if (eventId) {
      this.eventId = eventId[1];
    }
  }

  sel() {
    this.getData();
  }

  setFilters(r: FilterDefinitions): void {
  }

  getData() {
    const GameMode = this.eventId ? this.eventId : (this.gameMode || '8');
    const EventId = this.eventId ? +this.eventId : null;
    const req: MatchHistoryRequest = {
      PlayerId: this.playerId,
      EventId,
      GameMode,
      Filter: this.tableFilter,
      Page: this.page,
      Sort: this.sort,
      OtherPlayerIds: this.otherPlayerIds && [...this.otherPlayerIds],
    };
    this.req$.next(req);
  }

  setupData(r: MatchHistoryResult): void {
    this.result = r;
    if (r.Title) {
      this.title.setTitle(`Match Summary: ${r.Title}`);
    } else {
      this.title.setTitle(`Match Summary | HOTS Logs`);
    }
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

  voteDown([pid, rid, flag]: [number, number, boolean]) {
    this.svc.vote(false, pid, rid, flag).subscribe(r => this.voteResponse(r));
  }

  voteUp([pid, rid, flag]: [number, number, boolean]) {
    this.svc.vote(true, pid, rid, flag).subscribe(r => this.voteResponse(r));
  }

  voteResponse(r: VoteResponse): void {
    if (!r.Success) {
      alert(r.ErrorMessage);
      return;
    }

    var rply = this.result.Stats.find(z => z.ReplayID === r.ReplayId);
    var plr = rply.Summary.MatchDetails.find(z => z.PlayerID === r.TargetPlayer);
    plr.Reputation = r.TargetRep;
    if (r.Up) {
      plr.VoteUp = true;
    }
    if (r.Up === false) {
      plr.VoteUp = false;
    }
    if (r.Down) {
      plr.VoteDown = true;
    }
    if (r.Down === false) {
      plr.VoteDown = false;
    }
  }
}
