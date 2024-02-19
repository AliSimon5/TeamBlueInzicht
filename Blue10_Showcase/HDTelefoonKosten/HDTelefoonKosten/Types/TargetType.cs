using M.NetStandard.CommonInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDTelefoonKosten.Types
{
    public class TargetType : BoxItem
    {
        public string Target { get; set; }
        public string ID { get; set; }

        public string GetBoxItemTitle()
        {
            return Target;
        }
    }
}
