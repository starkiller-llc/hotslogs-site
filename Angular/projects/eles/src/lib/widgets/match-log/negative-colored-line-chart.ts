import * as Chart from 'chart.js';

export class NegativeColoredLineChart extends Chart.LineController {
  static id = 'NegativeColoredLine';
  static defaults = { ...Chart.LineController.defaults };

  update(mode) {
    // get the min and max values
    var data = this.chart.data.datasets[2].data.map((z: Chart.ScatterDataPoint) => z.y);

    // this.chart.data.datasets[2].data.forEach((item, i, arr) => data.push(item.y));

    var min = Math.min(...data);
    var max = Math.max(...data);

    var meta = this.getMeta();
    var yScale = this.getScaleForId(meta.yAxisID);

    // figure out the pixels for these and the value 0
    var top = Math.round(yScale.getPixelForValue(max, 0));
    var zero = Math.round(yScale.getPixelForValue(0, 0));
    var bottom = Math.round(yScale.getPixelForValue(min, 0));

    // build a gradient that switches color at the 0 point
    var ctx = this.chart.ctx;
    var ratio = (bottom - top) !== 0 ? Math.min((zero - top) / (bottom - top), 1) : 0;

    var bgGradient = ctx.createLinearGradient(0, top, 0, bottom);
    bgGradient.addColorStop(0, 'rgba(51, 122, 183, 0.6)');
    bgGradient.addColorStop(ratio, 'rgba(51, 122, 183, 0.6)');
    bgGradient.addColorStop(ratio, 'rgba(169, 68, 66, 0.6)');
    bgGradient.addColorStop(1, 'rgba(169, 68, 66, 0.6)');
    this.chart.data.datasets[2].backgroundColor = bgGradient;

    var lineGradient = ctx.createLinearGradient(0, top, 0, bottom);
    // var lineGradient = ctx.createLinearGradient(0, 0, 200, 200);
    lineGradient.addColorStop(0, 'rgba(51, 122, 183, 1)');
    lineGradient.addColorStop(ratio, 'rgba(51, 122, 183, 1)');
    lineGradient.addColorStop(ratio, 'rgba(169, 68, 66, 1)');
    lineGradient.addColorStop(1, 'rgba(169, 68, 66, 1)');
    this.chart.data.datasets[2].borderColor = lineGradient;

    return super.update(mode);
  }
}
