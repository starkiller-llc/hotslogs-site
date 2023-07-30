import { Component, Input } from '@angular/core';
import { FormGroup, FormBuilder, Validators, FormControl, AbstractControlOptions } from '@angular/forms';
import { AppUser } from '../../../models/app-user';
import { MigrationService } from '../../../services/migration.service';
import { OldPwdValidators } from '../../../utils/old-pwd.validators';

@Component({
  selector: 'app-change-password',
  templateUrl: './change-password.component.html',
  styleUrls: ['./change-password.component.scss']
})
export class ChangePasswordComponent {
  @Input() user: AppUser;

  form1: FormGroup;

  constructor(fb: FormBuilder, private svc: MigrationService) {
    this.form1 = fb.group({
      'oldPwd': new FormControl<string>('', Validators.required),
      'newPwd': new FormControl<string>('', Validators.required),
      'confirmPwd': new FormControl<string>('', Validators.required)
    }, {
      validator: OldPwdValidators.matchPwds
    } as AbstractControlOptions);
  }

  get oldPwd() {
    return this.form1.get('oldPwd');
  }

  get newPwd() {
    return this.form1.get('newPwd');
  }

  get confirmPwd() {
    return this.form1.get('confirmPwd');
  }

  changePassword() {
    const req = {
      CurrentPassword: this.oldPwd.value,
      NewPassword: this.newPwd.value,
    };
    this.svc.changePassword(req).subscribe({
      complete: () => location.reload()
    });
  }
}
