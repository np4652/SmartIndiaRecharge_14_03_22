using Fintech.AppCode.Model;
using Fintech.AppCode.Model.Reports;
using System;
using System.Collections.Generic;


namespace RoundpayFinTech.AppCode.Model.ProcModel
{
    public class Bankreq
    {
        public int ID { get; set; }
        public int BankID { get; set; }
        public string BranchName { get; set; }
        public string AccountHolder { get; set; }
        public string AccountNo { get; set; }
        public bool ISQRENABLE { get; set; }
        public bool NeftStatus { get; set; }
        public int NeftID { get; set; }
        public bool RTGSStatus { get; set; }
        public int RTGSID { get; set; }
        public bool IMPSStatus { get; set; }
        public int IMPSID { get; set; }
        public bool ThirdPartyTransferStatus { get; set; }
        public int ThirdPartyTransferID { get; set; }
        public bool CashDepositStatus { get; set; }
        public int CashDepositID { get; set; }
        public bool GCCStatus { get; set; }
        public int GCCID { get; set; }
        public bool ChequeStatus { get; set; }
        public int ChequeID { get; set; }
        public bool ScanPayStatus { get; set; }
        public int ScanPayID { get; set; }
        public bool UPIStatus { get; set; }
        public int UPIID { get; set; }
        public bool ExchangeStatus { get; set; }
        public int ExchangeID { get; set; }
        public string IFSCCode { get; set; }
        public decimal Charge { get; set; }
        public string RImageUrl { get; set; }
        public string UPINUmber { get; set; }
        public bool IsbankLogoAvailable { get; set; }
        public bool ShouldSerializeBankID() => (false);
        public bool IsVirtual { get; set; }
        public int CDMID { get; set; }
        public bool CDM { get; set; }
        public decimal CDMCharges { get; set; }
        public int CDMType { get; set; }
        public string Remark { get; set; }
        public bool IsShow { get; set; }

    }
    public class Bank : Bankreq
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
        public int AccParty { get; set; }
    }
    public class BankMasterSuperClass
    {
        public int ID { get; set; }
        public string BankName { get; set; }
        public int IIN { get; set; }
        public bool IsIMPS { get; set; }
        public bool IsNEFT { get; set; }
        public bool IsACVerification { get; set; }
        public bool ISAEPSStatus { get; set; }
        public string IFSC { get; set; }
        public string AadhpayIIN { get; set; }
        public bool IscashDeposit { get; set; }
        public bool IsAadharpay { get; set; }
    }
    public class BankMaster : BankMasterSuperClass
    {
        public int AccountLimit { get; set; }
        public int SprintBankID { get; set; }
        public string Code { get; set; }
        public string BankType { get; set; }
        public string Logo { get; set; }
        public int EKO_BankID { get; set; }
        public int Mahagram_BankID { get; set; }
        public string BAVENVEBankID { get; set; }
        public int RDaddyBankID { get; set; }
        public int Pay1MoneyBankID { get; set; }

        public int LoginID { get; set; }
        public int LTID { get; set; }
        public int StatusColumn { get; set; }
        public bool ShouldSerializeEKO_BankID() => false;
        public bool ShouldSerializeRDaddyBankID() => false;
        public bool ShouldSerializeBAVENVEBankID() => false;
        public bool ShouldSerializeMahagram_BankID() => false;
        public bool ShouldSerializeLoginID() => false;
        public bool ShouldSerializeLTID() => false;
        public bool ShouldSerializeStatusColumn() => false;
        public int BankID { get; set; }
        public string AccountNo { get; set; }

    }
    public class FundRequestToUser
    {
        public string ParentName { get; set; }
        public int ParentID { get; set; }
        public int ParentRoleID { get; set; }
    }
    public class BankHoliday : BankMaster
    {
        public int ID { get; set; }
        public string Date { get; set; }
        public string Entrydate { get; set; }
        public string ModifyDate { get; set; }
        public string Remark { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class UtrStatementUpload
    {
        public decimal closingAmount { get; set; }
        public string cr_dr { get; set; }
        public string description { get; set; }
        public decimal transactionAmount { get; set; }
        public DateTime transactionDate { get; set; }
        public string utr { get; set; }
        public string FileId { get; set; }

    }
    public class UtrStatementUploadReq : CommonReq
    {

        public List<UtrStatementUpload> Record { get; set; }
    }

    public class UtrStatementSetting
    {
        public string bank { get; set; }
        public string transactionType { get; set; }
        public string identifier { get; set; }
        public string startWith { get; set; }
        public string endWith { get; set; }
        public string bankID { get; set; }


    }
    public class websiteBanks
    {
        public int ID { get; set; }
        public string BankName { get; set; }
        public int BankID { get; set; }
        public string BranchName { get; set; }
        public string IFSCCode { get; set; }
        public string AccountHolder { get; set; }
        public string AccountNo { get; set; }
    }
}