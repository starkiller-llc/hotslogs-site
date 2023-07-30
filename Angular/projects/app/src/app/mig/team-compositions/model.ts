export interface TeamCompositionsRequest {
  Grouping: number;
  Map: string[];
  Hero: string;
}

export interface Stat {
  Character1: string;
  Character2: string;
  Character3: string;
  Character4: string;
  Character5: string;
  CharacterImageURL1: string;
  CharacterImageURL2: string;
  CharacterImageURL3: string;
  CharacterImageURL4: string;
  CharacterImageURL5: string;
  GamesPlayed: string;
  WinPercent: string;
}

export interface TeamCompositionsResult {
  Stats: Stat[];
}
