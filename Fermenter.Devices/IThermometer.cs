using System;
using System.Collections.Generic;
using System.Text;

namespace Fermenter.Devices
{
    public interface IThermometer
    {
        IObservable<double> CurrentTemperature { get; }
    }
}
