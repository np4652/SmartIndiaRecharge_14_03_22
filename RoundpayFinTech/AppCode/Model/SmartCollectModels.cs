using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model
{
    public class UserSmartDetailModel {
        public List<UserSmartDetail> USDList { get; set; }
        public string Name { get; set; }
        public string EmailID { get; set; }
        public string MobileNo { get; set; }
        public string GSTIN { get; set; }
    }
    public class UserSmartDetail
    {
        public int SmartCollectTypeID { get; set; }
        public string SmartCollectType { get; set; }
        public string CustomerID { get; set; }
        public string SmartAccountNo { get; set; }
        public string SmartVPA { get; set; }
        public string SmartQRShortURL { get; set; }
        public string BankName { get; set; }
        public string IFSC { get; set; }
    }
    public class SmartCollectCreateCustomerRequest
    {
        public string CustomerID { get; set; }
        public string Name { get; set; }
        public string Contact { get; set; }
        public string EmailID { get; set; }
        public string GSTIN { get; set; }
        public string NotesKey1 { get; set; }
        public string NotesKey2 { get; set; }
        
    }
    public class SmartCollectCreateCustomerResponse
    {
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public string CustomerID { get; set; }
    }
    public class SmartCollectionVACResponse
    {
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public string AccounID { get; set; }
        public string AccountNumber { get; set; }
        public string QRCodeID { get; set; }
        public string QRShortURL { get; set; }
        public string VPAId { get; set; }
        public string VPAAddress { get; set; }
        public string CustomerID { get; set; }
    }
    public class UpdateSmartCollectRequestModel {
        public int LoginID { get; set; }
        public int UserID { get; set; }
        public int SmartCollectTypeID { get; set; }
        public string CustomerID { get; set; }
        public string SmartAccountNo { get; set; }
        public string SmartVPA { get; set; }
        public string SmartQRShortURL { get; set; }
    }
}
