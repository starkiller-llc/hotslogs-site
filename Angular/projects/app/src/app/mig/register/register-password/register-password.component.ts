import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormGroup, FormBuilder, Validators, FormControl, AbstractControlOptions } from '@angular/forms';
import { Router } from '@angular/router';
import { MigrationService } from '../../../services/migration.service';
import { OldPwdValidators } from '../../../utils/old-pwd.validators';

@Component({
  selector: 'app-register-password',
  templateUrl: './register-password.component.html',
  styleUrls: ['./register-password.component.scss']
})
export class RegisterPasswordComponent {
  @Input() captcha: string;
  form1: FormGroup;
  @Output() failed = new EventEmitter<any>();

  constructor(fb: FormBuilder, private svc: MigrationService, private router: Router) {
    this.form1 = fb.group({
      'email': new FormControl<string>('', Validators.required),
      'username': new FormControl<string>('', Validators.required),
      'newPwd': new FormControl<string>('', Validators.required),
      'confirmPwd': new FormControl<string>('', Validators.required),
      'acceptTerms': new FormControl<string>('', Validators.requiredTrue)
    }, {
      validator: OldPwdValidators.matchPwds
    } as AbstractControlOptions);
  }

  get email() {
    return this.form1.get('email');
  }

  get username() {
    return this.form1.get('username');
  }

  get newPwd() {
    return this.form1.get('newPwd');
  }

  get confirmPwd() {
    return this.form1.get('confirmPwd');
  }

  get acceptTerms() {
    return this.form1.get('acceptTerms');
  }

  register() {
    const req = {
      Email: this.email.value,
      Username: this.username.value,
      NewPassword: this.newPwd.value,
      CaptchaResponse: this.captcha,
    };
    console.log(req);
    this.svc.register(req).subscribe({
      complete: () => {
        this.router.navigateByUrl('/');
      },
      error: err => {
        console.log('resetting');
        this.failed.emit(err);
      }
    });
  }
}
