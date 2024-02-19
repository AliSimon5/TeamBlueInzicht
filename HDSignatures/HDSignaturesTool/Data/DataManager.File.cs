using HDSignaturesTool.Types;
using Newtonsoft.Json;
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
        public static void WriteDataToFile(List<UserData> argListSavedData)
        {
            var tempSavedDataFilePath = GetSavedDataFilePath();

            var tempJSON = JsonConvert.SerializeObject(argListSavedData, Newtonsoft.Json.Formatting.Indented);

            File.WriteAllText(tempSavedDataFilePath, tempJSON);
        }
        public static string GetSavedDataFilePath()
        {
            var startupPath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            var tempSavedCompanyMarges = Path.Combine(startupPath, "HDSignatures.SavedData.json");
            return tempSavedCompanyMarges;
        }
        public static List<UserData> ReadSavedDataFile()
        {
            try
            {
                var tempSavedDataFilePath = GetSavedDataFilePath();

                var tempJSON = File.ReadAllText(tempSavedDataFilePath);
                var tempDeserializedSavedDataFileList = JsonConvert.DeserializeObject<List<UserData>>(tempJSON);

                return tempDeserializedSavedDataFileList;
            }
            catch (Exception ex)
            {
                return new List<UserData>();
            }
        }
        public static bool CheckForChanges(List<UserData> argUserList)
        {
            UserData tempFoundChanges = new UserData();
            var tempSavedDataList = ReadSavedDataFile();

            foreach (var tempUser in tempSavedDataList)
            {
                tempFoundChanges = argUserList.Find(x => x.UserName == tempUser.UserName && x.Description != tempUser.Description);
                if (tempFoundChanges != null)
                {
                    WriteDataToFile(argUserList);
                    return true;
                }
            }
            foreach (var tempUser in argUserList)
            {
                tempFoundChanges = tempSavedDataList.Find(x => x.UserName == tempUser.UserName);
                if (tempFoundChanges == null)
                {
                    WriteDataToFile(argUserList);
                    return true;
                }
            }
            if (tempSavedDataList.Count != argUserList.Count)
            {
                WriteDataToFile(argUserList);
                return true;
            }

            WriteDataToFile(argUserList);
            return false;
        }
    }
}
