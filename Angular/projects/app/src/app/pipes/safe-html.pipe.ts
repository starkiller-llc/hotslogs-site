import { Pipe, PipeTransform } from '@angular/core';
import { DomSanitizer } from '@angular/platform-browser';

@Pipe({
  name: 'safeHtml'
})
export class SafeHtmlPipe implements PipeTransform {

  constructor(private s: DomSanitizer) { }

  transform(value: unknown, ...args: unknown[]): unknown {
    if (typeof value === 'string') {
      return this.s.bypassSecurityTrustHtml(value);
    }
    return value;
  }

}
