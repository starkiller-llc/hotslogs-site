import { Pipe, PipeTransform } from '@angular/core';
import { DomSanitizer } from '@angular/platform-browser';
import { normalize } from '../utils/normalize';

@Pipe({
  name: 'hlSanitize'
})
export class HlSanitizePipe implements PipeTransform {

  constructor(private s: DomSanitizer) { }

  transform(value: unknown, ...args: unknown[]): unknown {
    if (typeof value === 'string') {
      const rc = normalize(value).replace(/\W/g, '');
      if (args?.[0] === 'lowercase') {
        return rc.toLowerCase();
      } else {
        return rc;
      }
    }
    return value;
  }

}
