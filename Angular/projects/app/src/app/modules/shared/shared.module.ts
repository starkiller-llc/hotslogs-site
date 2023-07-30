import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HlTrDirective } from './directives/hl-tr.directive';
import { HlTrPipe } from './hl-tr.pipe';
import { IfViewDirective } from './directives/if-view.directive';
import {LayoutModule} from '@angular/cdk/layout';

@NgModule({
  declarations: [
    HlTrDirective,
    HlTrPipe,
    IfViewDirective,
  ],
  imports: [
    CommonModule,
    LayoutModule,
  ],
  exports: [
    HlTrDirective,
    HlTrPipe,
    IfViewDirective,
  ]
})
export class SharedModule { }
