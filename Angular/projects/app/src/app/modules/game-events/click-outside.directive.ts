import { Directive, ElementRef, Input, OnDestroy, OnInit } from '@angular/core';
import { ClickOutsideService } from './click-outside.service';

@Directive({
  selector: '[appClickOutside]'
})
export class ClickOutsideDirective implements OnInit {
  @Input('appClickOutside') value: number | string;

  get target() {
    return this.el.nativeElement;
  }

  constructor(private svc: ClickOutsideService, private el: ElementRef) { }

  ngOnInit(): void {
    this.svc.register(this);
  }
}
