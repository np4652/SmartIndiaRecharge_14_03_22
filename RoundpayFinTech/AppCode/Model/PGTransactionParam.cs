using RoundpayFinTech.AppCode.Model.Paymentgateway;
using RoundpayFinTech.AppCode.Model.ProcModel;

namespace RoundpayFinTech.AppCode.Model
{
    public class PGTransactionParam: PGTransactionResponse
    {
        public string VendorID { get; set; }
        public string LiveID { get; set; }
        public string Remark { get; set; }
        public string PAYMENTMODE { get; set; }
        public string Checksum { get; set; }
        public string Signature { get; set; }
        public int UPGID { get; set; }
        public int RequestMode { get; set; }
        public int Status { get; set; }
    }
}
