using System;
using System.Reactive.Linq;

namespace Fermenter.Controller
{
    public class HeaterBandController
    {
        public IObservable<bool> HeaterOn { get; }

        public HeaterBandController(IObservable<double> setTemp, IObservable<double> bandTemp, TimeSpan maxHeatingTimeWithoutControl, IObservable<double> currentTemperature, IObservable<bool> heaterMasterSwitch)
        {
            var safeTemperature = currentTemperature.Timeout(maxHeatingTimeWithoutControl);
            
            var temperatureAboveBand = Observable.CombineLatest(setTemp, bandTemp, safeTemperature, (set, band, current) => current > set + band);
            var temperatureBelowBand = Observable.CombineLatest(setTemp, bandTemp, safeTemperature, (set, band, current) => current < set - band);

            var turnHeaterOff = temperatureAboveBand.Where(above => above == true).Select(_ => false);
            var turnHeaterOn = temperatureBelowBand.Where(below => below == true).Select(_ => true);

            var heaterControllerSwitch = Observable.Merge(turnHeaterOn, turnHeaterOff).DistinctUntilChanged();

            HeaterOn = Observable.CombineLatest(heaterMasterSwitch, heaterControllerSwitch, (heaterMasterState, heaterControllerState) => heaterMasterState && heaterControllerState).OnErrorResumeNext(Observable.Return(false)); // turn off heater on error
        }
    }
}
