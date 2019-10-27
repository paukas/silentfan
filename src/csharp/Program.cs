using System;

namespace gputempmon
{

    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                switch (args[0])
                {
                    case "--install":
                        new InstallerUi().Run();
                        break;
                    case "--run":
                        new TempMonitorUi().Run();
                        break;
                    default:
                        Console.WriteLine($"Unrecognized argument: {args[0]}");
                        break;
                }
            }
            else
            {
                new MainUi().Run();
            }
        }
    }
}
