using SSD1306;
using SSD1306.Fonts;
using SSD1306.I2CPI;
using System;
using System.Collections.Generic;
using System.Net;
using System.Numerics;
using System.Text;

namespace Fermenter.Devices
{
    public sealed class I2CBus : IDisposable
    {
        private readonly I2CBusPI bus;

        private SSD1306Driver ssd1306;
        public IDisplayDriver SSD1306DisplayDriver => ssd1306 ?? (ssd1306 = new SSD1306Driver(bus));

        public I2CBus()
        {
            bus = new I2CBusPI(1);
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

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~I2CBus()
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

        private class SSD1306Driver : IDisplayDriver, IDisposable
        {
            private readonly SSD1306.Display display;

            private readonly IFont font = new AdafruitSinglePageFont();

            public SSD1306Driver(I2CBusPI bus)
            {
                var i2cDevice = new I2CDevicePI(bus, SSD1306.Display.DefaultI2CAddress);
                display = new SSD1306.Display(i2cDevice, 128, 64);
                display.Init();
            }

            public double MaxValues => 117;

            private string Format(double? value, string format, string outsideBounds)
            {
                return value == null || value > 99 || value < 0 ? outsideBounds : value?.ToString(format);
            }

            public void Draw()
            {
                var degree = (char)251;
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
                var lastLineEndStartColumn = 25 - lastLineEnd.Length;
                display.WriteLineBuff(font, lastLineEnd, 7, (uint)lastLineEndStartColumn);

                // draw diagram
                int xstart = 10;
                int xend = 127;
                int ystart = 8;
                int yend = 55;

                // start with a box
                display.DrawPolygon(false, new Vector2(xend, ystart), new Vector2(xstart, ystart), new Vector2(xstart, yend), new Vector2(xend, yend));
                if (diagramData != null)
                {
                    var yDifference = yend - ystart;
                    var displayTempDifference = diagramData.YMax - diagramData.YMin;
                    var tempDifferencePerPixel = displayTempDifference / yDifference;

                    for (int i = 0; i < diagramData.Values.Length; i++)
                    {
                        var x = xend - i - diagramData.EmptyValuesAtStart;
                        if (x <= xstart)
                            break;
                        var value = diagramData.Values[i];
                        var y = yend - (int)Math.Round((value - diagramData.YMin) / tempDifferencePerPixel);
                        if (y <= ystart || y >= yend)
                            continue;

                        display.DrawPixel(x, y);
                    }
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
                this.ipAddress = ipAddress.ToString();
            }

            private double? setTemp;
            public void WriteSetTemp(double setTemp)
            {
                this.setTemp = setTemp;
            }

            #region IDisposable Support
            private bool disposedValue = false; // To detect redundant calls

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        display.DisplayOff();
                    }

                    // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                    // TODO: set large fields to null.

                    disposedValue = true;
                }
            }

            // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
            // ~SSD1306Driver()
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
}
