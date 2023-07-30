import { Injectable } from '@angular/core';
import { ClickOutsideDirective } from './click-outside.directive';

@Injectable({
  providedIn: 'root'
})
export class ClickOutsideService {
  registry: ClickOutsideDirective[] = [];

  constructor() { }

  register(d: ClickOutsideDirective) {
    const existing = this.registry.indexOf(d);
    if (existing === -1) {
      this.registry.push(d);
    }
  }

  unregister(d: ClickOutsideDirective) {
    const existing = this.registry.indexOf(d);
    if (existing !== -1) {
      this.registry.splice(existing, 1);
    }
  }

  getValue(e: HTMLElement) {
    while (e) {
      const found = this.registry.find(x => x.target === e);
      if (found) {
        return found.value;
      }
      e = e.parentElement;
    }
  }
}
