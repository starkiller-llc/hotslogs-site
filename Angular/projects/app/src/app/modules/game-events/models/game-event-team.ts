import { GameEventPlayer } from './game-event-player';

export interface GameEventTeam {
  id: number;
  name: string;
  players: GameEventPlayer[];
  logoUrl: string;
}
