import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Route, RouterModule } from '@angular/router';
import { GameEventListPageComponent } from './game-event-list-page/game-event-list-page.component';
import { GameEventPageComponent } from './game-event-page/game-event-page.component';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { ClickOutsideDirective } from './click-outside.directive';
import { TeamAssignComponent } from './game-event-page/team-assign/team-assign.component';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatSortModule } from '@angular/material/sort';
import { UserIsAdminGuard } from '../../services/user-is-admin.guard';
import { SharedModule } from '../shared/shared.module';

const routes: Route[] = [
  { path: 'events', component: GameEventListPageComponent, canActivate: [UserIsAdminGuard] },
  { path: 'event/:id/:teamId', component: GameEventPageComponent },
  { path: 'event/:id', component: GameEventPageComponent, canActivate: [UserIsAdminGuard] },
];

@NgModule({
  declarations: [
    GameEventListPageComponent,
    GameEventPageComponent,
    ClickOutsideDirective,
    TeamAssignComponent,
  ],
  imports: [
    CommonModule,
    MatButtonModule,
    MatInputModule,
    MatCardModule,
    MatTableModule,
    MatIconModule,
    MatSortModule,
    FormsModule,
    SharedModule,
    RouterModule.forChild(routes)
  ],
  exports: [
    RouterModule,
  ]
})
export class GameEventsModule { }
