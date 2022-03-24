using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.ThirdParty.RapiPay
{
    public class RapiPayOnboardRequest
    {
        public string customerMobileNO { get; set; }
        public string typeMobileWeb { get; set; }
        public string txnRef { get; set; }
        public string kycType { get; set; }
        public string clientIp { get; set; }
        public string createdSource { get; set; }
        public string agentType { get; set; }
        public string requestType { get; set; }
        public string createdBy { get; set; }
        public string browserFingurePrint { get; set; }
        public string shopLatitude { get; set; }
        public string shopLongitude { get; set; }
        public string shopName { get; set; }
        public string shopstateName { get; set; }
        public string shopPostalPinCode { get; set; }
        public string shopaddress { get; set; }
        public string address { get; set; }
        public string city { get; set; }
        public string districtName { get; set; }
        public string dob { get; set; }
        public string pinCode { get; set; }
        public string documentType { get; set; }
        public string documentId { get; set; }
        public string pancardId { get; set; }
        public string firstName { get; set; }
        public string middleName { get; set; }
        public string lastName { get; set; }
        public string companyName { get; set; }
        public string state { get; set; }
        public string emailID { get; set; }
        public string fullname { get; set; }
        public string shopCityName { get; set; }
        public string shopDistrictName { get; set; }
        public string reqFor { get; set; }
        public string timeStamp { get; set; }
        public string pancardImg { get; set; }
        public string documentName2 { get; set; }
        public string documentName3 { get; set; }
        public string documentName { get; set; }
        public string agentSign { get; set; }
    }
}
