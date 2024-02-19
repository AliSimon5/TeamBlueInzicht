using M.Core.Application.WPF.MessageBox;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDListFolders.Data
{
    internal partial class DataManager
    {
        public static DirectoryInfo[] GetAllSubDirectoriesFromPath(string argPath, bool argSearchAllDirectories)
        {
            DirectoryInfo[] tempAllSubDirectories = null;
            DirectoryInfo tempDirInfo = new DirectoryInfo(argPath);

            if (!argSearchAllDirectories)
                tempAllSubDirectories = tempDirInfo.GetDirectories("*.*");
            
            if (argSearchAllDirectories)
                tempAllSubDirectories = tempDirInfo.GetDirectories("*.*", SearchOption.AllDirectories);

            return tempAllSubDirectories;
        }
        public static FileInfo[] GetAllFilesFromPath(string argPath, bool argSearchAllDirectories)
        {
            FileInfo[] tempAllFiles = null;
            DirectoryInfo tempDirInfo = new DirectoryInfo(argPath);
           
            if (!argSearchAllDirectories)
                tempAllFiles = tempDirInfo.GetFiles("*.*");
           
            if (argSearchAllDirectories)
                tempAllFiles = tempDirInfo.GetFiles("*.*", SearchOption.AllDirectories);

            return tempAllFiles;
        }
    }
}
