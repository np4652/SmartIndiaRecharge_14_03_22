using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fintech.AppCode;
using Fintech.AppCode.Configuration;
using Fintech.AppCode.HelperClass;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using RoundpayFinTech.AppCode;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.MiddleLayer;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Reports.Filter;
using RoundpayFinTech.AppCode.StaticModel;
using RoundpayFinTech.Models;
using static RoundpayFinTech.AppCode.Model.ProcModel.PartnerDetailsResp;

namespace RoundpayFinTech.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class PartnerController : Controller
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly ISession _session;
        private readonly LoginResponse _lr;
        private readonly ILoginML loginML;
        public PartnerController(IHttpContextAccessor accessor, IHostingEnvironment env)
        {
            _accessor = accessor;
            _env = env;
            _session = _accessor.HttpContext.Session;
            _lr = _session.GetObjectFromJson<LoginResponse>(SessionKeys.LoginResponse);
            loginML = new LoginML(_accessor, _env);
        }

        [Route("PartnerView")]
        public IActionResult PartnerListView(string s)
        {
            if (loginML.IsInValidSession())
            {
                return RedirectToAction("Index", "Login");
            }
            if (s == "partners")
            {
                return View("PartnerListView", s);
            }
            return View();
        }
        [HttpPost]
        [Route("PartnerList")]
        public IActionResult _PartnerListView(string s)
        {
            IPartnerML partnerML = new PartnerML(_accessor, _env);
            int UserID = _lr.UserID;
            var model = partnerML.GetPartnerList(UserID, s);
            if (_lr.RoleID != Role.Admin && model.list.Count < 1)
            {
                RegisterAsChildPartnerResp model1 = new RegisterAsChildPartnerResp();
                return PartialView("Partial/_RegisterAsChildPartner");
            }
            else
            {
                model.RoleID = _lr.RoleID;
                return PartialView("Partial/_PartnerList", model);
            }
        }

        [Route("addnewpartner")]
        public IActionResult AddNewPartnerView()
        {
            if (loginML.IsInValidSession())
            {
                return RedirectToAction("Index", "Login");
            }
            PartnerCreate partner = new PartnerCreate();
            partner.DOB = DateTime.Now;
            return View(partner);
        }

        [Route("modifypartner")]
        public IActionResult ModifyPartnerView(int id)
        {
            if (loginML.IsInValidSession())
            {
                return RedirectToAction("Index", "Login");
            }
            IPartnerML partnerML = new PartnerML(_accessor, _env);
            var model = partnerML.GetPartnerByID(new PartnerCreate
            {
                ID = id,
                UserID = _lr.UserID
            });
            model.data.Banner = "Image/Partner/BGI/" + model.data.ID.ToString() + ".png";
            model.data.Logo = "Image/Partner/LOGO/" + model.data.ID.ToString() + ".png";
            return View(model);
        }

        [Route("savePartner")]
        [HttpPost]
        public IActionResult SavePartner([FromBody] PartnerCreate UserData)
        {

            if (loginML.IsInValidSession())
            {
                return RedirectToAction("Index", "Login");
            }
            IPartnerML partnerML = new PartnerML(_accessor, _env);
            IResponseStatus _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };
            _res = partnerML.CallSavePartner(UserData);
            return Json(_res);
        }

        [HttpPost]
        [Route("UploadImage")]
        public IActionResult UploadImage(int id)
        {
            if (loginML.IsInValidSession())
            {
                return RedirectToAction("Index", "Login");
            }
            var model = new UploadImageModel();
            if (id > 0)
            {
                model.Id = id;
                model.LogoPath = "/image/partner/logo/" + id + ".png";
                model.BgImgPath = "/image/partner/bgi/" + id + ".png";
                model.BannerImgPath = "/image/partner/banner/" + id + ".png";
            }
            return PartialView("Partial/_UploadImage", model);
        }

        [HttpPost]
        [Route("Admin/partner-upload-logo")]
        [Route("partner-upload-logo")]
        public IActionResult PartnerUploadLogo(IFormFile fileLogo, IFormFile fileBanner, IFormFile fileBackground, string PartnerID)
        {
            if (loginML.IsInValidSession())
            {
                return RedirectToAction("Index", "Login");
            }
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            IBannerML _bannerML = new ResourceML(_accessor, _env);

            var _res1 = _bannerML.UploadPartnerImages(fileLogo, PartnerID, _lr, FolderType.Logo);
            if (_res1.Statuscode == ErrorCodes.One)
            { _res.CommonStr = "Image/Partner/LOGO/" + PartnerID + ".png"; }
            else { _res.CommonStr = _res1.Msg; _res.Msg = _res1.Msg; }

            var _res2 = _bannerML.UploadPartnerImages(fileBackground, PartnerID, _lr, FolderType.BgImage);
            if (_res2.Statuscode == ErrorCodes.One)
            { _res.CommonStr2 = "Image/Partner/BGI/" + PartnerID + ".png"; }
            else
            { _res.CommonStr2 = _res2.Msg; _res.Msg = _res2.Msg; }

            var _res3 = _bannerML.UploadPartnerImages(fileBanner, PartnerID, _lr, FolderType.Banner);
            if (_res3.Statuscode == ErrorCodes.One)
            { _res.CommonStr3 = "Image/Partner/Banner/" + PartnerID + ".png"; }
            else
            { _res.CommonStr3 = _res3.Msg; _res.Msg = _res3.Msg; }

            if (_res1.Statuscode == ErrorCodes.One && _res2.Statuscode == ErrorCodes.One && _res3.Statuscode == ErrorCodes.One)
            {
                _res.Statuscode = ErrorCodes.One;
                _res.Msg = ErrorCodes.SUCCESS;
            }
            return Json(_res);
        }

        [HttpPost]
        [Route("Admin/partner-upload-banner")]
        [Route("partner-upload-banner")]
        public IActionResult PartnerUploadBanner(IFormFile file, string PartnerID)
        {
            if (loginML.IsInValidSession())
            {
                return RedirectToAction("Index", "Login");
            }
            IBannerML _bannerML = new ResourceML(_accessor, _env);
            var _res = _bannerML.UploadPartnerImages(file, PartnerID, _lr, FolderType.BgImage);
            _res.CommonStr = "Image/Partner/BGI/" + PartnerID + ".png";
            return Json(_res);
        }

        [HttpPost]
        [Route("Admin/TogglePartnerStatus")]
        [Route("TogglePartnerStatus")]
        public IActionResult ChangeStatus(int ID)
        {
            if (loginML.IsInValidSession())
            {
                return RedirectToAction("Index", "Login");
            }
            IPartnerML partnerML = new PartnerML(_accessor, _env);
            IResponseStatus _res = partnerML.ChangeStatus(ID);
            return Json(_res);
        }

        [HttpPost]
        [Route("partnerdetailView")]
        public IActionResult PartnerDetailView(int id)
        {
            if (loginML.IsInValidSession())
            {
                return RedirectToAction("Index", "Login");
            }
            IPartnerML partnerML = new PartnerML(_accessor, _env);
            var model = partnerML.GetPartnerByID(new PartnerCreate { ID = id, UserID = _lr.UserID });
            if (model.data.ID > 0)
            {
                model.data.Banner = "Image/Partner/BGI/" + model.data.ID.ToString() + ".png";
                model.data.Logo = "Image/Partner/LOGO/" + model.data.ID.ToString() + ".png";
            }
            model.RoleID = _lr.RoleID;
            return PartialView("Partial/_ViewData", model);
        }

        [HttpPost]
        [Route("Admin/UpdatePartnerStatus")]
        [Route("UpdatePartnerStatus")]
        public IActionResult UpdateStatus(int ID, int status)
        {
            if (loginML.IsInValidSession())
            {
                return RedirectToAction("Index", "Login");
            }
            IPartnerML partnerML = new PartnerML(_accessor, _env);
            IResponseStatus _res = partnerML.UpdateStatus(status, ID);
            return Json(_res);
        }

        [HttpGet]
        [Route("Partner/AEPS/RP-Redirect")]
        [Route("AEPS/RP-Redirect")]
        public async Task<IActionResult> AEPSVerify(string UrlSession)
        {
            IPartnerML partnerML = new PartnerML(_accessor, _env);
            var res = await partnerML.ValidateAEPSURL(UrlSession).ConfigureAwait(false);

            if (res.Statuscode == ErrorCodes.Minus1)
            {
                return RedirectToAction("InvalidPartner", new { msg = res.Msg });
            }
            return RedirectToAction("BankService");
        }
        [HttpGet]
        public IActionResult YourBank()
        {
            IPartnerML partnerML = new PartnerML(_accessor, _env);
            var aEPSURLSessionResp = partnerML.IsInValidPartnerSession();

            if (aEPSURLSessionResp != null)
            {
                // pass partner id in the below function. Static partner id passed for appropriate result
                var res = partnerML.PartnerAEPS(aEPSURLSessionResp.PartnerID);
                return View(res);
            }
            else
                return RedirectToAction("InvalidPartner", new { msg = "Session Expired" });
        }
        [HttpGet]
        public IActionResult BankService()
        {
            IPartnerML partnerML = new PartnerML(_accessor, _env);
            var aEPSURLSessionResp = partnerML.IsInValidPartnerSession();
            if (aEPSURLSessionResp != null)
            {
                var model = new AepsInterfaceModel
                {
                    AEPSInterfaceType = aEPSURLSessionResp.t
                };
                IBankML bankML = new BankML(_accessor, _env);
                var bankList = bankML.AEPSBankMasters();
                if (bankList.Count > 0)
                {
                    model.BankList = new SelectList(bankList, nameof(BankMaster.IIN), nameof(BankMaster.BankName));
                }
                model.BankDetails = bankList;
                IOperatorML opml = new OperatorML(_accessor, _env);
                var Operators = opml.GetOperators(OPTypes.AEPS);
                if (Operators.Any())
                {
                    model.Operators = Operators.Where(x => x.IsActive).OrderBy(o => o.Ind);
                }
                if (aEPSURLSessionResp.PartnerID > 0)
                {
                    model.PartnerDetail = partnerML.GetPartnerByID(new PartnerCreate
                    {
                        ID = aEPSURLSessionResp.PartnerID,
                        UserID = aEPSURLSessionResp.APIUserID
                    });
                }
                IOutletML reportML = new ReportML(_accessor, _env, false);
                model.outlet = reportML.GetOutletUserList(new OuletOfUsersListFilter { Criteria = aEPSURLSessionResp.OutletID }, aEPSURLSessionResp.APIUserID);
                return View("BankService", model);
            }
            else
                return RedirectToAction("InvalidPartner", new { msg = "Session Expired" });
        }
        [HttpPost]
        [Route("bindAEPSBanks")]
        public IActionResult bindAEPSBanks(string bankName)
        {
            IBankML bankML = new BankML(_accessor, _env, false);
            var bankList = bankML.bindAEPSBanks(bankName);
            return Json(bankList);
        }
        [HttpGet]
        public IActionResult InvalidPartner(string msg)
        {
            return View("InvalidPartner", msg);
        }

        [HttpPost]
        [Route("Partner/CheckPsaId")]
        public IActionResult CheckPsaId(string PSAId)
        {
            bool _res = false;
            if (PSAId.Length == 5)
            {
                IPartnerML partnerML = new PartnerML(_accessor, _env);
                _res = partnerML.CheckPsaId(PSAId.ToUpper());
            }
            return Json(_res);
        }

        [HttpPost]
        [Route("Partner/UpdatePsaId")]
        public IActionResult UpdatePsaId(string PSAId, string FatherName)
        {
            bool _res = false;
            if (PSAId.Length == 5)
            {
                IPartnerML partnerML = new PartnerML(_accessor, _env);
                _res = partnerML.UpdatePsaId(PSAId.ToUpper(), FatherName);
            }
            return Json(_res);
        }

        [HttpPost]
        [Route("TopRecent")]
        public async Task<IActionResult> TopFive()
        {
            IPartnerML partnerML = new PartnerML(_accessor, _env);
            var aEPSURLSessionResp = partnerML.IsInValidPartnerSession();
            if (aEPSURLSessionResp != null)
            {
                IReportML RML = new ReportML(_accessor, _env);
                return PartialView("Partial/_TopFive", await RML.GetTopRecentTransactions(OPTypes.AEPS, 5, aEPSURLSessionResp.APIUserID, aEPSURLSessionResp.OutletID).ConfigureAwait(false));
            }
            return Ok();
        }

        [HttpPost]
        [Route("Partner/getAepsBalance-p")]
        [Route("getAepsBalance-p")]
        public async Task<IActionResult> _GetBalance(string PidData, string aadhar, string bank, int t,string _lat,string _long)
        {
            IPartnerML partnerML = new PartnerML(_accessor, _env);
            var aEPSURLSessionResp = partnerML.IsInValidPartnerSession();
            if (aEPSURLSessionResp != null)
            {
                PidData pidData = new PidData();
                if (!string.IsNullOrEmpty(PidData))
                {
                    pidData = XMLHelper.O.DesrializeToObject(pidData, PidData, "PidData", true);
                }
                IDeviceML deviceMl = new DeviceML(_accessor, _env);
                return Json(await deviceMl.CheckBalance(pidData, aadhar, bank, t, aEPSURLSessionResp.APIUserID, aEPSURLSessionResp.OutletID, RequestMode.PARTNERPANEL, 0,string.Empty,_lat,_long, PidData).ConfigureAwait(false));
            }
            return Json(new { Statuscode = ErrorCodes.Minus1, Msg = ErrorCodes.InvaildSession });
        }
        [HttpPost]
        [Route("Partner/AepsWithdraw-p")]
        [Route("AepsWithdraw-p")]
        public async Task<IActionResult> _WithDraw(string PidData, string aadhar, string bank, int t, int amount, string _lat, string _long)
        {
            IPartnerML partnerML = new PartnerML(_accessor, _env);
            var aEPSURLSessionResp = partnerML.IsInValidPartnerSession();
            if (aEPSURLSessionResp != null)
            {
                PidData pidData = new PidData();
                if (!string.IsNullOrEmpty(PidData))
                {
                    pidData = XMLHelper.O.DesrializeToObject(pidData, PidData, "PidData", true);
                }
                IDeviceML deviceMl = new DeviceML(_accessor, _env);
                return Json(await deviceMl.Withdrawl(pidData, aadhar, bank, t, amount, aEPSURLSessionResp.APIUserID, aEPSURLSessionResp.OutletID, RequestMode.PARTNERPANEL, 0, string.Empty, _lat, _long, PidData).ConfigureAwait(false));
            }
            return Json(new { Statuscode = ErrorCodes.Minus1, Msg = ErrorCodes.InvaildSession });
        }
        [HttpPost]
        [Route("Partner/Aadharpay-p")]
        [Route("Aadharpay-p")]
        public async Task<IActionResult> Aadharpay(string PidData, string aadhar, string bank, int t, int amount, string _lat, string _long)
        {
            IPartnerML partnerML = new PartnerML(_accessor, _env);
            var aEPSURLSessionResp = partnerML.IsInValidPartnerSession();
            if (aEPSURLSessionResp != null)
            {
                PidData pidData = new PidData();
                if (!string.IsNullOrEmpty(PidData))
                {
                    pidData = XMLHelper.O.DesrializeToObject(pidData, PidData, "PidData", true);
                }
                IDeviceML deviceMl = new DeviceML(_accessor, _env);
                return Json(await deviceMl.Aadharpay(pidData, aadhar, bank, t, amount, aEPSURLSessionResp.APIUserID, aEPSURLSessionResp.OutletID, RequestMode.PARTNERPANEL, 0, string.Empty, _lat, _long, PidData).ConfigureAwait(false));
            }
            return Json(new { Statuscode = ErrorCodes.Minus1, Msg = ErrorCodes.InvaildSession });
        }
        [HttpPost]
        [Route("Partner/Aeps-MiniStatement-p")]
        [Route("Aeps-MiniStatement-p")]
        public async Task<IActionResult> _MiniStatement(string PidData, string aadhar, string bank, string bankIIN, int t, string _lat, string _long)
        {
            IPartnerML partnerML = new PartnerML(_accessor, _env);
            var aEPSURLSessionResp = partnerML.IsInValidPartnerSession();
            if (aEPSURLSessionResp != null)
            {
                PidData pidData = new PidData();
                if (!string.IsNullOrEmpty(PidData))
                {
                    pidData = XMLHelper.O.DesrializeToObject(pidData, PidData, "PidData", true);
                }
                IDeviceML deviceMl = new DeviceML(_accessor, _env);
                return PartialView("Partial/_MiniStatement", deviceMl.MiniStatement(pidData, aadhar, bank, bankIIN, t, aEPSURLSessionResp.APIUserID, aEPSURLSessionResp.OutletID, RequestMode.PARTNERPANEL, 0, SPKeys.AepsMiniStatement, string.Empty, _lat, _long, PidData));
            }
            return Json(new { Statuscode = ErrorCodes.Minus1, Msg = ErrorCodes.InvaildSession });
        }
        [HttpPost]
        [Route("Partner/Deposit-ot-p")]
        [Route("Deposit-ot-p")]
        public async Task<IActionResult> GenerateDepositOTP(int Amount, string account, string bankIIN, int t)
        {
            IPartnerML partnerML = new PartnerML(_accessor, _env);
            var aEPSURLSessionResp = partnerML.IsInValidPartnerSession();
            if (aEPSURLSessionResp != null)
            {
                IDeviceML deviceMl = new DeviceML(_accessor, _env);
                return PartialView("Partial/_DepositOTP", await deviceMl.DepositGenerateOTP(new DepositRequest
                {
                    UserID = aEPSURLSessionResp.APIUserID,
                    OutletID = aEPSURLSessionResp.OutletID,
                    InterfaceType = t,
                    AccountNo = account,
                    IIN = bankIIN,
                    Amount = Amount,
                    RMode = RequestMode.PARTNERPANEL
                }).ConfigureAwait(false));
            }
            return Ok(ErrorCodes.InvaildSession);
        }
        [HttpPost]
        [Route("Partner/Deposit-Verify-p")]
        [Route("Deposit-Verify-p")]
        public async Task<IActionResult> VerifyDepositOTP(int Amount, string account, string bankIIN, int t, string ref1, string ref2, string ref3, string otp)
        {
            IPartnerML partnerML = new PartnerML(_accessor, _env);
            var aEPSURLSessionResp = partnerML.IsInValidPartnerSession();
            if (aEPSURLSessionResp != null)
            {
                IDeviceML deviceMl = new DeviceML(_accessor, _env);
                var verifyOTPRes = await deviceMl.DepositVerifyOTP(new DepositRequest
                {
                    UserID = aEPSURLSessionResp.APIUserID,
                    OutletID = aEPSURLSessionResp.OutletID,
                    InterfaceType = t,
                    AccountNo = account,
                    IIN = bankIIN,
                    Amount = Amount,
                    RMode = RequestMode.PARTNERPANEL,
                    Reff1 = ref1,
                    Reff2 = ref2,
                    Reff3 = ref3,
                    OTP = otp
                }).ConfigureAwait(false);
                if (verifyOTPRes.Statuscode == ErrorCodes.One)
                {
                    return PartialView("Partial/_DepositConfirmation", verifyOTPRes);
                }
                return Json(verifyOTPRes);
            }
            return Json(new { Statuscode = ErrorCodes.Minus1, Msg = ErrorCodes.InvaildSession });
        }
        [HttpPost]
        [Route("Partner/Deposit-Now-p")]
        [Route("Deposit-Now-p")]
        public async Task<IActionResult> DepositNow(int Amount, string account, string bankIIN, int t, string ref1, string ref2, string ref3, string otp)
        {
            IPartnerML partnerML = new PartnerML(_accessor, _env);
            var aEPSURLSessionResp = partnerML.IsInValidPartnerSession();
            if (aEPSURLSessionResp != null)
            {
                IDeviceML deviceMl = new DeviceML(_accessor, _env);
                return Json(await deviceMl.DepositAccount(new DepositRequest
                {
                    UserID = aEPSURLSessionResp.APIUserID,
                    OutletID = aEPSURLSessionResp.OutletID,
                    InterfaceType = t,
                    AccountNo = account,
                    IIN = bankIIN,
                    Amount = Amount,
                    RMode = RequestMode.PARTNERPANEL,
                    Reff1 = ref1,
                    Reff2 = ref2,
                    Reff3 = ref3,
                    OTP = otp
                }).ConfigureAwait(false));
            }
            return Json(new { Statuscode = ErrorCodes.Minus1, Msg = ErrorCodes.InvaildSession });
        }
    }
}
