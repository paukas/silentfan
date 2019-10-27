using OpenHardwareMonitor.Hardware;
using System.Collections.Generic;

namespace gputempmon
{
    class SensorCollectorVisitor : SensorVisitor
    {
        public List<ISensor> Sensors { get; } = new List<ISensor>();

        public override void VisitSensor(ISensor sensor)
        {
            Sensors.Add(sensor);
        }
    }
}
