using HDSignaturesTool.Types;
using Microsoft.Win32;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDSignaturesTool.Data
{
    internal partial class DataManager
    {
        /// <summary>
        /// Pakt de pad naar de user via zijn SID nummer
        /// </summary>
        /// <param name="tempUserDataList"></param>
        public static void GetPathFromSIDNumber(List<UserData> tempUserDataList)
        {
            foreach (UserData tempUser in tempUserDataList)
            {
                RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\ProfileList\" + tempUser.SID, false);
                if (key?.GetValue("ProfileImagePath")?.ToString() != null)
                {
                    var strCheckForValue = key.GetValue("ProfileImagePath").ToString();
                    tempUser.UserPath = strCheckForValue.ToString();
                }
                else
                {
                    tempUser.UserPath = $"{Settings.strSchijf}:\\{Settings.strUsersPathName}\\{tempUser.UserName}";
                }
            }
        }
        /// <summary>
        /// Pakt de schijf soort van de gegeven pad
        /// </summary>
        /// <param name="argPath"></param>
        public static void GetSchijfFromPath(string argPath)
        {
            string[] tempSplittedPath = argPath.Split(':');
            if (tempSplittedPath[0].Length == 1)
                Settings.strSchijf = tempSplittedPath[0];
        }
    }
}
