import { Component, OnInit } from '@angular/core';
import { PageEvent } from '@angular/material/paginator';
import { ActivatedRoute } from '@angular/router';
import { finalize, map, Observable, ReplaySubject, Subject, switchMap, tap } from 'rxjs';
import { OverlayService } from '../../modules/shared/services/overlay.service';
import { MigrationService } from '../../services/migration.service';
import { PlayerSearchRequest, PlayerSearchResult } from './model';

@Component({
  selector: 'app-search',
  templateUrl: './search.component.html',
  styleUrls: ['./search.component.scss']
})
export class SearchComponent implements OnInit {
  page: PageEvent;
  req$ = new ReplaySubject<PlayerSearchRequest>(1);
  data$: Observable<PlayerSearchResult>;
  result: PlayerSearchResult;
  name: string;

  constructor(
    private route: ActivatedRoute, 
    ovl: OverlayService,
    svc: MigrationService) {
    this.data$ = this.req$.pipe(
      tap(r => ovl.setOverlay('search', true)),
      switchMap(r => svc.getPlayerSearch(r)),
      tap(r => this.setupData(r)),
      tap(r => ovl.setOverlay('search', false)),
      finalize(() => ovl.setOverlay('search', false)),
    )
  }

  setupData(r: PlayerSearchResult): void {
    this.result = r;
  }

  ngOnInit(): void {
    this.route.queryParamMap.subscribe(r => {
      const q = r.get('Name');
      this.name = q;
      if (!this.page) {
        this.page = {
          length: 0,
          pageIndex: 0,
          pageSize: 20,
          previousPageIndex: 0,
        };
      }
      this.getData();
    });
  }

  private getData() {
    const req: PlayerSearchRequest = {
      Name: this.name,
      Page: this.page,
    };
    this.req$.next(req);
  }

  pageChange(p: PageEvent) {
    this.page = {
      ...p
    };
    this.getData();
  }
}
