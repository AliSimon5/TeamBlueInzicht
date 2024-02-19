using HDSignaturesTool.Types;
using HDSignaturesTool.Data;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HDSignaturesTool.Logic;

namespace HDSignaturesTool.Logic
{
    internal partial class LogicManager
    {
        public static async void MakeSignaturesForDomain()
        {
            try
            {
                // Active Directory gebruikers
                var tempUserList = await GetUsersFromAD();
                ModifySignaturesForAD(tempUserList);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                Environment.Exit(1);
            }
        }


        public async static Task<UserLists> GetUsersFromAD()
        {
            UserLists userLists = new UserLists();

            try
            {
                // Pakt de volle lijst met Users
                List<UserData> tempAllUsersList = await DataManager.GetAllADUsersList();
                Log.Information("ALLE USERS");
                Log.Information($"{tempAllUsersList.Count} User(s)");
                foreach (UserData User in tempAllUsersList) Log.Verbose($"{User.UserName}");

                // Pakt de users zijn pad via de SID nummer
                DataManager.GetPathFromSIDNumber(tempAllUsersList);
                userLists.AllUsersList = tempAllUsersList;

                // Pakt alle Inactive Users
                List<UserData> tempInactiveADUserList = DataManager.GetInactiveADUsersList();
                Log.Information("INACTiVE USERS");
                Log.Information($"{tempInactiveADUserList.Count} Inactive user(s)");
                foreach (UserData User in tempInactiveADUserList) Log.Verbose($"{User.UserName}");

                // Pakt de users zijn pad via de SID nummer
                DataManager.GetPathFromSIDNumber(tempInactiveADUserList);
                userLists.InactiveUsersList = tempInactiveADUserList;

                // Pakt alle Active Users
                List<UserData> tempActiveADUsersWithDescriptionList = DataManager.GetActiveADUsersList();
                Log.Information("DESCRIPTION OF ACTIVE USERS");
                Log.Information($"{tempActiveADUsersWithDescriptionList.Count} Correcte description(s)");
                foreach (UserData User in tempActiveADUsersWithDescriptionList) Log.Verbose($"{User.Description}");

                // Pakt de users zijn pad via de SID nummer
                DataManager.GetPathFromSIDNumber(tempActiveADUsersWithDescriptionList);
                userLists.ActiveUsersLists = tempActiveADUsersWithDescriptionList;

                // Gebruikt die Beschrijving en zet het om in Info met Regex
                List<UserData> tempProcessedUserDataList = LogicManager.ParseUsersDescription(userLists.ActiveUsersLists);
                userLists.ProcessedUsersList = tempProcessedUserDataList;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                Environment.Exit(1);
            }
            return userLists;
        }

        public static void ModifySignaturesForAD(UserLists userLists)
        {
            try
            {
                // Maakt nieuwe directory/signatures folders aan
                if (Settings.blnMakeSignatureDirectory)
                    DataManager.CreateSignaturesDirectory(userLists.ProcessedUsersList);

                bool blnIsSignatureFolderFound = false;

                // Verwijderd alle signatures van de inactive Users
                DataManager.DeleteSignaturesOfInactiveUsers(userLists.InactiveUsersList, userLists.AllUsersList);

                // Verwijderd alle signatures van alle Users
                if (Settings.blnDeleteAllSignatures)
                    DataManager.DeleteAllSignatures(userLists.ProcessedUsersList, userLists.AllUsersList);

                if (Settings.blnCreateSignatures && !Settings.blnDeleteAllSignatures)
                {
                    // Past de Users zijn signature aan met de gegeven Info
                    DataManager.ModifySignatures(userLists.ProcessedUsersList, userLists.ActiveUsersLists);
                    DataManager.UpdateSignatures(userLists.ProcessedUsersList, userLists.ActiveUsersLists);
                }

                blnIsSignatureFolderFound = true;

                if (Settings.blnCopySignatures)
                    DataManager.CopyAndPasteSignatures();

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
