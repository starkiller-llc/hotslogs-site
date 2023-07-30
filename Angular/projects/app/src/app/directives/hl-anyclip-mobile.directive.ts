import { Directive, HostBinding } from '@angular/core';

@Directive({
  selector: '.anyclip-mobile'
})
export class HlAnyclipMobileDirective {
  @HostBinding('attr.id')
  get anyclip() {
    return window.innerWidth < 1070 ? 'video_au' : undefined;
  }

  constructor() { }

}
