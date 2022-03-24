using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.MoneyTransfer;
using RoundpayFinTech.AppCode.Model.Paymentgateway;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.ThirdParty.AggrePay;
using RoundpayFinTech.AppCode.ThirdParty.CashFree;
using RoundpayFinTech.AppCode.ThirdParty.Paytm;
using RoundpayFinTech.AppCode.ThirdParty.PayU;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Interfaces
{
    public interface IPaymentGatewayML
    {
        ResponseStatus CreateQRIntent(string BankRefID);
        ResponseStatus GetQRGeneration(int LT, int LoginID, int RefID, string TransactionID);
        IResponseStatus UpdateFromAggrePayApp(int TID, int Amount, string Hash);
        IResponseStatus UpdateFromAggrePayCallback(AggrePayResponse aggrePayResponse);
        IEnumerable<PaymentGatewayModel> GetPGDetailsUser(int WID,bool IsUPI);
        IEnumerable<PaymentGatewayDetail> GetPGDetails(int WID, bool IsUPI);
        PGModelForRedirection IntiatePGTransactionForWeb(int UserID, decimal Amount, int UPGID, int OID, int WalletID, string Domain,string VPA);
        PGModelForApp IntiatePGTransactionForApp(int UserID, int Amount, int UPGID, int OID, int WalletID, string IMEI);
        Task SavePGTransactionLog(int PGID, int TID, string Log, string TransactionID, string Checksum, int RequestMode, bool IsRequestGenerated,decimal Amount,string VendorID);
        IResponseStatus UpdateFromPayTMCallback(PaytmPGResponse paytmPGResponse);
        ResponseStatus CheckPGTransactionStatus(CommonReq param);
        void LoopPGTransactionStatus();
        VIANCallbackStatusResponse CallVIANService(VIANCallbackRequest vIANCallbackRequest);
        ResponseStatus UpdateCashfreeResponse(CashfreeCallbackResponse rpPGResponse);
        IResponseStatus UpdateFromCashFreeApp(int TID, int Amount, string Hash);
        CashfreeStatusResponse CashFreePgStatusCheck(string orderId, string orderToken, string TID = "");
        IResponseStatus UpdateFromPayUCallback(PayUResponse payuResponse);
    }
    public interface IUPIPaymentML {
        CollectUPPayResponse InitiateUPIPaymentForWeb(int UserID, decimal Amount, int UPGID, int OID, int WalletID, string UPIID);
        CollectUPPayResponse InitiateUPIPaymentForApp(int UserID, int Amount, int UPGID, int OID, int WalletID, string UPIID);
        ResponseStatus UpdateUPIPaymentStatus(UpdatePGTransactionRequest request);
        ResponseStatus GetUPIStatusFromDB(int TID);
        ResponseStatus GetUPIQR(int LT, int LoginID, int Amount);
        UserQRInfo GetUPIQRBankDetail(int LT, int LoginID);
    }
}
