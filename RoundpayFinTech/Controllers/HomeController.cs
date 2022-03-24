using Fintech.AppCode;
using Fintech.AppCode.Configuration;
using Fintech.AppCode.DB;
using Fintech.AppCode.HelperClass;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RoundpayFinTech.AppCode.Classes;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.MiddleLayer;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly IDAL _dal;
        private readonly IConnectionConfiguration _c;
        private readonly ISession _session;
        private readonly ILoginML loginML;
        private readonly WebsiteInfo _WInfo;
        public HomeController(IHttpContextAccessor accessor, IHostingEnvironment env)
        {
            _accessor = accessor;
            _env = env;
            _c = new ConnectionConfiguration(_accessor, _env);
            _dal = new DAL(_c.GetConnectionString());
            _session = _accessor.HttpContext.Session;
            loginML = new LoginML(_accessor, _env);
            _WInfo = loginML.GetWebsiteInfo();
        }

        public IActionResult Index_old()
        {
            string url = string.Empty;
            url = _WInfo != null ? "~/views/home/Themes/" + _WInfo.ThemeId + "/index.cshtml" : "~/views/login/index.cshtml";
            ViewData["Theme"] = _WInfo.ThemeId;
            var req = new CommonReq
            {
                CommonInt = _WInfo.WID,
                CommonInt2 = _WInfo.ThemeId
            };
            IProcedure proc = new ProcGetDisplayHtml(_dal);
            var response = (HomeDisplay)proc.Call(req);
            return View(url, response);
            //if (_WInfo != null)
            //{
            //    {
            //        if (_WInfo.WID != 1)
            //        {
            //            return RedirectToAction("Index", "Login");
            //        }
            //    }
            //    return Ok();
            //}
        }

        public async Task<IActionResult> Index()
        {
            IWebsiteML ml = new WebsiteML(_accessor, _env);
            var res = new IndexViewModel
            {
                WID = _WInfo.WID,
                SiteId = _WInfo.SiteId,
                Assets = GetAssets(_WInfo.SiteId),
                Content = await ml.GetWebsiteContentAsync(_WInfo.WID).ConfigureAwait(true)
            };
            return View(res);
        }
        private IEnumerable<string> GetAssets(int siteId)
        {
            var assets = new List<string>();
            try
            {
                var path = DOCType.SiteconfigJsonFilePath;
                if (System.IO.File.Exists(path))
                {
                    var jsonData = System.IO.File.ReadAllText(path);
                    var res = JsonConvert.DeserializeObject<List<SiteTemplate>>(jsonData);
                    assets = res.Where(x => x.TemplateId == siteId).FirstOrDefault()?.Assets.ToList();
                }
            }
            catch (Exception ex)
            {

            }
            return assets ?? new List<string>();
        }

        [HttpGet("aboutus")]
        public IActionResult AboutUs()
        {
            if (_WInfo != null)
            {
                if (_WInfo.WID != 1)
                {
                    return RedirectToAction("Index", "Login");
                }
            }
            return Ok();
            return View("about-us");

        }
        [HttpGet("contactus")]
        public IActionResult ContactUs()
        {
            if (_WInfo != null)
            {
                if (_WInfo.WID != 1)
                {
                    return RedirectToAction("Index", "Login");
                }
            }
            return Ok();
            return View("contact-us");

        }
        [HttpGet("servicedetail")]
        public IActionResult ServiceDetail()
        {
            if (_WInfo != null)
            {
                if (_WInfo.WID != 1)
                {
                    return RedirectToAction("Index", "Login");
                }
            }
            return Ok();
            return View("service-detail");

        }
        public IActionResult Hit(string s, bool IsE)
        {
            if (IsE)
            {
                return Ok(HashEncryption.O.Encrypt(s));
            }
            else
            {
                return Ok(HashEncryption.O.Decrypt(s));
            }
            //return Ok(HashEncryption.O.AppEncrypt(s));
        }
        [HttpGet("privacy")]
        public IActionResult PrivacyPolicy()
        {
            return View();
        }
        [HttpGet("t-c")]
        public IActionResult TermsAndCondition()
        {
            return View();
        }
        [HttpPost]
        [Route("/WebsitePopup")]
        public IActionResult WebsitePopUp()
        {
            var WebInfo = loginML.GetPopupInfo();
            if (WebInfo.ISWebSitePopup)
            {
                return PartialView("Partial/_websitePopup", WebInfo);
            }
            else
            {
                return Ok();
            }
        }
        [HttpPost]
        [Route("GetinTouch")]
        [Route("Home/GetinTouch")]
        public IActionResult UserSubscription([FromBody] GetIntouch LoginDetail)
        {
            IResponseStatus _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            ILoginML ml = new LoginML(_accessor, _env);
            var _resp = ml.UserSubscription(LoginDetail);
            _res.Statuscode = _resp.Statuscode;
            _res.Msg = _resp.Msg;
            return Json(_res);
        }

        [HttpGet]
        [Route("InviteApp/{id}")]
        public ActionResult InviteApp(int id)
        {
            ILoginML ml = new LoginML(_accessor, _env);
            WebsiteInfo data = ml.GetWebsitePackage(id);
            if (data.AppPackageID == "")
            {

                return Redirect("http://" + data.WebsiteName + "/SignUp?rid=" + id);
            }
            return View(data);
        }

        public IActionResult IrctcCertificate()
        {

            return PartialView("~/views/report/PartialView/_IrctcCertficated.cshtml");
        }
    }
}
