using System;
using System.IO.Ports;
using System.Linq;

namespace gputempmon
{
    class Arduino : IDisposable
    {
        private SerialPort _serialPort;

        public Arduino(SerialPort serialPort)
        {
            _serialPort = serialPort;
        }

        public void UpdateTemperature(double temperature)
        {
            _serialPort.WriteLine($"{temperature:0.##}");
        }

        public string ReadLogLine()
        {
            return _serialPort.ReadLine();
        }

        public static ArduinoComPort[] Detect()
        {
            return SerialPort.GetPortNames()
                .Select(x => new ArduinoComPort(x))
                .ToArray();
        }

        public void Dispose()
        {
            _serialPort.Dispose();
        }
    }
}
