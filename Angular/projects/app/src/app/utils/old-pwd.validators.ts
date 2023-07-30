import { AbstractControl, ValidationErrors } from '@angular/forms';

export class OldPwdValidators {
  static matchPwds(control: AbstractControl): ValidationErrors | null {
    let newPwd2 = control.get('newPwd');
    let confirmPwd2 = control.get('confirmPwd');
    if (newPwd2.value !== confirmPwd2.value) {
      return { pwdsDontMatch: true };
    }
    return null;
  }
}
