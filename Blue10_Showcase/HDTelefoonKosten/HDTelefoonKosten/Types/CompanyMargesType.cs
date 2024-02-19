using M.NetStandard.CommonInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDTelefoonKosten.Types
{
    public class CompanyMargesType : BoxItem
    {
        public string CompanyName { get; set; }
        public double CompanyMarge { get; set; }
        public double CompanyVoorschot { get; set; }
        public string CompanyId { get; set; }
        public string CompanyNickname { get; set; }
        public string GetBoxItemTitle()
        {
            if (string.IsNullOrEmpty(CompanyNickname))
                return $"{CompanyName} ({CompanyMarge}%) (€ {CompanyVoorschot})";
            else
                return $"{CompanyNickname} ({CompanyMarge}%) (€ {CompanyVoorschot})";
        }
    }
}
