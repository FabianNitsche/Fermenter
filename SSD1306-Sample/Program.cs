using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using SSD1306;
using SSD1306.I2CPI;
using SSD1306.Fonts;
using BoschDevices;
using System.Reactive.Linq;
using ReactiveGpio;

using MMALSharp;
using MMALSharp.Handlers;
using MMALSharp.Common;

namespace SSD1306_Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            MMALCamera cam = MMALCamera.Instance;

            // Create observable that will generate an incrementing number every second
            var observable = Observable.Generate(1, x => true, x => x + 1, x => x, x => TimeSpan.FromSeconds(1));

            var port = OutputPort.Create(17, OutputPort.InitialValue.Low).Result;

            // Write true whenever the number is even and odd when the number is odd
            using (var imgCaptureHandler = new ImageStreamCaptureHandler("/home/pi/images/", "jpg"))
            using (observable.Select(x => x % 2 == 0).Subscribe(port))
            using (var i2cBus = new I2CBusPI("/dev/i2c-1"))
            {
                var takePictureTask = cam.TakePicture(imgCaptureHandler, MMALEncoding.JPEG, MMALEncoding.I420);

                var i2cDevice = new I2CDevicePI(i2cBus, Display.DefaultI2CAddress);

                var sensor = new BME280Sensor(i2cBus, 1014);

                var display = new SSD1306.Display(i2cDevice, 128, 64);
                display.Init();

                var dfont = new AdafruitSinglePageFont();

                for (int i = 0; i < 100; i++)
                {
                    display.WriteLineBuff(dfont, $"Temperature: {sensor.ReadTemperature().Result} °C", $"Pressure: {sensor.ReadPressure().Result} Pa", $"Humidity: {sensor.ReadHumidity().Result} %", $"Altitude: {sensor.ReadAltitude().Result} m", "Line 5", "Line 6", "Line 7", "Line 8");
                    display.DisplayUpdate();
                }

                //for (int i = 0; i < 100; i++)
                //    display.DrawPixel(i, i);

                takePictureTask.Wait();

                display.ClearDisplay();

            }
            // releasing relay
            port.Write(true);
            // Cleanup disposes all unmanaged resources and unloads Broadcom library. To be called when no more processing is to be done
            // on the camera.
            cam.Cleanup();
        }
    }
}
