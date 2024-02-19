using HDTelefoonKosten.Types;
using M.NetStandard.CommonInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDTelefoonKosten
{
    public class CallData
    {
        public string ID { get; set; }  
        public string Direction { get; set; }
        public string Bedrijf { get; set; }
        public string Subscriber { get; set; }
        public string Originator { get; set; }
        public string Target { get; set; }
        public DateTime Date { get; set; }
        public double Duration { get; set; }

        public double Cost { get; set; }
        public double Marge { get; set; }
        public double CostWithMarge
        {
            get
            {
                return Cost * ((Marge + 100) / 100);
            }
            set
            {
                
            }
        }

        public static List<CallData> tempAllCallsList;

    }
}
