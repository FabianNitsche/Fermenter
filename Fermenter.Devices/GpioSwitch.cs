using ReactiveGpio;
using System;
using System.Threading.Tasks;

namespace Fermenter.Devices
{
    public sealed class GpioSwitch : IDisposable
    {
        public static async Task<GpioSwitch> Create(IGpioDriver driver, int gpioPin, bool lowIsOff, IObservable<bool> onOffTrigger)
        {
            var port = await OutputPort.Create(gpioPin, lowIsOff ? OutputPort.InitialValue.Low : OutputPort.InitialValue.High, driver);
            return new GpioSwitch(port, onOffTrigger);
        }

        private readonly OutputPort port;

        private readonly IDisposable subscription;

        private GpioSwitch(OutputPort port, IObservable<bool> onOffTrigger)
        {
            this.port = port;
            subscription = onOffTrigger.Subscribe(port);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    subscription.Dispose();
                    port.Write(false);
                    port.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~GpioSwitch()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
