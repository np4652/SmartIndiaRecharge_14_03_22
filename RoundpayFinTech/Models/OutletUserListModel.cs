using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.Models
{
    public class OutletUserListModel : ReportModelCommon
    {
        public IEnumerable<OutletsOfUsersList> Report { get; set; }
    }
}
