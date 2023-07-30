import { AfterViewInit, Directive, ElementRef, Input, OnChanges, OnDestroy, SimpleChanges } from '@angular/core';
import { Subscription } from 'rxjs';
import { normalize } from '../../../utils/normalize';
import { HlLocalService } from '../services/hl-local.service';

@Directive({
  selector: '[localize]'
})
export class HlTrDirective implements AfterViewInit, OnDestroy, OnChanges {
  @Input() localize: string;
  @Input() localizeParameters: string[] = [];

  savedValue: string;
  savedKey: string;
  sub: Subscription;
  observer: MutationObserver;
  modified = false;

  constructor(private el: ElementRef, private svc: HlLocalService) {
    this.sub = svc.change$.subscribe(() => this.doIt());
  }

  ngAfterViewInit(): void {
    this.saveKey();
    this.doIt();

    // this.installObserver();
  }

  ngOnChanges(changes: SimpleChanges): void {
    this.doIt();
  }

  private installObserver() {
    const config: MutationObserverInit = {
      characterData: true,
      subtree: true,
      childList: true,
    };
    const callback = (mutationsList, observer) => {
      console.log('callback');
      for (const mutation of mutationsList) {
        if (mutation.type === 'characterData') {
          this.saveKey();
          this.doIt();
        }
      }
    };
    this.observer = new MutationObserver(callback);
    this.observer.observe(this.el.nativeElement, config);
  }

  private saveKey() {
    const oldVal = this.el.nativeElement.innerText;
    const oldKey = normalize(oldVal).replace(/\W/g, '');
    this.savedValue = oldVal;
    this.savedKey = oldKey;
  }

  ngOnDestroy(): void {
    this.sub.unsubscribe();
    // this.observer.disconnect();
  }

  doIt() {
    const key = this.getKey(this.localize);
    let val = this.svc.get(key);

    if (val && this.localizeParameters) {
      for (let i = 0; i < 10; i++) {
        const rex = new RegExp(`\\{${i}\\}`, 'g');
        val = val.replace(rex, this.localizeParameters[i]);
      }
    }

    if (!val) {
      if (this.modified) {
        this.el.nativeElement.innerText = this.savedValue;
      }
    } else if (this.el?.nativeElement) {
      this.el.nativeElement.innerText = val;
      this.modified = true;
    }
  }

  private getKey(s: string) {
    if (!s) {
      return `Generic${this.savedKey}`;
    }
    if (s.endsWith('-')) {
      const pref = s.substring(0, s.length - 1);
      return `${pref}${this.savedKey}`;
    }
    const key = normalize(s).replace(/\W/g, '');
    return key;
  }
}
