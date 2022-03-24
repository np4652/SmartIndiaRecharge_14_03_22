using Fintech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Interfaces
{
    public interface IPartnerML
    {
        ResponseStatus CallSavePartner(PartnerCreate req);
        //PartnerListResp GetPartnerList(int UserID);
        PartnerDetailsResp GetPartnerByID(int ID);
        PartnerDetailsResp GetPartnerByID(PartnerCreate req);
        PartnerListResp GetPartnerList(int UserID, string s = "");
        IResponseStatus ChangeStatus(int ID);
        IResponseStatus UpdateStatus(int Status, int ID);
        Task<IResponseStatus> ValidateAEPSURL(string UrlSession);
        AEPSURLSessionResp IsInValidPartnerSession();
        bool CheckPsaId(string PSAId);
        bool UpdatePsaId(string PSAId, string FatherName);
        PartnerAEPSResponseModel PartnerAEPS(int partnerID);
        List<BankMaster> bindAEPSBanks(string bankName);
    }
}
