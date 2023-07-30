import { BreakpointObserver, Breakpoints, BreakpointState } from '@angular/cdk/layout';
import { Directive, Input, OnDestroy, TemplateRef, ViewContainerRef } from '@angular/core';
import { Subscription } from 'rxjs';

type Size = 'small' | 'medium' | 'large';

const config = {
  small: [Breakpoints.Small, Breakpoints.XSmall],
  medium: [Breakpoints.Medium],
  large: [Breakpoints.Large, Breakpoints.XLarge]
};

@Directive({
  selector: "[ifView]"
})
export class IfViewDirective implements OnDestroy {
  private subscription = new Subscription();

  @Input("ifView") set size(value: Size) {
    this.subscription.unsubscribe();
    this.subscription = this.observer
      .observe(config[value])
      .subscribe(this.updateView);
  }

  constructor(
    private observer: BreakpointObserver,
    private vcRef: ViewContainerRef,
    private templateRef: TemplateRef<any>
  ) {}

  updateView = ({ matches }: BreakpointState) => {
    if (matches && !this.vcRef.length) {
      this.vcRef.createEmbeddedView(this.templateRef);
    } else if (!matches && this.vcRef.length) {
      this.vcRef.clear();
    }
  };

  ngOnDestroy() {
    this.subscription.unsubscribe();
  }
}
