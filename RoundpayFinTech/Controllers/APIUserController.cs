using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Fintech.AppCode.Model;
using Fintech.AppCode.Configuration;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.Report;
using RoundpayFinTech.AppCode.MiddleLayer;

namespace APIOnly.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class APIUserController : Controller
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private RequestInfo _RequestInfo;
        private readonly ISession _session;
        private LoginResponse _lr;

        public APIUserController(IHttpContextAccessor accessor, IHostingEnvironment env)
        {
            _accessor = accessor;
            _env = env;
            _RequestInfo = new RequestInfo(_accessor, _env);
            _session = _accessor.HttpContext.Session;
            _lr = _session.GetObjectFromJson<LoginResponse>(SessionKeys.LoginResponse);
        }
        public IActionResult Index()
        {
            if (IsInValidSession())
            {
                return Redirect("~/");
            }
            return View();
        }


        public IActionResult UserProfile()
        {
            return View();
        }

        private bool IsInValidSession()
        {
            string AppSessionID = HttpContext.Session.GetString(SessionKeys.AppSessionID);
            return (string.IsNullOrEmpty(AppSessionID) || AppSessionID.Length != Numbers.THIRTY_TWO);
        }


        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (IsInValidSession() && (context.RouteData.Values["Action"].ToString() != "Index" || context.RouteData.Values["Controller"].ToString() != "APIUser"))
            {
                context.Result = new RedirectResult("~/");
            }
            else
            {
                base.OnActionExecuting(context);
            }
        }

        [HttpPost]
        [Route("APIUser/ChangeToken")]
        [Route("ChangeToken")]
        public IActionResult _ChangeApiUserToken()
        {
            if (_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.APIUser)
            {
                var req = new UserRequset
                {
                    LoginID = _lr.UserID,
                    LoginTypeID = _lr.LoginTypeID,
                    UserId = _lr.UserID,
                    MobileNo = ""
                };
                var userML = new UserML(_accessor, _env);
                var _res = userML.GetAppUserDetailByID(req);
                return PartialView("PartialView/_ChangeToken", _res);
            }
            return Ok();
        }
        [HttpPost]
        [Route("APIUser/Change-Token")]
        [Route("Change-Token")]
        public IActionResult ChangeApiUserToken()
        {
            IAPIUserML userML = new UserML(_accessor, _env);
            var _res = userML.SetGetToken();
            return Json(_res);
        }
        [Route("APIUser/CallBackUrl")]
        [Route("CallBackUrl")]
        public IActionResult CallBackUrl()
        {
            return View();
        }
        [HttpPost]
        [Route("APIUser/CallBack-Url")]
        [Route("CallBack-Url")]
        public IActionResult _CallBackUrl()
        {
            IAPIUserML userML = new UserML(_accessor, _env);
            var res = userML.GetCallBackUrl();
            return PartialView("PartialView/_CallBackUrl", res);
        }
        [HttpPost]
        [Route("APIUser/callbakcu")]
        [Route("callbakcu")]
        public IActionResult _CallBackUrlCU(int Type)
        {
            IAPIUserML userML = new UserML(_accessor, _env);
            var res = userML.GetCallBackUrl(Type);
            return PartialView("PartialView/_CallBackUrlCU", res);
        }
        [HttpPost]
        [Route("APIUser/save-callback")]
        [Route("save-callback")]
        public IActionResult _SubmitCallbackUrl(int Type, string URl, string Remark, string UpdateUrl)
        {
            IAPIUserML userML = new UserML(_accessor, _env);
            var req = new UserCallBackModel
            {
                CallbackType = Type,
                URL = URl,
                Remark = Remark,
                UpdateUrl = UpdateUrl
            };
            var res = userML.SaveCallback(req);
            return Json(res);
        }

        [Route("APIUser/GetToken")]
        [Route("GetToken")]
        public IActionResult _GetApiUserToken()
        {
            if (_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.APIUser)
            {
                var req = new UserRequset
                {
                    LoginID = _lr.UserID,
                    LoginTypeID = _lr.LoginTypeID,
                    UserId = _lr.UserID,
                    MobileNo = ""
                };
                var userML = new UserML(_accessor, _env);
                var _res = userML.GetAppUserDetailByID(req);

                return Json(_res);
            }
            return Ok();
        }

        [Route("_AgreementApprovalNotification")]
        [HttpPost]
        public IActionResult AgreementApprovalNotification()
        {
            IReportML ml = new ReportML(_accessor, _env);
            var _resp = ml.GetAgreementApprovalNotification(_lr.UserID);
            if (!_resp.Status)

                return PartialView("PartialView/_AgreementApprovalNotification", _resp);
            else
                return Json(new { status = true });

            //if (!_resp.Status)
            //    return PartialView("Partial/_AgreementApprovalNotification", _resp);
            //else
            //    return Json(new { status = true });
        }

    }
}