﻿using HDSignaturesTool.Types;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDSignaturesTool.Data
{
    internal partial class DataManager
    {
        #region Temporary Backup

        /// <summary>
        /// Functie die temporary backups maakt 
        /// </summary>
        public static void BackupSignatures()
        {
            if (CheckIfSignatureExists())
                CreateBackupOfSignatures();
        }

        /// <summary>
        /// Checkt of er signatures zijn om een backup van te maken
        /// </summary>
        /// <returns></returns>
        public static bool CheckIfSignatureExists()
        {
            string tempPath = $"{Settings.strAdminPath}";
            if (string.IsNullOrEmpty(tempPath))
                return false;
            int tempfCount = Directory.GetFiles(tempPath, "*", SearchOption.TopDirectoryOnly).Length;
            if (tempfCount >= 6)
                return true;
            else
                return false;
        }
        public static void CreateBackupOfSignatures()
        {
            try
            {
                // pakt de naam van waar de applicatie is gestart
                var startupPath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);

                // Maakt een Backups folder aan als die nog niet bestaat
                var tempBackupPath = Path.Combine(startupPath, "HDSignatures_TempBackups");
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
                string tempPath = $"{Settings.strAdminPath}";
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

        #endregion

        #region PermanentBackup

        public static void CopyTempBackupToPermBackup(string argPath)
        {
            try
            {
                var startupPath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);

                // Maakt een Backups folder aan als die nog niet bestaat
                var tempBackupPath = Path.Combine(startupPath, "HDSignatures_PermBackups");
                if (!Directory.Exists(tempBackupPath))
                    Directory.CreateDirectory(tempBackupPath);

                // Maakt de backups van vandaag folder aan als die nog niet bestaat
                string tempPathName = Path.GetFileName(argPath);
                var tempBackupLocationPath = Path.Combine(tempBackupPath, tempPathName);
                if (!Directory.Exists(tempBackupLocationPath))
                    Directory.CreateDirectory(tempBackupLocationPath);

                foreach (string dirPath in Directory.GetDirectories(argPath, "*", SearchOption.AllDirectories))
                {
                    if (!tempBackupLocationPath.Contains(dirPath))
                        Directory.CreateDirectory(dirPath.Replace(argPath, tempBackupLocationPath));
                }

                string[] tempAllFiles = Directory.GetFiles(argPath);
                foreach (var file in tempAllFiles)
                {
                    File.Copy(file, $"{tempBackupLocationPath}\\{Path.GetFileName(file)}", true);
                }
                // Kopieert alle files die in de _files en/of _bestanden zitten
                foreach (string newPath in Directory.GetFiles(argPath, "*.*", SearchOption.AllDirectories))
                {
                    File.Copy(newPath, newPath.Replace(argPath, tempBackupLocationPath), true);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }
        public static void DeletePermanentBackupSignature(string argPath)
        {
            Directory.Delete(argPath, true);
        }

        #endregion
    }

}