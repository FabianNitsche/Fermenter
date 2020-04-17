using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fermenter.Devices
{
    public sealed class DiagramData
    {
        public double YMin { get; }

        public double YMax { get; }

        public TimeSpan DiagramTimeSpan { get; }

        private readonly double[] values;

        public double[] Values => values.ToArray();

        public int EmptyValuesAtStart { get; }

        public DiagramData(double yMin, double yMax, TimeSpan diagramTimeSpan, double[] values, int emptyValuesAtStart)
        {
            YMin = yMin;
            YMax = yMax;
            DiagramTimeSpan = diagramTimeSpan;
            this.values = values.ToArray();
            EmptyValuesAtStart = emptyValuesAtStart;
        }
    }
}
