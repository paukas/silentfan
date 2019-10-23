using OpenHardwareMonitor.Hardware;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Threading.Tasks;

namespace gputempmon
{
    class ConsoleUi
    {
        private readonly object _consoleLock = new object();

        public void Initialize()
        {
            Console.Clear();
            Console.CursorVisible = false;
            PrintHeader("N/A", "N/A");
        }

        public void RefreshTemperature(double temperature, TimeSpan refreshOperationDuration)
        {
            PrintHeader(temperature.ToString(), refreshOperationDuration.TotalMilliseconds.ToString("0.##"));
        }

        public void PrintHeader(string temperature, string refreshDuration)
        {
            lock (_consoleLock)
            {
                int cursorTop = Console.CursorTop;
                int cursorLeft = Console.CursorLeft;

                Console.SetCursorPosition(0, 0);
                Console.WriteLine($"Current temperature: {temperature}".PadRight(Console.BufferWidth));
                Console.WriteLine($"Refresh duration: {refreshDuration} ms".PadRight(Console.BufferWidth));

                Console.SetCursorPosition(cursorTop, cursorLeft);
            }
        }

        public void AddArduinoLogEntry(string logEntry)
        {
            lock (_consoleLock)
            {
                Console.WriteLine(logEntry);
            }
        }

        public static ConsoleUi Create()
        {
            ConsoleUi ui = new ConsoleUi();
            ui.Initialize();
            return ui;
        }
    }

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

    class SensorCollectorVisitor : SensorVisitor
    {
        public List<ISensor> Sensors { get; } = new List<ISensor>();

        public override void VisitSensor(ISensor sensor)
        {
            Sensors.Add(sensor);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Start!");

            ArduinoComPort arduinoComPort = Arduino.Detect().Single();
            ConsoleUi ui = ConsoleUi.Create();

            using (Arduino arduino = arduinoComPort.Connect())
            using (GraphicsCard graphicsCard = GraphicsCard.Open())
            {
                while (true)
                {
                    Stopwatch stopwatch = Stopwatch.StartNew();
                    double temperature = graphicsCard.ReadTemperature();
                    stopwatch.Stop();

                    ui.RefreshTemperature(temperature, stopwatch.Elapsed);
                    arduino.UpdateTemperature(temperature);

                    Task.Delay(350).Wait();
                }
            }

            Console.WriteLine("The end...");
        }
    }

    internal class UpdateVisitor : IVisitor
    {
        public void VisitComputer(IComputer computer)
        {
            computer.Traverse(this);
        }

        public void VisitHardware(IHardware hardware)
        {
            hardware.Update();
            foreach (IHardware subHardware in hardware.SubHardware)
                subHardware.Accept(this);
        }

        public void VisitParameter(IParameter parameter)
        {
        }

        public void VisitSensor(ISensor sensor)
        {
        }
    }

    internal abstract class SensorVisitor : IVisitor
    {
        public void VisitComputer(IComputer computer)
        {
            computer.Traverse(this);
        }

        public void VisitHardware(IHardware hardware)
        {
            hardware.Traverse(this);
        }

        public void VisitParameter(IParameter parameter)
        {
            parameter.Traverse(this);
        }

        public abstract void VisitSensor(ISensor sensor);
    }

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
