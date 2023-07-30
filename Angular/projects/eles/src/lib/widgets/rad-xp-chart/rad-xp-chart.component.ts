import { AfterViewInit, Component, ElementRef, Input, OnChanges, OnInit, SimpleChanges, ViewChild } from '@angular/core';
import * as Chart from 'chart.js';
import * as _ from 'lodash-es';
import { dateParser } from '../../date-parser';

interface RadXpData {
  GameTimeMinute: number;
  WinnerMinionAndCreepXP: number;
  WinnerStructureXP: number;
  WinnerHeroXP: number;
  WinnerTrickleXP: number;
  MinionAndCreepXPTooltip: string;
  StructureXPTooltip: string;
  HeroXPTooltip: string;
  TrickleXPTooltip: string;
  LoserMinionAndCreepXP: number;
  LoserStructureXP: number;
  LoserHeroXP: number;
  LoserTrickleXP: number;
}

@Component({
  selector: 'lib-rad-xp-chart',
  templateUrl: './rad-xp-chart.component.html',
  styleUrls: ['./rad-xp-chart.component.scss']
})
export class RadXpChartComponent implements OnInit, OnChanges, AfterViewInit {
  private ctx_chart: any;
  private chart: any;

  private commonOptions = {
    maintainAspectRatio: false,
    responsive: true,
    animation: {
      duration: 0,
    },
    // plugins: {
    //   tooltip: {
    //     displayColors: false,
    //     backgroundColor: 'rgb(249, 163, 25)',
    //     bodyColor: 'black',
    //     callbacks: {
    //       title: () => '',
    //     },
    //   },
    // },
    scales: {
      y: {
        stacked: true,
        ticks: {
          color: 'white',
        },
      },
      x: {
        stacked: true,
        ticks: {
          color: 'white',
        },
      }
    }
  };

  private dataGeneric: RadXpData[];

  @ViewChild('cnvs', { static: false }) cnvs: ElementRef;

  @Input() jsondata: any;

  constructor() { }

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

  setupChart(srcData: RadXpData[]) {
    const el = this.cnvs?.nativeElement;
    if (!el) {
      return;
    }

    this.ctx_chart = el.getContext('2d');

    const wTrickle: number[] = srcData.map(r => r.WinnerTrickleXP);
    const wMinion: number[] = srcData.map(r => r.WinnerMinionAndCreepXP);
    const wHero: number[] = srcData.map(r => r.WinnerHeroXP);
    const wStructure: number[] = srcData.map(r => r.WinnerStructureXP);
    const lTrickle: number[] = srcData.map(r => r.LoserTrickleXP);
    const lMinion: number[] = srcData.map(r => r.LoserMinionAndCreepXP);
    const lHero: number[] = srcData.map(r => r.LoserHeroXP);
    const lStructure: number[] = srcData.map(r => r.LoserStructureXP);

    const labels = srcData.map(r => r.GameTimeMinute.toString());

    const data = {
      labels,
      datasets: [
        {
          label: 'Trickle',
          data: wTrickle,
          backgroundColor: '#e3a23b',
          stack: 'Winner',
        },
        {
          label: 'Minion',
          data: wMinion,
          backgroundColor: '#60cdcd',
          stack: 'Winner',
        },
        {
          label: 'Hero',
          data: wHero,
          backgroundColor: '#ff4500',
          stack: 'Winner',
        },
        {
          label: 'Structure',
          data: wStructure,
          backgroundColor: '#a660e8',
          stack: 'Winner',
        },
        {
          label: 'Trickle',
          data: lTrickle,
          backgroundColor: '#e3a23b',
          stack: 'Loser',
        },
        {
          label: 'Minion',
          data: lMinion,
          backgroundColor: '#60cdcd',
          stack: 'Loser',
        },
        {
          label: 'Hero',
          data: lHero,
          backgroundColor: '#ff4500',
          stack: 'Loser',
        },
        {
          label: 'Structure',
          data: lStructure,
          backgroundColor: '#a660e8',
          stack: 'Loser',
        },
      ],
    }

    let options = this.commonOptions;

    options = _.merge(options, {
      plugins: {
        tooltip: {
          enabled: false,
          external: (context) => this.createTooltip(context),
        },
      },
    });

    this.chart = new Chart.Chart(this.ctx_chart, {
      type: 'bar',
      data,
      options,
    });
  }

  createTooltip(context: any): any {
    const { chart, tooltip } = context;
    const tooltipEl = this.getOrCreateTooltip(chart);

    if (tooltip.body) {
      const root = tooltipEl.querySelector('div.rad-tt');
      const idx = tooltip.dataPoints[0].dataIndex;
      const category = tooltip.dataPoints[0].dataset.label;
      const data = this.dataGeneric[idx];
      let ttText;
      switch(category){
        case 'Trickle':
          ttText = data.TrickleXPTooltip;
          break;
        case 'Minion':
          ttText = data.MinionAndCreepXPTooltip;
          break;
        case 'Hero':
          ttText = data.HeroXPTooltip;
          break;
        case 'Structure':
          ttText = data.StructureXPTooltip;
          break;
      }

      root.innerHTML = ttText;
    }

    const { offsetLeft: positionX, offsetTop: positionY } = chart.canvas;

    // Display, position, and set styles for font
    tooltipEl.style.opacity = 1;
    tooltipEl.style.left = positionX + tooltip.caretX + 'px';
    tooltipEl.style.top = positionY + tooltip.caretY + 'px';
    tooltipEl.style.font = tooltip.options.bodyFont.string;
    tooltipEl.style.padding = tooltip.options.padding + 'px ' + tooltip.options.padding + 'px';
  }

  getOrCreateTooltip(chart: any) {
    let tooltipEl = chart.canvas.parentNode.querySelector('div');

    if (!tooltipEl) {
      tooltipEl = document.createElement('div');
      tooltipEl.style.background = 'rgba(0, 0, 0, 0.9)';
      tooltipEl.style.borderRadius = '3px';
      tooltipEl.style.color = 'white';
      tooltipEl.style.opacity = 1;
      tooltipEl.style.pointerEvents = 'none';
      tooltipEl.style.position = 'absolute';
      tooltipEl.style.transform = 'translate(-50%, 0)';
      tooltipEl.style.transition = 'all .1s ease';

      const table = document.createElement('div');
      table.style.margin = '0px';
      table.style.whiteSpace = 'nowrap';
      table.classList.add('rad-tt');

      tooltipEl.appendChild(table);
      chart.canvas.parentNode.appendChild(tooltipEl);
    }

    return tooltipEl;
  }
}
