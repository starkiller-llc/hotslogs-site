import { Component, OnInit } from '@angular/core';
import { environment } from '../../../environments/environment';
import { MigrationService } from '../../services/migration.service';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss']
})
export class RegisterComponent implements OnInit {
  captchaKey = environment.captchaKey;

  email = '';
  captchaResponse: string;

  constructor(private svc: MigrationService) { }

  ngOnInit(): void {
  }

  resolved(captchaResponse: string) {
    this.captchaResponse = captchaResponse;
  }

  submit() {
    const req = {
      Email: this.email,
      CaptchaResponse: this.captchaResponse,
    };
    this.svc.resetPassword(req).subscribe();
  }
}
