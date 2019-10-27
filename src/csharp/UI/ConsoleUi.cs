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
}
