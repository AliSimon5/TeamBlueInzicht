using M.NetStandard.CommonInterfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDSignaturesTool.Types
{
    internal class BackupType : BoxItem
    {
        public string strFolderPath;
        public string strFolderName;
        public BackupType(string argFolderPath)
        {
            strFolderPath = argFolderPath;
            strFolderName = Path.GetFileName(strFolderPath);
        }
        public string GetBoxItemTitle()
        {
            return strFolderName;
        }
    }
}
