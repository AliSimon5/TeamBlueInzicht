using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using HDSignaturesTool.Types;
using Microsoft.VisualBasic.ApplicationServices;

namespace HDSignaturesTool.Data
{
    internal partial class DataManager
    {
        /// <summary>
        /// Pakt alle local Users en voegt ze toe aan een list
        /// </summary>
        /// <returns></returns>
        public static List<UserData> GetAllLocalUsersList()
        {
            // maakt die list een variable
            var tempAllUserList = new List<UserData>();

            // zorgt ervoor dat hij alle users zoekt
            SelectQuery query = new SelectQuery("Win32_UserAccount");

            // pakt alle users en zet ze in searcher
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(query))
            {
                // voor elke User die hij vindt voegt hij hem toe aan tempAllUsersList
                foreach (ManagementObject User in searcher.Get())
                {
                    // slaat de Username en Description op
                    UserData userData = new UserData();

                    userData.UserName = (string)User["Name"];
                    userData.SID = (string)User["SID"];

                    tempAllUserList.Add(userData);
                    User.Dispose();
                }
            }
            return tempAllUserList;
        }

        /// <summary>
        /// Scheid de inactive Users en zet ze in een aparte list
        /// </summary>
        /// <returns></returns>
        public static List<UserData> GetInactiveLocalUsersList()
        {
            var tempInactiveUserList = new List<UserData>();
            // Voor elke userData die hij heeft gevonden in searcher pakt hij

            SelectQuery query = new SelectQuery("Win32_UserAccount");
            // pakt alle users en zet ze in searcher

            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(query))
            {
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
                    User.Dispose();
                }
            }
            return tempInactiveUserList;
        }

        /// <summary>
        /// Scheid de Active Users en zet ze in een aparte list
        /// </summary>
        /// <returns></returns>
        public static List<UserData> GetActiveLocalUsersList()
        {
            var tempActiveUserList = new List<UserData>();

            // Voor elke userData die hij heeft gevonden in searcher pakt hij
            SelectQuery query = new SelectQuery("Win32_UserAccount");

            // pakt alle users en zet ze in searcher
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(query))
            {
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
                        if (userData.Description == null || !userData.Description.ToUpper().Contains(Settings.strBasisNaam.ToUpper()))
                            continue;

                        // slaat de Username en Description op
                        tempActiveUserList.Add(userData);
                    }
                    User.Dispose();
                }
            }
            return tempActiveUserList;
        }
    }
}
