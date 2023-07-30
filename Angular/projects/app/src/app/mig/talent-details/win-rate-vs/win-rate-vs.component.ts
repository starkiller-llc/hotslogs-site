import { Component, Input, OnInit } from '@angular/core';
import { WinRateWithVs } from '../model';

@Component({
  selector: 'app-win-rate-vs',
  templateUrl: './win-rate-vs.component.html',
  styleUrls: ['./win-rate-vs.component.scss']
})
export class WinRateVsComponent implements OnInit {
  @Input() stats: WinRateWithVs[];
  @Input() squarePortrait = false;

  constructor() { }

  ngOnInit(): void {
  }
}
