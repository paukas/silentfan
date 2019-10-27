using OpenHardwareMonitor.Hardware;
using System;
using System.Collections.Generic;
using System.Linq;

namespace gputempmon
{
    class GraphicsCard : IDisposable
    {
        private Computer _computer;

        public GraphicsCard(Computer computer)
        {
            _computer = computer;
        }

        public double ReadTemperature()
        {
            IEnumerable<ISensor> temperatureSensors = GetTemperatureSensors();
            ISensor sensor = temperatureSensors.Single();
            float value = sensor.Value.GetValueOrDefault();
            return value;
        }

        private IEnumerable<ISensor> GetTemperatureSensors()
        {
            UpdateVisitor updateVisitor = new UpdateVisitor();
            SensorCollectorVisitor sensorCollector = new SensorCollectorVisitor();
            _computer.Accept(updateVisitor);
            _computer.Accept(sensorCollector);

            return sensorCollector.Sensors.Where(s => s.SensorType == SensorType.Temperature);
        }

        public void Dispose()
        {
            _computer.Close();
        }

        public static GraphicsCard Open()
        {
            Computer computer = new Computer
            {
                GPUEnabled = true
            };
            computer.Open();
            return new GraphicsCard(computer);
        }
    }
}
