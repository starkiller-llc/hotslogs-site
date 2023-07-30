import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { MatchSummaryResult } from '../../mig/match-summary/model';

@Component({
  selector: 'app-match-summary-widget',
  templateUrl: './match-summary-widget.component.html',
  styleUrls: ['./match-summary-widget.component.scss']
})
export class MatchSummaryWidgetComponent implements OnInit {
  @Input() result: MatchSummaryResult;
  @Input() replayId: number;
  @Output() voteDown = new EventEmitter<[number, boolean]>();
  @Output() voteUp = new EventEmitter<[number, boolean]>();

  constructor() { }

  ngOnInit(): void {
  }

}
