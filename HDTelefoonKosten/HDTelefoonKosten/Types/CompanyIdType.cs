using M.NetStandard.CommonInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDTelefoonKosten.Types
{
    public class CompanyIdType : BoxItem
    {
        public string CompanyId { get; set; }
        public double CompanyMarge { get; set; }
        public double CompanyVoorschot { get; set; }
        public string CompanyName { get; set; }
        public string CompanyNickName { get; set; }
        public string GetBoxItemTitle()
        {
            if (!string.IsNullOrEmpty(CompanyNickName))
                return $"{CompanyId} / {CompanyNickName}";
            else return $"{CompanyId}";
        }
    }
}
