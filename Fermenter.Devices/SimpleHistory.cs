using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;

namespace Fermenter.Devices
{
    public class SimpleHistory : IHistory<double>
    {
        private const int maxNumberOfElements = 1500;

        private readonly TimedValue<double>[] timedValues = new TimedValue<double>[maxNumberOfElements];

        private int currentIndex = -1;

        private bool full = false;

        private readonly TimeSpan approximateTimeStep;

        public SimpleHistory(IObservable<double> values)
        {
            approximateTimeStep = TimeSpan.FromSeconds(1);
            values.Buffer(approximateTimeStep).Subscribe(AddValue);
        }

        public TimedValue<double>[] GetValuesBeforeNow(TimeSpan timeSpan, out DateTime now)
        {
            now = DateTime.Now;
            var index = currentIndex;
            var isFull = full;

            if (index < 0)
                return new TimedValue<double>[0];

            var values = timedValues.ToArray();
            var startTime = now - timeSpan;

            var approximateStepsBack = timeSpan.Ticks / approximateTimeStep.Ticks;
            if (approximateStepsBack >= maxNumberOfElements)
                approximateStepsBack = maxNumberOfElements - 1;

            var result = new List<TimedValue<double>>((int)approximateStepsBack + 10);

            bool firstPass = isFull;
            for (int i = index; ;)
            {
                var value = values[i];
                if (value.Time >= startTime)
                {
                    result.Add(value);
                    if (--i < 0)
                    {
                        i = maxNumberOfElements - 1;
                        if (!firstPass)
                            break;
                        firstPass = false;
                    }
                }
                else
                {
                    break;
                }
            }

            return result.ToArray();
        }

        public void AddValue(IList<double> values)
        {
            if (!values.Any())
                return;

            var value = values.Average();

            var indexOfNewValue = currentIndex + 1;
            bool newFull = full;
            if (indexOfNewValue >= maxNumberOfElements)
            {
                indexOfNewValue = 0;
                newFull = true;
            }

            timedValues[indexOfNewValue] = new TimedValue<double>(value, DateTime.Now);

            Thread.MemoryBarrier();

            full = newFull;
            currentIndex = indexOfNewValue;
        }
    }
}
