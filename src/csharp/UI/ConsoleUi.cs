using gputempmon.UI.Chart;
using System;
using System.Linq;

namespace gputempmon
{
    class ChartUi
    {
        private readonly ChartState _chartState;
        private readonly ChartPainter _chartPainter;
        private readonly int _valueCount;
        private readonly int _lastValueIndex;

        public ChartUi(ChartState chartState)
        {
            _chartState = chartState;
            _chartPainter = new ChartPainter();

            _valueCount = _chartState.Values.Length;
            _lastValueIndex = _valueCount - 1;
        }

        public void Update(int value, int curstorTop, int cursorLeft)
        {
            int[] newValues = new int[_valueCount];
            Array.Copy(_chartState.Values, 1, newValues, 0, _valueCount - 1);
            newValues[_lastValueIndex] = value;

            _chartState.Values = newValues;
            string str = _chartPainter.PaintToString(_chartState);
            string[] lines = str.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

            Console.SetCursorPosition(cursorLeft, curstorTop);
            for (int i = 0; i < lines.Length; i++)
            {
                Console.SetCursorPosition(cursorLeft, curstorTop + i);
                Console.Write(lines[i]);
            }
        }
    }

    class ConsoleUi
    {
        private readonly object _consoleLock = new object();
        private readonly ChartUi _temperatureChart;
        private readonly ChartUi _fan1Rpm;
        private readonly ChartUi _fan2Rpm;
        private int _windowWidth;
        private int _windowHeight;

        public ConsoleUi()
        {
            _temperatureChart = new ChartUi(new ChartState
            {
                Values = new int[55],
                Guides = new int[] { 0, 50, 100 },
                YMax = 100,
                YMin = 0,
                Step = 10
            });
            _fan1Rpm = new ChartUi(new ChartState
            {
                Values = new int[53],
                Guides = new int[] { 0, 1000, 2000 },
                YMax = 2000,
                YMin = 0,
                Step = 200
            });
            _fan2Rpm = new ChartUi(new ChartState
            {
                Values = new int[53],
                Guides = new int[] { 0, 1000, 2000 },
                YMax = 2000,
                YMin = 0,
                Step = 200
            });
        }

        public void Initialize()
        {
            Console.Clear();
            Console.CursorVisible = false;

            UpdateWindowSize();
            Refresh(FormattedUiState.NotAvailable);
        }

        private void UpdateWindowSize()
        {
            _windowWidth = Console.WindowWidth;
            _windowHeight = Console.WindowHeight;
        }

        public void Refresh(UiState state)
        {
            Refresh(new FormattedUiState
            {
                Temperature = state.Temperature.ToString(),
                RefreshElapsed = state.RefreshElapsed.TotalMilliseconds.ToString("0.##"),
                FanStates = state.FanStates.Select(x => new FormattedUiFanState
                {
                    FanId = x.FanId,
                    Rpm = x.Rpm.ToString(),
                    CurrentDutyCycle = x.CurrentDutyCycle.ToString(),
                    NewDutyCycle = x.NewDutyCycle.ToString()
                }).ToArray()
            });
        }

        private void Refresh(FormattedUiState state)
        {
            lock (_consoleLock)
            {
                if (WindowSizeChanged())
                {
                    Console.Clear();
                    UpdateWindowSize();
                }

                int cursorTop = Console.CursorTop;
                int cursorLeft = Console.CursorLeft;

                Console.SetCursorPosition(0, 0);

                WriteWholeLine($"Temperature:       {state.Temperature}");
                WriteWholeLine("");

                foreach (FormattedUiFanState fanState in state.FanStates)
                {
                    WriteWholeLine($"Fan:        {fanState.FanId}");
                    WriteWholeLine($"RPM:        {fanState.Rpm}");
                    WriteWholeLine($"Duty cycle: {FormatDutyCycle(fanState.CurrentDutyCycle, fanState.NewDutyCycle)}");
                    WriteWholeLine("");
                }

                WriteWholeLine("");
                WriteWholeLine($"Refresh elapsed:   {state.RefreshElapsed}");

                if (int.TryParse(state.Temperature, out int temperature))
                {
                    _temperatureChart.Update(temperature, 0, 60);
                }
                if (state.FanStates.Length == 2)
                {
                    if (int.TryParse(state.FanStates[0].Rpm, out int fan1Rpm))
                        _fan1Rpm.Update(fan1Rpm, 15, 0);
                    if (int.TryParse(state.FanStates[1].Rpm, out int fan2Rpm))
                        _fan2Rpm.Update(fan2Rpm, 15, 60);
                }

                Console.SetCursorPosition(cursorLeft, cursorTop);
            }
        }

        private bool WindowSizeChanged()
        {
            return _windowWidth != Console.WindowWidth || _windowHeight != Console.WindowHeight;
        }

        private string FormatDutyCycle(string currentDutyCycle, string newDutyCycle)
        {
            if (currentDutyCycle == newDutyCycle)
            {
                return currentDutyCycle;
            }

            return $"{currentDutyCycle} -> {newDutyCycle}";
        }

        private void WriteWholeLine(string text)
        {
            Console.WriteLine(text.PadRight(Console.BufferWidth - 1));
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

        internal void RefreshFanState(IFanState fanState)
        {
            throw new NotImplementedException();
        }
    }

    class FormattedUiState
    {
        public static readonly FormattedUiState NotAvailable = new FormattedUiState
        {
            Temperature = "N/A",
            RefreshElapsed = "N/A",
            FanStates = new FormattedUiFanState[0]
        };

        public string Temperature { get; internal set; }
        public string RefreshElapsed { get; internal set; }
        public FormattedUiFanState[] FanStates { get; internal set; }
    }

    class FormattedUiFanState
    {
        public string FanId { get; set; }
        public string Rpm { get; set; }
        public string CurrentDutyCycle { get; set; }
        public string NewDutyCycle { get; set; }
    }

    public class UiState
    {
        public double Temperature { get; internal set; }
        public TimeSpan RefreshElapsed { get; internal set; }
        public UiFanState[] FanStates { get; internal set; }
    }

    public class UiFanState
    {
        public string FanId { get; set; }
        public int Rpm { get; set; }
        public int CurrentDutyCycle { get; set; }
        public int NewDutyCycle { get; set; }
    }
}
