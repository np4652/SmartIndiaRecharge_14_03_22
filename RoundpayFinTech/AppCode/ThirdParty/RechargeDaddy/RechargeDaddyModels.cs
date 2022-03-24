using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data;

namespace RoundpayFinTech.AppCode.ThirdParty.RechargeDaddy
{
    public class RechargeDaddySetting
    {
        public string MobileNo { get; set; }
        public string APIKey { get; set; }
        public string AgentCode { get; set; }
        public string Version { get; set; }
        public string BASEURL { get; set; }
    }
    public class GetSenderRequest
    {
        public ChecksumModel sRequest { get; set; }
    }

    public class ChecksumModel
    {
        public GetSenderNTDRequest request { get; set; }

    }
    public class GetSenderNTDRequest
    {
        public string REQTYPE { get; set; }
        public string CUSTOMERNO { get; set; }

    }
    public class RDaddyResponse
    {
        public NTDResponse NTDRESP { get; set; }
    }
    public class NTDResponse
    {
        public int? STATUSCODE { get; set; }
        public string STATUSMSG { get; set; }
        public string FNAME { get; set; }
        public string LNAME { get; set; }
        public string LIMIT { get; set; }
        public string USED { get; set; }
        public string REMAIN { get; set; }
        public int STATUS { get; set; }
        public string STATUSDESC { get; set; }
        public int OTPREQ { get; set; } 
        public string OTP { get; set; }
        public int BENEID { get; set; }
        public string BENENAME { get; set; }
        public string MOBILENO { get; set; }
        public int BANKID { get; set; }
        public string BANKNAME { get; set; }
        public string ACCNO { get; set; }
        public string IFSC { get; set; }
        public int VERIFIED { get; set; }
        public int IMPS_SCHEDULE { get; set; }
        public IEnumerable<BENELISTReq> BENELIST { get; set; }
        public string NAME { get; set; }
        public string VERIFYID { get; set; }
        public string REFNO { get; set; }
        public string TRNID { get; set; }
        public string TOTALCHARGE { get; set; } // 1-imps , 2-neft
        public string CHARGE { get; set; }  //0/1- yes
        public string GST { get; set; }
        public string APBD { get; set; } // WEB-website, APP-app
        public string ATDS { get; set; }
        public int? TRNSTATUS { get; set; } //0-accepted, chck appendix 
        public string TRNSTATUSDESC { get; set; }
        public string BANKREFNO { get; set; }
        public int BAL { get; set; }
        public string BANKCODE { get; set; }
        public string MIFSCCODE { get; set; }
        public int IMPS { get; set; }//0-unavailable, 1-avail
        public int NEFT { get; set; } //0-unavai, 1-avail
        public int ACVERIFYAVAIL { get; set; }
        public int CMPID { get; set; }
        public IEnumerable<BankListResponse> BANKLIST { get; set; }
    }
    public class BankListResponse
    {
        public int BANKID { get; set; }
        public string STATUSMSG { get; set; }
        public string BANKCODE { get; set; }
        public string BANKNAME { get; set; }
        public string MIFSCCODE { get; set; }
        public string IMPS { get; set; }
        public string NEFT { get; set; }
        public string ACVERIFYAVAIL { get; set; }
        public string DISORDER { get; set; }
        public string STATUS { get; set; }
    }
    public class BulkInsertBankList
    {
        public DataTable tp_BankList { get; set; }

    }
    public class CreateSenderNTDRequest
    {
        public string REQTYPE { get; set; }
        public string CUSTOMERNO { get; set; }
        public string FNAME { get; set; }
        public string LNAME { get; set; }
        public string ANAME { get; set; }
        public string ADD1 { get; set; }
        public string ADD2 { get; set; }
        public string CITY { get; set; }
        public string STATE { get; set; }
        public string COUNTRY { get; set; }
        public string PCODE { get; set; }

    }
    public class VerifySenderNTDRequest
    {
        public string REQTYPE { get; set; }
        public string CUSTOMERNO { get; set; }
        public string OTP { get; set; }
    }

    public class ResendCreateSenderOTPNTDReq
    {
        public string REQTYPE { get; set; }
        public string CUSTOMERNO { get; set; }
    }
    public class GetBeneficiaryNTDReq
    {
        public string REQTYPE { get; set; }
        public string CUSTOMERNO { get; set; }
        public int BENEID { get; set; }
    }
    public class GetBeneficiaryListReq
    {
        public string REQTYPE { get; set; }
        public string CUSTOMERNO { get; set; }
    }
    public class BENELISTReq
    {
        public int BENEID { get; set; }
        public string BENENAME { get; set; }
        public string MOBILENO { get; set; }
        public int BANKID { get; set; }
        public string BANKNAME { get; set; }
        public string ACCNO { get; set; }
        public string IFSC { get; set; }
        public int VERIFIED { get; set; }
        public int? IMPS_SCHEDULE { get; set; }
        public int STATUS { get; set; }
        public string STATUSDESC { get; set; }
    }
    public class CreateBeneficiaryNTDReq
    {
        public string REQTYPE { get; set; }
        public string CUSTOMERNO { get; set; }
        public string NAME { get; set; }
        public string MOBILENO { get; set; }
        public int BANKID { get; set; }
        public string ACCNO { get; set; }
        public string IFSC { get; set; }
    }

    public class ValidateBeneficiaryOTPNTDReq
    {
        public string REQTYPE { get; set; }
        public string CUSTOMERNO { get; set; }
        public int BENEID { get; set; }
        public string OTP { get; set; }
    }

    public class ResendCreateBeneficiaryOTPNTDReq
    {
        public string REQTYPE { get; set; }
        public string CUSTOMERNO { get; set; }
        public int BENEID { get; set; }
    }
    public class RemoveBeneficiaryNTDReq
    {
        public string REQTYPE { get; set; }
        public string CUSTOMERNO { get; set; }
        public string BENEID { get; set; }
    }   
    
    public class ResendRemoveBeneficiaryOTPNTDReq
    {
        public string REQTYPE { get; set; }
        public string CUSTOMERNO { get; set; }
        public int BENEID { get; set; }
    }
   
    public class VerifyAccountNTDReq
    {
        public string REQTYPE { get; set; }
        public string CUSTOMERNO { get; set; }
        public int BANKID { get; set; }
        public string ACCNO { get; set; }
        public string IFSC { get; set; }
        public string REFNO { get; set; }
    }
  
    //a/c transfer
    public class SendMoneyNTDReq
    {
        public string REQTYPE { get; set; }
        public string CUSTOMERNO { get; set; }
        public string BENEID { get; set; }
        public int AMT { get; set; }
        public int TRNTYPE { get; set; } // 1-imps , 2-neft
        public int IMPS_SCHEDULE { get; set; }  //0/1- yes
        public string REFNO { get; set; }
        public string CHN { get; set; } // WEB-website, APP-app
        public string CUR { get; set; }
        public string AG_LAT { get; set; }
        public string AG_LONG { get; set; }
    }
    public class GetTransactionStatusNTDReq
    {
        public string REQTYPE { get; set; }
        public string REFNO { get; set; }
    }
   
    public class GetTransactionRefundNTDReq
    {
        public string REQTYPE { get; set; }
        public string REFNO { get; set; }
        public int OTP { get; set; }
    }   
    public class ResendTransactionRefundOTPNTDReq
    {
        public string REQTYPE { get; set; }
        public string CUSTOMERNO { get; set; }
        public string REFNO { get; set; }
    }  
    public class GetPartnerBalanceNTDReq
    {
        public string REQTYPE { get; set; }
    }
    
    public class GetBankListNTDReq
    {
        public string REQTYPE { get; set; }
    }
  
    public class GenerateTransactionComplaintNTDReq
    {
        public string REQTYPE { get; set; }
        public string REFNO { get; set; }
        public int CMTYPE { get; set; }
        public string CMDESC { get; set; }
    }

    public class RDaddyIntAgentRegReq
    {
        public string REQTYPE { get; set; }
        public string AGC { get; set; }
        public string FNAME { get; set; }
        public string LNAME { get; set; }
        public string MNO { get; set; }
        public string FRNM { get; set; }
        public string ADD { get; set; }
        public string CITY { get; set; }
        public string PCODE { get; set; }
        public string PAN { get; set; }
        public string ANO { get; set; }

    }

}
