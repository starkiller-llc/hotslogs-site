import { Directive, HostBinding } from '@angular/core';

@Directive({
  selector: 'th[mat-header-cell]'
})
export class HlStyleDirective {

  constructor() { 
  }

  @HostBinding('class.rgHeader') rgHeader = true;
}
