using Fintech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace RoundpayFinTech.AppCode.Interfaces
{
    public interface IAPIML
    {
        RangeDetailModel GetAPICommissionRange(int APIID, int OType = 0);
        APIDetail GetAPIDetailByID(int APIID);
        Task<IEnumerable<APIDetail>> GetAllAPI(int opTypeId);
        Task<IResponseStatus> UpdateOpTypeWiseAPISwitch(OpTypeWiseAPISwitchingReq req);
        IEnumerable<APIDetail> GetAPIDetail();
        IResponseStatus SaveAPI(APIDetail req);
        IEnumerable<SlabCommission> GetAPICommission(int APIID);
        IResponseStatus UpdateAPISTATUSCHECK(APISTATUSCHECK apistatuscheck);
        Task<APISTATUSCHECK> GetAPISTATUSCHECK(APISTATUSCHECK apistatuscheck);
        IEnumerable<APISTATUSCHECK> GetAPISTATUSCHECKs(string CheckText, int Status);
        IResponseStatus DeleteApiStatusCheck(int Statusid);
        APIDetail GetAPIDetailByAPICode(string APICode);
        APIGroupDetail GetGroup(int GroupID);
        IEnumerable<APIGroupDetail> GetGroup();
        IEnumerable<APIDetail> GetAPIDetailForBalance();
        Task<APIBalanceResponse> GetBalanceFromAPI(int APIID);
        IResponseStatus UpdateDMRModelForAPI(int OID, int API, int DMRModelID);
    }
}
