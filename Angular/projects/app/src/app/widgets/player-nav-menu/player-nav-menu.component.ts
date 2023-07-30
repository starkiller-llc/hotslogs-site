import { Component, Input, OnChanges, OnInit, SimpleChanges } from '@angular/core';

@Component({
  selector: 'app-player-nav-menu',
  templateUrl: './player-nav-menu.component.html',
  styleUrls: ['./player-nav-menu.component.scss']
})
export class PlayerNavMenuComponent implements OnInit, OnChanges {
  @Input() playerId: number;
  @Input() eventId = 0;

  links: [string, string, boolean][] = [];

  constructor() { }
  ngOnInit(): void {
  }

  ngOnChanges(changes: SimpleChanges): void {
    this.links = [];
    this.links.push([`/Player/Profile`, `${this.eventId !== 0 ? 'Event ' : ''}Profile`, true]);
    if (this.eventId === 0) {
      this.links.push([`/Player/HeroOverview`, `Hero Overview`, true]);
      this.links.push([`/Team/Overview`, `Team Overview`, true]);
      this.links.push([`/Player/MatchAwards`, `Match Awards`, true]);
    }
    this.links.push([`/Player/MatchHistory`, `${this.eventId !== 0 ? 'Event ' : ''}Match History`, true]);
  }

  isActive(h: string) {
    return window.location.pathname.replace('/ang', '') === h;
  }
}
