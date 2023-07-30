import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AdblockComponent } from './info/adblock/adblock.component';
import { ApiComponent } from './info/api/api.component';
import { FaqComponent } from './info/faq/faq.component';
import { HeroRoleComponent } from './info/hero-role/hero-role.component';
import { MmrInformationComponent } from './info/mmr-information/mmr-information.component';
import { ReputationInfoComponent } from './info/reputation-info/reputation-info.component';
import { TeamDraftInfoComponent } from './info/team-draft-info/team-draft-info.component';
import { XpInfoComponent } from './info/xp-info/xp-info.component';
import { DefaultComponent } from './mig/default/default.component';
import { ErrorComponent } from './mig/error/error.component';
import { HeroAndMapComponent } from './mig/hero-and-map/hero-and-map.component';
import { HeroRankingsComponent } from './mig/hero-rankings/hero-rankings.component';
import { LoginComponent } from './mig/login/login.component';
import { LogoutComponent } from './mig/logout/logout.component';
import { ManageComponent } from './mig/manage/manage.component';
import { MapObjectivesComponent } from './mig/map-objectives/map-objectives.component';
import { MatchAwardsComponent } from './mig/match-awards/match-awards.component';
import { MatchHistoryComponent } from './mig/match-history/match-history.component';
import { MatchSummaryComponent } from './mig/match-summary/match-summary.component';
import { OverviewComponent } from './mig/overview/overview.component';
import { PrivacyComponent } from './mig/privacy/privacy.component';
import { ProfileComponent } from './mig/profile/profile.component';
import { RankingsComponent } from './mig/rankings/rankings.component';
import { RegisterComponent } from './mig/register/register.component';
import { ReputationComponent } from './mig/reputation/reputation.component';
import { ResetPasswdComponent } from './mig/reset-passwd/reset-passwd.component';
import { ScoreResultsComponent } from './mig/score-results/score-results.component';
import { SearchComponent } from './mig/search/search.component';
import { SubCancelComponent } from './mig/sub-cancel/sub-cancel.component';
import { SubConfirmComponent } from './mig/sub-confirm/sub-confirm.component';
import { TalentDetailsComponent } from './mig/talent-details/talent-details.component';
import { TeamCompositionsComponent } from './mig/team-compositions/team-compositions.component';
import { TosComponent } from './mig/tos/tos.component';
import { UploadComponent } from './mig/upload/upload.component';
import { FilterResolver } from './services/filter-resolver';
// import { ProfilePageComponent } from './profile-page/profile-page.component';
// import { ProfileResolverGuard } from './profile-page/profile-resolver.guard';
// import { UploadReplayPageComponent } from './upload-replay-page/upload-replay-page.component';
// import { TournamentPageComponent } from './tournament/tournament-page/tournament-page.component';
// import { TournamentListPageComponent } from './tournament/tournament-list-page/tournament-list-page.component';
// import { CreateTournamentPageComponent } from './tournament/create-tournament-page/create-tournament-page.component';
// import { UpdateTournamentPageComponent } from './tournament/update-tournament-page/update-tournament-page.component';
import { UserIsAdminGuard } from './services/user-is-admin.guard';

const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: '/default' },
  { path: 'default', component: DefaultComponent },
  { path: 'account/choosebattletagid', component: ManageComponent },
  { path: 'account/logout', component: LogoutComponent },
  { path: 'account/manage', component: ManageComponent },
  // Disable subscription -- Aviad 23-Nov-2022
  // { path: 'account/premium', component: ManageComponent },
  { path: 'account/subconfirm', component: SubConfirmComponent },
  { path: 'cancelsubscription', component: SubCancelComponent },
  { path: 'error', component: ErrorComponent },
  { path: 'herorankings', component: HeroRankingsComponent, resolve: { filters: FilterResolver } },
  { path: 'info/adblock', component: AdblockComponent },
  { path: 'info/api', component: ApiComponent },
  { path: 'info/experiencesummaryinfo', component: XpInfoComponent },
  { path: 'info/faq', component: FaqComponent },
  { path: 'info/herorole', component: HeroRoleComponent },
  { path: 'info/mmrinformation', component: MmrInformationComponent },
  { path: 'info/reputation', component: ReputationInfoComponent },
  { path: 'info/teamdraftinfo', component: TeamDraftInfoComponent },
  { path: 'login', component: LoginComponent },
  { path: 'passwordrecovery', component: ResetPasswdComponent },
  { path: 'player/herooverview', component: OverviewComponent, resolve: { filters: FilterResolver }, data: { player: true } },
  { path: 'player/matchawards', component: MatchAwardsComponent, resolve: { filters: FilterResolver }, data: { player: true } },
  { path: 'player/matchhistory', component: MatchHistoryComponent, resolve: { filters: FilterResolver } },
  { path: 'player/matchsummarycontainer', component: MatchSummaryComponent },
  { path: 'player/profile', component: ProfileComponent, resolve: { filters: FilterResolver } },
  { path: 'playersearch', component: SearchComponent },
  { path: 'privacy', component: PrivacyComponent },
  { path: 'rankings', component: RankingsComponent, resolve: { filters: FilterResolver } },
  { path: 'register', component: RegisterComponent },
  { path: 'resetpassword', component: ResetPasswdComponent },
  { path: 'sitewide/heroandmapstatistics', component: HeroAndMapComponent, resolve: { filters: FilterResolver } },
  { path: 'sitewide/mapobjectives', component: MapObjectivesComponent, resolve: { filters: FilterResolver } },
  { path: 'sitewide/matchawards', component: MatchAwardsComponent, resolve: { filters: FilterResolver } },
  { path: 'sitewide/reputationleaderboard', component: ReputationComponent },
  { path: 'sitewide/scoreresultstatistics', component: ScoreResultsComponent, resolve: { filters: FilterResolver } },
  { path: 'sitewide/talentdetails', component: TalentDetailsComponent, resolve: { filters: FilterResolver } },
  { path: 'sitewide/teamcompositions', component: TeamCompositionsComponent, resolve: { filters: FilterResolver } },
  { path: 'team/overview', component: OverviewComponent, resolve: { filters: FilterResolver } },
  { path: 'tos', component: TosComponent },
  { path: 'upload', component: UploadComponent },
  // { path: 'upload-replays', component: UploadReplayPageComponent },
  // { path: 'update-tournament', component: UpdateTournamentPageComponent, canActivate: [UserIsAdminGuard] },
  // { path: 'create-tournament', component: CreateTournamentPageComponent, canActivate: [UserIsAdminGuard] },
  // { path: 'tournaments', component: TournamentListPageComponent, canActivate: [UserIsAdminGuard] },
  // { path: 'tournament/:tournament_id', component: TournamentPageComponent, canActivate: [UserIsAdminGuard] },
  // { path: 'player/profile2', component: ProfilePageComponent, resolve: { profile: ProfileResolverGuard } },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
