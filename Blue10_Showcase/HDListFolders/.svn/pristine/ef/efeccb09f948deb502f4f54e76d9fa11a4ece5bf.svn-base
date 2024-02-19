using HDListFolders.Logic;
using M.NetStandard.CommonInterfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HDListFolders.Types
{
    public class FileData : BoxItem
    {
        public string strNewPath = string.Empty;
        public string FileName { get; set; }
        public string Path { get; set; }
        public string Type { get; set; }

        public string MainPath { get; set; }

        public string FileDate { get; set; }
        public string GetBoxItemTitle()
        {
            if (Path.Contains(MainPath))
                strNewPath = Path.Replace(MainPath, "");
            return $"{FileName} \n {strNewPath}\n {FileDate}";
        }
    }
}
