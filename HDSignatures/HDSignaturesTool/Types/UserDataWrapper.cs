﻿using M.NetStandard.CommonInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HDSignaturesTool.Types
{
    internal class UserDataWrapper : BoxItem
    {
        public string UserName_dg
        {
            get
            {
                return userData.UserName;
            }
            set
            {

            }
        }

        public string Description_dg
        {
            get
            {
                var tempText = string.Empty;
                foreach(var item in userData.UserValueDictionary)
                {
                    if (!string.IsNullOrEmpty(tempText)) tempText += "\n";
                    tempText += item.Key + ": " + item.Value;
                }
                return tempText;
            }
            set
            {

            }
        }
        public string Path_dg
        {
            get
            {
                userData.FullUserPath = $"{userData.UserPath}\\AppData\\Roaming\\Microsoft\\{Settings.strSignatureDirectoryName}\\{Settings.strFilePrefix}{userData.UserName.Replace(" ", "")}";

                return userData.FullUserPath;
            }
            set
            {

            }
        }
        public string blnCreate_dg
        {
            get
            {
                return blnCreate ? "✔" : "";
            }
            set
            {

            }
        }
        public bool blnCreate { get; set; } = true;

        public UserData userData = new UserData();
        public string GetBoxItemTitle()
        {
            return UserName_dg;
        }

        public static explicit operator UserDataWrapper(DataGridViewRow v)
        {
            throw new NotImplementedException();
        }
    }
}
