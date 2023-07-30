import { Component, Input, OnInit } from '@angular/core';
import { PopularTalentBuild } from '../model';

@Component({
  selector: 'app-popular-talent-builds-table',
  templateUrl: './popular-talent-builds-table.component.html',
  styleUrls: ['./popular-talent-builds-table.component.scss']
})
export class PopularTalentBuildsTableComponent implements OnInit {
  @Input() stats: PopularTalentBuild[];
  @Input() hero: string;

  constructor() { }

  ngOnInit(): void {
  }
}
