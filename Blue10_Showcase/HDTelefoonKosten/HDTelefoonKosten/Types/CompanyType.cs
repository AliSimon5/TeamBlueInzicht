using M.Core.Application.WPF.MessageBox;
using M.NetStandard.CommonInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace HDTelefoonKosten.Types
{
    public class CompanyType : BoxItem
    {
        public string strCompany { get; set; } = string.Empty;
        public bool blnFileDoesntHaveCompanyName = false;
        public string strNickname { get; set; }
        public string GetBoxItemTitle()
        {
            if (!string.IsNullOrEmpty(strNickname))
                return strNickname;
            if (blnFileDoesntHaveCompanyName)
            {
                return $"{strCompany} (Geen bedrijfsnaam)";
            }
            return strCompany;
        }


        public string strSubscriber { get; set; }
        public string strId { get; set; }
        public string strTarget { get; set; }


    }
}
