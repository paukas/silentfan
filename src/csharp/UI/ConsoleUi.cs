using System;
using System.Linq;

namespace gputempmon
{
    class ConsoleUi
    {
        private readonly object _consoleLock = new object();

        public void Initialize()
        {
            Console.Clear();
            Console.CursorVisible = false;

            Refresh(FormattedUiState.NotAvailable);
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

                Console.SetCursorPosition(cursorTop, cursorLeft);
            }
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
            Console.WriteLine(text.PadRight(Console.BufferWidth));
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
