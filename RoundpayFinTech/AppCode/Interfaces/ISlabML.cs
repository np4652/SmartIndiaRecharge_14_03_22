using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.App;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Interfaces
{
    public interface ISlabML
    {
        IEnumerable<SlabRangeDetail> GetRealtimeCommApp(int UserID);
        List<SlabSpecialCircleWise> GetSpecialSlabDetailApp(int OID, int UserID);
        IEnumerable<SlabRangeDetail> GetRealtimeComm();
        IResponseStatus UpdateDMRModelForSlabDetail(int OID, int SlabID, int DMRModelID);
        IResponseStatus UpdateCircleSlabAPI(SlabCommission slabCommission);
        IEnumerable<SlabCommission> CircleSlabAPIGet(int APIID, int OID);
        IResponseStatus UpdateCircleSlab(SlabCommission slabCommission);
        IEnumerable<SlabCommission> CircleSlabGet(int SlabID, int OID);
        IEnumerable<SlabCommissionSettingRes> SlabCommissionSetting(int OID);
        IEnumerable<DenomCommissionDetail> GetDenomCommissionDetail(int OID);
        IResponseStatus UpdateFlatCommission(int UserID, int RoleID, decimal CommRate);
        IEnumerable<FlatCommissionDetail> FlatCommissionDetails(int UserID);
        IEnumerable<AFSlabDetailDisplayLvl> GetAFSlabDetailForDisplay(int OID);
        SlabDetailModel GetAFSlabCommission(int OID);
        IResponseStatus RealAPIStatusUpdate(CommonReq Req);
        IResponseStatus UpdateAPICommission(SlabCommission apiCommission);
        IEnumerable<SlabMaster> GetSlabMaster();
        SlabMaster GetSlabMaster(int SlabID);
        IResponseStatus UpdateSlabMaster(SlabMaster slabMaster);
        SlabDetailModel GetSlabDetail(int SlabID, int OpTypeID);
        SlabDetailModel GetSlabDetailGI(int SlabID, int OpTypeID);
        IResponseStatus UpdateSlabDetail(SlabCommission slabCommission);
        IResponseStatus UpdateSlabDetailGI(SlabCommission slabCommission);
        IResponseStatus UpdateBulkSlabDetail(SlabCommissionReq req);
        IResponseStatus CopySlabDetail(int SlabID, string SlabName);
        SlabDetailModel GetSlabCommission(int SlabID);
        SlabDetailModel GetSlabCommissionApp(CommonReq commonReq);
        IEnumerable<SlabDetailDisplayLvl> GetSlabDetailForDisplay();
        IEnumerable<DTHSlabDetailDisplay> GetDTHSlabDetailForDisplay(int OID);
        IEnumerable<RangeSlabDetailDisplayLvl> GetSlabDetailForDisplayRange();
        IEnumerable<SlabDetailDisplayLvl> GetSlabDetailForDisplayForApp(CommonReq commonReq);
        IResponseStatus UpdateRangeSlabDetail(RangeCommission slabCommission);
        IResponseStatus UpdateRangeAPIComm(RangeCommission slabCommission);
        IEnumerable<RangeCommission> GetRangeSlabDetail(int SlabID);
        RangeDetailModel GetSlabDetailRange(int SlabID, int OType = 0);
        IResponseStatus RealAPIStatusUpdate(bool Status);
        Task<SlabCommission> GetLapuRealComm(CommonReq commonReq);
        Task<CommissionDisplay> GetDisplayCommission(CommonReq commonReq);
        DTHCommissionModel GetDTHCommissionDetail(int SlabID, int OpTypeID);
        IResponseStatus UpdateDTHCommission(DTHCommission slabCommission);
        IEnumerable<IncentiveDetail> GetIncentive(int OID);
        IEnumerable<IncentiveDetail> GetIncentive(CommonReq req);
        IEnumerable<SlabRangeDetail> GetSlabRangeDetail(int OID);
        List<SlabSpecialCircleWise> GetSpecialSlabDetail(int OID);
        SlabRangDetailRes GetSlabRangeDetailForApp(SlabRangDetailReq AppReq);
        Task<IResponseStatus> DeleteSlab(int slabID, int loginId);
    }
}
