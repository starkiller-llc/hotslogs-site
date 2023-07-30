import { Component, EventEmitter, Input, OnChanges, OnDestroy, OnInit, Output, SimpleChanges, ViewChild } from '@angular/core';
import { FormControl } from '@angular/forms';
import { MatSelect, MatSelectChange } from '@angular/material/select';

interface NameValueB<T> {
  value: T;
  key: string;
  selected?: boolean;
}

type NameValue = NameValueB<string>;
type NameValueN = NameValueB<number>;

@Component({
  selector: 'lib-dropdown',
  templateUrl: './dropdown.component.html',
  styleUrls: ['./dropdown.component.scss']
})
export class DropdownComponent<T> implements OnInit, OnChanges, OnDestroy {
  toppings = new FormControl<T>(null);
  toppingList: NameValueB<T>[] = [
  ];

  @Input() itemsjson: string;
  @Input() items: NameValueB<T>[];
  @Input() caption: string;
  @Output() selectionchanged = new EventEmitter<T>();
  @Output() valueChange = new EventEmitter<T>();
  initialValue: T;
  savedValue: T;

  // Public API
  @Input()
  public get value(): T {
    return this.toppings.value;
  }
  public set value(v: T) {
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
    const initialValue = this.toppingList.find(r => r.selected)?.value;
    setTimeout(() => this.initialValue = initialValue, 0);

    this.toppings.setValue(initialValue);
  }

  onSelectionChange(e) {
    if (e) {
      if (!this.savedValue && this.savedValue !== 0) {
        this.savedValue = this.toppings.value;
      }
      return;
    }

    if (!this.compStr(this.toppings.value, this.savedValue)) {
      return;
    }

    this.savedValue = null;
    this.valueChange.emit(this.value);
    this.selectionchanged.emit(this.value);
  }

  private compStr(v: T, sv: T) {
    if (v === sv) {
      return false;
    }

    return true;
  }

  clear() {
    this.toppings.setValue(null);
  }

  selChangeWhileClosed(e: MatSelectChange) {
    this.onSelectionChange(e.source.panelOpen);
  }
}
