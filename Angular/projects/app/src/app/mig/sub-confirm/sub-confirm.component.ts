import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { catchError, combineLatest, filter, map, Subscription, switchMap, tap } from 'rxjs';
import { MigrationService } from '../../services/migration.service';
import { UserService } from '../../services/user.service';

@Component({
  selector: 'app-sub-confirm',
  templateUrl: './sub-confirm.component.html',
  styleUrls: ['./sub-confirm.component.scss']
})
export class SubConfirmComponent implements OnInit, OnDestroy {
  subs: Subscription[];
  ok = 0;

  constructor(
    private svc: MigrationService,
    private usr: UserService,
    private route: ActivatedRoute,
    private router: Router,
  ) { }

  ngOnInit(): void {
    const subs: Subscription[] = [];
    const allStreams = combineLatest([this.route.queryParamMap]);
    subs.push(allStreams.pipe(
      map(([qmap]) => qmap.get('subid')),
      filter(q => !!q),
      tap(r => this.ok = 1),
      switchMap(subid => this.svc.confirmSubscription(subid)),
      tap(r => {
        this.ok = 2;
        this.usr.refresh();
      }),
      catchError(err => {
        this.ok = 3;
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
