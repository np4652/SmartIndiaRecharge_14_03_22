using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.Model.Reports;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Interfaces
{
    public interface IFundProcessML
    {
        IEnumerable<MasterRejectReason> MASTERRR();
        IResponseStatus FundTransfer(FundProcess fundProcess);
        FundRequetResp GetUserFundTransferData(int PID);
        IEnumerable<PaymentModeMaster> PaymentModes();
        IEnumerable<FundRequestToUser> FundRequestToUser();
        IEnumerable<FundRequestToUser> FundRequestToUserApp(CommonReq commonReq);
        IResponseStatus FundRequestOperation(FundRequest obj);
        IResponseStatus FundReject(FundProcess fundProcess);
        Task<IResponseStatus> MoveToWallet(CommonReq commonReq);
        IResponseStatus BankTransfer(WalletRequest req);
        List<FundRequestToRole> GetFundRequestToRole();
        IResponseStatus UpdateFundRequestToRole(FundRequestToRole fundRequest);
    }
}
