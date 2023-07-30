import { Injectable, InjectionToken } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

export const themeClass = new InjectionToken('theme-class');

@Injectable({
  providedIn: 'root'
})
export class ThemeService {
  private _initialTheme = localStorage.getItem('theme') || 'dark';
  private _theme = this._initialTheme;
  private _pTheme = new BehaviorSubject<string>(this._initialTheme);

  public get theme() {
    return this._theme;
  }
  public set theme(value) {
    this._theme = value;
    localStorage.setItem('theme', value);
    this._pTheme.next(value);
  }

  readonly theme$: Observable<string> = this._pTheme.asObservable();

  constructor() { }
}
