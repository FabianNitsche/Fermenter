using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Text;

namespace Fermenter.ViewModel
{
    public class DisplayViewModel 
    {
        public IObservable<double> SetTemperature { get; }

        public IObservable<double> CurrentTemperature { get; }

        public IObservable<TimeSpan> PlottingTimeSpan { get; }

        public IObservable<double> PlottingBand { get; }

        public IObservable<IPAddress> IpAdress { get; }

        public DisplayViewModel(IObservable<double> setTemperature, IObservable<double> currentTemperature)
        {
            SetTemperature = setTemperature;
            CurrentTemperature = currentTemperature;
            PlottingTimeSpan = Observable.Return(TimeSpan.FromSeconds(10));
            PlottingBand = Observable.Return(2.0);
            IpAdress = Observable.Return(Dns.GetHostEntry(Dns.GetHostName()).AddressList
                .FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork));
        }
    }
}
