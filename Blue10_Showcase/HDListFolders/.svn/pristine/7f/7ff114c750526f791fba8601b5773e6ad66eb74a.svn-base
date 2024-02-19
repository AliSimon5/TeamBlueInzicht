using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDListFolders.Logic
{
    internal partial class LogicManager
    {
        public static List<FileInfo> FilterFilesWithPrefix(FileInfo[] argAllFilesFromPath, string argFilter)
        {
            List<FileInfo> tempAllAllowedFiles = new List<FileInfo>();

            foreach (FileInfo tempFile in argAllFilesFromPath)
                if (tempFile.Name.ToLower().Contains(argFilter.ToLower()))
                    tempAllAllowedFiles.Add(tempFile);

            return tempAllAllowedFiles;
        }
        public static List<DirectoryInfo> FilterSubDirectoriesWithPrefix(DirectoryInfo[] argAllFilesFromPath, string argFilter)
        {
            List<DirectoryInfo> tempAllAllowedFiles = new List<DirectoryInfo>();

            foreach (DirectoryInfo tempFile in argAllFilesFromPath)
                if (tempFile.Name.ToLower().Contains(argFilter.ToLower()))
                    tempAllAllowedFiles.Add(tempFile);

            return tempAllAllowedFiles;
        }
    }
}
