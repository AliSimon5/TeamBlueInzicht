using M.NetStandard.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDSignaturesTool.Types
{
    public class Settings : AutoSettings
    {
        public static bool blnUsersInAD { get; set; } = false;
        public static bool blnUsersInLocal { get; set; } = false;
        public static string strSchijf { get; set; } = "";
        public static string strUsersPathName { get; set; } = "Users";
        public static string strAdminPath { get; set; } = "";

        public static string strSignatureDirectoryName { get; set; } = "";
        public static string strBedrijfsNaam { get; set; } = "";
        public static string strMailTo { get; set; } = "";
        public static string strMailFrom { get; set; } = "";
        public static bool blnMakeSignatureDirectory { get; set; } = false;

        public static int intSeconds { get; set; }
        public static int intCount { get; set; } = 0;
        public static bool blnUserGetsAllSignatures { get; set; } = false;
        public static string strFilePrefix { get; set; } = "";
        public static string strOrigineleFile { get; set; } = "";
        public static bool blnCreateFolderFiles { get; set; } = false;
        public static bool blnCreateFolderBestanden { get; set; } = false;
        public static string strBasisNaam { get; set; } = "";
        public static DateTime dtLastWriteTime { get; set; } = DateTime.Parse("1-1-0001 00:00:00");
        public static bool blnDeleteInactiveSignatures { get; set; } = false;
        public static bool blnDeleteAllSignatures { get; set; } = false;
        public static bool blnCreateSignatures = true;

        public static bool blnStartTimerOnStartup { get; set; } = false;
        public static bool blnRunApplicationOnStartup { get; set; } = false;
        public static bool blnStartAppOnStartup { get; set; } = false;
        public static bool blnCopySignatures { get; set; } = false;
        public static string strCopySignaturesToPath { get; set; } = "";

        public static bool blnAdminOnly { get; set; } = false;
        public static bool blnUpdateSignatures = true;
        public static bool blnCreateHtm { get; set; } = false;
        public static bool blnCreateRtf { get; set; } = false;
        public static bool blnCreateTxt { get; set; } = false;
        public static bool blnAllowMultipleInstances { get; set; } = false;
    }
}
