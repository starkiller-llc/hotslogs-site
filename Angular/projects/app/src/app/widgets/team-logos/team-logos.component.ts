import { Component, Input, OnInit } from '@angular/core';
import { Team } from '../../mig/model';

@Component({
  selector: 'app-team-logos',
  templateUrl: './team-logos.component.html',
  styleUrls: ['./team-logos.component.scss']
})
export class TeamLogosComponent implements OnInit {
  @Input() teams: Team[];

  constructor() { }

  ngOnInit(): void {
  }

}
