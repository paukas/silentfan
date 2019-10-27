using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using Microsoft.Win32;

namespace gputempmon
{

    class InstallerUi
    {
        private const string ExecutableName = "gputempmon.exe";
        private const string FolderName = "silentfan";
        private const string StartupEntryName = "silentfan";

        private FileSystem _fs = new FileSystem();

        public void Run()
        {
            if (!IsAdministrator())
            {
                Error("Installation must be run as administrator. Please restart application as administrator.");
                return;
            }

            string currentDir = GetCurrentDir();
            string defaultInstallationDir = GetDefaultDir();
            string installationDir = defaultInstallationDir;
            string applicationPath = Path.Combine(installationDir, ExecutableName);

            bool isValidDirectory = _fs.IsValidDirectory(installationDir);
            if (!isValidDirectory)
            {
                Error("Entered directory is invalid. Please enter valid directory:");
                return;
            }

            if (IsSameDir(currentDir, installationDir))
            {
                Error("! This application is already installed");
                return;
            }

            if (!_fs.TryCreateDir(installationDir))
            {
                Error("! Failed to create directory");
                return;
            }

            if (!TryKillRunningApplication(applicationPath))
            {
                Error("! Failed to kill already running application");
                return;
            }
            
            if (!_fs.TryCopyFiles(currentDir, installationDir))
            {
                Error("! Failed to copy files");
                return;
            }

            if (!TryAddToStartup(applicationPath))
            {
                Error("! Failed to add application to startup");
                return;
            }

            Console.WriteLine("Installation successfull. Launch?");
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

        private bool TryKillRunningApplication(string applicationPath)
        {
            string processName = Path.GetFileNameWithoutExtension(applicationPath);
            Process[] processes = Process.GetProcessesByName(processName);
            Process runningProcess = processes.FirstOrDefault(p => p.MainModule.FileName == applicationPath);
            if (runningProcess == null)
                return true;

            try
            {
                runningProcess.Kill();
                bool processExited = runningProcess.WaitForExit(1000);
                while (!processExited)
                {
                    Console.WriteLine("Waiting for process to exit");
                    processExited = runningProcess.WaitForExit(1000);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void Error(string text)
        {
            Console.WriteLine(text);
            new MenuUi<bool>(new[] { new MenuItem<bool>("> Exit", true) }).Choose();
            return;
        }

        private bool IsAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        private string GetCurrentDir()
        {
            return Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory);
        }

        private static string GetDefaultDir()
        {
            string programFilesDir = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            return Path.Combine(programFilesDir, FolderName);
        }

        private void Launch(string applicationPath)
        {
            Process.Start(applicationPath, "--run");
        }

        private bool TryAddToStartup(string applicationPath)
        {
            const string registryValueName = StartupEntryName;
            const string registryKeyName = @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run";
            string command = $"\"{applicationPath}\" --run";

            try
            {
                Registry.SetValue(registryKeyName, registryValueName, command);
                return true;
            }
            catch
            {
                return false;
            }
        }



        private bool IsSameDir(string currentDir, string installationDir)
        {
            return currentDir == installationDir;
        }
    }
}
