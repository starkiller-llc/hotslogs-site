import { DOCUMENT } from '@angular/common';
import { AfterViewInit, Directive, ElementRef, EmbeddedViewRef, Inject, Input, OnChanges, Renderer2, SimpleChanges, TemplateRef, ViewChild, ViewContainerRef } from '@angular/core';
import { MatchSummaryResult } from '../mig/match-summary/model';

@Directive({
  selector: '[appMatchSummary]'
})
export class MatchSummaryDirective<T> implements AfterViewInit, OnChanges {
  @Input() appMatchSummary: T;
  @Input() replayId: number;
  @Input() loading: boolean;

  @Input('detailsView') lala: TemplateRef<any>;
  view: EmbeddedViewRef<any>;

  constructor(
    private el: ElementRef<HTMLTableRowElement>,
    @Inject(DOCUMENT) private doc: Document,
    private fac: ViewContainerRef,
    private rnd: Renderer2) {
  }

  ngAfterViewInit(): void {
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (!this.appMatchSummary && !this.loading) {
      if (this.view) {
        this.view.destroy();
        this.view = null;
      }
      return;
    }

    if (!this.view) {
      this.setup();
    }

    this.view.context.loading = this.loading;
    this.view.context.result = this.appMatchSummary;
  }

  setup() {
    this.view = this.fac.createEmbeddedView(this.lala, { result: this.appMatchSummary, replayId: this.replayId, loading: this.loading });
  }
}
