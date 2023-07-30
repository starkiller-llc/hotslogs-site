import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Subscription, combineLatest, map, filter, tap, switchMap, catchError } from 'rxjs';
import { AppUser } from '../../models/app-user';
import { MigrationService } from '../../services/migration.service';
import { UserService } from '../../services/user.service';

@Component({
  selector: 'app-sub-cancel',
  templateUrl: './sub-cancel.component.html',
  styleUrls: ['./sub-cancel.component.scss']
})
export class SubCancelComponent implements OnInit, OnDestroy {
  subs: Subscription[];
  ok = 0;
  user: AppUser;

  constructor(
    private svc: MigrationService,
    private usr: UserService,
    private route: ActivatedRoute,
    private router: Router,
  ) { }

  ngOnInit(): void {
    const subs: Subscription[] = [];
    const allStreams = combineLatest([this.route.queryParamMap, this.usr.user$]);
    subs.push(allStreams.pipe(
      map(([qmap, user]) => {
        this.user = user;
        const subid = qmap.get('subid');
        if (!subid) {
          return null;
        }
        const basePath = window.location.pathname;
        window.history.replaceState({}, '', basePath);
        return subid;
      }),
      filter(q => !!q),
      tap(r => this.ok = 1),
      switchMap(subid => this.svc.cancelSubscription(subid)),
      tap(r => {
        this.ok = 2;
        this.usr.refresh();
      }),
      catchError(err => {
        this.ok = 2; // even though an 'error' occurred, we're still probably ok because the subscription was not found.
        this.usr.refresh();
        return [];
      })
    ).subscribe());

    this.subs = subs;
  }

  ngOnDestroy(): void {
    this.subs.forEach(s => s.unsubscribe());
  }
}
