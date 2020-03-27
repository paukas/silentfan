using System;
using System.IO.Ports;
using System.Linq;
using System.Threading.Tasks;

namespace gputempmon
{
    class Arduino : IDisposable, IArduino
    {
        private SerialPort _serialPort;

        public Arduino(SerialPort serialPort)
        {
            _serialPort = serialPort;
        }

        public void UpdateDutyCycle(string fanId, int dutyCycle)
        {
            _serialPort.WriteLine($"fan[{fanId}].pwm={dutyCycle}");
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

    class ArduinMock : IArduino
    {
        class LogLineMock
        {
            private readonly string _variableName;
            private readonly int _value;

            public LogLineMock(string variableName, int value)
            {
                _variableName = variableName;
                _value = value;
            }

            public string MakeLine()
            {
                return $"{_variableName}={_value}";
            }
        }

        private readonly LogLineMock[] _arduinoOutput = new[]
        {
            new LogLineMock("fan[0].pwm", 13),
            new LogLineMock("fan[0].rpm", 300),
            new LogLineMock("fan[1].pwm", 13),
            new LogLineMock("fan[1].rpm", 300)
        };
        private int _outputLineNo;

        public void Dispose()
        {
            
        }

        public string ReadLogLine()
        {
            Task.Delay(200).Wait();

            _outputLineNo++;
            int lineIndex = _outputLineNo % _arduinoOutput.Length;
            return _arduinoOutput[lineIndex].MakeLine();
        }

        public void UpdateDutyCycle(string fanId, int dutyCycle)
        {
            
        }
    }
}
