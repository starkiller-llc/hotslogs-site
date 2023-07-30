import { Component, Input, OnInit } from '@angular/core';
import { WinRateWithVs } from '../model';

@Component({
  selector: 'app-win-rate-with',
  templateUrl: './win-rate-with.component.html',
  styleUrls: ['./win-rate-with.component.scss']
})
export class WinRateWithComponent implements OnInit {
  @Input() stats: WinRateWithVs[];
  @Input() squarePortrait = false;

  constructor() { }

  ngOnInit(): void {
  }

}
