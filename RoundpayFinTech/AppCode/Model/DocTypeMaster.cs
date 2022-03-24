using System.Collections.Generic;

namespace RoundpayFinTech.AppCode.Model
{
    public class DocTypeMaster
    {

        public int ID { get; set; }
        public int DocTypeID { get; set; }
        public string DocName { get; set; }
        public bool IsOptional { get; set; }
        public string Remark { get; set; }
        public string ModifyDate { get; set; }

        public int UserId { get; set; }
        public int StatusCode { get; set; }
        public string EntryDate { get; set; }
        public string DocUrl { get; set; }
        public string Description { get; set; }
        public int VerifyStatus { get; set; }
        public int LoginId { get; set; }
        public string DRemark { get; set; }
        public int IsOutlet { get; set; }
        public int LoginTypeID { get; set; }
        public int OutletID { get; set; }
        public int KYCStatus { get; set; }
        public string Msg { get; set; }
        //kycmodel
        public string KycData { get; set; }
        public string Role { get; set; }
        public string Name { get; set; }
        public string OutletName { get; set; }
        public string MobileNo { get; set; }
        public string StateName { get; set; }
        public string AADHAR { get; set; }
        public string PAN { get; set; }
        public string GSTIN { get; set; }
        public string PartnerName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Pincode { get; set; }
        public string Prefix { get; set; }
        public string Qualification { get; set; }
        public string ShopType { get; set; }
        public string LocationType { get; set; }
        public string Landmark { get; set; }
        public string BankName { get; set; }
        public string IFSC { get; set; }
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public string DOB { get; set; }
        public bool IsRegisteredWithGST { get; set; }
    }
    public class OutletKYCDetail
    {
        public int OutletID { get; set; }
        public int ID { get; set; }
        public int DocTypeID { get; set; }
        public string DocName { get; set; }
        public string Link { get; set; }
        public string FileName { get; set; }
        public int DocVerifyStatus { get; set; }
        public bool IsMandatory { get; set; }
        public string DRemark { get; set; }
        public string Remark { get; set; }
        public string ModifyDate { get; set; }
    }
    public class SenderKYCDetail
    {
        public int SenderID { get; set; }
        public int ID { get; set; }
        public int DocTypeID { get; set; }
        public string DocName { get; set; }
        public string Link { get; set; }
        public string FileName { get; set; }
        public int DocVerifyStatus { get; set; }
        public bool IsMandatory { get; set; }
        public string DRemark { get; set; }
        public string Remark { get; set; }
        public string ModifyDate { get; set; }
        public int EKOVerifyStatus { get; set; }
    }
    public class CommonProReq {
        public int LT { get; set; }
        public int LoginID { get; set; }
        public string IP { get; set; }
        public string Browser { get; set; }
        public int RequestModeID { get; set; }
    }
    public class DocumentReq: CommonProReq
    {
        public string URL { get; set; }
        public int DocType { get; set; }
        public int ChildUserID { get; set; }
        public int APIUserOutletID { get; set; }
    }
    public class KYCStatusReq : CommonProReq
    {
        public int OutletID { get; set; }
        public int KYCStatus { get; set; }
    }
}
