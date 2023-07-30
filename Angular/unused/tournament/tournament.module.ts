import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TournamentBracketComponent } from './tournament-bracket/tournament-bracket.component';
import { TournamentBracketMatchComponent } from './tournament-bracket-match/tournament-bracket-match.component';
import { TournamentBracketRoundComponent } from './tournament-bracket-round/tournament-bracket-round.component';
import { TournamentCardComponent } from './tournament-card/tournament-card.component';
import { TournamentRegistrationFormComponent } from './tournament-registration-form/tournament-registration-form.component';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatSelectModule } from '@angular/material/select';
import { BrowserModule } from '@angular/platform-browser';
import { AppRoutingModule } from '../app-routing.module';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatInputModule } from '@angular/material/input';
import { TournamentPageComponent } from './tournament-page/tournament-page.component';
import { TournamentListPageComponent } from './tournament-list-page/tournament-list-page.component';
import { PlayerTournamentTableComponent } from './player-tournament-table/player-tournament-table.component';
import { UploadReplayComponent } from '../upload-replay/upload-replay.component';
import { CreateTournamentPageComponent } from './create-tournament-page/create-tournament-page.component';
import { UpdateTournamentPageComponent } from './update-tournament-page/update-tournament-page.component';

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    MatSelectModule,
    BrowserModule,
    AppRoutingModule,
    MatToolbarModule,
    MatButtonModule,
    MatIconModule,
    MatMenuModule,
    MatInputModule
  ],
  declarations: [
    TournamentBracketComponent,
    TournamentBracketMatchComponent,
    TournamentBracketRoundComponent,
    TournamentCardComponent,
    TournamentRegistrationFormComponent,
    TournamentPageComponent,
    TournamentListPageComponent,
    PlayerTournamentTableComponent,
    UploadReplayComponent,
    UpdateTournamentPageComponent,
    CreateTournamentPageComponent
  ],
  exports: [
    TournamentCardComponent,
    TournamentBracketComponent,
    TournamentBracketMatchComponent,
    TournamentBracketRoundComponent,
    TournamentCardComponent,
    TournamentRegistrationFormComponent,
    TournamentPageComponent,
    TournamentListPageComponent,
    PlayerTournamentTableComponent,
    UploadReplayComponent,
    UpdateTournamentPageComponent,
    CreateTournamentPageComponent
  ]
})
export class TournamentModule { }
