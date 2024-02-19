﻿using HDSignaturesTool.Data;
using HDSignaturesTool.Types;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDSignaturesTool.Logic
{
    internal partial class LogicManager
    {
        public static void MakeSignaturesForLocal()
        {
            try
            {
                // Lokaal gebruikers
                var tempUserList = GetUsersFromLocal();
                ModifySignaturesForLocal(tempUserList);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                Environment.Exit(1);
            }
        }
        public static UserLists GetUsersFromLocal()
        {
            UserLists userLists = new UserLists();

            try
            {
                // Pakt de volle lijst met Users
                List<UserData> tempAllUsersList = DataManager.GetAllLocalUsersList();
                Log.Information("ALLE USERS");
                Log.Information($"{tempAllUsersList.Count} User(s)");
                foreach (UserData User in tempAllUsersList) Log.Verbose($"{User.UserName}");

                // Pakt de users zijn pad via de SID nummer
                DataManager.GetPathFromSIDNumber(tempAllUsersList);
                userLists.AllUsersList = tempAllUsersList;

                // Pakt alle Inactive Users
                List<UserData> tempInactiveUserList = DataManager.GetInactiveLocalUsersList();
                Log.Information("INACTiVE USERS");
                Log.Information($"{tempInactiveUserList.Count} Inactive user(s)");
                foreach (UserData User in tempInactiveUserList) Log.Verbose($"INACTIVE: {User.UserName}");

                // Pakt de users zijn pad via de SID nummer
                DataManager.GetPathFromSIDNumber(tempInactiveUserList);
                userLists.InactiveUsersList = tempInactiveUserList;

                // Pakt alle Active Users
                List<UserData> tempActiveUserList = DataManager.GetActiveLocalUsersList();
                Log.Information("ACTIVE USERS");
                Log.Information($"{tempActiveUserList.Count} Active user(s)");
                foreach (UserData User in tempActiveUserList) Log.Verbose($"ACTIVE: {User.UserName}");

                // Pakt de users zijn pad via de SID nummer
                DataManager.GetPathFromSIDNumber(tempActiveUserList);
                userLists.ActiveUsersLists = tempActiveUserList;

                List<UserData> tempProcessedUserDataList = LogicManager.ParseUsersDescription(tempActiveUserList);
                userLists.ProcessedUsersList = tempProcessedUserDataList;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                Environment.Exit(1);
            }
            return userLists;
        }
        public static void ModifySignaturesForLocal(UserLists userLists)
        {
            try
            {
                // Maakt nieuwe directory/signatures folders aan
                if (Settings.blnMakeSignatureDirectory)
                    DataManager.CreateSignaturesDirectory(userLists.ProcessedUsersList);

                bool blnIsSignatureFolderFound = false;
                // Verwijderd alle signatures van de inactive Users
                DataManager.DeleteSignaturesOfInactiveUsers(userLists.InactiveUsersList, userLists.AllUsersList);

                if (Settings.blnDeleteAllSignatures)
                    // Verwijderd alle signatures van alle Users
                    DataManager.DeleteAllSignatures(userLists.ProcessedUsersList, userLists.AllUsersList);

                if (Settings.blnCreateSignatures && !Settings.blnDeleteAllSignatures)
                {
                    // Past de Users zijn signature aan met de gegeven Info
                    DataManager.ModifySignatures(userLists.ProcessedUsersList, userLists.ActiveUsersLists);
                    DataManager.UpdateSignatures(userLists.ProcessedUsersList, userLists.ActiveUsersLists);
                }

                if (Settings.blnCopySignatures)
                    DataManager.CopyAndPasteSignatures();

                blnIsSignatureFolderFound = true;

                if (!blnIsSignatureFolderFound)
                {
                    Log.Error($"\\{Settings.strSignatureDirectoryName}\\ FOLDER NIET KUNNEN VINDEN");

                    if (!Settings.blnMakeSignatureDirectory)
                        Log.Error("Geen nieuwe Signature Directory's kunnen maken, omdat blnMakeSignatureDirectory op FALSE staat");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                Environment.Exit(1);
            }
        }
    }
}
