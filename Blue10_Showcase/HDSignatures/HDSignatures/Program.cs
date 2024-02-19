﻿using M.Core.Application.Settings;
using Serilog;
using System.Diagnostics;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.DirectoryServices.AccountManagement;
using System.Management;
using System.Reflection;
using System.Text;
using System.Web;
using HDSignatures.Data;
using HDSignatures.Logic;
using HDSignatures.Types;

namespace HDSignatures;
internal class Program
{
    static void Main(string[] args)
    {
        AutoSettings.CreateDefaultOrLoadSettingsForAllAutoSettingsClasses();

        //zoekt waar de applicatie is opgestart en maakt een bestand aan die "HDSignatures_Logs" heet
        var startupPath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
        var logPath = Path.Combine(startupPath, "HDSignatures_Logs");

        //Logger
#if DEBUG

        Log.Logger = new LoggerConfiguration()
                        //Wat de bestanden hun naam zijn en waar ze naartoe moeten
                        .WriteTo.Console()
                        .WriteTo.File(Path.Combine(logPath, $"[{DateTime.Now:yyyy-MM-dd}] HDSignatures.log"),
                            rollingInterval: RollingInterval.Day,
                            fileSizeLimitBytes: 10000000,
                            retainedFileCountLimit: 10,
                            rollOnFileSizeLimit: true,
                            buffered: true,
                            flushToDiskInterval: TimeSpan.FromMilliseconds(150))
                        .MinimumLevel.Verbose()
                        //maakt de .log bestand aan
                        .CreateLogger();
#else
        Log.Logger = new LoggerConfiguration()
                        //Wat de bestanden hun naam zijn en waar ze naartoe moeten
                        .WriteTo.Console()
                        .WriteTo.File(Path.Combine(logPath, $"[{DateTime.Now:yyyy-MM-dd}] HDSignatures.log"),
                            rollingInterval: RollingInterval.Day,
                            fileSizeLimitBytes: 10000000,
                            retainedFileCountLimit: 10,
                            rollOnFileSizeLimit: true,
                            buffered: true,
                            flushToDiskInterval: TimeSpan.FromMilliseconds(150))
                        .MinimumLevel.Information()
                        //maakt de .log bestand aan
                        .CreateLogger();
#endif
        try
        {
            if (Settings.strFilePrefix == null || Settings.strFilePrefix == "")
                Settings.strFilePrefix = "";

            DataManager.BackupSignatures();
            if (!Settings.blnUsersInAD)
            {
                // Lokaal gebruikers
                MakeSignaturesForLocal();
            }
            else
            {
                // Active Directory gebruikers
                MakeSignaturesForDomain();
            }
            Log.CloseAndFlush();
        }
        catch (Exception ex)
        {
            Log.Error(ex.Message);
            Environment.Exit(1);
        }
        Environment.Exit(0);
    }
    public static bool CheckLocationOfDirectory(string argPathToSignatures)
    {
        string LogForDebug = ($"{Settings.strSchijf}:\\{Settings.strUsersPathName}\\{Settings.strAdminNaam}\\AppData\\Roaming\\Microsoft\\{argPathToSignatures}");
        Log.Verbose("Pad naar Signatures directory");
        Log.Verbose(LogForDebug);
        if (Directory.Exists($"{Settings.strSchijf}:\\{Settings.strUsersPathName}\\{Settings.strAdminNaam}\\AppData\\Roaming\\Microsoft\\{argPathToSignatures}"))
            return true;
        else
            return false;
    }
    public static void CreateSignaturesDirectory(List<UserData> tempProcessedUserDataList)
    {
        Log.Information("Maakt nieuwe Directory aan bij elke user");

        foreach (UserData tempUser in tempProcessedUserDataList)
        {
            if (!Directory.Exists($"{tempUser.UserPath}\\AppData\\Roaming\\Microsoft\\{Settings.strSignatureDirectoryName}"))
            {
                Log.Information($"Nieuwe Directory gemaakt voor {tempUser.UserName}");
                Directory.CreateDirectory($"{tempUser.UserPath}\\AppData\\Roaming\\Microsoft\\{Settings.strSignatureDirectoryName}");
            }
        }
    }

    public static void MakeSignaturesForLocal()
    {
        // Lokaal gebruikers
        try
        {
            // Pakt de volle lijst met Users
            List<UserData> tempAllUsersList = DataManager.GetLocalUserList();
            Log.Information("ALLE USERS");
            Log.Information($"{tempAllUsersList.Count} User(s)");
            foreach (UserData User in tempAllUsersList) Log.Verbose($"{User.UserName}");
            Console.WriteLine("\n\r");

            // Pakt de users zijn pad via de SID nummer
            LogicManager.GetPathFromSIDNumber(tempAllUsersList);

            // Pakt alle Inactive Users
            List<UserData> tempInactiveUserList = DataManager.GetInactiveLocalUsers(tempAllUsersList);
            Log.Information("INACTiVE USERS");
            Log.Information($"{tempInactiveUserList.Count} Inactive user(s)");
            foreach (UserData User in tempInactiveUserList) Log.Verbose($"INACTIVE: {User.UserName}");
            Console.WriteLine("\n\r");

            // Pakt de users zijn pad via de SID nummer
            LogicManager.GetPathFromSIDNumber(tempInactiveUserList);

            // Pakt alle Active Users
            List<UserData> tempActiveUserList = DataManager.GetActiveLocalUsers(tempAllUsersList);
            Log.Information("ACTIVE USERS");
            Log.Information($"{tempActiveUserList.Count} Active user(s)");
            foreach (UserData User in tempActiveUserList) Log.Verbose($"ACTIVE: {User.UserName}");
            Console.WriteLine("\n\r");

            // Pakt de users zijn pad via de SID nummer
            LogicManager.GetPathFromSIDNumber(tempActiveUserList);

            // Gebruikt die Beschrijving en zet het om in Info
            List<UserData> tempProcessedUserDataList = LogicManager.ParseUserDescription(tempActiveUserList);

            // Pakt de users zijn pad via de SID nummer
            LogicManager.GetPathFromSIDNumber(tempProcessedUserDataList);

            // Maakt nieuwe directory/signatures folders aan
            if (Settings.blnMakeSignatureDirectory)
                CreateSignaturesDirectory(tempProcessedUserDataList);

            bool blnIsSignatureFolderFound = false;
            if (CheckLocationOfDirectory(Settings.strSignatureDirectoryName))
            {
                blnIsSignatureFolderFound = true;
                Log.Information("SIGNATURES FOLDER GEVONDEN");
                // Verwijderd alle signatures van de inactive Users
                DataManager.DeleteSignaturesOfInactiveUsers(tempInactiveUserList, tempAllUsersList);

                if (Settings.blnDeleteAllSignatures)
                    // Verwijderd alle signatures van alle Users
                    DataManager.DeleteAllSignatures(tempProcessedUserDataList, tempAllUsersList);

                if (Settings.blnCreateSignatures && !Settings.blnDeleteAllSignatures)
                {
                    // Past de Users zijn signature aan met de gegeven Info
                    DataManager.ModifySignatures(tempProcessedUserDataList, tempActiveUserList);
                    DataManager.UpdateSignatures(tempProcessedUserDataList, tempActiveUserList);
                }
                if (Settings.blnCopySignatures)
                    DataManager.CopyAndPasteSignatures();
            }
            if (!blnIsSignatureFolderFound)
            {
                Log.Error($"\\{Settings.strSignatureDirectoryName}\\ FOLDER NIET KUNNEN VINDEN");
                if (!Settings.blnMakeSignatureDirectory)
                {
                    Log.Error("Geen nieuwe Signature Directory's kunnen maken, omdat blnMakeSignatureDirectory op FALSE staat");
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex.Message);
            Environment.Exit(1);
        }
    }
    public static void MakeSignaturesForDomain()
    {
        // Active Directory gebruikers
        try
        {
            Log.Verbose("is bij MakeSignaturesforDomain gekomen");
            // Pakt de volle lijst met Users
            List<UserData> tempAllUsersList = DataManager.GetAllADUsersList();
            Log.Information("ALLE USERS");
            Log.Information($"{tempAllUsersList.Count} User(s)");
            foreach (UserData User in tempAllUsersList) Log.Verbose($"{User.UserName}");
            Console.WriteLine("\n\r");

            // Pakt de users zijn pad via de SID nummer
            LogicManager.GetPathFromSIDNumber(tempAllUsersList);

            // Pakt alle Inactive Users
            List<UserData> tempInactiveADUserList = DataManager.GetInactiveADUserList();
            Log.Information("INACTiVE USERS");
            Log.Information($"{tempInactiveADUserList.Count} Inactive user(s)");
            foreach (UserData User in tempInactiveADUserList) Log.Verbose($"{User.UserName}");
            Console.WriteLine("\n\r");

            // Pakt de users zijn pad via de SID nummer
            LogicManager.GetPathFromSIDNumber(tempInactiveADUserList);

            // Pakt alle Active Users
            List<UserData> tempActiveADUsersWithDescriptionList = DataManager.GetActiveADUsersWithDescriptionList();
            Log.Information("DESCRIPTION OF ACTIVE USERS");
            Log.Information($"{tempActiveADUsersWithDescriptionList.Count} Correcte description(s)");
            foreach (UserData User in tempActiveADUsersWithDescriptionList) Log.Verbose($"{User.Description}");
            Console.WriteLine("\n\r");

            // Pakt de users zijn pad via de SID nummer
            LogicManager.GetPathFromSIDNumber(tempActiveADUsersWithDescriptionList);

            // Gebruikt die Beschrijving en zet het om in Info met Regex
            List<UserData> tempProcessedUserDataList = LogicManager.ParseUserDescription(tempActiveADUsersWithDescriptionList);

            // Pakt de users zijn pad via de SID nummer
            LogicManager.GetPathFromSIDNumber(tempProcessedUserDataList);

            // Maakt nieuwe directory/signatures folders aan
            if (Settings.blnMakeSignatureDirectory)
                CreateSignaturesDirectory(tempProcessedUserDataList);

            bool blnIsSignatureFolderFound = false;
            if (CheckLocationOfDirectory(Settings.strSignatureDirectoryName))
            {
                Log.Verbose("Signatures folder gevonden");
                // Verwijderd alle signatures van de inactive Users
                DataManager.DeleteSignaturesOfInactiveUsers(tempInactiveADUserList, tempAllUsersList);

                // Verwijderd alle signatures van alle Users
                if (Settings.blnDeleteAllSignatures)
                    DataManager.DeleteAllSignatures(tempProcessedUserDataList, tempAllUsersList);

                if (Settings.blnCreateSignatures && !Settings.blnDeleteAllSignatures)
                {
                    // Past de Users zijn signature aan met de gegeven Info
                    DataManager.ModifySignatures(tempProcessedUserDataList, tempActiveADUsersWithDescriptionList);
                    DataManager.UpdateSignatures(tempProcessedUserDataList, tempActiveADUsersWithDescriptionList);
                }
                blnIsSignatureFolderFound = true;
                if (Settings.blnCopySignatures)
                    DataManager.CopyAndPasteSignatures();
            }
            if (!blnIsSignatureFolderFound)
            {
                Log.Error($"\\{Settings.strSignatureDirectoryName}\\ FOLDER NIET KUNNEN VINDEN");
                if (!Settings.blnMakeSignatureDirectory)
                {
                    Log.Error("Geen nieuwe Signature Directory's kunnen maken, omdat blnMakeSignatureDirectory op FALSE staat");
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex.Message);
            Environment.Exit(1);
        }
    }
}