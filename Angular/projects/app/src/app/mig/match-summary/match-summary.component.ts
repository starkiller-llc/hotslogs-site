import { Component, OnInit } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { ActivatedRoute } from '@angular/router';
import { ReplaySubject, Observable, Subscription, switchMap, tap } from 'rxjs';
import { AppUser } from '../../models/app-user';
import { MigrationService } from '../../services/migration.service';
import { UserService } from '../../services/user.service';
import { MatchSummaryRequest, MatchSummaryResult, VoteResponse } from './model';

@Component({
  selector: 'app-match-summary',
  templateUrl: './match-summary.component.html',
  styleUrls: ['./match-summary.component.scss']
})
export class MatchSummaryComponent implements OnInit {
  replayId: number;

  req$ = new ReplaySubject<MatchSummaryRequest>(1);
  data$: Observable<MatchSummaryResult>;
  result: MatchSummaryResult;
  subs: Subscription[] = [];
  user: AppUser;

  public set querystring(value: string) {
    this.getQueryString(value || '');
  }

  constructor(private svc: MigrationService, private route: ActivatedRoute, public title: Title, private usr: UserService) {
    this.data$ = this.req$.pipe(
      switchMap(r => svc.getMatchSummary(r)),
      tap(r => this.setupData(r))
    );
    title.setTitle(`Match Summary | HOTS Logs`);
  }

  ngOnInit(): void {
    this.querystring = window.location.search;
    this.getData();
  }

  private getQueryString(value: string) {
    const spl = value.substring(1).split('&').map(r => r.split('=').map(z => decodeURIComponent(z.replace(/\+/g, ' '))));
    const replayId = spl.find(r => r[0] === 'ReplayID');
    if (replayId) {
      this.replayId = +replayId[1];
    }
  }

  getData(emit = true) {
    const req: MatchSummaryRequest = {
      ReplayId: this.replayId,
    };
    if (emit) {
      this.req$.next(req);
    }
  }

  setupData(r: MatchSummaryResult): void {
    this.result = r;
  }

  voteDown([pid, flag]: [number, boolean]) {
    this.svc.vote(false, pid, this.replayId, flag).subscribe(r => this.voteResponse(r));
  }

  voteUp([pid, flag]: [number, boolean]) {
    this.svc.vote(true, pid, this.replayId, flag).subscribe(r => this.voteResponse(r));
  }

  voteResponse(r: VoteResponse): void {
    if (!r.Success) {
      alert(r.ErrorMessage);
      return;
    }

    var plr = this.result.MatchDetails.find(z => z.PlayerID === r.TargetPlayer);
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
