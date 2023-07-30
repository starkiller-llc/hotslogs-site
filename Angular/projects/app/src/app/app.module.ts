import { Inject, NgModule } from '@angular/core';
import { DOCUMENT, APP_BASE_HREF } from '@angular/common';
import { BrowserModule } from '@angular/platform-browser';

import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatInputModule } from '@angular/material/input';

import { map } from 'rxjs/operators';

import { AppRoutingModule } from './app-routing.module';
import { UserService } from './services/user.service';
import { themeClass, ThemeService } from './services/theme.service';

import { AppComponent } from './app.component';
// import { ProfilePageComponent } from './profile-page/profile-page.component';
import { HomePageComponent } from './home-page/home-page.component';
// import { UploadReplayPageComponent } from './upload-replay-page/upload-replay-page.component';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatSelectModule } from '@angular/material/select';
// import { TournamentModule } from './tournament/tournament.module';
import { Validators } from '@angular/forms';
import { GameEventsModule } from './modules/game-events/game-events.module';
import { DefaultComponent } from './mig/default/default.component';
import { SafeHtmlPipe } from './pipes/safe-html.pipe';
import { ElesModule } from '../../../eles/src/public-api';
import { TalentDetailsComponent } from './mig/talent-details/talent-details.component';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { MatTableModule } from '@angular/material/table';
import { HlStyleDirective } from './directives/hl-style.directive';
import { MatSortModule } from '@angular/material/sort';
import { MigSortDirective } from './directives/mig-sort.directive';
import { HlTooltipDirective } from './directives/hl-tooltip.directive';
import { HlAnyclipDirective } from './directives/hl-anyclip.directive';
import { HlAnyclipMobileDirective } from './directives/hl-anyclip-mobile.directive';
import { TalentBuildsComponent } from './mig/talent-details/talent-builds/talent-builds.component';
import { PopularTalentBuildsTableComponent } from './mig/talent-details/popular-talent-builds-table/popular-talent-builds-table.component';
import { PopularTalentBuildsCarouselComponent } from './mig/talent-details/popular-talent-builds-carousel/popular-talent-builds-carousel.component';
import { WinRateVsComponent } from './mig/talent-details/win-rate-vs/win-rate-vs.component';
import { WinRateWithComponent } from './mig/talent-details/win-rate-with/win-rate-with.component';
import { MapStatsComponent } from './mig/talent-details/map-stats/map-stats.component';
import { MonkeyBrokerDirective } from './directives/monkey-broker.directive';
import { HeroAndMapComponent } from './mig/hero-and-map/hero-and-map.component';
import { PopularTalentBuildsComponent } from './mig/hero-and-map/popular-talent-builds/popular-talent-builds.component';
import { TeamLogosComponent } from './widgets/team-logos/team-logos.component';
import { HlSanitizePipe } from './pipes/hl-sanitize.pipe';
import { PortraitSpriteComponent } from './widgets/portrait-sprite/portrait-sprite.component';
import { MapSpriteComponent } from './widgets/map-sprite/map-sprite.component';
import { TalentSpriteComponent } from './widgets/talent-sprite/talent-sprite.component';
import { StatsComponent } from './mig/hero-and-map/stats/stats.component';
import { TableFilterComponent } from './widgets/table-filter/table-filter.component';
import { SharedModule } from './modules/shared/shared.module';
import { TitleComponent } from './widgets/title/title.component';
import { RoleButtonsComponent } from './widgets/role-buttons/role-buttons.component';
import { DefaultStatsComponent } from './mig/default/default-stats/default-stats.component';
import { MapObjectivesComponent } from './mig/map-objectives/map-objectives.component';
import { GenericTableComponent } from './mig/map-objectives/generic-table/generic-table.component';
import { ScoreResultsComponent } from './mig/score-results/score-results.component';
import { ScoreTableComponent } from './mig/score-results/score-table/score-table.component';
import { TeamCompositionsComponent } from './mig/team-compositions/team-compositions.component';
import { RoleSpriteComponent } from './widgets/role-sprite/role-sprite.component';
import { CompositionsTableComponent } from './mig/team-compositions/compositions-table/compositions-table.component';
import { MatchAwardsComponent } from './mig/match-awards/match-awards.component';
import { AwardsTableComponent } from './mig/match-awards/awards-table/awards-table.component';
import { AwardSpriteComponent } from './widgets/award-sprite/award-sprite.component';
import { PlayerNavMenuComponent } from './widgets/player-nav-menu/player-nav-menu.component';
import { RankingsComponent } from './mig/rankings/rankings.component';
import { RankingsTableComponent } from './mig/rankings/rankings-table/rankings-table.component';
import { HeroRankingsComponent } from './mig/hero-rankings/hero-rankings.component';
import { HeroRankingsTableComponent } from './mig/hero-rankings/hero-rankings-table/hero-rankings-table.component';
import { ReputationComponent } from './mig/reputation/reputation.component';
import { ReputationTableComponent } from './mig/reputation/reputation-table/reputation-table.component';
import { MatPaginatorModule } from '@angular/material/paginator';
import { UploadComponent } from './mig/upload/upload.component';
import { FileUploadModule } from 'ng2-file-upload';
import { MatchSummaryComponent } from './mig/match-summary/match-summary.component';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatchScoreTableComponent } from './mig/match-summary/match-score-table/match-score-table.component';
import { DetailsTableComponent } from './mig/match-summary/details-table/details-table.component';
import { MatchTalentUpgradesComponent } from './mig/match-summary/match-talent-upgrades/match-talent-upgrades.component';
import { MatchTalentUpgradeStacksComponent } from './mig/match-summary/match-talent-upgrade-stacks/match-talent-upgrade-stacks.component';
import { HeroBansTableComponent } from './mig/match-summary/hero-bans-table/hero-bans-table.component';
import { TeamObjectivesTableComponent } from './mig/match-summary/team-objectives-table/team-objectives-table.component';
import { MatchSummaryWidgetComponent } from './widgets/match-summary-widget/match-summary-widget.component';
import { ScoreTotalsTableComponent } from './mig/match-summary/score-totals-table/score-totals-table.component';
import { MatchHistoryComponent } from './mig/match-history/match-history.component';
import { HistoryTableComponent } from './mig/match-history/history-table/history-table.component';
import { MatchSummaryDirective } from './directives/match-summary.directive';
import { OverviewComponent } from './mig/overview/overview.component';
import { OverviewScoreTableComponent } from './mig/overview/overview-score-table/overview-score-table.component';
import { HeroTableComponent } from './mig/overview/hero-table/hero-table.component';
import { MapTableComponent } from './mig/overview/map-table/map-table.component';
import { MapDetailsTableComponent } from './mig/overview/map-details-table/map-details-table.component';
import { UpgradeTableComponent } from './mig/overview/upgrade-table/upgrade-table.component';
import { ProfileComponent } from './mig/profile/profile.component';
import { ProfileHeroTableComponent } from './mig/profile/profile-hero-table/profile-hero-table.component';
import { ProfileMapTableComponent } from './mig/profile/profile-map-table/profile-map-table.component';
import { FriendsTableComponent } from './mig/profile/friends-table/friends-table.component';
import { SearchComponent } from './mig/search/search.component';
import { ResultsTableComponent } from './mig/search/results-table/results-table.component';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { LoginComponent } from './mig/login/login.component';
import { LogoutComponent } from './mig/logout/logout.component';
import { ManageComponent } from './mig/manage/manage.component';
import { PaypalDirective } from './directives/paypal.directive';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { SubConfirmComponent } from './mig/sub-confirm/sub-confirm.component';
import { SubCancelComponent } from './mig/sub-cancel/sub-cancel.component';
import { ChangePasswordComponent } from './mig/manage/change-password/change-password.component';
import { RecaptchaModule } from "ng-recaptcha";
import { RegisterComponent } from './mig/register/register.component';
import { ResetPasswdComponent } from './mig/reset-passwd/reset-passwd.component';
import { ChoosePasswordComponent } from './mig/reset-passwd/choose-password/choose-password.component';
import { RegisterPasswordComponent } from './mig/register/register-password/register-password.component';
import { ErrorComponent } from './mig/error/error.component';
import { PrivacyComponent } from './mig/privacy/privacy.component';
import { TosComponent } from './mig/tos/tos.component';
import { UrlSerializer } from '@angular/router';
import { CaseInsensitiveUrlSerializer } from './services/case-insensitive-url-serializer';
import { AdblockComponent } from './info/adblock/adblock.component';
import { ApiComponent } from './info/api/api.component';
import { XpInfoComponent } from './info/xp-info/xp-info.component';
import { FaqComponent } from './info/faq/faq.component';
import { HeroRoleComponent } from './info/hero-role/hero-role.component';
import { MmrInformationComponent } from './info/mmr-information/mmr-information.component';
import { ReputationInfoComponent } from './info/reputation-info/reputation-info.component';
import { TeamDraftInfoComponent } from './info/team-draft-info/team-draft-info.component';

export const battleTagValidator = Validators.pattern('[a-zA-Z][a-zA-Z]*#\\d+');

@NgModule({
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    HttpClientModule,
    AppRoutingModule,
    MatToolbarModule,
    MatButtonModule,
    MatIconModule,
    MatMenuModule,
    MatInputModule,
    FormsModule,
    ReactiveFormsModule,
    MatSelectModule,
    MatSortModule,
    MatTableModule,
    MatPaginatorModule,
    MatExpansionModule,
    FileUploadModule,
    MatProgressSpinnerModule,
    MatCheckboxModule,

    // TournamentModule,
    GameEventsModule,
    ElesModule,
    SharedModule,
    RecaptchaModule,
  ],
  declarations: [
    AppComponent,
    // ProfilePageComponent,
    HomePageComponent,
    DefaultComponent,
    SafeHtmlPipe,
    HlSanitizePipe,
    TalentDetailsComponent,
    HlStyleDirective,
    MigSortDirective,
    HlTooltipDirective,
    HlAnyclipDirective,
    HlAnyclipMobileDirective,
    TalentBuildsComponent,
    PopularTalentBuildsTableComponent,
    PopularTalentBuildsCarouselComponent,
    WinRateVsComponent,
    WinRateWithComponent,
    MapStatsComponent,
    MonkeyBrokerDirective,
    HeroAndMapComponent,
    TeamLogosComponent,
    PortraitSpriteComponent,
    MapSpriteComponent,
    TalentSpriteComponent,
    PopularTalentBuildsComponent,
    StatsComponent,
    TableFilterComponent,
    TitleComponent,
    RoleButtonsComponent,
    DefaultStatsComponent,
    MapObjectivesComponent,
    GenericTableComponent,
    ScoreResultsComponent,
    ScoreTableComponent,
    TeamCompositionsComponent,
    RoleSpriteComponent,
    CompositionsTableComponent,
    MatchAwardsComponent,
    AwardsTableComponent,
    AwardSpriteComponent,
    PlayerNavMenuComponent,
    RankingsComponent,
    RankingsTableComponent,
    HeroRankingsComponent,
    HeroRankingsTableComponent,
    ReputationComponent,
    ReputationTableComponent,
    UploadComponent,
    MatchSummaryComponent,
    MatchScoreTableComponent,
    DetailsTableComponent,
    MatchTalentUpgradesComponent,
    MatchTalentUpgradeStacksComponent,
    HeroBansTableComponent,
    TeamObjectivesTableComponent,
    MatchSummaryWidgetComponent,
    ScoreTotalsTableComponent,
    MatchHistoryComponent,
    HistoryTableComponent,
    MatchSummaryDirective,
    OverviewComponent,
    OverviewScoreTableComponent,
    HeroTableComponent,
    MapTableComponent,
    MapDetailsTableComponent,
    UpgradeTableComponent,
    ProfileComponent,
    ProfileHeroTableComponent,
    ProfileMapTableComponent,
    FriendsTableComponent,
    SearchComponent,
    ResultsTableComponent,
    LoginComponent,
    LogoutComponent,
    ManageComponent,
    PaypalDirective,
    SubConfirmComponent,
    SubCancelComponent,
    ChangePasswordComponent,
    ChoosePasswordComponent,
    RegisterPasswordComponent,
    RegisterComponent,
    ResetPasswdComponent,
    ErrorComponent,
    PrivacyComponent,
    TosComponent,
    AdblockComponent,
    ApiComponent,
    XpInfoComponent,
    FaqComponent,
    HeroRoleComponent,
    MmrInformationComponent,
    ReputationInfoComponent,
    TeamDraftInfoComponent,
    // UploadReplayPageComponent,
    // UploadReplayComponent,
  ],
  exports: [
  ],
  providers: [
    { provide: themeClass, useFactory: (svc: ThemeService) => svc.theme$.pipe(map(r => `theme-${r}`)), deps: [ThemeService] },
    // { provide: HTTP_INTERCEPTORS, useClass: JsonHttpInterceptor, multi: true },
    { provide: APP_BASE_HREF, useValue: '/' },
    { provide: UrlSerializer, useClass: CaseInsensitiveUrlSerializer },
  ],
  bootstrap: [AppComponent]
})

export class AppModule {
  constructor(
    svcTheme: ThemeService,
    @Inject(themeClass) public theme$,
    @Inject(DOCUMENT) doc: Document,
    svcUser: UserService
  ) {
    theme$.subscribe(r => this.replaceTheme(doc, r));
    // interval(1000).subscribe(r => svcTheme.theme = r % 2 ? 'dark' : 'light');
  }

  private replaceTheme(doc: Document, theme: string): void {
    const classList = Array.from(doc.body.classList);
    const toRemove = classList.filter(r => r.startsWith('theme-'));
    toRemove.forEach(x => doc.body.classList.remove(x));
    doc.body.classList.add(theme);
  }
}
