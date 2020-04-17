using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;

namespace Fermenter.Devices
{
    public class Display : IDisposable
    {
        private readonly IHistory<double> temperatureHistory;

        private readonly IDisplayDriver display;

        private readonly IDisposable subscription;

        public Display(IHistory<double> temperatureHistory, IDisplayDriver display, IObservable<double> setTemperature, IObservable<double> currentTemperature, IObservable<TimeSpan> plottingTimeSpan, IObservable<double> plottingBand, IObservable<IPAddress> ipAdress)
        {
            this.temperatureHistory = temperatureHistory;
            this.display = display;

            var delay = TimeSpan.FromMilliseconds(500);

            subscription = Observable.CombineLatest(setTemperature, currentTemperature, plottingTimeSpan, plottingBand, ipAdress, CreatePlotData).Buffer(delay).Subscribe(plotData => Plot(plotData.Last()));
        }

        private PlotData CreatePlotData(double setTemperature, double currentTemperature, TimeSpan plottingTimeSpan, double plottingBand, IPAddress ipAddress) => new PlotData(setTemperature, currentTemperature, plottingTimeSpan, plottingBand, ipAddress);


        private class PlotData
        {
            public double SetTemperature { get; }

            public double CurrentTemperature { get; }

            public TimeSpan PlottingTimeSpan { get; }

            public double PlottingBand { get; }

            public IPAddress IPAddress { get; }

            public PlotData(double setTemperature, double currentTemperature, TimeSpan plottingTimeSpan, double plottingBand, IPAddress ipAddress)
            {
                SetTemperature = setTemperature;
                CurrentTemperature = currentTemperature;
                PlottingTimeSpan = plottingTimeSpan;
                PlottingBand = plottingBand;
                IPAddress = ipAddress;
            }
        }

        private Unit Plot(PlotData plotData)
        {
            var setTemperature = plotData.SetTemperature;
            var currentTemperature = plotData.CurrentTemperature;
            var ipAddress = plotData.IPAddress;
            var plottingBand = plotData.PlottingBand;
            var plottingTimeSpan = plotData.PlottingTimeSpan;

            display.WriteSetTemp(setTemperature);
            display.WriteCurrentTemp(currentTemperature);
            display.WriteIp(ipAddress);

            var yMin = setTemperature - plottingBand;
            var yMax = setTemperature + plottingBand;

            var temperaturesInPlottingRange = temperatureHistory.GetValuesBeforeNow(plottingTimeSpan, out var now);
            var bucketSizeInTicks = (long)(plottingTimeSpan.Ticks / display.MaxValues);
            var indexedBuckets = temperaturesInPlottingRange.GroupBy(t => (now - t.Time).Ticks / bucketSizeInTicks).ToDictionary(g => g.Key, g => g.Select(v => v.Value).ToArray());
            if (indexedBuckets.Any())
            {
                var maxIndex = indexedBuckets.Keys.Max();
                var minIndex = indexedBuckets.Keys.Min();
                var values = new double[maxIndex - minIndex + 1];
                values[0] = indexedBuckets[minIndex].Average();
                var startValueOfInterpolation = values[0];
                var indicesToInterpolate = new List<int>();
                for (int i = 1; i < values.Length; i++)
                {
                    if (indexedBuckets.TryGetValue(i, out var bucket))
                    {
                        var endValueOfInterpolation = values[i] = bucket.Average();
                        if (indicesToInterpolate.Any())
                        {
                            var yDifference = endValueOfInterpolation - startValueOfInterpolation;
                            var xDifference = indicesToInterpolate.Count + 1;
                            var gain = yDifference / xDifference;
                            for (int j = 0; j < indicesToInterpolate.Count; j++)
                            {
                                var index = indicesToInterpolate[j];
                                values[index] = startValueOfInterpolation + (j + 1) * gain;
                            }
                        }
                        startValueOfInterpolation = endValueOfInterpolation;
                        indicesToInterpolate.Clear();
                    }
                    else
                    {
                        indicesToInterpolate.Add(i);
                    }
                }

                display.DrawDiagram(new DiagramData(yMin, yMax, plottingTimeSpan, values, (int)minIndex));
            }
            else
                display.DrawDiagram(new DiagramData(yMin, yMax, plottingTimeSpan, new double[0], 0));

            display.Draw();

            return Unit.Default;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    subscription.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~Display()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
