using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Microsoft.AspNetCore.Http;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.App;
using RoundpayFinTech.AppCode.Model.BBPS;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Report;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Interfaces
{
    public interface IOperatorML
    {
        ResponseStatus MapPlansOperator(int toOid, int toMapOid);
        List<AccountOpData> GetAccountOpeningRedirectionDataByOpType(CommonReq req);
        IResponseStatus UpdateAccountOpeningSetting(int OID, string Content, string RedirectURI);
        IEnumerable<AccOpenSetting> AccountOpeningSetting(int OID);
        RPBillerModel GetRPBillerByID(string BillerID);
        RPBillerModel GetRPBillerByType(int OpTypeID);
        int GetServiceByOpTypeID(int ID);
        ResponseStatus UpdateRechPlans(int OID);
        ResponseStatus UpdateDTHPlans(int OID);
        ResponseStatus UpdateAmtVal(int OID, bool STS);
        void UpdateBillerLog(CommonReq req);
        IResponseStatus UpdateAllBillers(int OpTypeID);
        IResponseStatus UpdateBillerInfo(int OID);
        IEnumerable<OpTypeMaster> GetOptypeInRange();
        IEnumerable<OpTypeMaster> GetOptypeVendor(int id);
        IEnumerable<OperatorDetail> GetActiveOperators(int LoginID, int OpType);
        IEnumerable<DTHPackage> GetDTHDiscription(int PID, int OID);
        IEnumerable<OperatorOptionalStuff> OperatorOptionalStuff(int OPID = 0);
        ResponseStatus DeleteOperatorOption(int OptionType, int OID);
        IEnumerable<OperatorDetail> GetOperators(int Type = 0);
        IEnumerable<OperatorDetail> GetPaymentModesOp(int LoginID);
        IEnumerable<OperatorDetail> GetOperatorsActive(int Type = 0);
        OperatorDetail GetOperator(int ID);
        IEnumerable<OperatorDetail> GetOperators(string OpTypes);
        IResponseStatus SaveOperator(OperatorDetail operatorDetail);
        IResponseStatus UpdateBillerID(OperatorDetail operatorDetail);
        IEnumerable<OpTypeMaster> GetOptypes(int ServiceID=0);
        IEnumerable<EXACTNESSMaster> GetExactness();
        IEnumerable<APIOpCode> GetAPIOpCode(int OpTypeID);
        IResponseStatus UpdateAPIOpCode(APIOpCode aPIOpCode);
        IResponseStatus UpdateAPIOpCodeCircle(APIOpCode aPIOpCode);
        IEnumerable<APIOpCode> GetAPIOpCodeCircle(int OID, int APIID);
        Task<IEnumerable<OperatorDetail>> GetOPListBYServices(string ServiceTypeIDs);
        OperatorParamModels OperatorOptional(int OID);
        IEnumerable<OperatorOptional> OperatorOption(int OID);
        IResponseStatus UpdateOption(OperatorOptionalReq req);
        IEnumerable<OperatorDetail> GetOperatorsByGroup(int Type);
        Task<IEnumerable<string>> GetDowns();
        IResponseStatus UpdateBlockDenomination(APISwitched switched);
        ApiOperatorOptionalMappingModel AOPMapping(int A, int O);
        IResponseStatus SaveAOPMapping(ApiOperatorOptionalMappingModel model);
        IEnumerable<RangeModel> GetRange();
        RangeModel GetRange(int ID);
        IResponseStatus SaveRange(RangeModel rangeDetail);
        //IEnumerable<RangeCommission> GetRangeCommission(int SlabID);
        IEnumerable<DenominationModal> GetDenomination();
        DenominationModal GetDenomination(int ID);
        IResponseStatus SaveDenom(DenominationModal denomDetail);
        List<DenomDetailByRole> GetDenomDetailByRole(DenomDetailByRole DetailReq);
        IResponseStatus SaveDenomDetailByRole(DenomDetailByRole rangeDetail);
        IResponseStatus SaveTollFree(OperatorDetail operatorDetail);
        IResponseStatus UploadOperatorPDF(IFormFile file, int OID, LoginResponse _lr);
        IEnumerable<OperatorDetail> GetMobileTollFree();
        IEnumerable<OperatorDetail> GetDTHTollFree();
        IEnumerable<ServiceMaster> GetServices();
        APIDenomination GetApiDenom(APIDenominationReq req);
        APIDenomination GetApiDenomUser(APIDenominationReq req);
        DenominationRangeList GetDenominationRange();
        DenominationRange GetDenominationRange(int ID);
        IResponseStatus SaveDenomRange(DenominationRange denomRange);
        IEnumerable<OpTypeMaster> GetOptypeInSlab();
        IEnumerable<OpTypeMaster> GetAPIOpType(string APICode);
        IEnumerable<DTHChannelCategory> GetDTHChannelCategory(int ID);
        IResponseStatus SaveDTHPackage(DTHPackage req);

        IResponseStatus SaveDTHChannelCategory(DTHChannelCategory req);
        IResponseStatus SaveBulkDTHPackage(List<DTHPackage> req);
        IEnumerable<DTHChannel> GetDTHChannel(int ID);
        IEnumerable<ChannelCategory> GetChannelCategory();
        IResponseStatus SaveDTHChannel(DTHChannel req);
        IEnumerable<DTHChannelMap> MapChannelToPack(int packageID);
        IResponseStatus SaveChannelMapping(DTHChannelMap req);
        IEnumerable<DTHPackage> GetDTHPackage(int ID, int OID);
        string getMaxOpCode(int OpType);
        IResponseStatus changeValidationType(int OID, int CircleValidationType);
        DTHPackageResponse GetDTHPackageForApp(DTHPackageRequest AppReq);
        IEnumerable<ChannelUnderCategory> DTHChannelByPackage(int ID);
        DTHChannelResponse DTHChannelByPackageForApp(DTHChannelRequest AppReq);
        IEnumerable<OperatorDetail> GetOperatorsByService(string SCode);

        IResponseStatus SaveSpecialSlabDetail(CircleWithDomination rangeDetail);
        IResponseStatus SaveSpecialSlabDetailAPI(CircleWithDomination rangeDetail);
        Task<IEnumerable<CircleWithDomination>> GetCircleWithDominations(CircleWithDomination req);
        Task<IEnumerable<CircleWithDomination>> GetCircleWithDominationsAPI(CircleWithDomination req);
        Task<IEnumerable<CircleWithDomination>> GetRemainDominationsSpecialSlab(CircleWithDomination req);
        Task<IEnumerable<CircleWithDomination>> GetRemainDominationsSpecialSlabAPI(CircleWithDomination req);
        IResponseStatus UpdateSpecialSlabDomID(CircleWithDomination rangeDetail);
        IResponseStatus UpdateSpecialAPIIDDomID(CircleWithDomination rangeDetail);
        IResponseStatus UpdateGroupSpecialSlabDomID(CircleWithDomination rangeDetail);
        IResponseStatus UpdateGroupSpecialAPIIDDomID(CircleWithDomination rangeDetail);
        ResponseStatus UpdateAxisBankBillerList(int OpType, string APIOpType);
    }
    public interface ITargetML
    {
        List<TargetModel> ShowGiftImages();
        List<TargetModel> GetTarget(TargetModel targetReq);
        IResponseStatus SaveTarget(TargetModel req);
        IResponseStatus UploadGift(IFormFile file, string filename);
        TargetModel GetTargetByRole(TargetModel targetReq);
    }
    public interface IOperatorAppML {
        IEnumerable<OperatorDetail> GetOperatorsSession(int uid, int RoleID);
        Task<IEnumerable<APICircleCode>> APICircleCode();
        IResponseStatus SaveAPICircleCode(APICircleCode req);
        Task<IEnumerable<NumberSeries>> NumberList();
        Task<IEnumerable<CirlceMaster>> CircleList();
        IEnumerable<OperatorDetail> GetOperatorsApp(int RoleID);
        OperatorParamModels OperatorOptionalApp(CommonReq commonReq);
        Task<IEnumerable<CirlceMaster>> CircleListWithAll();
        IResponseStatus SaveSpecialSlabDetail(CircleWithDomination rangeDetail);
        IResponseStatus SaveSpecialSlabDetailAPI(CircleWithDomination rangeDetail);
        Task<IEnumerable<CircleWithDomination>> GetCircleWithDominations(CircleWithDomination req);
        Task<IEnumerable<CircleWithDomination>> GetCircleWithDominationsAPI(CircleWithDomination req);
        Task<IEnumerable<CircleWithDomination>> GetRemainDominationsSpecialSlab(CircleWithDomination req);
        Task<IEnumerable<CircleWithDomination>> GetRemainDominationsSpecialSlabAPI(CircleWithDomination req);
        IResponseStatus UpdateSpecialSlabDomID(CircleWithDomination rangeDetail);
        IResponseStatus UpdateSpecialAPIIDDomID(CircleWithDomination rangeDetail);
        IResponseStatus UpdateGroupSpecialSlabDomID(CircleWithDomination rangeDetail);
        IResponseStatus UpdateGroupSpecialAPIIDDomID(CircleWithDomination rangeDetail);
        IEnumerable<IndustryTypeModel> GetIndustryWiseOpTypeList();
    }
    public interface IErrorCodeMLParent
    {
        IEnumerable<ErrorCodeDetail> Get();
        ErrorCodeDetail Get(int ID);
        ErrorCodeDetail Get(string ErrCode);
        APIErrorCode GetAPIErrorCode(APIErrorCode APIErrCode);
    }
    public interface IErrorCodeML: IErrorCodeMLParent
    {
        IEnumerable<ErrorTypeMaster> GetTypes();
        IResponseStatus Save(ErrorCodeDetail errorCodeDetail);
        IResponseStatus update(ErrorCodeDetail req);
        List<APIErrorCode> GetAPIErrorCode();
        IResponseStatus UpdateAPIErCode(APIErrorCode aPIErrorCode);
        ErrorCodeDetail GetAPIErrorCodeDescription(string APIGroupCode, string APIErrorCode);
    }
}
