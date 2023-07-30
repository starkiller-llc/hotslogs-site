import { AfterViewInit, Directive, ElementRef } from '@angular/core';

declare var $: any;

@Directive({
  selector: '[libSlick]'
})
export class SlickDirective implements AfterViewInit {

  constructor(private el: ElementRef) { }

  ngAfterViewInit(): void {
    console.log('slick');
    $(this.el.nativeElement).slick({
      dots: true,
      variableWidth: true,
      centerMode: true,
      infinite: false
    });
  }

}
