import { Component, OnInit, Input } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';

@Component({
  selector: 'lib-talent-build',
  templateUrl: './talent-build.component.html',
  styleUrls: ['./talent-build.component.scss']
})
export class TalentBuildComponent implements OnInit {
  private _build: string;
  @Input()
  public get build(): string {
    return this._build;
  }
  public set build(value: string) {
    this._build = value;
    this.updateBuildText();
  }
  private _hero: string;
  @Input()
  public get hero(): string {
    return this._hero;
  }
  public set hero(value: string) {
    this._hero = value;
    this.updateBuildText();
  }

  buildText: string;

  constructor(private _snackBar: MatSnackBar) { }

  ngOnInit(): void {
    return;
  }

  updateBuildText() {
    this.buildText = `[T${this._build},${this._hero}]`;
  }

  copyBuild(e: MouseEvent) {
    navigator.clipboard.writeText(this.buildText);
    this.openSnackBar('Copied', null);
    e.preventDefault();
  }

  openSnackBar(message: string, action: string) {
    this._snackBar.open(message, action, {
      duration: 2000,
    });
  }
}
