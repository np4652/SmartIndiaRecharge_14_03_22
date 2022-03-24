using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model
{
    public class BenificiaryModel
    {
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public List<BenificiaryDetail> benificiaries { get; set; }
        public string VendorBeneID { get; set; }

    }
    public class BenificiaryDetail
    {
        public int _ID { get; set; }
        public int _SenderID { get; set; }
        public string _SenderMobileNo { get; set; }
        public string _Name { get; set; }
        public string _AccountNumber { get; set; }
        public string _MobileNo { get; set; }
        public string _IFSC { get; set; }
        public string _BankName { get; set; }
        public string _Branch { get; set; }
        public string _EntryDate { get; set; }
        public int _EntryBy { get; set; }
        public bool _DeleteStatus { get; set; }
        public string _ModifyDate { get; set; }
        public string _APICode { get; set; }
        public int _VerifyStatus { get; set; }
        public int _BankID { get; set; }
        public int _BeneAPIID { get; set; }
        public string _VendorBeneID { get; set; }
        public string _CashFreeID { get; set; }

    }
}
