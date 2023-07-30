import { Component, Input, OnInit } from '@angular/core';
import { TalentStatistic } from '../model';

@Component({
  selector: 'app-talent-builds',
  templateUrl: './talent-builds.component.html',
  styleUrls: ['./talent-builds.component.scss']
})
export class TalentBuildsComponent implements OnInit {
  @Input() stats: TalentStatistic[];
  @Input() hero: string;
  @Input() gamesPlayedIsHtml = false;

  constructor() { }

  ngOnInit(): void {
  }

}
