using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using SSD1306;
using SSD1306.I2CPI;
using SSD1306.Fonts;

namespace SSD1306_Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var i2cBus = new I2CBusPI("/dev/i2c-1"))
            {
                var i2cDevice = new I2CDevicePI(i2cBus, Display.DefaultI2CAddress);

                var display = new SSD1306.Display(i2cDevice, 128, 64);
                display.Init();

                var dfont = new AdafruitSinglePageFont();

                display.WriteLineBuff(dfont, "Hello World 123456", "Line 2", "Line 3", "Line 4", "Line 5", "Line 6", "Line 7", "Line 8");

                for (int i = 0; i < 100; i++)
                    display.DrawPixel(i, i);

                display.DisplayUpdate();

            }
        }
    }
}
