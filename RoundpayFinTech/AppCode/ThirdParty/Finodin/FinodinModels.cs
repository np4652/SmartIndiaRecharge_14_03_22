
using System;
using System.Collections.Generic;

namespace RoundpayFinTech
{
    public class FinodinAppSetting
    {
        public string BaseURL { get; set; }
        public string username { get; set; }
        public string password { get; set; }
    }
    public enum FinodinErrCode
    {
        success = 1,
        RemitterNotFound = 2,
        Error = 999,
        InSufficientFund = 101
    }
    public class ApiSources
    {
        public string PPBLMT { get; set; }
        public string NSDLMT { get; set; }
    }
    public class FinodinLoginResp
    {
        public int statusCode { get; set; }
        public string description { get; set; }
        public string token { get; set; }
        public ApiSources api_sources { get; set; }
    }
    public class FinodinResp
    {
        public string mobile { get; set; }
        public string remName { get; set; }
        public string remID { get; set; }
        public string benName { get; set; }
        public string benID { get; set; }
        public int remLimit { get; set; }
        public int statusCode { get; set; }
        public string description { get; set; }
        public List<FDBeneficiary> beneficiaries { get; set; }
        public FDTransaction transaction { get; set; }
    }
    public class FDTransaction
    {
        public string status { get; set; }
        public string tranID { get; set; }
        public int clientRefID { get; set; }
        public string rrn { get; set; }
        public string benName { get; set; }
        public string remarks { get; set; }
        public string error { get; set; }
    }

    public class FDBeneficiary
    {
        public string benID { get; set; }
        public string benName { get; set; }
        public string benMobile { get; set; }
        public string bank { get; set; }
        public string state { get; set; }
        public string city { get; set; }
        public string branch { get; set; }
        public string address { get; set; }
        public string ifscode { get; set; }
        public string accountnumber { get; set; }
    }

    public class FDAccTrResp
    {
        public List<FDTransaction> transaction { get; set; }
        public int statusCode { get; set; }
        public string description { get; set; }
    }


}
