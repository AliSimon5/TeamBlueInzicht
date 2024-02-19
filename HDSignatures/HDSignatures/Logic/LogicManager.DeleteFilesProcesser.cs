﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDSignatures.Logic
{
    internal partial class LogicManager
    {
        public static bool DeleteIfFileExists(string argpath)
        {
            if (File.Exists(argpath))
            {
                File.Delete(argpath);
                return true;
            }
            return false;
        }
        public static bool DeleteIfDirectoryExists(string argpath)
        {
            if (Directory.Exists(argpath))
            {
                Directory.Delete(argpath, true);
                return true;
            }
            return false;
        }
    }
}