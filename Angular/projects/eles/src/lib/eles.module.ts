import { NgModule } from '@angular/core';
import { CheckboxListComponent } from './widgets/checkbox-list/checkbox-list.component';
import { HotslogsNewsComponent } from './widgets/hotslogs-news/hotslogs-news.component';
import { MatchLogComponent } from './widgets/match-log/match-log.component';
import { RadChartComponent } from './widgets/rad-chart/rad-chart.component';
import { RadXpChartComponent } from './widgets/rad-xp-chart/rad-xp-chart.component';
import { TalentBuildComponent } from './widgets/talent-build/talent-build.component';
import { NegativeColoredLineChart } from './widgets/match-log/negative-colored-line-chart';
import * as Chart from 'chart.js';
import { materialModules } from './material';
import { ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { SlickDirective } from './directives/slick.directive';
import { DropdownComponent } from './widgets/dropdown/dropdown.component';
import { HTTP_INTERCEPTORS } from '@angular/common/http';
import { JsonHttpInterceptor } from './json-http.interceptor';



@NgModule({
  declarations: [
    RadXpChartComponent,
    RadChartComponent,
    TalentBuildComponent,
    MatchLogComponent,
    HotslogsNewsComponent,
    CheckboxListComponent,
    DropdownComponent,
    SlickDirective,
  ],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    materialModules,
  ],
  exports: [
    RadXpChartComponent,
    RadChartComponent,
    TalentBuildComponent,
    MatchLogComponent,
    HotslogsNewsComponent,
    CheckboxListComponent,
    DropdownComponent,
    SlickDirective,
  ],
  providers: [
    { provide: HTTP_INTERCEPTORS, useClass: JsonHttpInterceptor, multi: true },
  ],
})
export class ElesModule { 
  /**
   *
   */
  constructor() {
    Chart.Chart.register(Chart.CategoryScale);
    Chart.Chart.register(Chart.LinearScale);
    Chart.Chart.register(Chart.PointElement);
    Chart.Chart.register(Chart.LineElement);
    Chart.Chart.register(NegativeColoredLineChart);
    Chart.Chart.register(Chart.Filler);
    Chart.Chart.register(Chart.Tooltip);
    Chart.Chart.register(Chart.BarController);
    Chart.Chart.register(Chart.BarElement);
  }
}
