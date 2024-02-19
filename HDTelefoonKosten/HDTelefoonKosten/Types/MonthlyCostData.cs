using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDTelefoonKosten.Types
{
    public class MonthlyCostData
    {
        public string CompanyName { get; set; }
        public string TotalCalls { get; set; }
        public string TotalIns { get; set; }
        public string TotalOuts { get; set; }
        public string TotalCallTime { get; set; }
        public string TotalAmount { get; set; }
        public string FirstDate { get; set; }
        public string LastDate { get; set; }
        public string TimePeriod { get; set; }

        public string Subscriber { get; set; }
        public string YearOrMonth { get; set; }

        public string CostPerMinuteMobiel { get; set; }
        public string StartRateMobiel { get; set; }
        public string CostPerMinuteBinnenLand { get; set; }
        public string StartRateBinnenLand { get; set; }
        public string Voorschot { get; set; }
        public string TekortVoorschot { get; set; }

    }
}
