using Fintech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System.Collections.Generic;

namespace RoundpayFinTech.AppCode.Interfaces
{
    public interface IWhatsappSenderNoML
    {
        IResponseStatus UpdateMapNumber(string r, string mn, bool ia, int id);
        IEnumerable<WhatsappAPIDetail> GetWhatsappSenderNoList(int id);
        //SMSAPIDetail GetSMSAPIDetailByID(int APIID);
        IResponseStatus SaveWtSenderNo(WhatsappAPIDetail req);
        IResponseStatus DeleteWtSenderNo(int id);
        //IResponseStatus ISSMSAPIActive(int ID, bool IsActive, bool IsDefault);
    }
}
