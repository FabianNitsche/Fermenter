using System;
using System.Collections.Generic;
using System.Text;

namespace Fermenter.Devices
{
    public class TimedValue<T>
    {
        public T Value { get; }

        public DateTime Time { get; }

        public TimedValue(T value, DateTime time)
        {
            Value = value;
            Time = time;
        }
    }
}
