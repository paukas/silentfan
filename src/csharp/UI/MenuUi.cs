using System;

namespace gputempmon
{
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
}
