﻿namespace Theatre.Utils
{
    public static class FileUtils
    {

        [Obsolete("Unused, for removal", true)]
        public static byte[] ExtractResource(string filename)
        {
            System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();
            using Stream? resFilestream = a.GetManifestResourceStream(filename);
            string[] names = a.GetManifestResourceNames();
            if (resFilestream == null) return Array.Empty<byte>();
            byte[] ba = new byte[resFilestream.Length];
            resFilestream.Read(ba, 0, ba.Length);
            return ba;
        }

        public static void CreateDirectorySafe(string path)
        {
            if (Directory.Exists(path))
                return;
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            Directory.CreateDirectory(path);
        }

        public static void ReloadDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path);
            }

            Directory.CreateDirectory(path);
        }

        public static void CopyDirectory(string sourceDir, string destinationDir, bool recursive = true)
        {
            // Get information about the source directory
            var dir = new DirectoryInfo(sourceDir);

            // Check if the source directory exists
            if (!dir.Exists)
                throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

            // Cache directories before we start copying
            DirectoryInfo[] dirs = dir.GetDirectories();

            // Create the destination directory
            Directory.CreateDirectory(destinationDir);

            // Get the files in the source directory and copy to the destination directory
            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destinationDir, file.Name);
                if (!File.Exists(targetFilePath))
                    file.CopyTo(targetFilePath);
            }

            // If recursive and copying subdirectories, recursively call this method
            if (recursive)
            {
                foreach (DirectoryInfo subDir in dirs)
                {
                    string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                    CopyDirectory(subDir.FullName, newDestinationDir);
                }
            }
        }
    }
}