using SSD1306;
using SSD1306.Fonts;
using SSD1306.I2CPI;
using System;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Reactive;
using System.Reactive.Linq;

namespace Fermenter.Devices
{
    public sealed class I2CBus : IDisposable
    {
        private readonly I2CBusPI bus;

        private readonly IObservable<Unit> pollingTrigger;

        private SSD1306Driver ssd1306;
        public IDisplayDriver SSD1306DisplayDriver => ssd1306 ?? (ssd1306 = new SSD1306Driver(bus));

        private BME280Sensor bme280;

        public IThermometer Thermometer => bme280 ?? (bme280 = new BME280Sensor(bus, pollingTrigger));

        public I2CBus(IObservable<Unit> pollingTrigger)
        {
            bus = new I2CBusPI(1);
            this.pollingTrigger = pollingTrigger;
        }


        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    ssd1306.Dispose();
                    bus.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion

        public sealed class SSD1306Driver : IDisplayDriver, IDisposable
        {
            private readonly SSD1306.Display display;

            private readonly IFont font = new AdafruitSinglePageFont();

            public SSD1306Driver(I2CBusPI bus)
            {
                var i2cDevice = new I2CDevicePI(bus, SSD1306.Display.DefaultI2CAddress);
                display = new SSD1306.Display(i2cDevice, 128, 64, true);
                display.Init();
            }

            private const int XStart = 13;
            private const int XEnd = 127;
            public int MaxValues => XEnd - XStart;

            private string Format(double? value, string format, string outsideBounds)
            {
                return value == null || value > 99 || value < 0 ? outsideBounds : value?.ToString(format);
            }

            public void Draw()
            {
                display.ClearBuffer();

                var degree = (char)248;
                var firstLine = $"Set {Format(setTemp, "00", "**")} {degree}C Is {Format(currentTemp, "00.0", "**.*")} {degree}C";
                display.WriteLineBuff(font, firstLine, 0, 0);

                var secondLine = Format(diagramData?.YMax, "00", "**");
                display.WriteLineBuff(font, secondLine, 1, 0);

                var seventhLine = Format(diagramData?.YMin, "00", "**");
                display.WriteLineBuff(font, seventhLine, 6, 0);

                var timeMinimum = "-***";
                if (diagramData != null)
                {
                    if (diagramData.DiagramTimeSpan.TotalDays >= 2)
                        timeMinimum = (-diagramData.DiagramTimeSpan.TotalDays).ToString("#.#") + "d";
                    else if (diagramData.DiagramTimeSpan.TotalHours >= 2)
                        timeMinimum = (-diagramData.DiagramTimeSpan.TotalHours).ToString("#.#") + "h";
                    else
                        timeMinimum = (-diagramData.DiagramTimeSpan.TotalMinutes).ToString("#") + "min";
                }

                var lastLineStart = timeMinimum;
                display.WriteLineBuff(font, lastLineStart, 7, 0);

                var lastLineEnd = ipAddress == null ? "*.*.*.*" : ipAddress;
                if (lastLineEnd.Length > 16) lastLineEnd = lastLineEnd.Substring(lastLineEnd.Length - 16);
                var lastLineEndStartColumn = 128 - lastLineEnd.Length * 7;
                display.WriteLineBuff(font, lastLineEnd, 7, (uint)lastLineEndStartColumn);

                // draw diagram
                int ystart = 8;
                int yend = 55;

                // start with a box
                display.DrawPolygon(false, new Vector2(XEnd, ystart), new Vector2(XStart, ystart), new Vector2(XStart, yend), new Vector2(XEnd, yend));
                if (diagramData != null)
                {
                    var yDifference = yend - ystart;
                    var displayTempDifference = diagramData.YMax - diagramData.YMin;
                    var tempDifferencePerPixel = displayTempDifference / yDifference;

                    var points = new Vector2[diagramData.Values.Length];

                    for (int i = 0; i < diagramData.Values.Length; i++)
                    {
                        var x = XEnd - i - diagramData.EmptyValuesAtStart;
                        if (x <= XStart)
                            break;
                        var value = diagramData.Values[i];
                        var y = yend - (int)Math.Round((value - diagramData.YMin) / tempDifferencePerPixel);
                        // this is not really good.
                        if (y < ystart) y = ystart;
                        else if (y > yend) y = yend;

                        points[i] = new Vector2(x, y);
                    }
                    display.DrawPolygon(false, points);
                }

                diagramData = null;
                currentTemp = null;
                ipAddress = null;
                setTemp = null;

                display.DisplayUpdate();
            }

            private DiagramData diagramData;
            public void DrawDiagram(DiagramData diagramData)
            {
                this.diagramData = diagramData;
            }

            private double? currentTemp;
            public void WriteCurrentTemp(double currentTemp)
            {
                this.currentTemp = currentTemp;
            }

            private string ipAddress;
            public void WriteIp(IPAddress ipAddress)
            {
                this.ipAddress = ipAddress?.ToString();
            }

            private double? setTemp;
            public void WriteSetTemp(double setTemp)
            {
                this.setTemp = setTemp;
            }

            #region IDisposable Support
            private bool disposedValue = false; // To detect redundant calls

            private void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        display.DisplayOff();
                    }
                    disposedValue = true;
                }
            }

            public void Dispose()
            {
                Dispose(true);
            }
            #endregion
        }

        public sealed class BME280Sensor : IThermometer
        {
            public BME280Sensor(I2CBusPI bus, IObservable<Unit> pollingTrigger)
            {
                var sensor = new BoschDevices.BME280Sensor(bus, 1014);
                var delay = TimeSpan.FromMilliseconds(500);
                CurrentTemperature = pollingTrigger.Buffer(delay).Select(_ => (double)sensor.ReadTemperature().Result);
            }

            public IObservable<double> CurrentTemperature { get; }
        }
    }
}
