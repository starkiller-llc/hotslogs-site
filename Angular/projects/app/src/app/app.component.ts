import { BreakpointObserver } from '@angular/cdk/layout';
import { DOCUMENT } from '@angular/common';
import { AfterViewInit, Component, Inject, Renderer2 } from '@angular/core';
import { Router } from '@angular/router';
import { map, Observable } from 'rxjs';
import { environment } from '../environments/environment';
import { GameEvent } from './modules/game-events/models/game-event';
import { GameEventsService } from './modules/game-events/services/game-events.service';
import { HlLocalService } from './modules/shared/services/hl-local.service';
import { OverlayService } from './modules/shared/services/overlay.service';
import { MigrationService } from './services/migration.service';
import { themeClass, ThemeService } from './services/theme.service';
import { UserService } from './services/user.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements AfterViewInit {
  title = 'app';
  lang = 'en';
  narrow$: Observable<boolean>;
  isAdmin$: Observable<boolean>;
  isPremium$: Observable<boolean>;
  isLoggedIn$: Observable<boolean>;
  gameEvents$: Observable<GameEvent[]>;
  supporterSince$: Observable<Date>;

  constructor(
    bpObs: BreakpointObserver,
    public svcTheme: ThemeService,
    private gameEventsService: GameEventsService,
    private svcUser: UserService,
    private svcMig: MigrationService,
    private svcLocal: HlLocalService,
    public ovl: OverlayService,
    private router: Router,
    private r: Renderer2,
    @Inject(DOCUMENT) private document: Document,
    @Inject(themeClass) public theme$) {

    this.narrow$ = bpObs.observe(['(max-width: 400px)'])
      .pipe(map(z => z.matches));

    this.isAdmin$ = this.svcUser.user$.pipe(map(r => r?.isAdmin || false));
    this.isPremium$ = this.svcUser.user$.pipe(map(r => r?.isPremium || false));
    this.isLoggedIn$ = this.svcUser.user$.pipe(map(r => !!r));
    this.supporterSince$ = this.svcUser.user$.pipe(map(r => r?.supporterSince));

    this.gameEvents$ = this.gameEventsService.getGameEvents();

    this.svcMig.getLanguage().subscribe(r => {
      this.svcLocal.translation = r.Strings;
      return this.lang = r.LanguageCode;
    });
  }

  ngAfterViewInit(): void {
    var script = this.r.createElement('script') as HTMLScriptElement;
    script.type = 'text/javascript';
    script.src = `https://www.paypal.com/sdk/js?client-id=${environment.paypalUser}&vault=true`;
    script.attributes['data-sdk-integration-source'] = 'button-factory';
    this.r.appendChild(document.body, script);
  }

  changeLanguage(lang: string) {
    this.svcMig.changeLanguage(lang).subscribe(r => {
      location.reload();
    });
  }

  stopPropagation(event) {
    event.stopPropagation();
  }

  doSearch(e) {
    this.router.navigate(['/PlayerSearch'], { queryParams: { Name: e } });
  }

  click(e) {
    e.click();
  }

  focus(e) {
    setTimeout(() => e.focus(), 100);
  }
}
