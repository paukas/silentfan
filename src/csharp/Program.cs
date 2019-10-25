using OpenHardwareMonitor.Hardware;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace gputempmon
{
    class InstallerUi
    {
        public void Run()
        {
            string currentDir = "";
            string defaultDir = "";
            MenuItem<string>[] pathMenuItems =
            {
                new MenuItem<string>($"> {defaultDir}", string.Empty),
                new MenuItem<string>("> Custom", string.Empty)
            };
            
            string installationDir = new MenuUi<string>(pathMenuItems).Choose().Item;
            bool isValidDirectory = IsValidDirectory(installationDir);
            while (!isValidDirectory)
            {
                ConsoleColor foregroundColor = Console.ForegroundColor;
                ConsoleColor backgroundColor = Console.BackgroundColor;

                Console.ForegroundColor = backgroundColor;
                Console.BackgroundColor = foregroundColor;
                Console.Write("> ");
                installationDir = Console.ReadLine();
                Console.ForegroundColor = foregroundColor;
                Console.BackgroundColor = backgroundColor;

                isValidDirectory = IsValidDirectory(installationDir);
                if (!isValidDirectory)
                {
                    Console.WriteLine("Entered directory is invalid. Please enter valid directory:");
                }
            }

            if (IsSameDir(currentDir, installationDir))
            {
                Console.WriteLine("! This application is already installed");
                return;
            }

            if (!TryCreateDir(installationDir))
            {
                Console.WriteLine("! Failed to create directory");
                return;
            }

            if (!TryCopyFiles(currentDir, installationDir))
            {
                Console.WriteLine("! Failed to copy files");
                return;
            }

            string applicationFileName = "";
            string applicationPath = Path.Combine(installationDir, applicationFileName);
            if (!TryAddToStartup(applicationPath))
            {
                Console.WriteLine("! Failed to add application to startup");
                return;
            }

            Console.Write("Installation successfull. Launch?");
            bool launchProgram = new MenuUi<bool>(new[]
            {
                new MenuItem<bool>("> Yes", true),
                new MenuItem<bool>("> No", false),
            }).Choose().Item;

            if (launchProgram)
            {
                Launch(applicationPath);
                Console.WriteLine("Application launched");
            }
            
            Console.WriteLine("Goodbye");
        }

        private void Launch(string applicationPath)
        {
            Process.Start(applicationPath);
        }

        private bool TryAddToStartup(string applicationPath)
        {
            const string registryValueName = "silentfan";
            const string registryKeyName = @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run";

            try
            {
                Registry.SetValue(registryKeyName, registryValueName,
                    applicationPath);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool TryCopyFiles(string currentDir, string installationDir)
        {
            
        }

        private bool TryCreateDir(string installationDir)
        {
            if (!Directory.Exists(installationDir))
            {
                try
                {
                    Directory.CreateDirectory(installationDir);
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            return true;
        }

        private bool IsSameDir(string currentDir, string installationDir)
        {
            return currentDir != installationDir;
        }

        private bool IsValidDirectory(string directory)
        {
            return Path.IsPathRooted(directory) && DriveExists(directory);
            
            bool DriveExists(string dir)
            {
                return Directory.Exists(Path.GetPathRoot(dir));
            }
        }

    }

    class MainUi
    {
        public void Start()
        {
            MenuItem<Action>[] menuActions = 
            {
                new MenuItem<Action>("> run", Run),
                new MenuItem<Action>("> install", Install),
                new MenuItem<Action>("> uninstall", Uninstall)
            };
            MenuUi<Action> menuUi = new MenuUi<Action>(menuActions);
            MenuItem<Action> menuItem = menuUi.Choose();
            Action action = menuItem.Item;
            
            action.Invoke();
        }

        private void Uninstall()
        {
            throw new NotImplementedException();
        }

        private void Install()
        {
            new InstallerUi().Run();
        }

        private void Run()
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

    class MenuUi<TResult>
    {
        private readonly MenuItem<TResult>[] _menuActions;

        public MenuUi(MenuItem<TResult>[] menuActions)
        {
            _menuActions = menuActions;
        }

        public MenuItem<TResult> Choose()
        {
            int selectedAction = 0;
            
            Console.WriteLine("Choose option:");
            int topRow = Console.CursorTop;

            PrintMenu(_menuActions, selectedAction, topRow);
            while (true)
            {
                int previousAction = selectedAction;
                ConsoleKeyInfo consoleKey = Console.ReadKey(true);
                switch (consoleKey.Key)
                {
                    case ConsoleKey.UpArrow:
                        selectedAction = Math.Max(selectedAction - 1, 0);
                        break;
                    case ConsoleKey.DownArrow:
                        selectedAction = Math.Min(selectedAction + 1, _menuActions.Length - 1);
                        break;
                    case ConsoleKey.Enter:
                        return _menuActions[selectedAction];
                }

                if (previousAction != selectedAction)
                {
                    PrintMenu(_menuActions, selectedAction, topRow);
                }
            }
        }

        private static void PrintMenu<T>(MenuItem<T>[] menuActions, int selectedAction, int topRow)
        {
            Console.CursorLeft = 0;
            Console.CursorTop = topRow;
            for (var i = 0; i < menuActions.Length; i++)
            {
                MenuItem<T> menuItem = menuActions[i];
                menuItem.Print(i == selectedAction);
            }
        }
    }

    class MenuItem<T>
    {
        public T Item { get; }
        private readonly string _actionText;

        public MenuItem(string actionText, T item)
        {
            Item = item;
            _actionText = actionText;
        }

        public void Print(bool selected)
        {
            if (selected)
            {
                ConsoleColor foregroundColor = Console.ForegroundColor;
                ConsoleColor backgroundColor = Console.BackgroundColor;

                Console.ForegroundColor = backgroundColor;
                Console.BackgroundColor = foregroundColor;
                Console.WriteLine(_actionText);
                Console.ForegroundColor = foregroundColor;
                Console.BackgroundColor = backgroundColor;
            }
            else
            {
                Console.WriteLine(_actionText);
            }
        }
    }

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
            new MainUi().Start();
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
