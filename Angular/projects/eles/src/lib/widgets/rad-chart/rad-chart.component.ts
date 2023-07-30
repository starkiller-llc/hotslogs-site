import { DOCUMENT } from '@angular/common';
import { AfterViewInit, Component, ElementRef, Inject, Input, OnChanges, OnInit, SimpleChanges, ViewChild } from '@angular/core';
import * as Chart from 'chart.js';
import * as _ from 'lodash-es';
import { dateParser } from '../../date-parser';

type RadChart =
  | RadNumberChart
  | RadDateChart;

interface RadBaseChart<T> {
  Name?: string;
  Data: RadData<T>[];
  MinY?: number;
  MaxY?: number;
  SuggestedMinY?: number;
  SuggestedMaxY?: number;
  YType: 'number' | 'percent';
  YTitle: string;
}

interface RadNumberChart extends RadBaseChart<number> {
  Type: 'number';
}

interface RadDateChart extends RadBaseChart<Date> {
  Type: 'date';
}

interface RadData<T> {
  X: T;
  GamesPlayed?: number;
  WinPercent: number;
}

@Component({
  selector: 'lib-rad-chart',
  templateUrl: './rad-chart.component.html',
  styleUrls: ['./rad-chart.component.scss']
})
export class RadChartComponent implements OnInit, OnChanges, AfterViewInit {
  private ctx_chart: any;
  private chart: any;

  private commonLineOptions = {
    borderColor: '#f9a319',
    borderWidth: 1,
    pointRadius: 5,
    pointBackgroundColor: '#3d3d3d',
    pointHoverRadius: 6,
    pointHitRadius: 10,
    pointBorderWidth: 2,
    pointHoverBackgroundColor: '#f9a319',
    pointHoverBorderColor: 'white',
    pointHoverBorderWidth: 2,
    tension: 0.2,
  };

  private commonOptions = {
    maintainAspectRatio: false,
    // animation: {
    //   duration: 0,
    // },
    plugins: {
      tooltip: {
        displayColors: false,
        backgroundColor: 'rgb(249, 163, 25)',
        bodyColor: 'black',
        callbacks: {
          title: () => '',
        },
      },
    },
    scales: {
      y: {
        title: {
          display: true,
          color: 'white',
          font: {
            size: 16,
          },
        },
        color: '#919191',
        grid: {
          color: '#636363',
        },
        ticks: {
          color: 'white',
          callback: x => `${(x * 100).toFixed(1)} %`,
        }
      },
      x: {
        title: {
          display: false,
          color: 'white',
          font: {
            size: 16,
          },
        },
        color: '#919191',
        grid: {
          color: '#636363',
        },
        ticks: {
          color: 'white',
        }
      }
    }
  };

  private dataGeneric: RadChart;

  @ViewChild('cnvs', { static: false }) cnvs: ElementRef;

  @Input() jsondata: any;

  constructor(@Inject(DOCUMENT) private document: Document) { }

  ngOnInit(): void {
    return;
  }

  ngOnChanges(changes: SimpleChanges): void {
    this.trySetupChart();
  }

  ngAfterViewInit(): void {
    this.trySetupChart();
  }

  private trySetupChart() {
    if (!this.jsondata) {
      return;
    }

    if (!this.cnvs?.nativeElement) {
      return;
    }

    this.dataGeneric = JSON.parse(this.jsondata, dateParser);
    this.setupChart(this.dataGeneric);
  }

  setupChart(srcChart: RadChart) {
    const el = this.cnvs?.nativeElement;
    if (!el) {
      return;
    }

    this.ctx_chart = el.getContext('2d');

    const srcData = srcChart.Data;
    const datasetsData: number[] = srcChart.Data.map(r => r.WinPercent);

    let labels: string[];
    switch (srcChart.Type) {
      case 'date':
        labels = srcChart.Data.map(r => r.X.toLocaleDateString('en-US'));
        break;
      case 'number':
        labels = srcChart.Data.map(r => r.X.toString());
        break;
    }

    const ytitle = srcChart.YTitle;

    const data = {
      labels,
      datasets: [{
        data: datasetsData,
        ...this.commonLineOptions,
        label: ytitle,
      }],
    }

    var num = x => x.toFixed();
    var pct = x => `${(x * 100).toFixed(1)}%`;
    var cbk = srcChart.YType === 'number' ? num : pct;

    let options = _.merge(this.commonOptions, {
      scales: {
        y: {
          title: {
            text: ytitle,
          },
        },
      },
      plugins: {
        tooltip: {
          callbacks: {
            label: (c) => {
              const d = srcData[c.dataIndex];
              const l1 = `${ytitle}: ${cbk(d.WinPercent)}`;
              const l2 = (d.GamesPlayed || d.GamesPlayed === 0) && `Games Played: ${d.GamesPlayed}`;
              return [l1, l2].filter(r => r);
            },
          }
        }
      },
    });

    if (srcChart.MinY || srcChart.MinY === 0) {
      options = _.merge(options, {
        scales: {
          y: {
            min: srcChart.MinY,
          },
        },
      });
    }

    if (srcChart.MaxY || srcChart.MaxY === 0) {
      options = _.merge(options, {
        scales: {
          y: {
            max: srcChart.MaxY,
          },
        },
      });
    }

    if (srcChart.SuggestedMinY || srcChart.SuggestedMinY === 0) {
      options = _.merge(options, {
        scales: {
          y: {
            suggestedMin: srcChart.SuggestedMinY,
            ticks: {
              color: context => {
                const r = context.tick.value;
                if (r > (srcChart.SuggestedMaxY || 1)) return 'rgb(125, 209, 0)';
                if (r < (srcChart.SuggestedMinY || 0)) return 'red';
                return 'white';
              },
            }
          },
        },
      });
    }

    if (srcChart.SuggestedMaxY || srcChart.SuggestedMaxY === 0) {
      options = _.merge(options, {
        scales: {
          y: {
            suggestedMax: srcChart.SuggestedMaxY,
          },
        },
      });
    }

    if (srcChart.Name) {
      options = _.merge(options, {
        scales: {
          x: {
            title: {
              display: true,
              text: srcChart.Name,
            },
          },
        },
      });
    }

    if (srcChart.YType === 'number') {
      options = _.merge(options, {
        scales: {
          y: {
            ticks: {
              callback: x => x.toFixed(0),
            }
          }
        }
      });
    }

    // let pos = document.documentElement.scrollTop;
    if (this.chart) {
      this.chart.destroy();
    }

    this.chart = new Chart.Chart(this.ctx_chart, {
      type: 'line',
      data,
      options,
    });

    // this.document.documentElement.scrollTop = pos;
  }
}
