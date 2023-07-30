import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';

@Component({
  selector: 'app-table-filter',
  templateUrl: './table-filter.component.html',
  styleUrls: ['./table-filter.component.scss']
})
export class TableFilterComponent implements OnInit {
  query = '';
  flt: string;

  @Output() filterChange = new EventEmitter<string>();
  @Input() showRoles = false;

  constructor() { }

  ngOnInit(): void {
  }

  filter(f: string) {
    this.flt = f;
    this.filterChange.emit(f);
  }
}
