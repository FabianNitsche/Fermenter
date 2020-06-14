using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Reactive.Linq;
using Fermenter.Devices;
using System.Reactive;
using Fermenter.Controller;
using ReactiveGpio.Drivers;
using Fermenter.ViewModel;
using MMALSharp;
using MMALSharp.Handlers;
using MMALSharp.Common;
using System.Threading.Tasks;
using MMALSharp.Native;
using SSD1306.I2CPI;
using BoschDevices;

namespace Fermenter
{
    public class Program
    {
        private static ManualResetEvent quitEvent = new ManualResetEvent(false);
        
        public static void Main(string[] args)
        {
            // light gpio = 25 high = on.

            Console.CancelKeyPress += (sender, eArgs) => {
                quitEvent.Set();
                eArgs.Cancel = true;
            };
            var pollingTrigger = Observable.Interval(TimeSpan.FromMilliseconds(1000)).Select(_ => Unit.Default);

            var gpioDriver = new FileDriver();

            using (var setTemperatureDecrementButton = GpioTrigger.Create(gpioDriver, 24).Result)
            using (var setTemperatureIncrementButton = GpioTrigger.Create(gpioDriver, 23).Result)
            using (var thermometerBus = new I2CBusPI(3))
            using (var displayBus = new I2CBusPI(1))
            using (var display = new I2CBus.SSD1306Driver(displayBus))
            {
                var thermometer = new I2CBus.BME280Sensor(thermometerBus, pollingTrigger);
                var setTemperatureController = new SetTemperatureController(setTemperatureIncrementButton.Where(b => b == true).Select(_ => Unit.Default), setTemperatureDecrementButton.Where(b => b == true).Select(_ => Unit.Default), Observable.Return(1.0));

                var heaterController = new HeaterBandController(setTemperatureController.SetTemperature, Observable.Return(1.0), TimeSpan.FromSeconds(5), thermometer.CurrentTemperature, Observable.Return(true));

                using (var relaySwitch = GpioSwitch.Create(gpioDriver, 4, false, heaterController.HeaterOn))
                {
                    var displayViewModel = new DisplayViewModel(setTemperatureController.SetTemperature, thermometer.CurrentTemperature);
                    var displayView = new DisplayView(new SimpleHistory(thermometer.CurrentTemperature), display, displayViewModel);

                    Task.Delay(5000).Wait();

                    quitEvent.WaitOne();
                }
            }

        }

        public static async Task TakePicture()
        {
            // Singleton initialized lazily. Reference once in your application.
            MMALCamera cam = MMALCamera.Instance;
            MMALCameraConfig.Rotation = 180;
            MMALCameraConfig.ShutterSpeed = 6000000;
            MMALCameraConfig.ISO = 800;

            using (var imgCaptureHandler = new ImageStreamCaptureHandler("/home/pi/images/", "jpg"))
            {
                await cam.TakePicture(imgCaptureHandler, MMALEncoding.JPEG, MMALEncoding.I420);
            }

            // Cleanup disposes all unmanaged resources and unloads Broadcom library. To be called when no more processing is to be done
            // on the camera.
            cam.Cleanup();
        }

        public static async Task TakeVideo()
        {
            // Singleton initialized lazily. Reference once in your application.
            MMALCamera cam = MMALCamera.Instance;

            using (var vidCaptureHandler = new VideoStreamCaptureHandler("/home/pi/videos/", "avi"))
            {
                var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

                await cam.TakeVideo(vidCaptureHandler, cts.Token);
            }

            // Cleanup disposes all unmanaged resources and unloads Broadcom library. To be called when no more processing is to be done
            // on the camera.
            cam.Cleanup();
        }
    }
}
