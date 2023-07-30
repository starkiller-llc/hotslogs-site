import { ChangeDetectionStrategy, Component, Input, OnInit, SimpleChanges } from '@angular/core';
import { normalize } from '../../../utils/normalize';
import { Stat } from '../model';
import * as _ from 'lodash-es';

type StatEx = Stat & {
  cellClass1?: Record<string, boolean>;
  cellClass2?: Record<string, boolean>;
  cellClass3?: Record<string, boolean>;
  cellClass4?: Record<string, boolean>;
  cellClass5?: Record<string, boolean>;
};

@Component({
  selector: 'app-compositions-table',
  templateUrl: './compositions-table.component.html',
  styleUrls: ['./compositions-table.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CompositionsTableComponent implements OnInit {
  private _stats: StatEx[];

  allColumns = ['GamesPlayed', 'WinPercent', 'img1', 'img2', 'img3', 'img4', 'img5'];
  columns = this.allColumns;

  @Input()
  public get stats(): Stat[] {
    return this._stats;
  }
  public set stats(value: Stat[]) {
    this._stats = _.cloneDeep(value);
  }
  @Input() roles = true;

  constructor() { }

  ngOnInit(): void {
  }

  ngOnChanges(changes: SimpleChanges): void {
    const exclude = [];
    this.columns = this.allColumns.filter(c => !exclude.includes(c));
    this._stats.forEach(s => {
      s.cellClass1 = this.getClass(s.CharacterImageURL1);
      s.cellClass2 = this.getClass(s.CharacterImageURL2);
      s.cellClass3 = this.getClass(s.CharacterImageURL3);
      s.cellClass4 = this.getClass(s.CharacterImageURL4);
      s.cellClass5 = this.getClass(s.CharacterImageURL5);
    });
  }

  private getClass(s: string) {
    const pref = this.roles ? 'role' : 'hero';
    return {
      [`${pref}-${normalize(s).replace(/\W/g, '')}`]: true,
      [pref]: true,
    };
  }
}
