import { Component, OnInit, Input } from '@angular/core';
import { lastValueFrom } from 'rxjs';
import { TournamentService } from '../services/tournament.service';
import { Validators, FormGroup, FormBuilder } from '@angular/forms';
import { Tournament } from '../models/tournament';
import { getLocaleDateTimeFormat } from '@angular/common';
import { HttpEventType } from '@angular/common/http';

@Component({
  selector: 'app-create-tournament-page',
  templateUrl: './create-tournament-page.component.html',
  styleUrls: ['./create-tournament-page.component.scss']
})
export class CreateTournamentPageComponent implements OnInit {
  submitted: boolean = false;
  submitSuccess: boolean = false;
  submitError: string = "";
  tournamentForm: FormGroup;

  constructor(
    private tournamentService: TournamentService,
    private fb: FormBuilder,
  ) { }

  public onSubmit() {
    this.submitted = true;
    this.submitError = "";
    
    if (true || this.tournamentForm.valid) {
      this.createTournament();
    }
  }

  get tournamentFormControl() {
    return this.tournamentForm.controls;
  }

  private async createTournament() {
    const v = this.tournamentForm.value;
    const tournament: Tournament = {
      tournamentId: 0,
      tournamentName: v.tournament_name,
      tournamentDescription: v.tournament_description,
      registrationDeadline: new Date(v.registration_deadline),
      endDate: null,
      isPublic: 1,
      maxNumTeams: v.max_num_teams,
      entryFee: v.entry_fee,
      numTeams: 0
    }
    
    this.tournamentService.createTournament(tournament).subscribe(event => {
          
      if (event && event.type === HttpEventType.Response) {
        if (event && event.status === 200 && event.body =="") {
          this.submitSuccess = true;
        }
        else {
          this.submitError = "Sorry, we cannot create the tournament at this time. If this problem persists, please contact us on our Discord or at bugreport@hotslogs.com.";
        }
      }
    });
  }

  ngOnInit(): void {
    this.tournamentForm = this.fb.group({
      tournament_name: ['', Validators.required],
      tournament_description: ['', Validators.required],
      registration_deadline: ['', [Validators.required]],
      max_num_teams: ['', [Validators.required]],
      entry_fee: ['', [Validators.required]],
      // paypal_email: this.entry_fee ? ['', [Validators.required, Validators.email]] : [''],
    })
  }

}
