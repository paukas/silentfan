using System;
using System.IO.Ports;
using System.Threading.Tasks;

namespace gputempmon
{
    class ArduinoIOMonitor
    {
        private readonly SerialPort _serialPort;
        private Task _task;
        private bool _stopping;

        public ArduinoIOMonitor(SerialPort serialPort)
        {
            _serialPort = serialPort;
        }

        public void Start()
        {
            _task = Task.Factory.StartNew(Run, TaskCreationOptions.LongRunning);
        }

        private void Run()
        {
            while (!_stopping)
            {
                string line = _serialPort.ReadLine();
                Console.WriteLine(line);
            }
        }

        public void Stop()
        {
            _stopping = true;
            _task.Wait();
        }
    }
}
