using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Microsoft.AspNetCore.Http;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Interfaces
{
    public interface IEmailAPIML
    {
        IEnumerable<EmailAPIDetail> GetEmailAPIDetail();
        EmailAPIDetail GetEmailAPIDetailByID(int APIID);
        IResponseStatus SendEmailToId(int APIID, string ToMail);

        IResponseStatus SaveEmailAPI(EmailAPIDetail req);
        List<EmailProvider> GetEmailProviderDetail();
    }
}
