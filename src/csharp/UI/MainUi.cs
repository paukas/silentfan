using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace gputempmon
{
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
}
