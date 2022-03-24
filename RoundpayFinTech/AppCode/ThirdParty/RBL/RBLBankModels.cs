using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.ThirdParty.RBL
{
    /***************************************
    * RBL Only Models
    * 
    * **/
    public static class RBPaymentStatus
    {
        public const int pending = 0;
        public const int processing = 1;
        public const int credited = 2;
        public const int refund = 3;
        public const int refundprocess = 4;
    }
    public class RBLAppSetting
    {
        public string LDAP { get; set; }
        public string LDAPPASSWORD { get; set; }
        public string USERNAME { get; set; }
        public string CLIENTID { get; set; }
        public string CLIENTSECRET { get; set; }
        public string PASSWORD { get; set; }
        public string BCAGENT { get; set; }
        public string BASEURL { get; set; }
        public string PFXPD { get; set; }
        public string CPID { get; set; }
    }
    public class channelpartnerloginreq
    {
        public string username { get; set; }
        public string password { get; set; }
        public string bcagent { get; set; }
    }
    public class channelpartnerloginres
    {
        public string sessionid { get; set; }
        public string sessiontoken { get; set; }
        public string timeout { get; set; }
        public int status { get; set; }
        public string wadh { get; set; }
    }
    public class RBLHeader
    {
        public string sessiontoken { get; set; }
    }
    public class remitterregistrationrmreq
    {
        public RBLHeader header { get; set; }
        public string bcagent { get; set; }
        public string remittermobilenumber { get; set; }
        public string remittername { get; set; }
        public string remitteraddress1 { get; set; }
        public string remitteraddress2 { get; set; }
        public int pincode { get; set; }
        public string cityname { get; set; }
        public string statename { get; set; }
        public string alternatenumber { get; set; }
        public string idproof { get; set; }
        public string idproofnumber { get; set; }
        public string idproofissuedate { get; set; }
        public string idproofexpirydate { get; set; }
        public string idproofissueplace { get; set; }
        public string lremitteraddress { get; set; }
        public int lpincode { get; set; }
        public string lstatename { get; set; }
        public string lcityname { get; set; }
    }


    public class remitterregistrationvalidatereq
    {
        public RBLHeader header { get; set; }
        public string remitterid { get; set; }
        public string mobilenumber { get; set; }
        public string verficationcode { get; set; }
        public string channelpartnerrefno { get; set; }
    }
    public class remitterregistrationresendotpreq
    {
        public RBLHeader header { get; set; }
        public string remitterid { get; set; }
    }

    public class beneficiaryregistrationreq
    {
        public RBLHeader header { get; set; }
        public string bcagent { get; set; }
        public string remitterid { get; set; }
        public string beneficiaryname { get; set; }
        public string beneficiarymobilenumber { get; set; }
        public string beneficiaryemailid { get; set; }
        public int relationshipid { get; set; }
        public string ifscode { get; set; }
        public string accountnumber { get; set; }
        public string mmid { get; set; }
        public int flag { get; set; }
    }
    public class beneficiaryregistrationvalidatereq
    {
        public RBLHeader header { get; set; }
        public string beneficiaryid { get; set; }
        public string verficationcode { get; set; }
    }
    public class beneficiaryresendotpreq
    {
        public RBLHeader header { get; set; }
        public string remitterid { get; set; }
        public string beneficiaryid { get; set; }
    }
    public class remitterdetailsreq
    {
        public RBLHeader header { get; set; }
        public string bcagent { get; set; }
        public string mobilenumber { get; set; }
        public int flag { get; set; }
    }
    public class beneficairydeletereq
    {
        public RBLHeader header { get; set; }
        public string remitterid { get; set; }
        public string beneficairyid { get; set; }
    }

    public class beneficairydeletevalidationreq
    {
        public RBLHeader header { get; set; }
        public string beneficairyid { get; set; }
        public string verficationcode { get; set; }
    }
    public class transactionreq
    {
        public RBLHeader header { get; set; }
        public string bcagent { get; set; }
        public string remitterid { get; set; }
        public string beneficiaryid { get; set; }
        public int amount { get; set; }
        public string remarks { get; set; }
        public string cpid { get; set; }
        public string channelpartnerrefno { get; set; }
        public int flag { get; set; }
    }
    public class transactionres
    {
        public string channelpartnerrefno { get; set; }
        public string RBLtransactionid { get; set; }
        public int? status { get; set; }
        public decimal amount { get; set; }
        public decimal servicecharge { get; set; }
        public decimal grossamount { get; set; }
        public int kycstatus { get; set; }
        public string remarks { get; set; }
        public string bankrefno { get; set; }
        public string NPCIResponsecode { get; set; }
        public string UTRNo { get; set; }
    }
    public class transactionrequeryreq
    {
        public RBLHeader header { get; set; }
        public string bcagent { get; set; }
        public string channelpartnerrefno { get; set; }
    }
    public class transactionrequeryres
    {
        public string bcagent { get; set; }
        public string bcagentname { get; set; }
        public string transactiondt { get; set; }
        public string transactionid { get; set; }
        public string amount { get; set; }
        public string servicechrg { get; set; }
        public string tamount { get; set; }
        public int? paymentstatus { get; set; }
        public string remittername { get; set; }
        public string remittermblno { get; set; }
        public string beneficiaryname { get; set; }
        public string relationship { get; set; }
        public string relationshiptype { get; set; }
        public string bank { get; set; }
        public string ifsccode { get; set; }
        public string accountnumber { get; set; }
        public string branch { get; set; }
        public string address1 { get; set; }
        public string address2 { get; set; }
        public string idproof { get; set; }
        public string idproofnumber { get; set; }
        public string idproofissuedate { get; set; }
        public string idproofexpdate { get; set; }
        public int? status { get; set; }
        public string UTRNo { get; set; }
        public string PONum { get; set; }
        public string BankReferenceNo { get; set; }
        public string TranType { get; set; }
        public string BankRemarks { get; set; }
    }
    public class beneficiaryaccvalidationreq
    {
        public RBLHeader header { get; set; }
        public string bcagent { get; set; }
        public string remitterid { get; set; }
        public string beneficiaryname { get; set; }
        public string beneficiarymobilenumber { get; set; }
        public string accountnumber { get; set; }
        public string ifscode { get; set; }
        public string channelpartnerrefno { get; set; }
    }
    public class beneficiaryaccvalidationres
    {
        public int? status { get; set; }
        public string benename { get; set; }
        public string channelpartnerrefno { get; set; }
        public int amount { get; set; }
        public string remarks { get; set; }
        public string bankrefno { get; set; }
        public string NPCIResponsecode { get; set; }
    }
    public class refundotpreq
    {
        public RBLHeader header { get; set; }
        public string RBLtransactionid { get; set; }
    }
    public class refundotpres
    {
        public int? status { get; set; }
    }
    public class refundreq
    {
        public RBLHeader header { get; set; }
        public string bcagent { get; set; }
        public string channelpartnerrefno { get; set; }
        public string verficationcode { get; set; }
        public int flag { get; set; }
    }
    public class refundres
    {
        public string bcagent { get; set; }
        public string amount { get; set; }
        public string tamount { get; set; }
        public string servicecharge { get; set; }
        public int? status { get; set; }
    }
    public class beneficiarydeletevalidationres
    {
        public int? status { get; set; }
    }
    public class remitterdetailsres
    {
        public string bcagent { get; set; }
        public int? status { get; set; }
        public RBLRemitter remitterdetail { get; set; }
        public List<beneficiary> beneficiarydetail { get; set; }
    }
    public class RBLRemitter
    {
        public string remitterid { get; set; }
        public string remittername { get; set; }
        public string remitteraddress1 { get; set; }
        public string remitteraddress2 { get; set; }
        public string pincode { get; set; }
        public string cityid { get; set; }
        public string stateid { get; set; }
        public string alternatenumber { get; set; }
        public string idproof { get; set; }
        public string idproofnumber { get; set; }
        public string idproofissuedate { get; set; }
        public string idproofexpirydate { get; set; }
        public string idproofissueplace { get; set; }
        public string laddress { get; set; }
        public string lpincode { get; set; }
        public string lcity { get; set; }
        public string lstate { get; set; }
        public int remitterstatus { get; set; }
        public int kycstatus { get; set; }
        public decimal consumedlimit { get; set; }
        public decimal remaininglimit { get; set; }
        public string kycremarks { get; set; }
        public string Title { get; set; }
        public string Gender { get; set; }
        public string Religion { get; set; }
        public string Category { get; set; }
        public string Education { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string NameofNominee { get; set; }
        public string Relationship { get; set; }
        public string NomineeAge { get; set; }
        public string NomineeDateofBirth { get; set; }
        public string MaidenName { get; set; }
        public string FatherOrSpouseName { get; set; }
        public string MotherName { get; set; }
        public string DateofBirth { get; set; }
        public string MaritalStatus { get; set; }
        public string Nationality { get; set; }
        public string ResidentialStatus { get; set; }
        public string Emailid { get; set; }
    }

    public class beneficiary
    {
        public string beneficiaryid { get; set; }
        public string beneficiaryname { get; set; }
        public string beneficiarymobilenumber { get; set; }
        public string beneficiaryemailid { get; set; }
        public string relationshipid { get; set; }
        public string bank { get; set; }
        public string state { get; set; }
        public string city { get; set; }
        public string branch { get; set; }
        public string address { get; set; }
        public string ifscode { get; set; }
        public string accountnumber { get; set; }
        public string mmid { get; set; }
        public int beneficiarystatus { get; set; }
        public int impsstatus { get; set; }
        public int neftflag { get; set; }
    }

    public class beneficiarydeleteres
    {
        public string beneficiaryid { get; set; }
        public int? status { get; set; }
    }
    public class remitterregistrationrmres
    {
        public string bcagent { get; set; }
        public int remitterid { get; set; }
        public int? status { get; set; }
    }
    public class remitterregistrationvalidateres
    {
        public int remitterid { get; set; }
        public int? Status { get; set; }
        public string Enrollmentfee { get; set; }
    }
    public class remitterregistrationresendotpres
    {
        public int? status { get; set; }
    }
    public class beneficiaryregistrationres
    {
        public string bcagent { get; set; }
        public int remitterid { get; set; }
        public int beneficiaryid { get; set; }
        public int? status { get; set; }
    }
    public class beneficiaryregistrationvalidateres
    {
        public string beneficiaryid { get; set; }
        public int? status { get; set; }
    }
    public class beneficiaryresendotpres
    {
        public int? status { get; set; }
    }

    public class errorres
    {
        public int? status { get; set; }
        public string description { get; set; }
    }
}
