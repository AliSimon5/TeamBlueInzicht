using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using HDSignatures.Types;

namespace HDSignatures.Data
{
    internal partial class DataManager
    {
        /// <summary>
        /// Pakt alle local Users en voegt ze toe aan een list
        /// </summary>
        /// <returns></returns>
        public static List<UserData> GetLocalUserList()
        {
            // maakt die list een variable
            var tempAllUserList = new List<UserData>();
            // zorgt ervoor dat hij alle users zoekt
            SelectQuery query = new SelectQuery("Win32_UserAccount");
            // pakt alle users en zet ze in searcher
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
            // voor elke User die hij vindt voegt hij hem toe aan tempAllUsersList
            foreach (ManagementObject User in searcher.Get())
            {
                // slaat de Username en Description op
                UserData userData = new UserData();
                userData.UserName = (string)User["Name"];
                userData.SID = (string)User["SID"];
                tempAllUserList.Add(userData);
            }
            return tempAllUserList;
        }

        /// <summary>
        /// Scheid de inactive Users en zet ze in een aparte list
        /// </summary>
        /// <param name="tempAllUserList">Alle Users</param>
        /// <returns></returns>
        public static List<UserData> GetInactiveLocalUsers(List<UserData> tempAllUserList)
        {
            var tempInactiveUserList = new List<UserData>();
            // Voor elke tempUserData die hij heeft gevonden in searcher pakt hij
            SelectQuery query = new SelectQuery("Win32_UserAccount");
            // pakt alle users en zet ze in searcher
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
            // voor elke User die hij vindt voegt hij hem toe aan tempAllUsersList
            foreach (ManagementObject User in searcher.Get())
            {
                // slaat de Username en Description op
                bool output = (bool)User["Disabled"];
                if (output)
                {
                    UserData userData = new UserData();
                    userData.UserName = (string)User["Name"];
                    userData.Description = (string)User["Description"];
                    userData.SID = (string)User["SID"];
                    tempInactiveUserList.Add(userData);
                }
            }
            return tempInactiveUserList;
        }

        /// <summary>
        /// Scheid de Active Users en zet ze in een aparte list
        /// </summary>
        /// <param name="tempAllUserList">Alle Users</param>
        /// <returns></returns>
        public static List<UserData> GetActiveLocalUsers(List<UserData> tempAllUserList)
        {
            var tempActiveUserList = new List<UserData>();

            // Voor elke tempUserData die hij heeft gevonden in searcher pakt hij
            SelectQuery query = new SelectQuery("Win32_UserAccount");
            // pakt alle users en zet ze in searcher
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
            // voor elke User die hij vindt voegt hij hem toe aan tempAllUsersList
            foreach (ManagementObject User in searcher.Get())
            {
                // Checkt of de User Enabled(True) is
                bool output = (bool)User["Disabled"];
                if (!output)
                {
                    UserData userData = new UserData();
                    userData.UserName = (string)User["Name"];
                    userData.Description = (string)User["Description"];
                    userData.SID = (string)User["SID"];
                    // Checkt of de user de correcte Beschrijving heeft
                    if (userData.Description == null || !userData.Description.Contains(Settings.strBasisNaam, StringComparison.OrdinalIgnoreCase))
                        continue;
                    // slaat de Username en Description op
                    tempActiveUserList.Add(userData);
                }
            }
            return tempActiveUserList;
        }
    }
}
