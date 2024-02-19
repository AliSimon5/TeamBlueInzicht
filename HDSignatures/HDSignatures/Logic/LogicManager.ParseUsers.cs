﻿using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using HDSignatures.Types;

namespace HDSignatures.Logic
{
    internal partial class LogicManager
    {
       
        /// <summary>
        /// Parsed de beschrijving/description van elke User die de correcte volgorde heeft
        /// </summary>
        /// <param name="argUserDataList">List met alle goedgekeurde beschrijvingen/descriptions</param>
        /// <returns></returns>
        public static List<UserData> ParseUserDescription(List<UserData> tempActiveUserList)
        {
            List<UserData> tempProcessedUserDataList = new List<UserData>();
            Log.Verbose("PARSING OF USERS");
            // voor elke beschrijving/description gaat hij langs alle regex functies
            foreach (UserData tempUserData in tempActiveUserList)
            {
                try
                {
                    var tempDescription = tempUserData.Description;

                    if (!tempDescription.Contains(Settings.strBasisNaam, StringComparison.OrdinalIgnoreCase))
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
                    tempUserData.UserValueDictionary = tempDictionary;
                    foreach (var item in tempUserData.UserValueDictionary)
                    {
                        Log.Verbose("Key: " + item.Key);
                        Log.Verbose("Value: " + item.Value);

                    }
                    tempProcessedUserDataList.Add(tempUserData);
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message);
                }
            }
            return tempProcessedUserDataList;
        }
    }
}