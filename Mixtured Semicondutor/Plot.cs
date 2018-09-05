using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;

namespace Mixtured_Semicondutor
{
    class Plot
    {
        public Plot(List<double> x, List<double> y, Chart chart, int series)
        {

            chart.Series.Add(series.ToString());
            chart.Series[series].ChartType = SeriesChartType.Line;
            chart.Legends.Clear();
            for (int j = 0; j < y.Count; j++)
            {
                if (!((double.IsInfinity(y[j])) || ((double.IsInfinity(x[j])))))
                    chart.Series[series].Points.AddXY(x[j], y[j]);
            }

        }
    }
}
