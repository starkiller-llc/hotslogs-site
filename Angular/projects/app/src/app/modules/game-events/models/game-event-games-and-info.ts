import { GameEvent } from './game-event';
import { GameEventGame } from './game-event-game';
import { GameEventTeam } from './game-event-team';

export interface GameEventGamesAndInfo {
  gameEvent: GameEvent;
  games: GameEventGame[];
  teams: GameEventTeam[];
}
