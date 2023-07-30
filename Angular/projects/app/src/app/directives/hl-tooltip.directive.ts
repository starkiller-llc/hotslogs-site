import { DOCUMENT } from '@angular/common';
import { Directive, ElementRef, HostBinding, HostListener, Inject, Input, OnChanges, OnDestroy, Renderer2, SimpleChanges } from '@angular/core';

@Directive({
  selector: 'tr[title], img[title], div[title], tr[data-tooltip], img[data-tooltip], div[data-tooltip]'
})
export class HlTooltipDirective implements OnDestroy, OnChanges {
  insertedSpan: HTMLSpanElement;
  @Input('title') title?: string;
  @Input('data-tooltip') dataTooltip?: string;
  @HostBinding('attr.data-tooltip') attrDataTooltip;

  constructor(
    private renderer: Renderer2,
    private el: ElementRef<HTMLElement>,
    @Inject(DOCUMENT) private document: Document) { }

  ngOnChanges(changes: SimpleChanges): void {
    this.attrDataTooltip = this.title || this.dataTooltip;
  }

  ngOnDestroy(): void {
    if (this.insertedSpan) {
      this.renderer.removeChild(this.insertedSpan.parentNode, this.insertedSpan);
    }
  }

  @HostListener('mouseenter')
  onMouseEnter() {
    var title = this.getTitle();
    if (title) {
      this.addTooltip(title);
    }
  }

  @HostListener('mouseleave')
  onMouseLeave() {
    const span = this.renderer.nextSibling(this.el.nativeElement);
    if (!span) {
      return;
    }

    this.renderer.removeChild(span.parentNode, span);
    this.insertedSpan = null;
  }

  addTooltip(text: string) {
    const target = this.el.nativeElement;
    const oldSpan = Array.from(target.childNodes).find(r => r.nodeName === 'span') as HTMLSpanElement;
    if (oldSpan) {
      this.renderer.removeChild(target, oldSpan);
    }
    const span = this.document.createElement("span");
    span.className = "titlePopup";
    span.innerHTML = text;
    this.renderer.insertBefore(target.parentNode, span, target.nextSibling);
    this.insertedSpan = span;
  }

  getTitle() {
    return this.title || this.dataTooltip;
  }
}
