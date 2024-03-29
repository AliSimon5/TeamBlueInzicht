﻿using HDSignatures.Types;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDSignatures.Data
{
    internal partial class DataManager
    {
        public static void BackupSignatures()
        {
            if (CheckIfSignatureExists())
                CreateBackupForSignatures();
        }

        public static bool CheckIfSignatureExists()
        {
            string tempPath = $"{Settings.strSchijf}:\\{Settings.strUsersPathName}\\{Settings.strAdminNaam}\\AppData\\Roaming\\Microsoft\\{Settings.strSignatureDirectoryName}";
            int tempfCount = Directory.GetFiles(tempPath, "*", SearchOption.TopDirectoryOnly).Length;
            if (tempfCount >= 6)
                return true;

            else
                return false;
        }
        public static void CreateBackupForSignatures()
        {
            try
            {
                // pakt de naam van waar de applicatie is gestart
                var startupPath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);

                // Maakt een Backups folder aan als die nog niet bestaat
                var tempBackupPath = Path.Combine(startupPath, "HDSignatures_Backups");
                if (!Directory.Exists(tempBackupPath))
                    Directory.CreateDirectory(tempBackupPath);
                else
                {   // Verwijderd de oudste backups
                    var dirs = Directory.GetDirectories(tempBackupPath).ToList();
                    DeleteOldBackups(dirs);
                }

                // Maakt de backups van vandaag folder aan als die nog niet bestaat
                var tempBackupLocationPath = Path.Combine(tempBackupPath, $"[{DateTime.Now:yyyy-MM-dd}]-Backup");
                if (!Directory.Exists(tempBackupLocationPath))
                    Directory.CreateDirectory(tempBackupLocationPath);

                // Kopieert de files naar de directory
                string tempPath = $"{Settings.strSchijf}:\\{Settings.strUsersPathName}\\{Settings.strAdminNaam}\\AppData\\Roaming\\Microsoft\\{Settings.strSignatureDirectoryName}";
                string[] tempAllFiles = Directory.GetFiles(tempPath);
                foreach (var file in tempAllFiles)
                {
                    File.Copy(file, $"{tempBackupLocationPath}\\{Path.GetFileName(file)}", true);
                }

                // Maakt Directorys van de _files en/of _bestanden aan
                foreach (string dirPath in Directory.GetDirectories(tempPath, "*", SearchOption.AllDirectories))
                {
                    if (!tempBackupLocationPath.Contains(dirPath))
                        Directory.CreateDirectory(dirPath.Replace(tempPath, tempBackupLocationPath));
                }

                // Kopieert alle files die in de _files en/of _bestanden zitten
                foreach (string newPath in Directory.GetFiles(tempPath, "*.*", SearchOption.AllDirectories))
                {
                    File.Copy(newPath, newPath.Replace(tempPath, tempBackupLocationPath), true);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }
        public static void DeleteOldBackups(List<string> dir)
        {
            // zorgt ervoor dat hij de oudste dir gaat verwijderen
            dir = dir.OrderByDescending(x => File.GetLastWriteTime(x)).ToList();

            if (dir.Count > 10)
            {
                // een loop om te kijken hoeveel bestanden er verwijderd moeten worden om de bestanden gelijk te maken met het getal
                for (int i = 10; i < dir.Count; ++i)
                {
                    // verwijderd de directory
                    Directory.Delete(dir[i], true);
                }
            }
        }
    }

}
