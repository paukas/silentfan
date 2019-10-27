using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace gputempmon
{
    class TempMonitorUi
    {
        public void Run()
        {
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
}
