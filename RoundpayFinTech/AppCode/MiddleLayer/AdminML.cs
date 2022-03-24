using Fintech.AppCode.Configuration;
using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.MiddleLayer
{
    public sealed class AdminML
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly IDAL _dal;
        private readonly IConnectionConfiguration _c;
        private readonly IRequestInfo _info;
        public AdminML(IHttpContextAccessor accessor, IHostingEnvironment env)
        {
            _accessor = accessor;
            _env = env;
            _c = new ConnectionConfiguration(_accessor, _env);
            _dal = new DAL(_c.GetConnectionString());
            _info = new RequestInfo(_accessor, _env);
        }
        public IResponseStatus ValidateAdmin(TranxnSummaryReq tranxnSummaryReq)
        {
            tranxnSummaryReq.IP = _info.GetRemoteIP();
            tranxnSummaryReq.Browser = _info.GetBrowser();
            IProcedure proc = new ProcValidateAdminAPIKey(_dal);
            return (ResponseStatus)proc.Call(tranxnSummaryReq);
        }
        public List<AlertReplacementModel> GetListForLowBalanceAlert()
        {
            IProcedure proc = new ProcGetLowBalanceAlert(_dal);
            var res = (List<AlertReplacementModel>)proc.Call();
            return res;
        }

        public async Task LowBalanceLAlert()
        {
            await Task.Delay(0).ConfigureAwait(false);
            var _List = GetListForLowBalanceAlert();
            if (_List != null && _List.Count > 0)
            {
                SMSSetting smsSetting = new SMSSetting { WID = 0 };
                EmailSettingswithFormat emailSetting = new EmailSettingswithFormat { WID = 0 };
                foreach (var item in _List)
                {
                    //item.LoginID = 1;
                    IAlertML alertMl = new AlertML(_accessor, _env);
                    Parallel.Invoke(() => alertMl.LowBalanceSMS(item, smsSetting),
                        () => alertMl.LowBalanceEmail(item, emailSetting),
                        () => alertMl.LowBalanceNotification(item),
                        () => alertMl.WebNotification(item));
                }
            }
        }

        public async Task LowBalanceLAlertMultiThread(List<AlertReplacementModel> _List)
        {
            await Task.Delay(0).ConfigureAwait(false);
            SMSSetting smsSetting = new SMSSetting { WID = 0 };
            EmailSettingswithFormat emailSetting = new EmailSettingswithFormat { WID = 0 };

            foreach (var item in _List)
            {
                //item.LoginID = 1;
                IAlertML alertMl = new AlertML(_accessor, _env, false);
                Parallel.Invoke(() => alertMl.LowBalanceSMS(item, smsSetting),
                    () => alertMl.LowBalanceEmail(item, emailSetting),
                    () => alertMl.LowBalanceNotification(item),
                    () => alertMl.WebNotification(item));
            }
        }

        #region BirtdhayWishAlert
        public List<AlertReplacementModel> GetListForBirthdayAlert()
        {
            IProcedure proc = new ProcGetBirthdayWishAlert(_dal);
            var res = (List<AlertReplacementModel>)proc.Call();
            return res;
        }
        public async Task BirthdayWishAlert()
        {
            await Task.Delay(0).ConfigureAwait(false);
            var _List = GetListForBirthdayAlert();
            if (_List != null && _List.Count > 0)
            {
                SMSSetting smsSetting = new SMSSetting { WID = 0 };
                EmailSettingswithFormat emailSetting = new EmailSettingswithFormat { WID = 0 };
                foreach (var item in _List)
                {
                    //item.LoginID = 1;
                    IAlertML alertMl = new AlertML(_accessor, _env);
                    Parallel.Invoke(() => alertMl.BirthdayWishSMS(item, smsSetting),
                        () => alertMl.BirthdayWishEmail(item, emailSetting),
                        () => alertMl.BirthdayWishNotification(item),
                        () => alertMl.WebNotification(item));
                }
            }
        }
        #endregion
    }
}
