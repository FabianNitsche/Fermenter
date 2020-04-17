using ReactiveGpio;
using ReactiveGpio.Drivers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fermenter.Devices
{
    public class GpioSwitch : IDisposable
    {
        private readonly IDisposable subscription;

        private readonly int gpioPin;

        private readonly IGpioDriver driver;

        private readonly OutputPort port;

        public GpioSwitch(IGpioDriver driver, int gpioPin, bool lowIsOff, IObservable<bool> trigger)
        {
            this.gpioPin = gpioPin;
            this.driver = driver;

            port = OutputPort.Create(gpioPin, lowIsOff ? OutputPort.InitialValue.Low : OutputPort.InitialValue.High, driver).Result;
            subscription = trigger.Subscribe(port);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    subscription.Dispose();
                    port.Write(false);
                    driver.UnAssignPin(gpioPin.ToString());
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~Heater()
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
