using Fintech.AppCode.Model;
using Microsoft.AspNetCore.Mvc.Rendering;
using RoundpayFinTech.AppCode.Model.Report;
using System.Collections.Generic;
using System.Data;

namespace RoundpayFinTech.AppCode.Model.ProcModel
{
    public class OperatorDetail : ServiceMaster
    {
        public int Type { get; set; }
        public string Name { get; set; }
        public int OID { get; set; }
        public string OPID { get; set; }
        public string BillerID { get; set; }
        public string Operator { get; set; }
        public string TollFree { get; set; }
        public int OpType { get; set; }

        public int ExactNessID { get; set; }
        public string ExactNess { get; set; }
        public string OperatorType { get; set; }
        public bool IsBBPS { get; set; }
        public bool IsBilling { get; set; }
        public bool InSlab { get; set; }
        public bool IsTakeCustomerNum { get; set; }
        public string SPKey { get; set; }
        public decimal Min { get; set; }
        public decimal Max { get; set; }
        public string BusinessModel { get; set; }
        public string Status { get; set; }
        public bool BackUpApiStatus { get; set; }
        public int ModifyBy { get; set; }
        public string ModifyDate { get; set; }
        public int Length { get; set; }
        public int LengthMax { get; set; }
        public string HSNCode { get; set; }
        public string StartWith { get; set; }
        public string Image { get; set; }
        public bool IsPartial { get; set; }
        public int CircleValidationType { get; set; }
        public string CircleValidation { get; set; }
        public bool IsOPID { get; set; }
        public string AccountName { get; set; }
        public string AccountRemak { get; set; }
        public bool IsAccountNumeric { get; set; }
        public bool IsGroupLeader { get; set; }
        public int CommSettingType { get; set; }
        public int MinRange { get; set; }
        public int MaxRange { get; set; }
        public int RangeId { get; set; }
        public decimal Charge { get; set; }
        public bool ChargeAmtType { get; set; }
        public int StateID { get; set; }
        public int Ind { get; set; }
        public string AccountNoKey { get; set; }
        public string RegExAccount { get; set; }
        public string CustomerNoKey { get; set; }
        public string PlanDocName { get; set; }
        public bool IsAmountValidation { get; set; }
        public int AllowedChannel { get; set; }

        public int PlanOID { get; set; }
        public int RofferOID { get; set; }
        public int DTHCustInfoOID { get; set; }
        public int DTHHREFOID { get; set; }

        public bool ShouldSerializeOPID() => (false);
        public bool ShouldSerializeOperatorType() => (false);
        //public bool ShouldSerializeSPKey() => (false);
        public bool ShouldSerializeBusinessModel() => (false);
        public bool ShouldSerializeStatus() => (false);
        public bool ShouldSerializeBackUpApiStatus() => (false);
        public bool ShouldSerializeModifyBy() => (false);
        public bool ShouldSerializeModifyDate() => (false);
        public bool ShouldSerializeHSNCode() => (false);
        public bool ShouldSerializeCircleValidationType() => (false);
        public bool ShouldSerializeCircleValidation() => (false);
        public bool ShouldSerializeIsOPID() => (false);
        public string AllowChannel { get; set; }
        public bool IsSpecialOp { get; set; }
    }
    public class SlabCommission : OperatorDetail
    {
        public int APIID { get; set; }
        public int SlabID { get; set; }
        public int SlabDetailID { get; set; }
        public decimal Comm { get; set; }
        public int CommType { get; set; }
        public int AmtType { get; set; }
        public int ComOnAmtType { get; set; }
        public int RoleID { get; set; }
        public decimal RComm { get; set; }
        public int RCommType { get; set; }
        public int RAmtType { get; set; }
        public int VID { get; set; }
        public string VName { get; set; }
        public int CircleID { get; set; }
        public string Circle { get; set; }
        public decimal TPComm { get; set; }
        public int TPCommType { get; set; }
        public int TPAmtType { get; set; }
        public int TPComOnAmtType { get; set; }

    }
    public class CommissionDisplay
    {
        public decimal Commission { get; set; }
        public bool CommType { get; set; }
        public decimal RCommission { get; set; }
        public bool RCommType { get; set; }
    }
    public class SlabRequest : CommonReq
    {
        public SlabCommission Commission { get; set; }
    }
    public class OperatorRequest : CommonReq
    {
        public OperatorDetail Detail { get; set; }
    }
    public class MLM_SlabDetailModel
    {
        public int SlabID { get; set; }
        public bool IsAdminDefined { get; set; }
        public bool IsChannel { get; set; }
        public List<MLM_SlabCommission> mlmSlabDetails { get; set; }
        public List<MLM_SlabCommission> mlmParentSlabDetails { get; set; }
        public List<RoleMaster> Roles { get; set; }
        public List<OperatorDetail> Operators { get; set; }
        public List<OpTypeMaster> OpTypes { get; set; }
        public List<AffiliateVendors> AfVendors { get; set; }
        public int OpTypeID { get; set; }
        public SelectList DMRModelSelect { get; set; }
    }
    public class MLM_SlabCommission : OperatorDetail
    {
        public int APIID { get; set; }
        public int SlabID { get; set; }
        public int SlabDetailID { get; set; }
        public decimal Comm { get; set; }
        public int CommType { get; set; }
        public int AmtType { get; set; }
        public int RoleID { get; set; }
        public int LevelID { get; set; }
        public decimal RComm { get; set; }
        public int RCommType { get; set; }
        public int RAmtType { get; set; }
        public int VID { get; set; }
        public string VName { get; set; }
        public int CircleID { get; set; }
        public string Circle { get; set; }
    }
    public class MLMSlabRequest : CommonReq
    {
        public MLM_SlabCommission Commission { get; set; }
    }
    public class SlabDetailModel
    {
        public int SlabID { get; set; }
        public bool IsAdminDefined { get; set; }
        public bool IsChannel { get; set; }
        public List<SlabCommission> SlabDetails { get; set; }
        public List<SlabCommission> ParentSlabDetails { get; set; }
        public List<RoleMaster> Roles { get; set; }
        public List<OperatorDetail> Operators { get; set; }
        public List<OpTypeMaster> OpTypes { get; set; }
        public List<AffiliateVendors> AfVendors { get; set; }
        public int OpTypeID { get; set; }
        public SelectList DMRModelSelect { get; set; }
    }
    public class SlabDetailDisplayLvl
    {
        public int OID { get; set; }
        public int CommSettingType { get; set; }
        public int ServiceID { get; set; }
        public string SCode { get; set; }
        public string Operator { get; set; }
        public string OpType { get; set; }
        public int OpTypeID { get; set; }
        public string SPKey { get; set; }
        public bool IsBBPS { get; set; }
        public bool IsBilling { get; set; }
        public string BusinessModel { get; set; }
        public decimal Min { get; set; }
        public decimal Max { get; set; }
        public List<SlabRoleCommission> RoleCommission { get; set; }
        public string AllowChannel { get; set; }
        public bool IsSpecialOp { get; set; }
    }
    public class RangeSlabDetailDisplayLvl
    {
        public int OID { get; set; }
        public int RangeId { get; set; }
        public string Operator { get; set; }
        public string OpType { get; set; }
        public string SPKey { get; set; }
        public bool IsBBPS { get; set; }
        public bool IsBilling { get; set; }
        public string BusinessModel { get; set; }
        public decimal Min { get; set; }
        public decimal Max { get; set; }
        public decimal MinRange { get; set; }
        public decimal MaxRange { get; set; }
        public int DMRModelID { get; set; }
        public List<SlabRoleCommission> RoleCommission { get; set; }
    }
    public class SlabRoleCommission
    {
        public int RoleID { get; set; }
        public string Prefix { get; set; }
        public string Role { get; set; }
        public decimal Comm { get; set; }
        public decimal MaxComm { get; set; }
        public int CommType { get; set; }
        public int AmtType { get; set; }
        public decimal RComm { get; set; }
        public decimal RMaxComm { get; set; }
        public int RCommType { get; set; }
        public int RAmtType { get; set; }
        public string ModifyDate { get; set; }
    }
    public class OpTypeMaster
    {
        public int ID { get; set; }
        public string OpType { get; set; }
        public string APIOpType { get; set; }
        public string Remark { get; set; }
        public string SCode { get; set; }
        public int ServiceTypeID { get; set; }
        public bool IsB2CVisible { get; set; }
    }
    public class ErrorTypeMaster
    {
        public int ID { get; set; }
        public string ErrorType { get; set; }
        public string Remark { get; set; }
    }
    public class OperatorOptionRequest
    {
        public int LoginID { get; set; }
        public int OPID { get; set; }
        public int OptionID { get; set; }
        public List<OperatorDetail> OPList { get; set; }
        public List<OperatorDetail> OptionList { get; set; }
        public string DisplayName { get; set; }
        public string Remark { get; set; }
        public bool IsList { get; set; }
        public bool IsMultiSelection { get; set; }
    }
    public class APIOpCode
    {
        public int LoginID { get; set; }
        public int CircleID { get; set; }
        public int OID { get; set; }
        public string Operator { get; set; }
        public string Circle { get; set; }
        public string OpType { get; set; }
        public int APIID { get; set; }
        public string OpCode { get; set; }
        public string ModifyDate { get; set; }
        public string BillOpCode { get; set; }
        public IDictionary<string, string> APIs { get; set; }
        public List<APIDetail> APINameIDs { get; set; }
    }
    public class APIOpCodeReq : CommonReq
    {
        public APIOpCode aPIOpCode { get; set; }
    }
    public class ServicesPM
    {
        public IEnumerable<OperatorDetail> Detail { get; set; }
        public IEnumerable<CirlceMaster> Circles { get; set; }
        public int OPType { get; set; }
        public int ServiceID { get; set; }
        public bool IsDoubleFactor { get; set; }
        public List<LoanTypes> loanTypes { get; set; }
        public List<CustomerTypes> customerTypes { get; set; }
        public List<InsuranceTypes> insuranceTypes { get; set; }
        public List<BankMaster> bankMasters { get; set; }
    }
    public class SwitchAPIUser
    {
        public int LoginID { get; set; }
        public int OID { get; set; }
        public int UserID { get; set; }
        public int APIID { get; set; }
    }
    public class SwitchAPIUserReq : CommonReq
    {
        public SwitchAPIUser switchAPIUser { get; set; }
    }
    public class IncentiveDetail
    {
        public int Denomination { get; set; }
        public decimal Comm { get; set; }
        public bool AmtType { get; set; }
    }
    public class FlatCommissionDetail
    {
        public int RoleID { get; set; }
        public string Role { get; set; }
        public string LastModified { get; set; }
        public decimal CommRate { get; set; }
        public int UserID { get; set; }
    }
    public class UserWiseLimitResp
    {
        public int OID { get; set; }
        public int UserLimitID { get; set; }
        public string OperatorName { get; set; }
        public string OpType { get; set; }
        public decimal UsedLimit { get; set; }

    }
    public class UserLimitCUReq
    {
        public int LoginTypeID { get; set; }
        public int UserID { get; set; }
        public int OID { get; set; }
        public decimal UsedLimit { get; set; }
    }
    public class OpRechargePlanResp
    {
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public string URL { get; set; }
        public string URL2 { get; set; }
        public string APICode { get; set; }
        public int APIID { get; set; }
        public List<OpRechCircleDetals> CircleCodes { get; set; }
        public List<OpRechPlanType> PlanType { get; set; }
        public List<MasterRechPlanType> RechPlanType { get; set; }
    }
    public class MasterRechPlanType
    {
        public int ID { get; set; }
        public string RechargePlanType { get; set; }
    }
    public class OpRechCircleDetals
    {
        public int CircleID { get; set; }
        public string CircleCode { get; set; }
    }
    public class OpRechPlanType
    {
        public int TypeID { get; set; }
        public string PlanType { get; set; }
    }
    public class BulkInsertionObj
    {
        public int LT { get; set; }
        public int LoginID { get; set; }
        public DataTable tp_Rechargeplans { get; set; }
        public int OID { get; set; }
        public int PackageID { get; set; }

    }
    public class SaveBillerAxisBankRequest {
        public int OpTypeID { get; set; }
        public List<RoundpayFinTech.AppCode.ThirdParty.AxisBank.AxisBankBillerListModel> billerList { get; set; }
    }
    public class MapOperatorReq
    {
        public int OID { get; set; }
        public int OpTypeID { get; set; }
        public string OpName { get; set; }
        public IEnumerable<OperatorDetail> OperatorList { get; set; }
    }

    public class EXACTNESSMaster
    {
        public int ID { get; set; }
        public string EXACTNESS { get; set; }
    }
}
