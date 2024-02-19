using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDListFolders.Logic
{
    internal partial class LogicManager
    {
        public static string ParsePath(string argFullPath, string argMainPath)
        {
            var tempParsedPath = argFullPath.Replace(argMainPath, string.Empty);
            return tempParsedPath;
        }
    }
}
