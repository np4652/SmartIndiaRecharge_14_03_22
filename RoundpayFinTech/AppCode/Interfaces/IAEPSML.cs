using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Interfaces
{
    public interface IAEPSML
    {
        //Task<AEPSTransactionServiceResp> MakeAEPSTransaction(AEPSTransactionServiceReq transactionServiceReq);
        Task<AEPSTransactionServiceResp> UpdateAEPSTransaction(AEPSTransactionServiceReq transactionServiceReq);
        AEPSTransactionServiceResp MakeAEPSMiniStmtTransaction(InititateMiniStatementTransactionRequest transactionServiceReq);
    }
    public interface IMiniBankML
    {
        Task<MiniBankTransactionServiceResp> MakeMiniBankTransaction(MiniBankTransactionServiceReq transactionServiceReq);
        MBStatuscheckResponseApp MBStatusCheck(MBStatusCheckRequest transactionServiceReq);
        ResponseStatus UpdateMiniBankResponse(MiniBankTransactionServiceResp apiReqForProc);
    }
}
