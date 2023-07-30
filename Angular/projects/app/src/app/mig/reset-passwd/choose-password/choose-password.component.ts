import { Component, Input } from '@angular/core';
import { FormGroup, FormBuilder, Validators, FormControl, AbstractControlOptions } from '@angular/forms';
import { Router } from '@angular/router';
import { MigrationService } from '../../../services/migration.service';
import { OldPwdValidators } from '../../../utils/old-pwd.validators';

@Component({
  selector: 'app-choose-password',
  templateUrl: './choose-password.component.html',
  styleUrls: ['./choose-password.component.scss']
})
export class ChoosePasswordComponent {
  @Input() id: number;
  @Input() token: string;

  form1: FormGroup;

  constructor(fb: FormBuilder, private svc: MigrationService, private router: Router) {
    this.form1 = fb.group({
      'newPwd': new FormControl<string>('', Validators.required),
      'confirmPwd': new FormControl<string>('', Validators.required)
    }, {
      validator: OldPwdValidators.matchPwds
    } as AbstractControlOptions);
  }

  get newPwd() {
    return this.form1.get('newPwd');
  }

  get confirmPwd() {
    return this.form1.get('confirmPwd');
  }

  changePassword() {
    const req = {
      Id: this.id,
      Token: this.token,
      NewPassword: this.newPwd.value,
    };
    this.svc.resetPasswordConfirm(req).subscribe({
      complete: () => {
        this.router.navigateByUrl('/', { skipLocationChange: true }).then(() =>
          this.router.navigate(['/PasswordRecovery'], { queryParams: { ok: 'true' } }));
      }
    });
  }
}
