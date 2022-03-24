using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.App;
using RoundpayFinTech.AppCode.Model.MoneyTransfer;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Interfaces
{
    public interface ISellerML
    {
        MDMTResponse DoUPIPayment(ReqSendMoney reqSendMoney,int userID);
        Task<IResponseStatus> DoDTHSubscription(DTHConnectionServiceRequest serviceReq);
        Task<IResponseStatus> DoDTHSubscription(DTHConnectionServiceModel mdl);
        IEnumerable<Package_Cl> GetPackage(int UserID);
        List<Package_Cl> GetPackage();
        Task<IResponseStatus> Recharge(RechargeRequest req);
        Task<AppTransactionRes> AppRecharge(AppRechargeRequest req);
        List<PieChartList> DashBoard();
        ChargeAmount GetChargeForApp(CommonReq _req);
        TransactionDetail DMRReceiptApp(CommonReq req);
        ValidateOuletModel CheckOnboardUser(int OID, string OTP,int OTPRefID,string PidData, bool IsBio,int BioAuthtype);
        Task<IResponseStatus> PSATransaction(RechargeRequest req);
        Task<IResponseStatus> PSATransaction(_RechargeAPIRequest _req);
        Task<IResponseStatus> DoKYCSender(SenderRequest senderKYC);
    }
}
