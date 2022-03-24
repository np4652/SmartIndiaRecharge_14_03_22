using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model.Employee
{
    public class Purpuse
    {
        public int PurpuseID { get; set; }
        public string PurpuseDetail { get; set; }
    }

    public class ReasonToUseOBrand
    {
        public int ReasonID { get; set; }
        public string Reason { get; set; }
    }

    public class ReasonAndPurpuse
    {
        public int Id { get; set; }
        public IEnumerable<Purpuse> Purpuse { get; set; }
        public IEnumerable<ReasonToUseOBrand> Resaon { get; set; }
    }
}
