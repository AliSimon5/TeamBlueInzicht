using M.NetStandard.CommonInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDSignaturesTool.Types
{
    public class UserData : BoxItem
    {
        public string UserName;
        public string Description;
        public string SID;
        public Dictionary<string, string> UserValueDictionary = new Dictionary<string, string>();
        public string UserPath;
        public string FullUserPath;
        public bool MakeSignature;

        public string GetBoxItemTitle()
        {
            return UserName;
        }
    }
}
