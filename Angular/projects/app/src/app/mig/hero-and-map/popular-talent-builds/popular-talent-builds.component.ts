import { Component, Input, OnInit } from '@angular/core';
import { Role } from '../../model';
import { PopularTalentBuild } from '../model';

@Component({
  selector: 'app-popular-talent-builds',
  templateUrl: './popular-talent-builds.component.html',
  styleUrls: ['./popular-talent-builds.component.scss']
})
export class PopularTalentBuildsComponent implements OnInit {
  @Input() stats: PopularTalentBuild[];
  @Input() roles: Role[];

  constructor() { }

  ngOnInit(): void {
  }

}
