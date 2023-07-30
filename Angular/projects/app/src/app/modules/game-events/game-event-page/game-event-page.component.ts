import { AfterViewChecked, Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { BehaviorSubject, combineLatest, map, Observable, switchMap, tap } from 'rxjs';
import { GameEventGamesAndInfo } from '../models/game-event-games-and-info';
import { GameEventsService } from '../services/game-events.service';
import * as _ from 'lodash-es';
import { GameEventTeam } from '../models/game-event-team';
import { UserService } from '../../../services/user.service';
import { GameEventGame } from '../models/game-event-game';
import { MatTableDataSource } from '@angular/material/table';
import { MatSort } from '@angular/material/sort';
import { DatePipe } from '@angular/common';

@Component({
  selector: 'app-game-event-page',
  templateUrl: './game-event-page.component.html',
  styleUrls: ['./game-event-page.component.scss']
})
export class GameEventPageComponent implements OnInit, AfterViewChecked {
  @ViewChild('tbl') tbl: MatSort;
  @ViewChild('matches') matches: ElementRef;

  gameEvent$: Observable<GameEventGamesAndInfo>;
  teamsMap: Record<number, GameEventTeam>;
  teamsReverseMap: Record<string, GameEventTeam>;
  assignWinningTeam: Record<number, boolean> = {};
  assignLosingTeam: Record<number, boolean> = {};
  isAdmin$: Observable<boolean>;
  refresh$ = new BehaviorSubject<number>(0);
  selectedTeam?: GameEventTeam;
  selectedTeamGames: MatTableDataSource<GameEventGame>;
  unassignedGames: GameEventGame[];
  showSpoiler = false;

  displayedColumns = ['map', 'dateTime', 'vs', 'ops'];
  gamesMap: Record<number, { id: number; vs: string; win: boolean; }>;

  constructor(
    private svc: GameEventsService,
    private route: ActivatedRoute,
    private svcUser: UserService) {
    this.isAdmin$ = this.svcUser.user$.pipe(map(r => r.isAdmin));
  }

  ngOnInit(): void {
    this.gameEvent$ = combineLatest([this.route.paramMap, this.refresh$]).pipe(
      map(([z, a]) => [+z.get('id'), +z.get('teamId')] as const),
      switchMap(([id, teamId]) => this.svc.getGameEvent(id).pipe(map(z => [z, teamId] as readonly [GameEventGamesAndInfo, number]))),
      tap(([ge, teamId]) => this.setMaps(ge, teamId)),
      map(([ge, teamId]) => ge)
    );
  }

  ngAfterViewChecked(): void {
    if (this.selectedTeamGames) {
      this.selectedTeamGames.sort = this.tbl;
    }
  }

  private setMaps(ge: GameEventGamesAndInfo, teamId: number) {
    this.teamsMap = _.mapKeys(ge.teams, x => x.id);
    this.teamsReverseMap = _.mapKeys(ge.teams, x => x.name);
    this.unassignedGames = ge.games.filter(x => !x.losingTeamId || !x.winningTeamId);
    if (teamId) {
      const t = this.teamsMap[teamId];
      this.selectTeam(ge, t);
    }
  }

  assignTeam(replayId: number, winningTeam: boolean, teamName: string) {
    let teamId = 0;
    if (this.teamsReverseMap[teamName]) {
      teamId = this.teamsReverseMap[teamName].id;
    } else if (!teamName) {
      console.error('team name must be specified');
    }
    this.svc.assignTeam(replayId, winningTeam, teamId, teamName).subscribe(r => {
      this.refresh$.next(0);
    });
  }

  selectTeam(ge: GameEventGamesAndInfo, t: GameEventTeam) {
    const datePipe = new DatePipe('en-US');
    this.selectedTeam = t;
    this.selectedTeamGames = new MatTableDataSource(ge.games.filter(x => x.winningTeamId === t.id || x.losingTeamId === t.id));
    const gameResults = this.selectedTeamGames.data.map(x => ({
      id: x.replayId,
      vs: x.winningTeamId === t.id ? this.teamsMap[x.losingTeamId].name : this.teamsMap[x.winningTeamId].name,
      win: x.winningTeamId === t.id,
    }))
    this.gamesMap = _.mapKeys(gameResults, x => x.id);

    const o = this.selectedTeamGames.sortingDataAccessor;
    this.selectedTeamGames.sortingDataAccessor = (data: GameEventGame, sortHeaderId: string) => {
      if (sortHeaderId === 'winner') {
        return this.teamsMap[data.winningTeamId].name;
      } else if (sortHeaderId === 'loser') {
        return this.teamsMap[data.losingTeamId].name;
      } else if (sortHeaderId === 'vs') {
        return this.gamesMap[data.replayId].vs;
      } else {
        return o(data, sortHeaderId);
      }
    };

    this.selectedTeamGames.filterPredicate = (data: GameEventGame, filter: string) => {
      const o = {
        map: data.map,
        dateTime: datePipe.transform(data.dateTime, 'dd-MMM-yyyy HH:mm'),
        winner: this.teamsMap[data.winningTeamId]?.name,
        loser: this.teamsMap[data.losingTeamId]?.name,
      };
      return JSON.stringify(o).toLowerCase().indexOf(filter.toLowerCase()) !== -1;
    };

    // setTimeout(() => {
    //   const el = this.matches.nativeElement as HTMLElement;
    //   return el.scrollIntoView({ behavior: 'smooth' });
    // }, 0);
  }

  doShowSpoiler() {
    this.showSpoiler = true;

    setTimeout(() => {
      const el = this.matches.nativeElement as HTMLElement;
      return el.scrollIntoView({ behavior: 'smooth' });
    }, 0);
  }
}
