﻿using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HDSignatures
{
    public enum UserAccountControlFlag : int
    {
        SCRIPT = 0x0001,
        ACCOUNTDISABLE = 0x0002,
        HOMEDIR_REQUIRED = 0x0008,
        LOCKOUT = 0x0010,
        PASWD_NOTREQD = 0x0020,
        PASSWD_CANT_CHANGE = 0x0040,
        ENCRYPTED_TEXT_PWD_ALLOWED = 0x0080,
        TEMP_DUPLICATE_ACCOUNT = 0x0100,
        NORMAL_ACCOUNT = 0x0200,
        INTERDOMAIN_TRUST_ACCOUNT = 0x0800,
        WORKSTATION_TRUST_ACCOUNT = 0x1000,
        SERVER_TRUST_ACCOUNT = 0x2000,
        DONT_EXPIRE_PASSWORD = 0x10000,
        MNS_LOGON_ACCOUNT = 0x20000,
        SMARTCARD_REQUIRED = 0x40000,
        TRUSTED_FOR_DELEGATION = 0x80000,
        NOT_DELEGATED = 0x100000,
        USE_DES_KEY_ONLY = 0x200000,
        DONT_REQ_PREAUTH = 0x400000,
        PASSWORD_EXPIRED = 0x800000,
        TRUSTED_TO_AUTH_FOR_DELEGATION = 0x1000000
    }
    public enum GroupTypeFlag : int
    {
        BUILTIN_LOCAL_GROUP = 0x00000001,
        ACCOUNT_GROUP_GLOBAL = 0x00000002, // Global
        RESOURCE_GROUP_DOMAIN = 0x00000004, // MakeSignaturesForDomain
        UNIVERSAL_GROUP = 0x00000008,
        APP_BASIC_GROUP = 0x00000010,
        APP_QUERY_GROUP = 0x00000020,
        SECURITY_ENABLED = -0x80000000 // Als deze flag niet ingesteld is, dan wordt het een distribution group
    }
    internal class ActiveDirectoryManager
    {
        private string _strServer, _strDomain, _strUsername, _strPassword;

        public Action<string> actionLogActivity;


        public ActiveDirectoryManager(string argDomain, string argUsername, string argPassword)
        {
            _strDomain = argDomain;
            _strServer = @"LDAP://" + argDomain + "/";
            _strUsername = argUsername;
            _strPassword = argPassword;

        }

        #region Directory Entries / AD Objects
        public DirectoryEntry GetDirectoryEntry(string argDN)
        {
            // bv OU=CLOUD, DC=CLOUD, DC=LOCAL

            try
            {
                DirectoryEntry child = new DirectoryEntry(_strServer + argDN, _strUsername, _strPassword);

                return child;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, "ActiveDirectoryManager");
            }

            return null;

        }

        public bool DoesDNExist(string argDN)
        {
            // bv OU=CLOUD, DC=CLOUD, DC=LOCAL

            try
            {
                var tempBool = DirectoryEntry.Exists(_strServer + argDN);

                return tempBool;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, "ActiveDirectoryManager");
            }

            return false;

        }

        public List<string> GetChildrenOfDN(string argDN)
        {
            // bv OU=CLOUD, DC=CLOUD, DC=LOCAL

            try
            {
                var tempList = new List<string>();

                DirectoryEntry entry = new DirectoryEntry(_strServer + argDN, _strUsername, _strPassword);

                foreach (DirectoryEntry child in entry.Children)
                {
                    tempList.Add(child.Path.ToString().Replace(_strServer, ""));
                }

                return tempList;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, "ActiveDirectoryManager");
            }

            return new List<string>();

        }

        #endregion

        #region Object properties

        public Dictionary<string, string> GetPropertiesOfDN(string argDN)
        {
            // bv OU=CLOUD, DC=CLOUD, DC=LOCAL

            try
            {
                var tempDict = new Dictionary<string, string>();

                DirectoryEntry entry = new DirectoryEntry(_strServer + argDN, _strUsername, _strPassword);

                foreach (string strAttrName in entry.Properties.PropertyNames)
                {
                    if (entry.Properties[strAttrName].Value.ToString() == "System.__ComObject")
                    {
                        // tempDict.Add(strAttrName, AdsiUtils.AdsLongValue(entry.Properties[strAttrName].Value).ToString());
                    }
                    else if (entry.Properties[strAttrName].Value.ToString() == "System.Byte[]")
                    {
                        //tempDict.Add(strAttrName, entry.Properties[strAttrName].Value.ToString());
                    }
                    else if (entry.Properties[strAttrName].Value.ToString() == "System.Object[]")
                    {
                        object[] tempObj = (object[])entry.Properties[strAttrName].Value;
                        List<string> tempList = new List<string>();


                        foreach (var item in tempObj) tempList.Add(item.ToString());

                        string tempStr = JsonConvert.SerializeObject(tempList);

                        tempDict.Add(strAttrName, tempStr);
                    }
                    else tempDict.Add(strAttrName, entry.Properties[strAttrName].Value.ToString());
                }


                return tempDict;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message + "< " + argDN, "ActiveDirectoryManager");
            }

            return null;

        }

        #endregion
    }
}