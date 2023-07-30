import { Team } from '../model';

export interface MapObjectivesRequest {
  GameMode: string;
  GameModeEx: string;
  Tournament: string;
  Map: string[];
}

export interface Datum {
  RowTitle: string;
  GamesPlayed: number;
  Value: string;
}

export interface Table {
  Heading: string;
  FieldName: string;
  Data: Datum[];
}

export interface Chart {
  Heading: string;
  JsonData: string;
}

export interface MapObjectivesResult {
  Teams?: Team[];
  Tables: Table[];
  Charts: Chart[];
}
