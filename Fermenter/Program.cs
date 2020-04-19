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

namespace Fermenter
{
    public class Program
    {
        private static ManualResetEvent quitEvent = new ManualResetEvent(false);
        
        public static void Main(string[] args)
        {
            Console.CancelKeyPress += (sender, eArgs) => {
                quitEvent.Set();
                eArgs.Cancel = true;
            };
            var pollingTrigger = Observable.Interval(TimeSpan.FromMilliseconds(500)).Select(_ => Unit.Default);

            var gpioDriver = new FileDriver();

            using (var setTemperatureDecrementButton = GpioTrigger.Create(gpioDriver, 24).Result)
            using (var setTemperatureIncrementButton = GpioTrigger.Create(gpioDriver, 23).Result)
            using (var bus = new I2CBus(pollingTrigger))
            {
                var thermometer = bus.Thermometer;
                var display = bus.SSD1306DisplayDriver;
                var setTemperatureController = new SetTemperatureController(setTemperatureIncrementButton.Where(b => b == true).Select(_ => Unit.Default), setTemperatureDecrementButton.Where(b => b == true).Select(_ => Unit.Default), Observable.Return(1.0));

                var heaterController = new HeaterBandController(setTemperatureController.SetTemperature, Observable.Return(1.0), TimeSpan.FromSeconds(5), thermometer.CurrentTemperature, Observable.Return(true));

                using (var relaySwitch = GpioSwitch.Create(gpioDriver, 17, true, heaterController.HeaterOn))
                {
                    var displayViewModel = new DisplayViewModel(setTemperatureController.SetTemperature, thermometer.CurrentTemperature);
                    var displayView = new DisplayView(new SimpleHistory(thermometer.CurrentTemperature), display, displayViewModel);

                    quitEvent.WaitOne();
                }
            }
        }
    }
}
