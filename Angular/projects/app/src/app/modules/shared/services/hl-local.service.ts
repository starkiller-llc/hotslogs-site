import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class HlLocalService {
  private _translation: Record<string, string>;
  public get translation(): Record<string, string> {
    return this._translation;
  }
  public set translation(value: Record<string, string>) {
    const lc: Record<string, string> = {};
    for (const [k, v] of Object.entries(value)) {
      lc[k.toLowerCase()] = v;
    }
    this._translation = {
      ...lc,
      ...value,
    };
    this.pChange.next();
  }
  private pChange = new Subject<void>();
  change$ = this.pChange.asObservable();

  constructor() { }

  get(key: string): string {
    return this.translation?.[key] || this.translation?.[key.toLowerCase()];
  }
}
