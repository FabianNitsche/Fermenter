using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Fermenter.Devices
{
    public interface IDisplayDriver
    {
        void WriteCurrentTemp(double currentTemp);

        void WriteSetTemp(double setTemp);

        void WriteIp(IPAddress address);

        void DrawDiagram(double yMin, double yMax, TimeSpan diagramTimeSpan, double[] values, int emptyValuesAtStart);

        double MaxValues { get; }

        void Draw();
    }
}
