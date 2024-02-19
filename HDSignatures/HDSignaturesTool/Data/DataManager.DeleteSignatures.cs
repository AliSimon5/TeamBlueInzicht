using HDSignaturesTool.Helpers;
using HDSignaturesTool.Logic;
using HDSignaturesTool.Types;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HDSignaturesTool.Data
{
    internal partial class DataManager
    {
        public static void DeleteSignaturesOfInactiveUsers(List<UserData> tempInactiveUserList, List<UserData> tempAllUserList)
        {
            if (Settings.blnDeleteInactiveSignatures)
            {
                foreach (var tempInactiveUser in tempInactiveUserList)
                {
                    try
                    {
                        var tempDescription = tempInactiveUser.Description;

                        if (!tempDescription.Contains(Settings.strBasisNaam))
                            continue;

                        var tempSplitDescription = tempDescription.Split(';');
                        var tempDictionary = new Dictionary<string, string>();

                        foreach (var tempItem in tempSplitDescription)
                        {
                            var tempSplitItem = tempItem.Split('=');
                            if (tempSplitItem.Length >= 2)
                            {
                                if (!tempDictionary.ContainsKey(tempSplitItem[0]))
                                    tempDictionary.Add(tempSplitItem[0], tempSplitItem[1]);
                                Regex.Match(tempSplitItem[0], Settings.strBasisNaam);
                                
                            }
                        }

                        Log.Information($"SIGNATURES VAN {tempInactiveUser.UserName} WORDEN VERWIJDERD");

                        // Pakt de naam van de User

                        // Voor elke User gaat hij checken of er signatures van de disabled user in de bestand zit zowel verwijderd hij hem zo niet doet hij niks
                        foreach (var tempUser in tempAllUserList)
                        {
                            bool tempDeleted = false;
                            //if (!Directory.Exists($"{tempUser.UserPath}\\AppData\\Roaming\\Microsoft\\{Settings.strSignatureDirectoryName}"))
                               // continue;
                            // Het pad naar de User met de signature van de disabled user
                            tempDeleted = DeleteIfExists.DeleteIfFileExists($"{tempUser.UserPath}\\AppData\\Roaming\\Microsoft\\{Settings.strSignatureDirectoryName}\\{Settings.strFilePrefix}{tempInactiveUser.UserName.Replace(" ", "")}.txt") ? true : tempDeleted;
                            tempDeleted = DeleteIfExists.DeleteIfFileExists($"{tempUser.UserPath}\\AppData\\Roaming\\Microsoft\\{Settings.strSignatureDirectoryName}\\{Settings.strFilePrefix}{tempInactiveUser.UserName.Replace(" ", "")}.rtf") ? true : tempDeleted;
                            tempDeleted = DeleteIfExists.DeleteIfFileExists($"{tempUser.UserPath}\\AppData\\Roaming\\Microsoft\\{Settings.strSignatureDirectoryName}\\{Settings.strFilePrefix}{tempInactiveUser.UserName.Replace(" ", "")}.htm") ? true : tempDeleted;
                            tempDeleted = DeleteIfExists.DeleteIfDirectoryExists($"{tempUser.UserPath}\\AppData\\Roaming\\Microsoft\\{Settings.strSignatureDirectoryName}\\{Settings.strFilePrefix}{tempInactiveUser.UserName.Replace(" ", "")}_files") ? true : tempDeleted;
                            tempDeleted = DeleteIfExists.DeleteIfDirectoryExists($"{tempUser.UserPath}\\AppData\\Roaming\\Microsoft\\{Settings.strSignatureDirectoryName}\\{Settings.strFilePrefix}{tempInactiveUser.UserName.Replace(" ", "")}_bestanden") ? true : tempDeleted;

                            if (tempDeleted) Log.Verbose($"{tempInactiveUser.UserName} - Signatures zijn verwijderd bij -> {tempUser.UserName}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex.Message);
                    }
                }
            }
        }
        public static void DeleteAllSignatures(List<UserData> tempProcessedUserDataList, List<UserData> tempAllUsersList)
        {
            if (Settings.blnDeleteAllSignatures)
            {
                // Eerst een loopje voor elke user die gevonden
                foreach (var tempUser in tempProcessedUserDataList)
                {
                    try
                    {
                        // Pakt de Users zijn hele description
                        var tempDescription = tempUser.Description;

                        // Checkt of de Basisnaam in de description zit zo niet gaat hij naar volgende user zo wel gaat hij door
                        if (!tempDescription.ToUpper().Contains(Settings.strBasisNaam.ToUpper()))
                            continue;
                        var tempSplitDescription = tempDescription.Split(';');
                        var tempDictionary = new Dictionary<string, string>();

                        foreach (var tempItem in tempSplitDescription)
                        {
                            var tempSplitItem = tempItem.Split('=');
                            if (tempSplitItem.Length >= 2)
                            {
                                if (!tempDictionary.ContainsKey(tempSplitItem[0]))
                                    tempDictionary.Add(tempSplitItem[0], tempSplitItem[1]);
                            }
                        }
                        Log.Information($"SIGNATURES VAN {tempUser.UserName} WORDEN VERWIJDERD");

                        // Pakt de naam van de User

                        // Voor elke User gaat hij checken of er signatures van de disabled user in de bestand zit zowel verwijderd hij hem zo niet doet hij niks
                        bool tempDeleted = false;

                        foreach (var tempAllUsers in tempAllUsersList)
                        {
                            // Het pad naar de User met de signature van de disabled user
                            tempDeleted = DeleteIfExists.DeleteIfFileExists($"{tempAllUsers.UserPath}\\AppData\\Roaming\\Microsoft\\{Settings.strSignatureDirectoryName}\\{Settings.strFilePrefix}{tempUser.UserName.Replace(" ", "")}.txt") ? true : tempDeleted;
                            tempDeleted = DeleteIfExists.DeleteIfFileExists($"{tempAllUsers.UserPath}\\AppData\\Roaming\\Microsoft\\{Settings.strSignatureDirectoryName}\\{Settings.strFilePrefix}{tempUser.UserName.Replace(" ", "")}.rtf") ? true : tempDeleted;
                            tempDeleted = DeleteIfExists.DeleteIfFileExists($"{tempAllUsers.UserPath}\\AppData\\Roaming\\Microsoft\\{Settings.strSignatureDirectoryName}\\{Settings.strFilePrefix}{tempUser.UserName.Replace(" ", "")}.htm") ? true : tempDeleted;
                            tempDeleted = DeleteIfExists.DeleteIfDirectoryExists($"{tempAllUsers.UserPath}\\AppData\\Roaming\\Microsoft\\{Settings.strSignatureDirectoryName}\\{Settings.strFilePrefix}{tempUser.UserName.Replace(" ", "")}_files") ? true : tempDeleted;
                            tempDeleted = DeleteIfExists.DeleteIfDirectoryExists($"{tempAllUsers.UserPath}\\AppData\\Roaming\\Microsoft\\{Settings.strSignatureDirectoryName}\\{Settings.strFilePrefix}{tempUser.UserName.Replace(" ", "")}_bestanden") ? true : tempDeleted;
                            if (tempDeleted) Log.Verbose($"{tempUser.UserName} - Signatures zijn verwijderd bij -> {tempAllUsers.UserName}");

                        }

                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex.Message);
                    }
                }
            }
        }
    }
}
