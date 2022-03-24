using RoundpayFinTech.AppCode.Model.ROffer;

namespace RoundpayFinTech.AppCode.Interfaces
{
    public interface ILookUpPlanAPI
    {
        HLRResponseStatus GetLookUp(string URL, int APIID, int UserId, string Mobile = "");
    }

    public interface ILookUpGoRecharge
    {
        HLRResponseStatus GetLookUpGoRecharge(string URL, int APIID, int UserId, string Mobile);
    }
    public interface ILookUpMyPlan
    {
        HLRResponseStatus GetLookUpApiMyPlans(string URL, int APIID, int UserId, string Mobile = "");
    }
    public interface ILookUpRoundpay
    {
        HLRResponseStatus GetLookUpRoundpay(string URL, int APIID, int UserId, string Mobile = "");
    }
    public interface ILookUpAPIBox
    {
        HLRResponseStatus GetLookUpAPIBox(string URL, int APIID, int UserId, string Mobile = "");
    }
    public interface ILookUpMPLAN
    {
        HLRResponseStatus GetLookUpMplan(string URL, int APIID, int UserId, string Mobile = "");
    }

    public interface ILookUpVASTWEB
    {
        HLRResponseStatus GetHLRVastWeb(string URL, int APIID, int UserId, string Mobile = "");
    }

    public interface ILookUpInfoAPI
    {
        HLRResponseStatus GetHLRINFOAPI(string URL, int APIID, int UserId, string Mobile = "");
    }

    public interface ILookUpAirtelPP
    {
        HLRResponseStatus GetHLRAirtelPostpaid(string URL, int APIID, int UserId, string Mobile = "");
    }
}
