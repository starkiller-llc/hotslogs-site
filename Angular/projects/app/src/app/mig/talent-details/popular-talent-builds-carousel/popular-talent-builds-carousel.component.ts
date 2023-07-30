import { Component, Input, OnInit } from '@angular/core';
import { TalentBuildStatistic } from '../model';

@Component({
  selector: 'app-popular-talent-builds-carousel',
  templateUrl: './popular-talent-builds-carousel.component.html',
  styleUrls: ['./popular-talent-builds-carousel.component.scss']
})
export class PopularTalentBuildsCarouselComponent implements OnInit {
  @Input() stats: TalentBuildStatistic[];
  @Input() hero: string;

  js = [0, 1, 2, 3, 4, 5, 6];
  tiers = [1, 4, 7, 10, 13, 16, 20];

  constructor() { }

  ngOnInit(): void {
  }
}
