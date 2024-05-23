using SQLiteData;
using Plotly.NET;
using Plotly.NET.LayoutObjects;

namespace Graph
{
    public class CurrentValuesGraph
    {
        public static void GenerateGraph(string robotUuid, Dictionary<int, List<Entry>> robotData, string path)
        {
            // Generate graph
            // Documentation available at https://plotly.com/csharp/

            foreach (int axis in robotData.Keys)
            {
                DateTime[] x = robotData[axis].Select(entry => entry.Time).ToArray();
                double[] y = robotData[axis].Select(entry => entry.Value).ToArray();
                double[] thresholds = robotData[axis].Select(entry => entry.Threshold).ToArray();

                DateTime minDate = x.Min();

                Color[] markerColors = new Color[x.Length];

                for(int i = 0; i < markerColors.Length; i++)
                {
                    if (y[i] > thresholds[i])
                    {
                        markerColors[i] = Color.fromRGB(220, 20, 60);
                    }
                    else
                    {
                        markerColors[i] = Color.fromRGB(0, 144, 158);
                    }
                }

                var title = Title.init(Text: $"Robot UUID {robotUuid}: Axis {axis}");

                var layout = Layout.init<IConvertible>(Title: title, PlotBGColor: Color.fromString("#e5ecf6"), Width: 1000);

                var xAxis = LinearAxis.init<IConvertible, IConvertible, IConvertible, IConvertible, IConvertible, IConvertible>(
                    Title: Title.init("Time"),
                    ZeroLineColor: Color.fromString("#ffff"),
                    GridColor: Color.fromString("#ffff"),
                    ZeroLineWidth: 2,
                    Range: StyleParam.Range.ofMinMax<IConvertible, IConvertible>(min: minDate, max: minDate.AddHours(1)),
                    RangeSlider: RangeSlider.init());

                var yAxis = LinearAxis.init<IConvertible, IConvertible, IConvertible, IConvertible, IConvertible, IConvertible>(
                    Title: Title.init("Current (A)"),
                    ZeroLineColor: Color.fromString("#ffff"),
                    GridColor: Color.fromString("#ffff"),
                    ZeroLineWidth: 2);

                var threshold = Chart2D.Chart.Line<DateTime, double, string>(x: x, y: thresholds, ShowMarkers: true, LineColor: Color.fromRGB(137, 219, 236), Name: "threshold");

                var values = Chart2D.Chart.Line<DateTime, double, string>(x: x, y: y, ShowMarkers: true, LineColor: Color.fromRGB(0, 144, 158), Name: "peakCurrVal", MarkerColor: Color.fromColors(markerColors));

                var trace = Chart.Combine(new[] { values, threshold })
                    .WithXAxis(xAxis)
                    .WithYAxis(yAxis)
                    .WithLayout(layout);

                trace.SaveHtml($"{path}\\{robotUuid}_Axis{axis}", false); // Save graph
            }
        }
    }
}
