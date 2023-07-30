import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, ReplaySubject } from 'rxjs';
import { AppUser } from '../models/app-user';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private _pUser = new ReplaySubject<AppUser>(1);
  user$: Observable<AppUser> = this._pUser.asObservable();

  private url(api: string): string {
    return `/api/User/${api}`;
  }

  constructor(private http: HttpClient) {
    this.refresh();
  }

  public refresh() {
    this.http.get<AppUser>(this.url('GetUser')).subscribe(r => this._pUser.next(r));
  }
}
