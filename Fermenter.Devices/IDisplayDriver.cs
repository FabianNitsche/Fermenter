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

        void DrawDiagram(DiagramData diagramData);

        double MaxValues { get; }

        void Draw();
    }
}
