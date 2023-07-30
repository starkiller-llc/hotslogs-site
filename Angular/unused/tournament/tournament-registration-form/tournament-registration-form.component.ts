import { Component, Input, OnInit } from '@angular/core';
import { Validators, FormGroup, FormBuilder } from '@angular/forms';
import { TournamentService } from '../services/tournament.service';
import { HttpEventType } from '@angular/common/http';
import { TournamentRegistrationApplication } from '../models/tournament-registration';
import { battleTagValidator } from '../../app.module';

@Component({
  selector: 'app-tournament-registration-form',
  templateUrl: './tournament-registration-form.component.html',
  styleUrls: ['./tournament-registration-form.component.scss']
})
export class TournamentRegistrationFormComponent implements OnInit {
  submitted: boolean = false;
  submitSuccess: boolean = false;
  submitError: string = "";
  @Input() tournament_id: number;
  @Input() entry_fee: number;
  @Input() registration_deadline: string;
  teamForm: FormGroup;

  constructor(
    private tournamentService: TournamentService,
    private fb: FormBuilder,
  ) { }

  private async registerForTournament() {
    const v = this.teamForm.value;
    const r: TournamentRegistrationApplication = {
      tournamentId: this.tournament_id,
      teamName: v.team_name,
      captainEmail: v.captain_email,
      battletag1: v.battletag_1,
      battletag2: v.battletag_2,
      battletag3: v.battletag_3,
      battletag4: v.battletag_4,
      battletag5: v.battletag_5,
      battletag6: v.battletag_6,
      battletag7: v.battletag_7,
      paypalEmail: v.paypal_email,
      isPaid: 0,
    }
    this.tournamentService.registerForTournament(r).subscribe(event => {

      if (event && event.type === HttpEventType.Response) {
        const message = 'Upload success.';
        if (event && event.status === 200 && event.body === '') {
          this.submitSuccess = true;
        }
        else {
          this.submitError = "Sorry, we cannot register your team at this time. If this problem persists, please contact us at bugreport@hotslogs.com.";
        }
      }
    });
  }

  public onSubmit() {
    this.submitted = true;
    this.submitError = "";
    if (this.teamForm.valid) {
      this.registerForTournament();
    }
  }

  get teamFormControl() {
    return this.teamForm.controls;
  }

  ngOnInit(): void {
    this.teamForm = this.fb.group({
      team_name: ['', Validators.required],
      captain_email: ['', [Validators.required, Validators.email]],
      battletag_1: ['', [Validators.required, battleTagValidator]],
      battletag_2: ['', [Validators.required, battleTagValidator]],
      battletag_3: ['', [Validators.required, battleTagValidator]],
      battletag_4: ['', [Validators.required, battleTagValidator]],
      battletag_5: ['', [Validators.required, battleTagValidator]],
      battletag_6: ['', [battleTagValidator]],
      battletag_7: ['', [battleTagValidator]],
      paypal_email: this.entry_fee ? ['', [Validators.required, Validators.email]] : [''],
    })
  }

}
