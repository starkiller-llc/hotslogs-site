import { Component, OnInit } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { ActivatedRoute, Router } from '@angular/router';
import { combineLatest, Observable, ReplaySubject, switchMap, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AppUser } from '../../models/app-user';
import { MigrationService } from '../../services/migration.service';
import { UserService } from '../../services/user.service';
import { AccountData, PlayerProfileSlim } from './model';

@Component({
  selector: 'app-manage',
  templateUrl: './manage.component.html',
  styleUrls: ['./manage.component.scss']
})
export class ManageComponent implements OnInit {
  user: AppUser;
  querystring: string;
  req$ = new ReplaySubject<any>(1);
  data$: Observable<AccountData>;
  result: AccountData;

  showAltNote = false;
  showVerify = false;
  showPaypal = false;
  profileImageUrl: string;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private usr: UserService,
    public title: Title,
    private svc: MigrationService) {
    this.data$ = this.req$.pipe(
      switchMap(r => svc.getAccountData(r)),
      tap(r => this.setupData(r))
    );
    title.setTitle(`Manage Account`);
  }

  ngOnInit(): void {
    const subs = [];
    const allStreams = combineLatest([this.usr.user$, this.route.data, this.route.queryParamMap]);
    subs.push(allStreams.subscribe(([user, r]) => {
      if (!user) {
        this.router.navigate(['/Login'], { queryParams: { returnUrl: window.location.pathname + window.location.search } });
        return;
      }
      this.user = user;
      this.querystring = window.location.search;
      this.getData();
    }));
  }

  getData(emit = true) {
    if (emit) {
      this.req$.next(null);
    }
  }

  setupData(r: any): void {
    this.result = r;
    this.profileImageUrl = '/profileimage/' + this.result.Main?.Id;
  }

  verifyBnet(btag: string, region: number) {
    this.svc.bnetAuth(btag, region).subscribe(r => {
      window.location.href = `${environment.apiBase}/mig/Auth/bnetauth?region=${region}`;
    });
  }

  changeOptOut(b: boolean) {
    this.svc.changeOptOut(b).subscribe();
  }

  changeGameMode(gm: number) {
    this.svc.changeGameMode(gm).subscribe();
  }

  makeMain(alt: PlayerProfileSlim) {
    this.svc.makeMain(alt.Id).subscribe(r => this.getData(false));
  }

  removeAlt(alt: PlayerProfileSlim) {
    if (!confirm('Are you sure you wish to remove this alt character?')) {
      return;
    }
    this.svc.removeAlt(alt.Id).subscribe(r => this.getData(false));
  }

  addAlt(region: number) {
    window.location.href = `${environment.apiBase}/mig/Auth/bnetauth?region=${region}`;
  }
}
