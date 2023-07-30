import { Component, Input, OnInit } from '@angular/core';
import { Tournament } from '../models/tournament';

@Component({
  selector: 'app-tournament-card',
  templateUrl: './tournament-card.component.html',
  styleUrls: ['./tournament-card.component.scss']
})
export class TournamentCardComponent implements OnInit {
  @Input() tournament: Tournament;
  public has_started: boolean;
  public registration_deadline: string;
  public has_entry_fee: boolean = false;

  constructor() { }

  ngOnInit(): void {
    this.has_entry_fee = this.tournament.entryFee > 0;
    this.registration_deadline = this.tournament.registrationDeadline.toDateString();
    this.has_started = this.tournament.registrationDeadline <= new Date();
  }
}
