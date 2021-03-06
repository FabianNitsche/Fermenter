﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Fermenter.Devices
{
    public interface IHistory<T>
    {
        TimedValue<T>[] GetValuesBeforeNow(TimeSpan timeSpan, out DateTime now);

    }
}
