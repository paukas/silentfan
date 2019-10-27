using System;

namespace gputempmon
{
    class MenuItem<T>
    {
        public T Item { get; }
        private readonly string _actionText;

        public MenuItem(string actionText, T item)
        {
            Item = item;
            _actionText = actionText;
        }

        public void Print(bool selected)
        {
            if (selected)
            {
                ConsoleColor foregroundColor = Console.ForegroundColor;
                ConsoleColor backgroundColor = Console.BackgroundColor;

                Console.ForegroundColor = backgroundColor;
                Console.BackgroundColor = foregroundColor;
                Console.WriteLine(_actionText);
                Console.ForegroundColor = foregroundColor;
                Console.BackgroundColor = backgroundColor;
            }
            else
            {
                Console.WriteLine(_actionText);
            }
        }
    }
}
