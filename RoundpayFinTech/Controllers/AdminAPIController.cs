using System;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RoundpayFinTech.Models;
using RoundpayFinTech.AppCode.MiddleLayer;
using Fintech.AppCode.StaticModel;
using System.Collections.Generic;
using RoundpayFinTech.AppCode.Interfaces;
using System.Linq;
using RoundpayFinTech.AppCode.Model;
using System.Threading.Tasks;
using Fintech.AppCode;
using Fintech.AppCode.Model;
using Fintech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Configuration;
using RoundpayFinTech.AppCode.Model.App;
using RoundpayFinTech.AppCode;
using RoundpayFinTech.AppCode.ThirdParty.PanMitra;
using Newtonsoft.Json;
using System.Threading;

namespace RoundpayFinTech.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class AdminAPIController : Controller
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        public AdminAPIController(IHttpContextAccessor accessor, IHostingEnvironment env)
        {
            _accessor = accessor;
            _env = env;
        }
        public IActionResult Index()
        {
            return Ok("");
        }
        [HttpPost]
        public IActionResult Test([FromForm] PANMitraOnboardRequest onboardRequest)
        {
            return Json(onboardRequest);
        }
        [HttpGet]
        public IActionResult TransactionSummary(string APIKey, string FDate, string TDate)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("APIKey:");
                sb.Append(APIKey ?? "");
                sb.Append("|");
                sb.Append("FDate:");
                sb.Append(FDate ?? "");
                sb.Append("|");
                sb.Append("TDate:");
                sb.Append(TDate ?? "");
                sb.Append("|");
                var tranxnSummaryReq = new TranxnSummaryReq
                {
                    APIKey = APIKey ?? "",
                    Req = sb.ToString()
                };
                var adminML = new AdminML(_accessor, _env);
                var respStatus = adminML.ValidateAdmin(tranxnSummaryReq);
                var adminTranModel = new AdminTranModel
                {
                    Statuscode = "1",
                    Status = "Fail",
                    Fdate = Convert.ToDateTime(FDate).ToString("yyyy-MM-dd 00:00:00"),
                    Tdate = Convert.ToDateTime(TDate).ToString("yyyy-MM-dd 00:00:00"),
                    TRNSUMMARY = new List<AdminTransactionSummary>()
                };
                if (respStatus.Statuscode == ErrorCodes.One)
                {
                    IAppReportML appReportML = new ReportML(_accessor, _env, false);
                    var TRNSUMMARY = appReportML.GetUSalesSummaryByAdmin(Convert.ToDateTime(FDate).ToString("dd MMM yyyy"), Convert.ToDateTime(TDate).ToString("dd MMM yyyy"));
                    if (TRNSUMMARY.Any())
                    {
                        adminTranModel.Statuscode = "0";
                        adminTranModel.Status = "Success";
                        adminTranModel.TRNSUMMARY = TRNSUMMARY;
                    }
                }
                return Json(adminTranModel);
            }
            catch (Exception ex)
            {
                return Json(new { exception = ex.Message });
            }
        }

        [Route("LowBalanceAlert")]
        public async Task<IActionResult> LowBalanceAlert()
        {
            if (ApplicationSetting.IsLowBalanceAlertAllowed)
            {
                var adminML = new AdminML(_accessor, _env);
                await adminML.LowBalanceLAlert().ConfigureAwait(false);
            }
            return Ok();
        }

        [Route("LowBalanceAlertMultiTrhead")]
        public async Task<IActionResult> LowBalanceAlertMultiTrhead()
        {
            try
            {
                await Task.Delay(0);
                if (ApplicationSetting.IsLowBalanceAlertAllowed)
                {
                    var adminML = new AdminML(_accessor, _env);
                    var _List = adminML.GetListForLowBalanceAlert();
                    var threads = new List<Thread>();
                    if (_List != null && _List.Count > 0)
                    {
                        int countThread = (int)Math.Ceiling((decimal)_List.Count() / 100);
                        for (int i = 0; i < countThread; i++)
                        {
                            var partialList = _List.Skip(i * 100).Take(100).ToList();
                            Thread thr = new Thread(() => adminML.LowBalanceLAlertMultiThread(partialList));
                            threads.Add(thr);
                        }
                    }
                    threads.ForEach(t => t.Start());
                    //await adminML.LowBalanceLAlertMultiThread(_List).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {

            }
            return Ok("Task Sheduled");
        }

        [HttpPost("AdminAPI/API/UserSubscription")]
        [Route("API/UserSubscription")]
        public IActionResult UserSubscriptionApp([FromBody] GetIntouch getIntouch)
        {
            ILoginML _loginML = new LoginML(_accessor, _env, false);
            var responseStatus = _loginML.UserSubscriptionApp(getIntouch);
            return Json(new { responseStatus.Statuscode, responseStatus.Msg });
        }

        [HttpPost("AdminAPI/API/UserCreation")]
        [Route("API/UserCreation")]
        public IActionResult UserCreation([FromBody] UserCreate userCreate)
        {
            userCreate.RequestModeID = RequestMode.API;
            userCreate.WID = 1;
            IUserAPPML userML = new UserML(_accessor, _env, false);
            var resp = userML.CallSignupFromApp(userCreate);
            var plansAPIML = new PlansAPIML(_accessor, _env, false);
            plansAPIML.LogAppReqResp(new CommonReq
            {
                CommonStr = "UserCreation",
                CommonStr2 = JsonConvert.SerializeObject(userCreate),
                CommonStr3 = JsonConvert.SerializeObject(resp)
            });
            return Json(new { resp.Statuscode, resp.Msg });
        }
        [Route("BirthdayWishAlert")]
        public async Task<IActionResult> BirthdayWishAlert()
        {
            if (ApplicationSetting.IsBirthdayWishAlert)
            {
                var adminML = new AdminML(_accessor, _env);
                await adminML.BirthdayWishAlert().ConfigureAwait(false);
            }
            return Ok();
        }

    }
}