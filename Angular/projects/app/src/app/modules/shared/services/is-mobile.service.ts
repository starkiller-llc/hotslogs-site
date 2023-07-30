import { BreakpointObserver, Breakpoints, BreakpointState } from '@angular/cdk/layout';
import { Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class IsMobileService {
  mobile$: Observable<boolean>;

  constructor(private observer: BreakpointObserver) {
    this.mobile$ = observer
      .observe([Breakpoints.Small, Breakpoints.XSmall, Breakpoints.Medium])
      .pipe(map(({ matches }: BreakpointState) => !!matches));
  }
}
