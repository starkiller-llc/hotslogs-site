import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { MigrationService } from '../../services/migration.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit, OnDestroy {
  loginForm: FormGroup;
  error: string;
  returnUrl: string;
  sub: Subscription;

  constructor(fb: FormBuilder, private svc: MigrationService, private router: Router, private route: ActivatedRoute) {
    this.loginForm = fb.group({
      Username: [''],
      Password: [''],
      RememberMe: [false],
    });
  }

  ngOnInit(): void {
    this.sub = this.route.queryParamMap.subscribe(r => this.returnUrl = r.get('returnUrl'));
  }

  ngOnDestroy(): void {
    this.sub.unsubscribe();
  }

  login() {
    this.svc.login(this.loginForm.value).subscribe({
      complete: () => {
        this.error = null;
        this.router.navigate([this.returnUrl?.replace(/^\/?ang/, '') || '/Player/Profile']);
      },
      error: (err) => this.error = err.detail,
    });
  }
}
