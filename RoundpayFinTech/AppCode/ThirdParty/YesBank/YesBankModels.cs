using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RoundpayFinTech.AppCode.ThirdParty.YesBank
{
    public class YesBankTransactionStatus
    {
        /**
         *  0 - Pending
            1 - NEFT initialized
            2 - Credited
            3 - Rejected
            4 - Refund Processed
            5 - Sender Registration
            6 - Failure
            7 - Card Registration
            8 - Initialized
         * **/
        public const int Pending = 0;
        public const int NEFT_initialized = 1;
        public const int Credited = 2;
        public const int Rejected = 3;
        public const int Refund_Processed = 4;
        public const int Sender_Registration = 5;
        public const int Failure = 6;
        public const int Card_Registration = 7;
        public const int Initialized = 8;
    }
    public class YesBankAppSetting
    {
        public string BCAGENT { get; set; }
        public string CPID { get; set; }
        public string USERNAME { get; set; }
        public string PASSWORD { get; set; }
        public string BASEURL { get; set; }
        public string S { get; set; }
        public string P { get; set; }
    }


    public class channelpartnerloginreqforekyc
    {
        public string username { get; set; }
        public string password { get; set; }
        public string bcagent { get; set; }
    }
    public class channelpartnerloginforekycres
    {
        public string sessionid { get; set; }
        public string timeout { get; set; }
        public int status { get; set; }
        public string wadh { get; set; }
    }
    public class YesHeader
    {
        public string sessionid { get; set; }
    }
    public class senderregistrationrmreq
    {
        public YesHeader header { get; set; }
        public string bcagent { get; set; }
        public string sendermobilenumber { get; set; }
        public string sendername { get; set; }
        public string senderaddress1 { get; set; }
        //public string lsenderaddress { get; set; }
        public string senderaddress2 { get; set; }
        public int pincode { get; set; }
        public string cityname { get; set; }
        public string statename { get; set; }
        public string alternatenumber { get; set; }
        public string idproof { get; set; }
        public string idproofnumber { get; set; }
        public string idproofissuedate { get; set; }
        public string idproofexpirydate { get; set; }
        public string idproofissueplace { get; set; }
        public string lsenderaddress { get; set; }
        public int lpincode { get; set; }
        public string lstatename { get; set; }
        public string lcityname { get; set; }
        public string channelpartnerrefno { get; set; }
        public string dob { get; set; }
    }
    public class senderregistrationvalidatereq
    {
        public YesHeader header { get; set; }
        public int senderid { get; set; }
        public string mobilenumber { get; set; }
        public int verficationcode { get; set; }
        public string channelpartnerrefno { get; set; }
    }
    public class senderregistrationvalidateres
    {
        public int remitterid { get; set; }
        public int status { get; set; }
        public int Enrollmentfee { get; set; }
    }
    public class senderregistrationresendotpreq
    {
        public YesHeader header { get; set; }
        public int senderid { get; set; }
    }
    public class senderregistrationresendotpres
    {
        public int status { get; set; }
    }
    public class receiverregistrationvalidatereq
    {
        public YesHeader header { get; set; }
        public int receiverid { get; set; }
        public int verficationcode { get; set; }
        public int flag { get; set; } = 2;
    }
    public class receiverdeletevalidationreq
    {
        public YesHeader header { get; set; }
        public int receiverid { get; set; }
        public int verficationcode { get; set; }
    }
    public class receiverregistrationvalidateres
    {
        public string receiverid { get; set; }
        public string status { get; set; }
    }
    public class receiverresendotpreq
    {
        public YesHeader header { get; set; }
        public int senderid { get; set; }
        public int receiverid { get; set; }
    }
    public class receiverresendotpres
    {
        public int status { get; set; }
    }
    public class beneficiaryaccountvericifationreq
    {
        public YesHeader header { get; set; }
        public string bcagent { get; set; }
        public int senderid { get; set; }
        public string accountnumber { get; set; }
        public string ifscode { get; set; }
        public string channelpartnerrefno { get; set; }
    }
    public class beneficiaryaccountvericifationres
    {
        public string channelpartnerrefno { get; set; }
        public string yesbanktransactionid { get; set; }
        public int? status { get; set; }
        public decimal amount { get; set; }
        public decimal servicecharge { get; set; }
        public decimal grossamount { get; set; }
        public int kycstatus { get; set; }
        public string benename { get; set; }
    }
    public class transactionreq
    {
        public YesHeader header { get; set; }
        public string bcagent { get; set; }
        public int senderid { get; set; }
        public int receiverid { get; set; }
        public int amount { get; set; }
        public string remarks { get; set; }
        public string cpid { get; set; }
        public string channelpartnerrefno { get; set; }
        public int flag { get; set; }
    }
    public class transactionres
    {
        public string channelpartnerrefno { get; set; }
        public string YEStransactionid { get; set; }
        public int? status { get; set; }
        public decimal amount { get; set; }
        public decimal servicecharge { get; set; }
        public decimal grossamount { get; set; }
        public int kycstatus { get; set; }
        public string remarks { get; set; }
        public string bankrefno { get; set; }
        public string NPCIResponsecode { get; set; }
    }
    public class transactionrequeryreq
    {
        public YesHeader header { get; set; }
        public string bcagent { get; set; }
        public string channelpartnerrefno { get; set; }
    }
    public class transactionrequeryres
    {
        public int status { get; set; }
        public string yesbanktransactionid { get; set; }
        public string BankReferenceNumber { get; set; }
        public string Bankremarks { get; set; }
    }
    public class receiverdeletereq
    {
        public YesHeader header { get; set; }
        public int senderid { get; set; }
        public int receiverid { get; set; }

    }
    public class receiverdeleteres
    {
        public int receiverid { get; set; }
        public int status { get; set; }
    }
    public class senderregistrationrmres : Ekycsenderregistrationrmres
    {
        public int senderid { get; set; }
    }
    public class senderdetailsreq
    {
        public YesHeader header { get; set; }
        public string bcagent { get; set; }
        public string mobilenumber { get; set; }
        public int flag { get; set; }
    }
    public class senderdetailsres
    {
        public string bcagent { get; set; }
        public int status { get; set; }
        public YesSenderDetail senderdetail { get; set; }
        public List<receiver> receiverdetail { get; set; }
    }
    public class YesSenderDetail : senderregistrationrmreq
    {
        public int senderid { get; set; }
        public int remitterstatus { get; set; }
        public int kycstatus { get; set; }
        public string TransactionDone { get; set; }
        public string AviableLimit { get; set; }
    }

    public class receiver
    {
        public int receiverid { get; set; }
        public string receivername { get; set; }
        public string receivermobilenumber { get; set; }
        public string receiveremailid { get; set; }
        public string relationshipid { get; set; }
        public string bank { get; set; }
        public string state { get; set; }
        public string city { get; set; }
        public string branch { get; set; }
        public string address { get; set; }
        public string ifscode { get; set; }
        public string accountnumber { get; set; }
        public int mmid { get; set; }
        public int beneficiarystatus { get; set; }
        public int receiverstatus { get; set; }
        public int impsstatus { get; set; }
        public string bankbenename { get; set; }
        public string benebankvalidatestatus { get; set; }
        public int neftflag { get; set; }
    }
    public class receiverregistrationreq
    {
        public YesHeader header { get; set; }
        public string bcagent { get; set; }
        public int senderid { get; set; }
        public string receivername { get; set; }
        public string receivermobilenumber { get; set; }
        public string receiveremailid { get; set; }
        public int relationshipid { get; set; }
        public string ifscode { get; set; }
        public string accountnumber { get; set; }
        public string mmid { get; set; }
        public int flag { get; set; } = 2;
        public string channelpartnerrefno { get; set; }
    }
    public class receiverregistrationres
    {
        public string bcagent { get; set; }
        public int senderid { get; set; }
        public int beneficiaryid { get; set; }
        public int status { get; set; }
    }
    public class Ekycsenderregistrationrmreq
    {
        public YesHeader header { get; set; }
        public string bcagent { get; set; }
        public string sendermobilenumber { get; set; }
        public string sendername { get; set; }
        public string senderlastname { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string pincode { get; set; }
        public string uidtoken { get; set; }
        public string channelpartnerrefno { get; set; }
        public string dob { get; set; }
    }
    public class Ekycsenderupdationmreq : Ekycsenderregistrationrmreq
    {
        public int senderid { get; set; }
    }
    public class Ekycsenderregistrationrmres
    {
        public string bcagent { get; set; }
        public int remitterid { get; set; }
        public int status { get; set; }
    }
    public class EkycsenderregistrationrmforRDreq
    {
        public string Encryptstring { get; set; }
    }
    public class EkycsenderregistrationrmforRDres
    {
        public string AadhaarNo { get; set; }
        public string AgentID { get; set; }
        public string DeviceCertExpiryDate { get; set; }
        public string DeviceDataXml { get; set; }
        public string DeviceHmacXml { get; set; }
        public string DeviceSerialNumber { get; set; }
        public string DeviceSessionKey { get; set; }
        public int DeviceType { get; set; }
        public string DeviceVersionNumber { get; set; }
        public string AadhaarNumberType { get; set; }
        public string sessionid { get; set; }
        public string Wadh { get; set; }
    }


    public class senderekycregistrationres
    {
        public int status { get; set; }
        public string Sendermobilenumber { get; set; }
        public string RequesterID { get; set; }
        public string ServiceName { get; set; }
        public string ReqRefNum { get; set; }
        public string AadhaarName { get; set; }
        public string AadhaarPhoto { get; set; }
        public string City { get; set; }
        public string ContactPerson { get; set; }
        public string DOB { get; set; }
        public string District { get; set; }
        public string Email { get; set; }
        public string Gender { get; set; }
        public string HouseNo { get; set; }
        public string LandMark { get; set; }
        public string Location { get; set; }
        public string Phone { get; set; }
        public string PinCode { get; set; }
        public string PostOffice { get; set; }
        public string State { get; set; }
        public string Street { get; set; }
        public string SubDistrict { get; set; }
        public string TimeStamp { get; set; }
        public string uidtoken { get; set; }
    }
    public class Ekycsenderupdationmres
    {
        public string bcagent { get; set; }
        public int remitterid { get; set; }
        public int status { get; set; }
    }
    public class errorres
    {
        public int status { get; set; }
        public string description { get; set; }
    }
    public class bcagentregistrationforekycreq
    {
        public YesHeader header { get; set; }
        public string bcagentname { get; set; }
        public string middlename { get; set; }
        public string lastname { get; set; }
        public string companyname { get; set; }
        public string address { get; set; }
        public string area { get; set; }
        public string cityname { get; set; }
        public string distrcit { get; set; }
        public string statename { get; set; }
        public int pincode { get; set; }
        public string mobilenumber { get; set; }
        public string localaddress { get; set; }
        public string localarea { get; set; }
        public string localcity { get; set; }
        public string localdistrcit { get; set; }
        public string localstate { get; set; }
        public int localpincode { get; set; }
        public int telephone { get; set; }
        public string alternatenumber { get; set; }
        public string emailid { get; set; }
        public string dob { get; set; }//(dd/mm/yyyy)
        public string idproof { get; set; }
        public string idproofnumber { get; set; }
        public string shopaddress { get; set; }
        public string shoparea { get; set; }
        public string shopcity { get; set; }
        public string shopdistrcit { get; set; }
        public string shopstate { get; set; }
        public int shoppincode { get; set; }
        public string ifsccode { get; set; }
        public int accountnumber { get; set; }
        public int nooftransactionperday { get; set; }
        public decimal transferamountperday { get; set; }
        public string bcagenttype { get; set; }
        public string PanCard { get; set; }
        public string bcagentid { get; set; }
        public string vid { get; set; }
        public string uidtoken { get; set; }
    }
    public class bcagentregistrationforekycres
    {
        public int status { get; set; }
        public string bcagentid { get; set; }
        public string description { get; set; }
    }

    public class Ekycagentregistrationrmreqwithout_en
    {
        public string AadhaarNo { get; set; }
        public string AgentID { get; set; }
        public string DeviceCertExpiryDate { get; set; }
        public string DeviceHmacXml { get; set; }
        public string DeviceDataXml { get; set; }
        public string DeviceSerialNumber { get; set; }
        public string DeviceSessionKey { get; set; }
        public string DeviceType { get; set; }
        public string DeviceVersionNumber { get; set; }
        public int AadhaarNumberType { get; set; }
        public string sessionid { get; set; }
        public string Wadh { get; set; }
    }
    public class EkycAgentregistrationrmforRDres
    {
        public int status { get; set; }
        public string RequesterID { get; set; }
        public string ReqRefNum { get; set; }
        public string AadhaarName { get; set; }
        public string AadhaarPhoto { get; set; }
        public string City { get; set; }
    }
}
