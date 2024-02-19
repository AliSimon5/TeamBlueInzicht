﻿using M.Core.Application.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDSignatures.Types
{
    public class Settings : AutoSettings
    {
        public static bool blnUsersInAD { get; set; } = false;
        public static string strSchijf { get; set; } = "C";
        public static string strUsersPathName { get; set; } = "Users";
        public static string strAdminNaam { get; set; } = "Administrator";

        public static string strSignatureDirectoryName { get; set; } = "Signatures";
        public static bool blnMakeSignatureDirectory { get; set; } = true;

        public static bool blnUserGetsAllSignatures { get; set; } = true;
        public static string strFilePrefix { get; set; } = "";
        public static string strOrigineleFile { get; set; } = "WensTravel";
        public static bool blnCreateFolderFiles { get; set; } = true;
        public static bool blnCreateFolderBestanden { get; set; } = false;
        public static string strBasisNaam { get; set; } = "Naam";
        public static bool blnDeleteInactiveSignatures { get; set; } = true;
        public static bool blnDeleteAllSignatures { get; set; } = false;
        public static bool blnCreateSignatures = true;

        public static bool blnCopySignatures { get; set; } = true;
        public static string strCopySignaturesToPath { get; set; } = "D:\\Handtekeningentest\\SignaturesTest";

        public static bool blnAdminOnly { get; set; } = false;
        public static bool blnUpdateSignatures = true;
        public static bool blnCreateHtm { get; set; } = true;
        public static bool blnCreateRtf { get; set; } = true;
        public static bool blnCreateTxt { get; set; } = true;
    }
}
