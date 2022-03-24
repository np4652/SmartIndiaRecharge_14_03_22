using RoundpayFinTech.AppCode.Model.ProcModel;
using System.Collections.Generic;

namespace RoundpayFinTech.AppCode.Model.Reports.Filter
{
    public class ULFundReceiveReportFilter
    {
        public int ServiceID { get; set; }
        public int LoginId { get; set; }
        public string MobileNo { get; set; }
        public string TID { get; set; }
        public string FDate { get; set; }
        public string TDate { get; set; }
        public int UserID { get; set; }
        public bool IsSelf { get; set; }
        public int WalletTypeID { get; set; }
        public string OtherUserMob { get; set; }
        public int OtherUserID { get; set; }
        public int LT { get; set; }
    }
    public class FundReciveModel {
        public string LoginMob { get; set; }
        public IEnumerable<WalletType> Walletes { get; set; }
        public List<ProcFundReceiveStatementResponse> ProcFundReceiveStatementResponses { get; set; }
        public int ServiceID { get; set; }
    }
}
