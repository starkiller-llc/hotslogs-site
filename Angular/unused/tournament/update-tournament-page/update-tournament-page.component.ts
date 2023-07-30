import { Component, OnInit, Input } from '@angular/core';
import { lastValueFrom } from 'rxjs';
import { TournamentService } from '../services/tournament.service';
import { Validators, FormGroup, FormBuilder } from '@angular/forms';

@Component({
  selector: 'update-tournament-page',
  templateUrl: './update-tournament-page.component.html',
  styleUrls: ['./update-tournament-page.component.scss']
})
export class UpdateTournamentPageComponent implements OnInit {
  tournamentForm: FormGroup;

  constructor(
    private tournamentService: TournamentService,
    private fb: FormBuilder,
  ) { }

  public async updateTournamentRegistrationDeadline() {
    const t = await lastValueFrom(this.tournamentService.updateTournamentRegistrationDeadline(parseInt(this.tournamentForm.value.tournamentId)));
  }

  public async updateTournamenMatchDeadline() {
    const t = await lastValueFrom(this.tournamentService.updateTournamenMatchDeadline(parseInt(this.tournamentForm.value.tournamentId)));
  }

  public async createTournamentMatches() {
    const t = await lastValueFrom(this.tournamentService.createTournamentMatches());
  }

  get tournamentFormControl() {
    return this.tournamentForm.controls;
  }

  ngOnInit(): void {
    this.tournamentForm = this.fb.group({
      tournamentId: ['', Validators.required],
    })
  }

}
