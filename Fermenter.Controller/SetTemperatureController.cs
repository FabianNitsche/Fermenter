using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;

namespace Fermenter.Controller
{
    public class SetTemperatureController
    {
        public IObservable<double> SetTemperature { get; }

        public SetTemperatureController(IObservable<Unit> incrementTrigger, IObservable<Unit> decrementTrigger, IObservable<double> increment, double start = 33)
        {
            var incrementTemperature = incrementTrigger.Select(_ => increment.TakeLast(1)).Switch();
            var decrementTemperature = decrementTrigger.Select(_ => increment.TakeLast(1)).Switch().Select(value => -value);

            var deltaTemperature = Observable.Merge(incrementTemperature, decrementTemperature);

            SetTemperature = deltaTemperature.Aggregate(start, (previous, delta) => previous + delta);
        }
    }
}
