﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HDSignatures.Types;
using Serilog;

namespace HDSignatures.Data
{
    internal partial class DataManager
    {
        public static void CopyAndPasteSignatures()
        {
            string TempCopyDirectoryPath = $"{Settings.strSchijf}:\\{Settings.strUsersPathName}\\{Settings.strAdminNaam}\\AppData\\Roaming\\Microsoft\\{Settings.strSignatureDirectoryName}";
            string tempPasteDirectoryPath = Settings.strCopySignaturesToPath;
            if (!Directory.Exists(tempPasteDirectoryPath))
            {
                Log.Error($"Kon {tempPasteDirectoryPath} niet vinden Kopieren mislukt!");
                return;
            }
            //loop om de Folders aan te maken
            var allDirectories = Directory.GetDirectories(TempCopyDirectoryPath, "*", SearchOption.AllDirectories);
            foreach (string dir in allDirectories)
            {
                string dirToCreate = dir.Replace(TempCopyDirectoryPath, tempPasteDirectoryPath);
                Directory.CreateDirectory(dirToCreate);
            }
            var allFiles = Directory.GetFiles(TempCopyDirectoryPath, "*.*", SearchOption.AllDirectories);
            foreach (string newPath in allFiles)
            {
                File.Copy(newPath, newPath.Replace(TempCopyDirectoryPath, tempPasteDirectoryPath), true);
            }
        }
    }
}
