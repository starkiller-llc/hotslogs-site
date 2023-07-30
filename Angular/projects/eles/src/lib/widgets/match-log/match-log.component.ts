import { Component, OnInit, ViewChild, ElementRef, AfterViewInit, OnDestroy, Input, OnChanges, SimpleChanges, Output, EventEmitter } from '@angular/core';
import { MatchDetailsService } from '../../match-details.service';
import { Observable, of } from 'rxjs';
import { MatchDetails } from '../../models/match-details';
import { catchError, tap } from 'rxjs/operators';
import * as Chart from 'chart.js';

@Component({
  selector: 'lib-match-log',
  templateUrl: './match-log.component.html',
  styleUrls: ['./match-log.component.scss']
})
export class MatchLogComponent implements OnInit, AfterViewInit, OnDestroy, OnChanges {
  @Input() rid: string;
  @Output() replayNotFound = new EventEmitter<void>();
  @ViewChild('cnvs', { static: false }) cnvs: ElementRef;
  ctx_chart: any;

  heroImages: { [key: string]: HTMLImageElement } = {};
  eventImages: { [key: string]: HTMLImageElement } = {};
  chart: any;
  matchDetails$: Observable<MatchDetails>;
  matchDetails: MatchDetails;

  constructor(private svc: MatchDetailsService) { }

  ngOnInit(): void {
    this.fetch();
  }

  ngOnDestroy(): void {
    if (this.chart) {
      this.chart.destroy();
    }
  }

  ngAfterViewInit(): void {
    if (!this.matchDetails) {
      return;
    }

    this.setup2();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes.rid) {
      this.fetch();
    }
  }

  private fetch() {
    if (isNaN(+this.rid)) {
      return;
    }
    this.matchDetails$ = this.svc.get(+this.rid).pipe(
      tap(r => this.setup(r)),
      catchError(err => {
        this.replayNotFound.emit();
        return of(null);
      })
    );
  }

  setup(matchDetails: MatchDetails): boolean {
    this.matchDetails = matchDetails;
    if (!this.cnvs) {
      return;
    }

    this.setup2();
  }

  normalize(s: string, keepCase = true) {
    const rc = s.normalize('NFD').replace(/[\u0300-\u036f]/g, '').trim();
    return keepCase ? rc : rc.toLowerCase();
  }
  
  setup2(): boolean {
    const matchDetails = this.matchDetails;
    const el = this.cnvs.nativeElement;
    this.ctx_chart = el.getContext('2d');

    matchDetails.Heroes.forEach(x => {
      var img = new Image();
      var norm = this.normalize(x).replace(/\W/g, '');
      img.src = `/Images/Heroes/Portraits/${norm}.png`;
      img.width = 25;
      img.height = 25;
      this.heroImages[`hero${x}`] = img;
    });

    matchDetails.TeamObjectiveNames.forEach((x, i) => {
      var img = new Image();
      img.src = matchDetails.TeamObjectiveImages[i];
      this.eventImages[x] = img;
    });

    const pointStyleEvents = matchDetails.TeamObjectiveStyles.map(x => this.eventImages[x]);
    const pointStyleDeaths = matchDetails.HeroDeathStyles.map(x => this.heroImages[x]);
    const dataPoints1 = matchDetails.XValues.map((x, i) => ({ x, y: matchDetails.YValues[i] }));
    const dataPoints2 = matchDetails.XHeroDeaths.map((x, i) => ({ x, y: matchDetails.YHeroDeaths[i] }));
    const dataPoints3 = matchDetails.XDiff.map((x, i) => ({ x, y: matchDetails.YDiff[i] }));
    const data = {
      datasets: [{
        type: "line",
        fill: false,
        showLine: false,
        pointHitRadius: 15,
        pointStyle: pointStyleEvents,
        data: dataPoints1,
      },
      {
        type: "line",
        fill: false,
        showLine: false,
        pointHitRadius: 10,
        pointStyle: pointStyleDeaths,
        data: dataPoints2,
      },
      {
        type: "NegativeColoredLine",
        pointHoverBorderColor: "#337ab7",
        pointHoverBackgroundColor: "#FFFFFF",
        borderWidth: 5,
        pointRadius: 2,
        pointHoverRadius: 5,
        pointBorderWidth: 0,
        pointHoverBorderWidth: 1,
        pointHitRadius: 7,
        label: "XP Difference",
        data: dataPoints3,
        fill: { value: 0 },
      }]
    };

    const options = {
      maintainAspectRatio: false,
      title: { display: false },
      legend: { display: false },
      plugins: {
        tooltip: {
          displayColors: false,
          bodyFontSize: 14,
          callbacks: {
            title: tooltipItems => {
              var seconds;

              if (tooltipItems[0].datasetIndex === 0) {
                var matchEventsTimers = matchDetails.MatchEventTimers;
                seconds = matchEventsTimers[tooltipItems[0].dataIndex];
              } else if (tooltipItems[0].datasetIndex === 1) {
                var deathTimers = matchDetails.DeathTimers;
                seconds = deathTimers[tooltipItems[0].dataIndex];
              } else
                seconds = tooltipItems[0].raw.x;

              return this.secondsToMMSS(seconds);
            },
            label: tooltipItem => {
              if (tooltipItem.datasetIndex === 0) {
                var eventLabels = matchDetails.EventLabels;
                return eventLabels[tooltipItem.dataIndex];
              } else if (tooltipItem.datasetIndex === 2) {
                var datasetLabel = tooltipItem.dataset.label || "";
                return datasetLabel + ": " + tooltipItem.formattedValue;
              } else {
                var deathLabels = matchDetails.DeathLabels;
                return deathLabels[tooltipItem.dataIndex];
              }
            }
          }
        },
      },
      hover: { mode: "nearest" as const },
      scales: {
        x: {
          type: "linear",
          position: "bottom",
          gridLines: { display: true },
          ticks: {
            stepSize: 120,
            max: matchDetails.MaxXpDiffTick,
            callback: (value: number, index: any, values: any) => {
              if (value % 120 === 0)
                return this.secondsToMMSS(value);
              else
                return "";
            }
          }
        },
        y: {
          gridLines: { display: true },
          ticks: { min: -matchDetails.MaxXpDifference, max: matchDetails.MaxXpDifference }
        }
      }
    };

    this.chart = new Chart.Chart(this.ctx_chart, {
      type: 'line',
      data: data as any,
      options: options as any,
    });
    return true;
  }

  secondsToMMSS(value) {
    var sec = Number(value);
    var m = Math.floor(sec / 60);
    var s = Math.floor(sec % 60);

    return m + ':' + (s < 10 ? '0' + s : s);
  }
}
