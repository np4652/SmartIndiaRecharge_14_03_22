using Fintech.AppCode.Model.Reports;
using System.Collections.Generic;


namespace RoundpayFinTech.AppCode.Model.ProcModel
{
    
    public class Device
    {
        public string BankName { get; set; }
        public string Logo { get; set; }
        public string BankQRLogo { get; set; }
        public string CID { get; set; }
        public int EntryBy { get; set; }
        public int LT { get; set; }
        public List<BankMaster> BankMasters { get; set; }
        public string QRPath { get; set; }
        public int PreStatusofQR { get; set; }
        public List<PaymentModeMaster> Mode { get; set; }
    }
    public class DeviceMaster
    {
        public int ID { get; set; }
        public string DeviceName { get; set; }
        public string Remark { get; set; }
        public bool IsActive { get; set; }
        public string Img { get; set; }

    }
    public class DeviceSaveResp
    {
        public bool Status { get; set; }
        public string Msg { get; set; }
    }
    
}
