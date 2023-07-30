import { AfterViewInit, Directive, ElementRef, Input, OnChanges, SimpleChanges } from '@angular/core';
import { Router } from '@angular/router';
import { environment } from '../../environments/environment';

declare const paypal: any;

@Directive({
  selector: '[paypal]'
})
export class PaypalDirective implements AfterViewInit, OnChanges {
  @Input() paypal: string;
  init = false;
  created = false;

  constructor(private el: ElementRef, private router: Router) { }

  ngOnChanges(changes: SimpleChanges): void {
    this.createButtons();
  }

  ngAfterViewInit(): void {
    this.init = true;
    this.createButtons();
  }

  private async createButtons() {
    if (this.created || !this.init || !this.paypal) {
      return;
    }

    this.created = true;

    const doCreate = () => {
      if (typeof (paypal) === 'undefined') {
        return false;
      }

      paypal.Buttons({
        style: {
          shape: 'pill',
          color: 'blue',
          layout: 'horizontal',
          label: 'subscribe',
          tagline: 'false',
        },
        createSubscription: (data, actions) => {
          return actions.subscription.create({
            'plan_id': this.paypal
          });
        },
        onApprove: (data, actions) => {
          this.router.navigate(['/Account/SubConfirm'], { queryParams: { subid: data.subscriptionID } });
          // window.location.href = `${environment.pageBase}/Account/SubConfirm?subid=${data.subscriptionID}`;
        },
        onClick: (e) => {
          return true;
        },
      }).render(`#${this.el.nativeElement.id}`);

      return true;
    };

    while (!doCreate()) {
      await new Promise(resolve => setTimeout(resolve, 100));
    }
  }
}
