using Fintech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model.ProcModel;


namespace RoundpayFinTech.AppCode.Interfaces
{
    public interface IUserWebsite
    {
        CompanyProfileDetail GetCompanyProfile(int WID);
        IResponseStatus CompanyProfileCU(CompanyProfileDetailReq req);
        CompanyProfileDetail GetCompanyProfileUser(int UserId);
    }
}
