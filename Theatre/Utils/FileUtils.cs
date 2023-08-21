﻿using SharpCompress;
using System.Text;

namespace Theatre.Utils
{
    public static class FileUtils
    {
        /**
         * Ensures the creation of a directory
         */
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

        /**
         * Reloads the content of a directory
         */
        public static void ReloadDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path);
            }

            CreateDirectorySafe(path);
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
        public static byte[] ToBytes(this Dictionary<ulong, (string, AFile[])> dict)
        {
            using MemoryStream stream = new();
            using BinaryWriter writer = new(stream, Encoding.UTF8);
            writer.Write(dict.Count);
            dict.Keys.ForEach(x => writer.Write(x));
            foreach (var (name, files) in dict.Values)
            {
                writer.Write(name);
                writer.Write(files.Length);
                foreach (var file in files)
                {
                    writer.Write(file.idRow);
                    writer.Write(file.sFile);
                    writer.Write(file.nFilesize);
                    writer.Write(file.sDescription);
                    writer.Write(file.tsDateAdded);
                    writer.Write(file.nDownloadCount);
                    writer.Write(file.sAnalysisState);
                    writer.Write(file.sDownloadUrl);
                    writer.Write(file.sMd5Checksum);
                    writer.Write(file.sClamAvResult);
                    writer.Write(file.sAnalysisResult);
                    writer.Write(file.bContainsExe);
                }
            }
            return stream.ToArray();
        }
        public static Dictionary<ulong, (string, AFile[])> FromBytes(byte[] data)
        {
            using MemoryStream stream = new(data);
            using BinaryReader reader = new(stream, Encoding.UTF8);
            int count = reader.ReadInt32();
            Dictionary<ulong, (string, AFile[])> result = new(count);
            for (int i = 0; i < count; i++)
                result[reader.ReadUInt64()] = (string.Empty, Array.Empty<AFile>());
            foreach (var key in result.Keys)
            {
                string name = reader.ReadString();
                AFile[] files = new AFile[reader.ReadInt32()];
                for (int i = 0; i < files.Length; i++)
                    files[i] = new();
                for (int i = 0; i < files.Length; i++)
                {
                    ref var file = ref files[i];
                    file.idRow = reader.ReadUInt64();
                    file.sFile = reader.ReadString();
                    file.nFilesize = reader.ReadUInt64();
                    file.sDescription = reader.ReadString();
                    file.tsDateAdded = reader.ReadUInt64();
                    file.nDownloadCount = reader.ReadUInt64();
                    file.sAnalysisState = reader.ReadString();
                    file.sDownloadUrl = reader.ReadString();
                    file.sMd5Checksum = reader.ReadString();
                    file.sClamAvResult = reader.ReadString();
                    file.sAnalysisResult = reader.ReadString();
                    file.bContainsExe = reader.ReadBoolean();
                }
                result[key] = (name, files);
            }
            return result;
        }
        public static void FromBytes(ref Dictionary<ulong, (string, AFile[])> dict, byte[] data)
        {
            dict = FromBytes(data);
        }
        public static void UpdateEntries(this Dictionary<ulong, (string, AFile[])> dict, GBGame game)
        {
            var submissons = GBUtils.GetSubmissions(game);
            submissons.Where(x => !dict.ContainsKey(x)).
                Select(x => (x, GBUtils.GetSubmissionData(x)))
                .ForEach(tup => dict[tup.x] = tup.Item2);
        }
    }
}
