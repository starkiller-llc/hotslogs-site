import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, Resolve, RouterStateSnapshot } from '@angular/router';
import { Observable } from 'rxjs';
import { FilterDefinitions } from '../models/filters';
import { MigrationService } from './migration.service';

@Injectable({
  providedIn: 'root'
})
export class FilterResolver implements Resolve<FilterDefinitions> {
  constructor(private svc: MigrationService) {
  }
  
  resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): FilterDefinitions | Observable<FilterDefinitions> | Promise<FilterDefinitions> {
    return this.svc.getFilters();
  }

}
