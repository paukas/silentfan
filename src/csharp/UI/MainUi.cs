using System;

namespace gputempmon
{
    class MainUi
    {
        public void Run()
        {
            MenuItem<Action>[] menuActions = 
            {
                new MenuItem<Action>("> run", RunTempMonitor),
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
            Console.WriteLine("Uninstall not implemented yet");
            Console.WriteLine("Uninstall manually:");
            Console.WriteLine("- delete silentfan value from registry (HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\CurrentVersion\\Run)");
            Console.WriteLine("- delete silentfan directory");
            return;
        }

        private void Install()
        {
            new InstallerUi().Run();
        }

        private void RunTempMonitor()
        {
            new TempMonitorUi().Run();
        }
    }
}
