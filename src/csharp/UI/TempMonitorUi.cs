using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace gputempmon
{
    class DutyCycleCalculator
    {
        public int CalculateDutyCycle(double temperature)
        {
            if (temperature > 70)
                return 100;
            if (temperature > 65)
                return 90;
            if (temperature > 60)
                return 80;
            if (temperature > 50)
                return 70;
            if (temperature > 40)
                return 60;

            return 40;
        }
    }

    interface IFanState
    {
        int DutyCycle { get; }
        int Rpm { get; }
    }

    class FanState : IFanState
    {
        public int DutyCycle { get; set; }

        public int Rpm { get; set; }
    }

    class FanMonitorService
    {
        private readonly FanState _fanState = new FanState();

        private readonly Arduino _arduino;

        public FanMonitorService(Arduino arduino)
        {
            _arduino = arduino;
        }

        public Task Start()
        {
            return Task.Factory.StartNew(Run, TaskCreationOptions.LongRunning);
        }

        private Task Run()
        {
            while (true)
            {
                const string RPM_PREFIX = "fan[0].rpm=";
                const string PWM_PREFIX = "fan[0].pwm=";

                string logLine = _arduino.ReadLogLine();
                if (logLine.StartsWith(RPM_PREFIX))
                    _fanState.Rpm = int.Parse(logLine.Substring(RPM_PREFIX.Length));
                else if (logLine.StartsWith(PWM_PREFIX))
                    _fanState.DutyCycle = int.Parse(logLine.Substring(PWM_PREFIX.Length));
            }
        }

        public IFanState GetFanState()
        {
            return _fanState;
        }
    }

    class MonitorUi
    {
        public void Run()
        {
            ArduinoComPort arduinoComPort = Arduino.Detect().Single();
            ConsoleUi ui = ConsoleUi.Create();
            DutyCycleCalculator dutyCycleCalculator = new DutyCycleCalculator();

            using (Arduino arduino = arduinoComPort.Connect())
            using (GraphicsCard graphicsCard = GraphicsCard.Open())
            {
                FanMonitorService fanMonitorService = new FanMonitorService(arduino);
                fanMonitorService.Start();

                while (true)
                {
                    Stopwatch stopwatch = Stopwatch.StartNew();
                    double temperature = graphicsCard.ReadTemperature();
                    stopwatch.Stop();

                    IFanState fanState = fanMonitorService.GetFanState();
                    int dutyCycle = dutyCycleCalculator.CalculateDutyCycle(temperature);

                    ui.Refresh(new UiState
                    {
                        Temperature = temperature,
                        FanRpm = fanState.Rpm,
                        FanDutyCycle = fanState.DutyCycle,
                        FanDutyCycleNew = dutyCycle,
                        RefreshElapsed = stopwatch.Elapsed
                    });

                    arduino.UpdateDutyCycle(dutyCycle);

                    Task.Delay(350).Wait();
                }
            }
        }
    }
}
