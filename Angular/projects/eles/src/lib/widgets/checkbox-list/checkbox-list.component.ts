import { Component, EventEmitter, Input, OnChanges, OnDestroy, OnInit, Output, SimpleChanges } from '@angular/core';
import { FormControl } from '@angular/forms';
import { MatSelectChange } from '@angular/material/select';

interface NameValue<T> {
  value: T;
  key: string;
  selected?: boolean;
}

@Component({
  selector: 'lib-checkbox-list',
  templateUrl: './checkbox-list.component.html',
  styleUrls: ['./checkbox-list.component.scss']
})
export class CheckboxListComponent<T> implements OnInit, OnChanges, OnDestroy {
  toppings = new FormControl<T[]>([]);
  toppingList: NameValue<T>[] = [
  ];

  @Input() itemsjson: string;
  @Input() items: NameValue<T>[];
  @Input() caption: string;
  @Output() selectionchanged = new EventEmitter<T[]>();
  @Output() valueChange = new EventEmitter<T[]>();
  initialValue: T[];
  savedValue: T[];

  // Public API
  @Input()
  public get value(): T[] {
    return this.toppings.value;
  }
  public set value(v: T[]) {
    this.toppings.setValue(v);
  }

  constructor() {
  }

  ngOnInit(): void {
  }

  ngOnDestroy(): void {
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes.itemsjson) {
      try {
        this.toppingList = JSON.parse(this.itemsjson);
      } catch (e) {
        return;
      }
      this.processInitialValue();
    }
    if (changes.items) {
      this.toppingList = this.items;
      this.processInitialValue();
    }
  }

  private processInitialValue() {
    const initialValue = this.toppingList.filter(r => r.selected);
    const initialValueStr = initialValue.map(r => r.value).sort();
    setTimeout(() => this.initialValue = initialValueStr, 0);

    this.toppings.setValue(initialValueStr);
  }

  onSelectionChange(e) {
    if (e) {
      this.savedValue = this.savedValue || this.toppings.value;
      return;
    }

    if (!this.compArr(this.toppings.value, this.savedValue)) {
      return;
    }

    this.savedValue = null;
    this.valueChange.emit(this.value);
    this.selectionchanged.emit(this.value);
  }

  private compArr(v: T[], sv: T[]) {
    if (v.length === sv?.length) {
      const same = v.every((x, i) => x === sv[i]);
      if (same) {
        return false;
      }
    }

    return true;
  }

  clear() {
    this.toppings.setValue([]);
  }

  selChangeWhileClosed(e: MatSelectChange) {
    this.onSelectionChange(e.source.panelOpen);
  }
}
