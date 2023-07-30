import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { combineLatest, Subscription } from 'rxjs';
import { environment } from '../../../environments/environment';
import { MigrationService } from '../../services/migration.service';

@Component({
  selector: 'app-reset-passwd',
  templateUrl: './reset-passwd.component.html',
  styleUrls: ['./reset-passwd.component.scss']
})
export class ResetPasswdComponent implements OnInit, OnDestroy {
  captchaKey = environment.captchaKey;

  email = '';
  captchaResponse: string;
  subs: Subscription[];
  id: number;
  token: string;
  step = 1;

  constructor(private svc: MigrationService, private route: ActivatedRoute, private router: Router) { }

  ngOnInit(): void {
    const subs: Subscription[] = [];
    const allStreams = combineLatest([this.route.queryParamMap]);
    subs.push(allStreams.subscribe(([params]) => {
      if(params.has('ok') && params.get('ok') === 'true') {
        this.step = 4;
        return;
      }
      if (!params.has('id') || !params.has('token')) {
        return;
      }
      const id = +params.get('id');
      const token = params.get('token');
      this.id = id;
      this.token = token;
      this.step = 2;
    }));

    this.subs = subs;
  }

  ngOnDestroy(): void {
    this.subs.forEach(s => s.unsubscribe());
  }

  resolved(captchaResponse: string) {
    this.captchaResponse = captchaResponse;
  }

  submit() {
    const req = {
      Email: this.email,
      CaptchaResponse: this.captchaResponse,
    };
    this.svc.resetPassword(req).subscribe(r => this.step = 3);
  }
}
