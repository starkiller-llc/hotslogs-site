import { Component, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import { GameEvent } from '../models/game-event';
import { GameEventsService } from '../services/game-events.service';

@Component({
  selector: 'app-game-event-list-page',
  templateUrl: './game-event-list-page.component.html',
  styleUrls: ['./game-event-list-page.component.scss']
})
export class GameEventListPageComponent implements OnInit {
  gameEvents$: Observable<GameEvent[]>;

  constructor(private gameEventsService: GameEventsService) { 
    this.gameEvents$ = this.gameEventsService.getGameEvents();
  }

  ngOnInit(): void {
  }

}
