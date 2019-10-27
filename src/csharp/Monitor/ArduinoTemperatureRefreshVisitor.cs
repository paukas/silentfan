using OpenHardwareMonitor.Hardware;

namespace gputempmon
{
    class ArduinoTemperatureRefreshVisitor : SensorVisitor
    {
        private Arduino _arduino;

        public ArduinoTemperatureRefreshVisitor(Arduino arduino)
        {
            _arduino = arduino;
        }

        public override void VisitSensor(ISensor sensor)
        {
            if (sensor.SensorType == SensorType.Temperature)
            {
                _arduino.UpdateTemperature(sensor.Value.GetValueOrDefault());
            }
        }
    }
}
