using Fintech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System.Collections.Generic;

namespace RoundpayFinTech.AppCode.Interfaces
{
    public interface ISMSAPIML
    {
        IEnumerable<SMSAPIDetail> GetSMSAPIDetail();
        SMSAPIDetail GetSMSAPIDetailByID(int APIID);
        IResponseStatus SaveSMSAPI(APIDetail req);
        IResponseStatus ISSMSAPIActive(int ID, bool IsActive, bool IsDefault);
    }
}
