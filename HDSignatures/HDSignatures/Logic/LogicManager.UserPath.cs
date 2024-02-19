﻿using HDSignatures.Types;
using Microsoft.Win32;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDSignatures.Logic
{
    internal partial class LogicManager
    {
        public static void GetPathFromSIDNumber(List<UserData> tempUserDataList)
        {
            foreach (UserData tempUser in tempUserDataList)
            {
                RegistryKey key = Registry.LocalMachine.OpenSubKey(@$"SOFTWARE\Microsoft\Windows NT\CurrentVersion\ProfileList\{tempUser.SID}", false);
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
    }
}