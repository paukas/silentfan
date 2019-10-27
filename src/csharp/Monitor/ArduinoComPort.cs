using System.IO.Ports;

namespace gputempmon
{
    class ArduinoComPort
    {
        private string _name;

        public ArduinoComPort(string name)
        {
            _name = name;
        }

        public Arduino Connect()
        {
            SerialPort serialPort = new SerialPort(_name);
            serialPort.Open();
            return new Arduino(serialPort);
        }
    }
}
