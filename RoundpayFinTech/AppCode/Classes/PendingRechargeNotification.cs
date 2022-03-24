using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.MiddleLayer;

namespace RoundpayFinTech.AppCode.Classes
{
    public class PendingNotifications
    {
        public async Task PendingRechargeNotificationAsync(IHttpContextAccessor _accessor, IHostingEnvironment _env, int UserID, bool isCallByAutorply = false, string userName = "", string msg = "")
        {
            IReportML ml = new ReportML(_accessor, _env);
            IAlertML alert = new AlertML(_accessor, _env);
            var list = await ml.PendingRechargeNotification(isCallByAutorply, UserID).ConfigureAwait(true);
            var distinctAPIId = list.Select(x => x.APIID).Distinct();
            foreach (var id in distinctAPIId)
            {
                var accountNo = list.Where(x => x.APIID == id).Select(y => "%0a" + y.AccountNo + " : " + y.Duration + " (min)");
                if (accountNo != null && accountNo.Count() > 0)
                    accountNo = accountNo.Distinct();
                var f = list.Where(x => x.APIID == id).Select(y => new { y.APIID, y.APICode, y.WhatsappNo, y.HangoutId, y.TelegramNo, y.Name, y.CCID, y.CCName }).FirstOrDefault();
                string formatedMsg = string.Empty;
                if (!string.IsNullOrEmpty(msg))
                {
                    msg = removeMobileNos(msg);
                    formatedMsg = string.Concat(msg, string.Join(",", accountNo));
                }
                var modal = new AlertReplacementModel
                {
                    WhatsappNo = f.WhatsappNo,
                    TelegramNo = f.TelegramNo,
                    HangoutNo = f.HangoutId,
                    SenderName = f.Name,
                    AccountNo = string.Join(",", accountNo),
                    FormatID = MessageFormat.PendingRechargeNotification,
                    LoginID = UserID,
                    LoginUserName = userName,
                    CCID = f.CCID,
                    CCName = f.CCName,
                    APICode = f.APICode,
                    Message = formatedMsg
                };
                alert.SocialAlert(modal);
            }
        }
        public async Task PendingRefundNotificationAsync(IHttpContextAccessor _accessor, IHostingEnvironment _env, int UserID, bool isCallByAutorply = false, int APIId = 0, string userName = "", string msg = "")
        {
            IReportML ml = new ReportML(_accessor, _env);
            IAlertML alert = new AlertML(_accessor, _env);
            var list = await ml.PendingRefundNotification(isCallByAutorply, UserID, 1, APIId).ConfigureAwait(true);
            var distinctAPIId = list.Select(x => x.APIID).Distinct();
            foreach (var id in distinctAPIId)
            {
                var accountNo = list.Where(x => x.APIID == id).Select(y => "%0a" + y.AccountNo + " : " + y.Duration + " (min)");
                if (accountNo != null && accountNo.Count() > 0)
                    accountNo = accountNo.Distinct();
                var f = list.Where(x => x.APIID == id).Select(y => new { y.APIID, y.APICode, y.WhatsappNo, y.HangoutId, y.TelegramNo, y.Name, y.CCID, y.CCName }).FirstOrDefault();
                string formatedMsg = string.Empty;
                if (!string.IsNullOrEmpty(msg))
                {
                    msg = removeMobileNos(msg);
                    formatedMsg = string.Concat(msg, string.Join(",", accountNo));
                }
                var modal = new AlertReplacementModel
                {
                    WhatsappNo = f.WhatsappNo,
                    TelegramNo = f.TelegramNo,
                    HangoutNo = f.HangoutId,
                    SenderName = f.Name,
                    AccountNo = string.Join(",", accountNo),
                    FormatID = MessageFormat.PendingRefundNotification,
                    LoginID = UserID,
                    LoginUserName = userName,
                    CCID = f.CCID,
                    CCName = f.CCName,
                    APICode = f.APICode,
                    Message = formatedMsg
                };
                alert.SocialAlert(modal);
            }
        }

        public string removeMobileNos(string text)
        {
            //var exp = new Regex(@"(\(?[0-9]{10}\)?)?\-?[0-9]{3}\-?[0-9]{4}",RegexOptions.IgnoreCase);
            var exp = new Regex(@"((\+*)((0[ -]*)*|((91 )*))((\d{12})+|(\d{10})+))|\d{5}([- ]*)\d{6}", RegexOptions.IgnoreCase);
            return exp.Replace(text ?? string.Empty, string.Empty);
        }
    }
}