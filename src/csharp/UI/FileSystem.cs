using System.IO;

namespace gputempmon
{
    class FileSystem
    {
        public bool TryCopyFiles(string currentDir, string installationDir)
        {
            try
            {
                CopyFilesRecursively(
                    new DirectoryInfo(currentDir), 
                    new DirectoryInfo(installationDir));
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void CopyFilesRecursively(DirectoryInfo source, DirectoryInfo target)
        {
            foreach (DirectoryInfo dir in source.GetDirectories())
                CopyFilesRecursively(dir, target.CreateSubdirectory(dir.Name));
            foreach (FileInfo file in source.GetFiles())
                file.CopyTo(Path.Combine(target.FullName, file.Name), true);
        }

        public bool TryCreateDir(string installationDir)
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

        public bool IsValidDirectory(string directory)
        {
            return Path.IsPathRooted(directory) && DriveExists(directory);

            bool DriveExists(string dir)
            {
                return Directory.Exists(Path.GetPathRoot(dir));
            }
        }
    }
}
