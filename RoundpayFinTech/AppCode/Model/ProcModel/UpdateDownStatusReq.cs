using Fintech.AppCode.Model;
using System.Collections.Generic;
using System.Data;

namespace RoundpayFinTech.AppCode.Model.ProcModel
{
    public class UpdateDownStatusReq
    {
        public int LT { get; set; }
        public int LoginID { get; set; }
        public DataTable Tp_IDStatus { get; set; }
        public bool IsEmail { get; set; }
        public bool IsSMS { get; set; }
        public List<DataKV> DataKVs { get; set; }
        public string IP { get; set; }
        public string Browser { get; set; }
    }
    public class DataKV
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
    public class UpdateDownStatus
    {
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public string UpOperators { get; set; }
        public string DownOperators { get; set; }
        public List<UserDetail> UserData { get; set; }
        public List<string> TransactionID { get; set; }
        public string UpMessage { get; set; }
        public string DownMessage { get; set; }
        public string SMSAPI { get; set; }
        public string Company { get; set; }
        public string CompanyAddress { get; set; }
        public string CompanyDomain { get; set; }
        public string AccountContactNumber { get; set; }
        public string AccountEmail { get; set; }
        public string SupportNumber { get; set; }
        public string SupportEmail { get; set; }
        public string OutletName { get; set; }
        public string UserName { get; set; }
        public string UserMobileNo { get; set; }
        public string BrandName { get; set; }
        public int SMSAPIID { get; set; }
        public int WID { get; set; }
        public int LoginID { get; set; }
    }
}
