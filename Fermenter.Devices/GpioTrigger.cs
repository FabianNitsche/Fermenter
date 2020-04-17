using ReactiveGpio;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Fermenter.Devices
{
    public sealed class GpioTrigger : IObservable<bool>, IDisposable
    {
        public async Task<GpioTrigger> Create(IGpioDriver driver, int gpioPin)
        {
            var port = await InputPort.Create(gpioPin, GpioEdge.Both, driver);
            return new GpioTrigger(port);
        }

        public IDisposable Subscribe(IObserver<bool> observer)
        {
            return port.Subscribe(observer);
        }

        public void Dispose()
        {
            port.Dispose();
        }

        private GpioTrigger(InputPort port)
        {
            this.port = port;
        }

        private readonly InputPort port;
    }
}
