using OpenHardwareMonitor.Hardware;
using System;

namespace gputempmon
{
    internal class TemperaturePrintingVisitor : SensorVisitor
    {
        public override void VisitSensor(ISensor sensor)
        {
            if (sensor.SensorType == SensorType.Temperature)
            {
                Console.WriteLine($"{sensor.Name}: {sensor.Value}");
            }
        }
    }
}
