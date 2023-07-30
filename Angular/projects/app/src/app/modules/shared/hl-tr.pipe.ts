import { Pipe, PipeTransform } from '@angular/core';
import { normalize } from '../../utils/normalize';
import { HlLocalService } from './services/hl-local.service';

@Pipe({
  name: 'localize'
})
export class HlTrPipe implements PipeTransform {
  constructor(private svc: HlLocalService) { }

  transform(value: string, ...args: unknown[]): string {
    const arg = args[0] as string;
    const key = this.getKey(arg, value as string);
    const val = this.svc.get(key);
    return val || value;
  }

  private getKey(s: string, v: string) {
    const intrnl = (s: string, v: string) => {
      if (!s) {
        return `Generic${v}`;
      }
      if (s.endsWith('-')) {
        const pref = s.substring(0, s.length - 1);
        return `${pref}${v}`;
      }
      return key;
    };
    const key = normalize(intrnl(s, v)).replace(/\W/g, '');;
    return key;
  }
}
