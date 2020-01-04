using System;

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
                FanRpm = state.FanRpm.ToString(),
                FanDutyCycle = state.FanDutyCycle.ToString(),
                FanDutyCycleNew = state.FanDutyCycleNew.ToString(),
                RefreshElapsed = state.RefreshElapsed.TotalMilliseconds.ToString("0.##"),
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
                WriteWholeLine($"RPM:               {state.FanRpm}");
                WriteWholeLine($"Duty cycle:        {FormatDutyCycle(state.FanDutyCycle, state.FanDutyCycleNew)}");
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
            FanDutyCycle = "N/A",
            FanDutyCycleNew = "N/A",
            FanRpm = "N/A",
            RefreshElapsed = "N/A"
        };

        public string Temperature { get; internal set; }
            public string FanRpm { get; internal set; }
            public string FanDutyCycle { get; internal set; }
            public string RefreshElapsed { get; internal set; }
        public string FanDutyCycleNew { get; internal set; }
    }

    public class UiState
    {
        public double Temperature { get; internal set; }
        public int FanRpm { get; internal set; }
        public int FanDutyCycle { get; internal set; }
        public TimeSpan RefreshElapsed { get; internal set; }
        public int FanDutyCycleNew { get; internal set; }
    }
}
