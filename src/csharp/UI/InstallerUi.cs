using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;

namespace gputempmon
{
    class InstallerUi
    {
        public void Run()
        {
            string currentDir = "";
            string defaultDir = "";
            MenuItem<string>[] pathMenuItems =
            {
                new MenuItem<string>($"> {defaultDir}", string.Empty),
                new MenuItem<string>("> Custom", string.Empty)
            };
            
            string installationDir = new MenuUi<string>(pathMenuItems).Choose().Item;
            bool isValidDirectory = IsValidDirectory(installationDir);
            while (!isValidDirectory)
            {
                ConsoleColor foregroundColor = Console.ForegroundColor;
                ConsoleColor backgroundColor = Console.BackgroundColor;

                Console.ForegroundColor = backgroundColor;
                Console.BackgroundColor = foregroundColor;
                Console.Write("> ");
                installationDir = Console.ReadLine();
                Console.ForegroundColor = foregroundColor;
                Console.BackgroundColor = backgroundColor;

                isValidDirectory = IsValidDirectory(installationDir);
                if (!isValidDirectory)
                {
                    Console.WriteLine("Entered directory is invalid. Please enter valid directory:");
                }
            }

            if (IsSameDir(currentDir, installationDir))
            {
                Console.WriteLine("! This application is already installed");
                return;
            }

            if (!TryCreateDir(installationDir))
            {
                Console.WriteLine("! Failed to create directory");
                return;
            }

            if (!TryCopyFiles(currentDir, installationDir))
            {
                Console.WriteLine("! Failed to copy files");
                return;
            }

            string applicationFileName = "";
            string applicationPath = Path.Combine(installationDir, applicationFileName);
            if (!TryAddToStartup(applicationPath))
            {
                Console.WriteLine("! Failed to add application to startup");
                return;
            }

            Console.Write("Installation successfull. Launch?");
            bool launchProgram = new MenuUi<bool>(new[]
            {
                new MenuItem<bool>("> Yes", true),
                new MenuItem<bool>("> No", false),
            }).Choose().Item;

            if (launchProgram)
            {
                Launch(applicationPath);
                Console.WriteLine("Application launched");
            }
            
            Console.WriteLine("Goodbye");
        }

        private void Launch(string applicationPath)
        {
            Process.Start(applicationPath);
        }

        private bool TryAddToStartup(string applicationPath)
        {
            const string registryValueName = "silentfan";
            const string registryKeyName = @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run";

            try
            {
                Registry.SetValue(registryKeyName, registryValueName,
                    applicationPath);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool TryCopyFiles(string currentDir, string installationDir)
        {
            
        }

        private bool TryCreateDir(string installationDir)
        {
            if (!Directory.Exists(installationDir))
            {
                try
                {
                    Directory.CreateDirectory(installationDir);
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            return true;
        }

        private bool IsSameDir(string currentDir, string installationDir)
        {
            return currentDir != installationDir;
        }

        private bool IsValidDirectory(string directory)
        {
            return Path.IsPathRooted(directory) && DriveExists(directory);
            
            bool DriveExists(string dir)
            {
                return Directory.Exists(Path.GetPathRoot(dir));
            }
        }

    }
}
