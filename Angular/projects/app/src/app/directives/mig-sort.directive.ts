import { AfterViewInit, Directive, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort, Sort } from '@angular/material/sort';
import { MatTable, MatTableDataSource } from '@angular/material/table';
import { getLiteralValue } from '../utils/get-literal-value';

@Directive({
  selector: '[migSort]',
  exportAs: 'migSort',
})
export class MigSortDirective<T> implements AfterViewInit, OnChanges {
  @Input() migSort: T[];
  @Input() paginator: MatPaginator;
  @Input() total: number;
  @Input() filterFields: (keyof T)[] = null;
  @Input() serverSort = false;
  @Output() sortChange = new EventEmitter<Sort>();

  viewInit = false;
  ds: MatTableDataSource<T>;

  constructor(private sort: MatSort, private tbl: MatTable<T>) { }

  ngAfterViewInit(): void {
    this.viewInit = true;
    this.setup();
  }

  private setup() {
    if (!this.migSort || !this.viewInit) {
      return;
    }
    const ds = new MatTableDataSource<T>(this.migSort);
    ds.sortingDataAccessor = (data, sortHeaderId) => getLiteralValue(data[sortHeaderId], ds.sort.direction);
    if (!this.serverSort) {
      ds.sort = this.sort;
    }
    this.sort.sortChange.subscribe(r => this.sortChange.emit(r));
    this.sort.start = 'desc';
    this.tbl.dataSource = ds;
    if (this.filterFields) {
      ds.filterPredicate = (data, filter) => {
        const f = filter.toLowerCase();
        return this.filterFields
          .filter(r => data[r])
          .some(fld => (data[fld] as string).toLowerCase().includes(f));
      };
    }
    this.ds = ds;
    if (ds.paginator) {
      ds.paginator.length = this.total;
      ds.paginator = this.paginator;
    }
  }

  ngOnChanges(changes: SimpleChanges): void {
    this.setup();
  }

  filter(f: string) {
    const ds = this.tbl.dataSource as MatTableDataSource<T>;
    ds.filter = f;
  }
}
