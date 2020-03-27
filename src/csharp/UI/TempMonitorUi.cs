using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace gputempmon
{
    interface IDutyCycleCalculator
    {
        int CalculateDutyCycle(double temperature);
    }

    class Fan2DudyCycleCalculator : IDutyCycleCalculator
    {
        private Dictionary<int, int> _curve = new Dictionary<int, int>();
        private KeyValuePair<int, int> _min;
        private KeyValuePair<int, int> _max;

        public Fan2DudyCycleCalculator()
        {
            _curve = _curve
                .Union(Linear(40, 59, 13, 20))
                .Union(Linear(60, 69, 21, 40))
                .Union(Linear(70, 79, 40, 100))
                .ToDictionary(x => x.Key, x => x.Value);

            int minTemp = _curve.Keys.Min();
            int maxTemp = _curve.Keys.Max();

            _min = new KeyValuePair<int, int>(minTemp, _curve[minTemp]);
            _max = new KeyValuePair<int, int>(maxTemp, _curve[maxTemp]);
        }

        private IEnumerable<KeyValuePair<int, int>> Linear(int tFrom, int tTo, int dcFrom, int dcTo)
        {
            int deltaT = tTo - tFrom;
            int deltaDc = dcTo - dcFrom;

            double multiplier = (double)deltaDc / (double)deltaT;
            for (int t = tFrom; t <= tTo; t++)
                yield return new KeyValuePair<int, int>(t, dcFrom + (int)((t - tFrom) * multiplier));
        }

        public int CalculateDutyCycle(double temperature)
        {
            if (_curve.TryGetValue((int)temperature, out int dutyCycle))
                return dutyCycle;
            else if (temperature < _min.Key)
                return _min.Value;
            else if (temperature > _max.Key)
                return _max.Value;
            else
                return 100;
        }
    }

    class SmoothDutyCycleCalculator : IDutyCycleCalculator
    {
        private Dictionary<int, int> _curve = new Dictionary<int, int>();
        private KeyValuePair<int, int> _min;
        private KeyValuePair<int, int> _max;

        public SmoothDutyCycleCalculator()
        {
            _curve = _curve
                .Union(Linear(40, 59, 13, 59))
                .Union(Linear(60, 69, 60, 75))
                .Union(Linear(70, 79, 76, 100))
                .ToDictionary(x => x.Key, x => x.Value);

            int minTemp = _curve.Keys.Min();
            int maxTemp = _curve.Keys.Max();

            _min = new KeyValuePair<int, int>(minTemp, _curve[minTemp]);
            _max = new KeyValuePair<int, int>(maxTemp, _curve[maxTemp]);
        }

        private IEnumerable<KeyValuePair<int, int>> Linear(int tFrom, int tTo, int dcFrom, int dcTo)
        {
            int deltaT = tTo - tFrom;
            int deltaDc = dcTo - dcFrom;

            double multiplier = (double)deltaDc / (double)deltaT;
            for (int t = tFrom; t <= tTo; t++)
                yield return new KeyValuePair<int, int>(t, dcFrom + (int)((t - tFrom) * multiplier));
        }

        public int CalculateDutyCycle(double temperature)
        {
            if (_curve.TryGetValue((int)temperature, out int dutyCycle))
                return dutyCycle;
            else if (temperature < _min.Key)
                return _min.Value;
            else if (temperature > _max.Key)
                return _max.Value;
            else
                return 100;
        }
    }

    class RoughDutyCycleCalculator : IDutyCycleCalculator
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
        string FanId { get; }
    }

    class FanState : IFanState
    {
        public string FanId { get; set; }
        public int DutyCycle { get; set; }
        public int Rpm { get; set; }

        public FanState(string fanId)
        {
            FanId = fanId;
        }
    }

    class FanMonitorService
    {
        private readonly Dictionary<string, FanState> _fanStates = new Dictionary<string, FanState>();

        private readonly IArduino _arduino;

        public FanMonitorService(IArduino arduino)
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
                string logLine = _arduino.ReadLogLine();
                ProcessLine(logLine);
            }
        }

        private void ProcessLine(string line)
        {
            const string fanPrefix = "fan[";
            if (!line.StartsWith(fanPrefix))
                return;

            var endOfFanId = line.IndexOf("]", fanPrefix.Length);
            if (endOfFanId == -1)
                return;

            int fanIdLength = endOfFanId - fanPrefix.Length;
            string fanId= line.Substring(fanPrefix.Length, fanIdLength);

            string rpmKey = $"fan[{fanId}].rpm=";
            string pwmKey = $"fan[{fanId}].pwm=";
            if (line.StartsWith(rpmKey))
            {
                int rpm = int.Parse(line.Substring(rpmKey.Length));
                SetRpm(fanId, rpm);
            }
            else if (line.StartsWith(pwmKey))
            {
                int pwm = int.Parse(line.Substring(pwmKey.Length));
                SetPwm(fanId, pwm);
            }
        }

        private void SetPwm(string fanId, int pwm)
        {
            EnsureFanStateInList(fanId);
            _fanStates[fanId].DutyCycle = pwm;
        }

        private void SetRpm(string fanId, int rpm)
        {
            EnsureFanStateInList(fanId);
            _fanStates[fanId].Rpm = rpm;
        }

        private void EnsureFanStateInList(string fanId)
        {
            if (!_fanStates.ContainsKey(fanId))
                _fanStates.Add(fanId, new FanState(fanId));
        }

        public IFanState[] GetFanStates()
        {
            return _fanStates.Values.ToArray();
        }
    }

    class MonitorUi
    {
        public void Run()
        {
            ArduinoComPort arduinoComPort = Arduino.Detect().Single();
            using (Arduino arduino = arduinoComPort.Connect())
            {
                Run(arduino);
            }
        }

        private void Run(IArduino arduino)
        {
            ConsoleUi ui = ConsoleUi.Create();
            IDutyCycleCalculator[] calculators = new IDutyCycleCalculator[]
            {
                new SmoothDutyCycleCalculator(),
                new Fan2DudyCycleCalculator()
            };

            using (GraphicsCard graphicsCard = GraphicsCard.Open())
            {
                FanMonitorService fanMonitorService = new FanMonitorService(arduino);
                fanMonitorService.Start();

                while (true)
                {
                    Stopwatch stopwatch = Stopwatch.StartNew();
                    double temperature = graphicsCard.ReadTemperature();
                    stopwatch.Stop();

                    IFanState[] fanStates = fanMonitorService.GetFanStates();

                    UiFanState[] uiFanStates = fanStates.Select((x, i) => new UiFanState
                    {
                        FanId = x.FanId,
                        Rpm = x.Rpm,
                        CurrentDutyCycle = x.DutyCycle,
                        NewDutyCycle = calculators[i].CalculateDutyCycle(temperature)
                    }).ToArray();

                    ui.Refresh(new UiState
                    {
                        Temperature = temperature,
                        RefreshElapsed = stopwatch.Elapsed,
                        FanStates = uiFanStates
                    });

                    foreach (UiFanState uiFanState in uiFanStates)
                    {
                        arduino.UpdateDutyCycle(uiFanState.FanId, uiFanState.NewDutyCycle);
                    }

                    Task.Delay(1000).Wait();
                }
            }
        }

        public void RunTest()
        {
            Run(new ArduinMock());
        }
    }
}
