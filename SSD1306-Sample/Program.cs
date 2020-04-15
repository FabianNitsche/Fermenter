using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using SSD1306;
using SSD1306.I2CPI;
using SSD1306.Fonts;
using BoschDevices;

namespace SSD1306_Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var i2cBus = new I2CBusPI("/dev/i2c-1"))
            {
                var i2cDevice = new I2CDevicePI(i2cBus, Display.DefaultI2CAddress);

                var sensor = new BME280Sensor(i2cBus, 1014);

                var temperatureTask = sensor.ReadTemperature();
                temperatureTask.Wait();
                var temperature = temperatureTask.Result;

                var pressureTask = sensor.ReadPressure();
                pressureTask.Wait();
                var pressure = pressureTask.Result;

                var humidityTask = sensor.ReadHumidity();
                humidityTask.Wait();
                var humidity = humidityTask.Result;

                var altitudeTask = sensor.ReadAltitude();
                altitudeTask.Wait();
                var altitude = altitudeTask.Result;

                var display = new SSD1306.Display(i2cDevice, 128, 64);
                display.Init();

                var dfont = new AdafruitSinglePageFont();

                for (int i = 0; i < 1000; i++)
                {
                    display.WriteLineBuff(dfont, $"Temperature: {sensor.ReadTemperature().Result} °C", $"Pressure: {sensor.ReadPressure().Result} Pa", $"Humidity: {sensor.ReadHumidity().Result} %", $"Altitude: {sensor.ReadAltitude().Result} m", "Line 5", "Line 6", "Line 7", "Line 8");
                    display.DisplayUpdate();
                }

                //for (int i = 0; i < 100; i++)
                //    display.DrawPixel(i, i);

                display.ClearDisplay();

            }
        }
    }
}
