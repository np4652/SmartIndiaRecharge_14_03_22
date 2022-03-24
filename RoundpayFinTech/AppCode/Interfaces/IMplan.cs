using RoundpayFinTech.AppCode.Model.ROffer;


namespace RoundpayFinTech.AppCode.Interfaces
{
    public interface IMplan
    {
        SubMplanRofferResp GetRoffer(string AccountNo, int OID);
        SubMplanSimplePlanResp GetSimplePlan(int CircleID, int OID);
        SubMplanDTHSimplePlanResp GetDTHSimplePlan(int OID);
        SubMplanDTHSimplePlanResp GetDTHChannelPlan(int OID);
        SubMplanDTHCustomerInfo GetDTHCustInfo(string AccountNo, int OID);
        MplanDTHHeavyRefresh GetDTHHeavyRefresh(string AccountNo, int OID);
    }
    public interface IRoundpayPlan {
        SubMplanRofferResp GetRofferRoundpay(string AccountNo, int OID);
        RoundpaySimplePlanResp GetSimplePlanRoundpay(int CircleID, int OID);
        RoundpaySubMplanDTHSimplePlanResp GetDTHSimplePlanRoundpay(int OID);
        SubMplanDTHCustomerInfo GetDTHCustInfoRoundpay(string AccountNo, int OID);
        RoundpayDTHHeavyRefresh GetDTHRPHeavyRefresh(string AccountNo, int OID);
        RPDTHPlansSimpleOfPackages RPDTHSimplePlansOfPackages(int OID);
        RPGetDTHChannelList RPDTHSimplePlansChannelList(string PackageID, int OID);
        RPDTHPlansSimpleOfPackages RPDTHPlanListByLanguage(int OID, string Langauge);
    }
    public interface IPlanAPIPlan {
        PlanApi GetRofferPLANAPI(string AccountNo, int OID);
        PlanApiViewPlan GetSimplePlanAPI(int CircleID, int OID);
        DTHPlan GetDTHSimplePlanAPI(int OID);
        DTHPlan GetDTHChannelPlanAPI(int OID);
        DTHCustomerInfo GetDTHCustInfoPlanAPI(string AccountNo, int OID);
    }
    #region Cyrus
    public interface ICyrusAPIPlan
    {
        CyrusPlanAPI GetRofferCYRUS(string AccountNo, int OID);
    }
    #endregion

    #region VastWeb
    public interface IVastWebPlan
    {
        VastWebRPlan GetRofferVastWeb(string AccountNo, int OID);
        VastWebDTHCustInfo GetDTHCustInfoVastWeb(string AccountNo, int OID);
    }
    #endregion

    public interface IMyPlanAPI
    {
        MyPlanApi GetRofferMyPlanApi(string AccountNo, int OID);
        MyPlanApiSimplePlan GetSimpleMyPlanAPI(int CircleID, int OID);
        MyPlanDTHCustomerInfo GetDTHCustInfoMyPlan(string AccountNo, int OID);
        MyPlanApiDthplan GetDthSimpleMyPlanApi(int OID);
        SubMplanDTHSimplePlanResp AppGetDthSimpleMyPlan(int OID);
        Root GetDthHaveyRefershMyPlan(string AccountNo, int OID);
    }
}
