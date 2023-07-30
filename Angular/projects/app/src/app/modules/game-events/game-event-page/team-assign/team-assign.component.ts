import { Component, ElementRef, EventEmitter, HostListener, Input, OnInit, Output, ViewChild } from '@angular/core';
import { ClickOutsideService } from '../../click-outside.service';
import { GameEventTeam } from '../../models/game-event-team';

@Component({
  selector: 'app-team-assign',
  templateUrl: './team-assign.component.html',
  styleUrls: ['./team-assign.component.scss']
})
export class TeamAssignComponent implements OnInit {
  @Input() replayId: number;
  @Input() teamId?: number;
  @Input() isAdmin = false;
  @Input() teamsMap: Record<number, GameEventTeam>;
  @Output() assignTeam = new EventEmitter<string>();

  @ViewChild('inp') inp: ElementRef;

  assign = false;
  teamName = '';

  constructor(private svcClickOutside: ClickOutsideService) { }

  ngOnInit(): void {
  }

  // @HostListener("window:click", ['$event'])
  clickOutside(e: MouseEvent) {
    const except = this.svcClickOutside.getValue(e.target as HTMLElement);
    if (except !== this.replayId) {
      this.assign = false;
    }
  }

  startEdit() {
    this.assign = true;
    if (this.teamId) {
      this.teamName = this.teamsMap[this.teamId].name;
    }
    setTimeout(() => (this.inp.nativeElement as HTMLElement).focus(), 0);
  }

  stopEdit() {
    this.assign = false;
    if (!this.teamId || this.teamName !== this.teamsMap[this.teamId].name) {
      this.assignTeam.emit(this.teamName);
    }
  }
}
