using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using OfficeOpenXml;
using RoundpayFinTech.AppCode;
using RoundpayFinTech.AppCode.Configuration;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.MiddleLayer;
using RoundpayFinTech.AppCode.MiddleLayer.DepartmentMiddleLayer;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.DepartmentModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Report;
using RoundpayFinTech.AppCode.StaticModel;
using RoundpayFinTech.Models;
using Validators;

namespace RoundpayFinTech.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public partial class AdminController : Controller
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly ISession _session;
        private readonly LoginResponse _lr;
        private readonly ILoginML loginML;
        private readonly IUnitOfWork uow;
        public AdminController(IHttpContextAccessor accessor, IHostingEnvironment env, IUnitOfWork Uow)
        {
            _accessor = accessor;
            _env = env;
            uow = Uow;
            _session = _accessor.HttpContext.Session;
            _lr = _session.GetObjectFromJson<LoginResponse>(SessionKeys.LoginResponse);
            loginML = new LoginML(_accessor, _env);
        }
        public IActionResult Index()
        {
            if (loginML.IsInValidSession())
            {
                return RedirectToAction("Index", "Login");
            }
            if (_lr.RoleID == Role.Admin && LoginType.ApplicationUser == _lr.LoginTypeID)
                return View();
            return Ok();
        }

        #region BonafideAccount
        [HttpGet]
        [Route("Home/BonafideAccount")]
        [Route("BonafideAccount")]
        public IActionResult BonafideAccount()
        {
            return View();
        }

        [HttpPost]
        [Route("Home/Bonafide-Account")]
        [Route("Bonafide-Account")]
        public IActionResult _BonafideAccount(int TopRows, string KeyWords, string Status, string Account)
        {
            IUserML opml = new UserML(_accessor, _env);
            CommonReq _req = new CommonReq
            {
                CommonInt = TopRows,
                CommonStr = KeyWords,
                CommonStr2 = Account,
                CommonStr3 = Status,
                LoginID = _lr.UserID,
                LoginTypeID = _lr.LoginTypeID
            };
            IEnumerable<BonafideAccount> list = opml.GetBonafideAccount(_req);
            return PartialView("Partial/_BonafideAccount", list);
        }

        [HttpPost]
        [Route("Admin/BonafideAccountUpdate")]
        [Route("BonafideAccountUpdate")]
        public IActionResult BonafideAccountUpdate(int ID, bool IsDeleted)
        {
            IUserML userML = new UserML(_accessor, _env);
            IResponseStatus _res = userML.BonafideAccountSetting(ID, IsDeleted);
            return Json(_res);
        }
        #endregion

        #region FlatCommissionOnUserList
        [HttpPost]
        [Route("FLTCommDetail")]
        public IActionResult FLTCommDetail(int uid)
        {
            ISlabML slabML = new SlabML(_accessor, _env);
            var res = slabML.FlatCommissionDetails(uid);
            return PartialView("Partial/_FlatCommissionDetails", res);
        }
        [HttpPost]
        [Route("update-fltc")]
        public IActionResult UpdateFlatCommission(int uid, int rid, decimal c)
        {
            ISlabML slabML = new SlabML(_accessor, _env);
            var res = slabML.UpdateFlatCommission(uid, rid, c);
            return Json(res);
        }
        #endregion

        #region BulkActionUsers
        [HttpGet]
        [Route("BulkUserAction")]
        public IActionResult BulkUserAction()
        {
            IUserML userML = new UserML(_lr);
            if (_lr.LoginTypeID == LoginType.ApplicationUser && !userML.IsEndUser() || userML.IsCustomerCareAuthorised(ActionCodes.BulkActions))
            {
                return View("BulkAction/BulkUserAction");
            }
            return Ok();
        }
        [HttpPost]
        [Route("Admin/BulkUserReff")]
        [Route("BulkUserReff")]
        public IActionResult BulkUserReff(string m)
        {
            IUserML userML = new UserML(_accessor, _env);
            if (_lr.LoginTypeID == LoginType.ApplicationUser && !userML.IsEndUser() || userML.IsCustomerCareAuthorised(ActionCodes.BulkActions))
            {
                m = string.IsNullOrEmpty(m) ? (_lr.LoginTypeID == LoginType.CustomerCare ? "1" : _lr.UserID.ToString()) : m;
                var userRegModel = userML.GetReffDeatilFromBulk(m);
                userRegModel.userBalnace = userML.GetUserBalnace(0);
                var DMRModels = userML.GetDMRModelList();
                if (DMRModels.Any())
                {
                    userRegModel.DMRModelSelect = new SelectList(DMRModels, "ID", "Name"); ;
                }
                return PartialView("BulkAction/Partial/_IntroRolecshtml", userRegModel);
            }
            return Ok();
        }
        [HttpPost]
        [Route("Admin/BulkUser")]
        [Route("BulkUser")]
        public IActionResult BulkUser(int RoleID, bool IsWhole, string ReffID, bool IsSelf)
        {
            IUserML userML = new UserML(_accessor, _env);
            if (_lr.LoginTypeID == LoginType.ApplicationUser && !userML.IsEndUser() || userML.IsCustomerCareAuthorised(ActionCodes.BulkActions))
            {
                var req = new CommonReq
                {
                    CommonInt2 = RoleID,
                    CommonInt = Convert.ToInt32(HashEncryption.O.Decrypt(ReffID)),
                    CommonBool = IsWhole,
                    CommonBool1 = IsSelf
                };
                return PartialView("BulkAction/Partial/_BulkUserAction", userML.BulkAction(req));
            }
            return Ok();
        }
        [HttpGet]
        [Route("Admin/BulkUser")]
        [Route("BulkUser")]
        public async Task<IActionResult> BulkUserExcel(int RoleID, bool IsWhole, string ReffID, bool IsSelf)
        {
            var req = new CommonReq
            {
                CommonInt2 = RoleID,
                CommonInt = Convert.ToInt32(HashEncryption.O.Decrypt(ReffID)),
                CommonBool = IsWhole,
                CommonBool1 = IsSelf
            };
            IUserML userML = new UserML(_accessor, _env);
            var IsAdmin = _lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.BulkActions);
            List<BulkExcel> _reportBulAction;

            _reportBulAction = await userML.BulkActionFixedRoles(req);
            var dataTable = ConverterHelper.O.ToDataTable(_reportBulAction);
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("UserList1");
                worksheet.Cells["A1"].LoadFromDataTable(dataTable, PrintHeaders: true);
                worksheet.Row(1).Height = 20;
                worksheet.Row(1).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Row(1).Style.Font.Bold = true;

                for (var col = 1; col < dataTable.Columns.Count + 1; col++)
                {
                    worksheet.Column(col).AutoFit();
                }
                ExportToExcel exportToExcel = new ExportToExcel
                {
                    Contents = package.GetAsByteArray(),
                    FileName = "UserList.xlsx"
                };
                return File(exportToExcel.Contents, DOCType.XlsxContentType, exportToExcel.FileName);
            }
        }
        [HttpPost]
        [Route("Admin/DoBulkUser")]
        [Route("DoBulkUser")]
        public IActionResult DoBulkUser([FromBody] BulkActionReq req)
        {
            var user = new UserML(_accessor, _env);
            return Json(user.DoBulkAction(req));
        }
        [HttpGet]
        [Route("BulkSMS")]
        public IActionResult BulkSMS()
        {
            var userML = new UserML(_accessor, _env);
            return View("BulkAction/BulkSms", userML.GetBulkSms());
        }
        [HttpPost]
        [HttpPost]
        [Route("BulkUsers")]
        public IActionResult BulkUsers(string RoleID, bool IsEmail, string parentMobile, bool IsSelf, bool IsSocial, int SocialType, bool IsDirect, int ActiveDays)
        {
            var userML = new UserML(_accessor, _env);
            var _req = new CommonReq
            {
                LoginID = _lr.UserID,
                LoginTypeID = _lr.LoginTypeID,
                IsListType = IsEmail,
                CommonStr = RoleID,
                CommonBool = IsSelf,
                CommonStr2 = parentMobile,
                CommonBool1 = IsSocial,
                CommonInt = SocialType,
                CommonBool2 = IsDirect,
                CommonInt2 = ActiveDays,
            };
            return Json(userML.GetUserFromRole(_req));
        }
        [HttpPost]
        [Route("Admin/bdc")]
        [Route("bdc")]
        public IActionResult _BDC(CommonReq commonReq)
        {
            return PartialView("BulkAction/Partial/_BulkDC", commonReq);
        }
        [HttpPost]
        [Route("Admin/badc")]
        [Route("badc")]
        public IActionResult _BDCA(CommonReq commonReq)
        {
            IUserML userML = new UserML(_accessor, _env);
            var res = userML.BulkDebitCredit(commonReq);
            return Json(res);
        }
        [HttpPost]
        [Route("BulkOpertorMessage")]
        public IActionResult BulkOpertorMessage()
        {
            var userML = new UserML(_accessor, _env);
            return PartialView("BulkAction/Partial/_BulkSend", userML.GetOpertorMessage());
        }
        [HttpPost]
        [Route("GetMessageFormat")]
        public IActionResult GetMessageFormat(int FormatID)
        {
            var userML = new UserML(_accessor, _env);
            var _res = userML.GetMessageFormat(FormatID);
            return Json(_res);
        }
        [HttpPost]
        [Route("SendBulkSms")]
        public IActionResult SendBulkSms(int ApiID, string MobileNOs, string Message)
        {
            IUserML ml = new UserML(_accessor, _env);
            var data = ml.GetUserDeatilForAlert(_lr.UserID);
            data.MobileNos = MobileNOs;
            data.Msg = Message;
            var AlertML = new AlertML(_accessor, _env);
            var res = AlertML.SendSMS(ApiID, data);
            return Json(res);
        }

        [HttpGet]
        [Route("BulkEmail")]
        public IActionResult BulkEmail()
        {
            var userML = new UserML(_accessor, _env);
            return View("BulkAction/BulkEmail", userML.GetBulkSms());
        }
        [HttpPost]
        [Route("SendBulkMail")]
        public IActionResult SendBulkMail(string Emails, string Subject, string Message)
        {
            IUserML ml = new UserML(_accessor, _env);
            var data = ml.GetUserDeatilForAlert(_lr.UserID);
            data.bccList = !string.IsNullOrEmpty(Emails) ? Emails.Split(',').ToList() : new List<string>();
            data.Subject = Subject;
            data.Msg = Message;
            var AlertML = new AlertML(_accessor, _env);
            var res = AlertML.SendEmail(data);
            return Json(res);
        }
        [HttpGet]
        [Route("BulkApp")]
        public IActionResult BulkApp()
        {
            IUserML ml = new UserML(_accessor, _env);
            var role = ml.GetMasterRole();
            return View("BulkAction/BulkApp", role);
        }
        [HttpPost]
        [Route("BulkNotification")]
        public IActionResult BulkNotification(IFormFile file, string Title, string Url, string Message, bool IsWebNotify, string Operators = "", string Roles = "")
        {
            IResponseStatus res = new ResponseStatus();
            if (string.IsNullOrEmpty(Title) && IsWebNotify)
            {
                res.Statuscode = ErrorCodes.Minus1;
                res.Msg = "Please Fill Title";
                return Json(res);
            }
            if (string.IsNullOrEmpty(Message) && IsWebNotify)
            {
                res.Statuscode = ErrorCodes.Minus1;
                res.Msg = "Please Fill Message";
                return Json(res);
            }
            var userMl = new UserML(_accessor, _env);
            if (IsWebNotify)
            {
                var resp = new ResponseStatus
                {
                    Statuscode = ErrorCodes.One
                };
                StringBuilder ImageName = new StringBuilder();
                if (file != null)
                {
                    string _d = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss").Replace("-", "").Replace(":", "").Replace(" ", "");
                    IResourceML ml = new ResourceML(_accessor, _env);
                    ImageName.Append("Notification_{LoginID}_{DateTime}");
                    ImageName.Replace("{LoginID}", Convert.ToString(_lr.UserID)).Replace("{DateTime}", _d);
                    var uploadResponse = (ResponseStatus)ml.UploadWebNotificationImage(file, _lr, ImageName.ToString());
                    resp.Statuscode = uploadResponse.Statuscode;
                    resp.Msg = uploadResponse.Msg;
                    ImageName.Append(".png");
                }

                if (resp.Statuscode == ErrorCodes.One)
                {
                    var alertParam = userMl.GetUserDeatilForAlert(_lr.UserID);
                    alertParam.UserID = -1;
                    alertParam.Msg = Message;
                    alertParam.NotificationTitle = Title;
                    alertParam.Operator = Operators;
                    alertParam.URL = ImageName.ToString();
                    alertParam.Roles = Roles;
                    IAlertML alertMl = new AlertML(_accessor, _env);
                    Parallel.Invoke(() => alertMl.BulkWebNotification(alertParam));
                    res = new ResponseStatus
                    {
                        Statuscode = ErrorCodes.One,
                        Msg = "Notification Send Successfully"
                    };
                }
                else
                {
                    res.Statuscode = resp.Statuscode;
                    res.Msg = resp.Msg;
                }
            }
            else
            {
                var noti = new Notification
                {
                    Title = Title,
                    Message = Message,
                    Url = Url,
                    ImageUrl = "",
                    LoginID = _lr.UserID,
                    file = file,
                    FCMID = "-1"
                };
                res = userMl.SendNotification(noti);
            }
            return Json(res);
        }
        [HttpPost]
        [Route("NotiReport")]
        public IActionResult NotificationList()
        {
            IUserML userMl = new UserML(_accessor, _env);
            var res = userMl.GetNotifications();
            return PartialView("BulkAction/Partial/_NotiReport", res);
        }
        [HttpPost]
        [Route("Del_Notification")]
        public IActionResult DeleteNotification(int id)
        {
            IUserML userMl = new UserML(_accessor, _env);
            var res = userMl.RemoveNotification(id);
            return Json(res);
        }
        [HttpPost]
        [Route("ChildRole")]
        public IActionResult GetChildRoles(int r)
        {
            IUserML userMl = new UserML(_accessor, _env);
            if (r > 0)
                return Json(userMl.GetChildRole(r));
            else
                return Json(userMl.GetChildRole());
        }
        [HttpPost]
        [Route("Admin/GetAllEmplyee")]
        [Route("GetAllEmplyee")]
        public IActionResult GetAllEmplyee()
        {
            IEmpML mL = new EmpML(_accessor, _env);
            return Json(mL.GetAllEmpInBulk());
        }
        #endregion

        #region MastersRegion
        public IActionResult CCRoleMaster()
        {
            return View();
        }
        #endregion

        #region ErrorMasterRegion
        [HttpGet]
        [Route("Home/error-master")]
        [Route("error-master")]
        public IActionResult ErrorMaster()
        {
            var AdminOrAPIUser = _lr.LoginTypeID == LoginType.ApplicationUser ? (_lr.RoleID == Role.Admin ? 1 : 0) : (_lr.RoleID == Role.APIUser ? 2 : 0);
            return View(AdminOrAPIUser);
        }
        [HttpPost]
        [Route("Home/Error-Master")]
        [Route("Error-Master")]
        public IActionResult _ErrorMaster()
        {
            IErrorCodeML ecMl = new ErrorCodeML(_accessor, _env);
            var list = ecMl.Get();
            ViewBag.AdminOrAPIUser = _lr.LoginTypeID == LoginType.ApplicationUser ? (_lr.RoleID == Role.Admin ? 1 : 0) : (_lr.RoleID == Role.APIUser ? 2 : 0);
            return PartialView("Partial/_ErrorMaster", list);
        }
        [HttpPost]
        [Route("Home/Error-Edit/{id}")]
        [Route("Error-Edit/{id}")]
        public IActionResult _ErrorEdit(int ID)
        {
            IErrorCodeML ecMl = new ErrorCodeML(_accessor, _env);
            var errorCodeDetail = ecMl.Get(ID);
            ViewBag.Types = ecMl.GetTypes();
            return PartialView("Partial/_ErrorCU", errorCodeDetail);
        }
        [HttpPost]
        [Route("Home/Error-Edit")]
        [Route("Error-Edit")]
        public IActionResult ErrorEdit([FromBody] ErrorCodeDetail errorCodeDetail)
        {
            IErrorCodeML ecMl = new ErrorCodeML(_accessor, _env);
            IResponseStatus _resp = ecMl.Save(errorCodeDetail);
            return Json(_resp);
        }
        [HttpPost]
        [Route("Error-update")]
        public IActionResult Errorupdate(ErrorCodeDetail errorCodeDetail)
        {
            IErrorCodeML ecMl = new ErrorCodeML(_accessor, _env);
            IResponseStatus _resp = ecMl.update(errorCodeDetail);
            return Json(_resp);
        }
        [HttpPost]
        [Route("Home/ErrCode")]
        [Route("ErrCode")]
        public IActionResult ErrorEdit(int ID, string Code)
        {
            IResponseStatus _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvalidParam
            };
            if (Code == null || ID == 0)
                return Json(_resp);
            var errorCodeDetail = new ErrorCodeDetail
            {
                EID = ID,
                Code = Code,
                IsCode = true
            };
            IErrorCodeML ecMl = new ErrorCodeML(_accessor, _env);
            _resp = ecMl.Save(errorCodeDetail);
            return Json(_resp);
        }

        [HttpGet]
        [Route("Home/APIErCode")]
        [Route("APIErCode")]
        public IActionResult APIErrorCode()
        {
            return View();
        }
        [HttpPost]
        [Route("Home/API-ErCode")]
        [Route("API-ErCode")]
        public IActionResult _APIErrorCode()
        {
            IErrorCodeML ecMl = new ErrorCodeML(_accessor, _env);
            IAPIML apiMl = new APIML(_accessor, _env);
            var mdl = new ErrorAPICodeModel
            {
                ErrorCodeDetails = ecMl.Get(),
                APIGroupDetails = apiMl.GetGroup(),
                APIErrorCodes = ecMl.GetAPIErrorCode()
            };
            return PartialView("Partial/_APIErrorCode", mdl);
        }

        [HttpPost]
        [Route("Home/API-Er-Code")]
        [Route("API-Er-Code")]
        public IActionResult APIErCode([FromBody] APIErrorCode aPIErrorCode)
        {
            IErrorCodeML ecMl = new ErrorCodeML(_accessor, _env);
            IResponseStatus _resp = ecMl.UpdateAPIErCode(aPIErrorCode);
            return Json(_resp);
        }
        #endregion

        #region FundProcess
        [HttpPost]
        [Route("Home/F-T")]
        [Route("F-T")]
        public IActionResult _FundTransfer(int pid, int uid)
        {
            var userML = new UserML(_accessor, _env);
            var fundTransferModel = new FundTransferModel
            {
                fundRequetResp = userML.GetUserFundTransferData(pid),
                IsDoubleFactor = _lr.IsDoubleFactor,
                IsAdmin = _lr.RoleID == Role.Admin || userML.IsCustomerCareAuthorised(ActionCodes.FundTransfer)
            };
            if (pid > 0)
            {
                fundTransferModel.userBalnace = userML.GetUserBalnace(fundTransferModel.fundRequetResp.UserId);
            }
            else
            {
                fundTransferModel.userBalnace = userML.GetUserBalnace(uid);
            }
            var res = userML.GetDebitRquesttStatus();
            ViewBag.CanDebit = _lr.RoleID == 1 ? true : res.Candebit;
            return PartialView("Partial/_FundTransferPanel", fundTransferModel);
        }

        [HttpPost]
        [Route("Home/FT")]
        [Route("FT")]
        public IActionResult FundTransfer([FromBody] FundProcess req)
        {
            IResponseStatus _res = new ResponseStatus()
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvalidParam
            };
            if (req != null)
            {
                req.RequestMode = RequestMode.PANEL;
                IFundProcessML userML = new UserML(_accessor, _env);
                _res = userML.FundTransfer(req);
            }
            return Json(_res);
        }


        #endregion

        #region SettingMenu
        [HttpPost]
        [Route("Home/SystemSetting/Save")]
        [Route("SystemSetting/Save")]
        public IActionResult SystemSetting([FromBody] SystemSetting req)
        {
            ISettingML settings = new SettingML(_accessor, _env);
            return Json(settings.SaveSystemSetting(req));
        }

        [HttpGet]
        [Route("Home/SystemSetting")]
        [Route("SystemSetting")]
        public IActionResult SystemSetting()
        {
            ISettingML settings = new SettingML(_accessor, _env);
            return View(settings.GetSettings());
        }

        #region AddMoneySetting
        [HttpPost]
        [Route("Home/AMoney-mode")]
        [Route("AMoney-mode")]
        public IActionResult AMoneyMode()
        {
            IOperatorML opML = new OperatorML(_accessor, _env);
            var modes = opML.GetOperators(OPTypes.AddMoney);
            return PartialView("Partial/_AMoneyMode", modes);
        }
        [HttpPost]
        [Route("Home/AMoney-Charge")]
        [Route("AMoney-Charge")]
        public IActionResult AMoneyCharge(int o, decimal c, bool Is)
        {
            ISettingML sml = new SettingML(_accessor, _env);
            var res = sml.UpdateAddMoneyCharge(o, c, Is);
            return Json(res);
        }
        #endregion

        [HttpPost]
        [Route("Trans-Mode-Code/{Code}")]
        public IActionResult _TransMode(string Code)
        {
            var commonReq = new CommonReq
            {
                CommonStr2 = Code
            };
            IUserML userML = new UserML(_accessor, _env);
            var res = userML.GetTransactionMode(commonReq);
            return PartialView("Partial/_TransMode", res);
        }

        [HttpPost]
        [Route("Update-Trans-Mode")]
        public IActionResult UpdateTransMode([FromBody] CommonReq commonReq)
        {
            IUserML userML = new UserML(_accessor, _env);
            var res = userML.UpdateTransactionMode(commonReq);
            return Json(res);
        }
        #endregion

        #region APISTATUSCHECK_Section
        [HttpGet]
        [Route("Home/api-status-check")]
        [Route("api-status-check")]
        public IActionResult APISTATUSCHECK()
        {
            IUserML userML = new UserML(_lr);
            if (_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.APISTATUSCHECK))
            {
                IErrorCodeML errorCodeML = new ErrorCodeML(_accessor, _env);
                var errors = errorCodeML.Get();
                var items = new SelectList(errors, "Code", "ErrorWithCode");
                return View(items);
            }
            return Ok();
        }
        [HttpPost]
        [Route("Home/asc")]
        [Route("asc")]
        public IActionResult APISTATUS_CHECK([FromBody] APISTATUSCHECK apistatuscheck)
        {
            IAPIML aPIML = new APIML(_accessor, _env);
            IResponseStatus res = aPIML.UpdateAPISTATUSCHECK(apistatuscheck);
            return Json(res);
        }
        [HttpPost]
        [Route("Home/api-status-check")]
        [Route("api-status-check")]
        public async Task<IActionResult> APISTATUSCHECK([FromBody] APISTATUSCHECK apistatuscheck)
        {
            IAPIML aPIML = new APIML(_accessor, _env);
            apistatuscheck = await aPIML.GetAPISTATUSCHECK(apistatuscheck);
            return Json(apistatuscheck);
        }
        [HttpPost]
        [Route("Home/a-s-c-l")]
        [Route("a-s-c-l")]
        public IActionResult _APISTATUSCHECKList()
        {
            return PartialView("Partial/_APISTATUSCHECKList");
        }
        [HttpPost]
        [Route("Home/a-s-c-l-h")]
        [Route("a-s-c-l-h")]
        public IActionResult _APISTATUSCHECKListHelper(string t, int s)
        {
            IAPIML aPIML = new APIML(_accessor, _env);
            IEnumerable<APISTATUSCHECK> lst = aPIML.GetAPISTATUSCHECKs(t, s);
            return PartialView("Partial/_APIStatusCheckListHelper", lst);
        }
        [HttpPost]
        [Route("Home/d-a-s-c")]
        [Route("d-a-s-c")]
        public IActionResult _DeleteAPISTATUSCHECK(int id)
        {
            IAPIML aPIML = new APIML(_accessor, _env);
            IResponseStatus res = aPIML.DeleteApiStatusCheck(id);
            return Json(res);
        }
        #endregion

        #region AddWAlletSection
        [HttpPost]
        [Route("Add-Wallet")]
        public IActionResult _AddWallet()
        {
            IUserML userML = new UserML(_accessor, _env);
            if (_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.AddWallet))
            {
                var ub = userML.GetUserBalnace(0);
                return PartialView("Partial/_Wallet", ub);
            }
            return Ok();
        }
        [HttpPost]
        [Route("AW")]
        public async Task<IActionResult> AddWallet([FromBody] FundProcess req)
        {
            IAdminML userml = new UserML(_accessor, _env);
            FundProcessReq fundProcessReq = new FundProcessReq
            {
                fundProcess = req
            };
            var res = await userml.AddWallet(fundProcessReq);
            return Json(res);
        }

        #endregion

        #region NewsSection
        [HttpGet]
        [Route("Admin/News")]
        [Route("News")]
        public IActionResult News()
        {
            return View("News/News");
        }
        [HttpPost]
        [Route("Admin/GetNews")]
        [Route("GetNews")]
        public IActionResult GetNews()
        {
            UserNews model = new UserNews();
            INewsML settings = new NewsML(_accessor, _env);
            model = settings.GetNews();
            return PartialView("News/_News", model);
        }



        [HttpPost]
        [Route("Admin/GetNewsRoleAssing")]
        [Route("GetNewsRoleAssing")]
        public IActionResult GetNewsRoleAssing(int Id)
        {
            UserNews model = new UserNews();
            INewsML settings = new NewsML(_accessor, _env);
            if (Id > 0)
            {
                model = settings.GetNewsRoleAssign(Id);
            }
            return Json(model.ListNews);

        }
        [Route("Admin/UpdateNews")]
        [Route("UpdateNews")]
        [HttpPost]
        public IActionResult UpdateNews([FromBody] News req)
        {
            IUpdateNewsML settings = new UpdateNewsML(_accessor, _env);
            return Json(settings.UpdateNews(req));
        }

        [HttpPost]
        [Route("NewsAddNew")]
        public IActionResult NewsAddNew(int id)
        {
            var model = new News();
            INewsML settings = new NewsML(_accessor, _env);
            model = settings.EditNews(id);
            return PartialView("News/NewsAddNew", model);
        }

        [Route("Admin/DeleteNews/{id}")]
        [Route("DeleteNews/{id}")]
        public IActionResult DeleteNews(int Id)
        {
            UserNews model = new UserNews();
            INewsML settings = new NewsML(_accessor, _env);
            if (Id > 0)
            {
                CommonReq ReqData = new CommonReq
                {
                    LoginID = _lr.UserID,
                    LoginTypeID = _lr.LoginTypeID,
                    CommonInt = Id
                };
                IResponseStatus _resp = settings.DeleteNews(ReqData);
                return RedirectToAction("News");
            }
            return RedirectToAction("News");

        }
        #endregion

        #region MessageSetting
        [HttpGet]
        [Route("MessageSetting")]
        public IActionResult MessageSetting()
        {
            var mL = new ReportML(_accessor, _env);
            return View("Masters/MessageSetting", mL.GetTemplate(0));
        }
        [HttpPost]
        [Route("/Get-KeyWords")]
        public IActionResult _GetKeywords(int FormatID)
        {
            var mL = new ReportML(_accessor, _env);
            var _res = mL.GetTemplate(FormatID);
            return PartialView("Masters/_getKeywords", _res);
        }

        [HttpPost]
        [Route("UpdateMessageFormat")]
        public IActionResult UpdateMessageFormat(MessageTemplateParam param)
        {
            var mL = new ReportML(_accessor, _env);
            return Json(mL.UpdateMessageFormat(param));
        }

        [HttpPost]
        [Route("MessagePopUp")]
        public IActionResult MessagePopUp()
        {
            var mL = new UserML(_accessor, _env);
            return PartialView("Masters/Partial/_AddMessageType");
        }
        [HttpPost]
        [Route("Home/MapTemplateAndKey")]
        [Route("MapTemplateAndKey")]
        public async Task<IActionResult> MapTemplateAndKey(int FormatID, int KeyID, bool IsActive)
        {
            var ml = new DepartmentML(_accessor, _env);
            var _res = await ml.MapTemplateAndKey(FormatID, KeyID, IsActive).ConfigureAwait(false);
            return Json(_res);
        }
        [HttpPost]
        [Route("Home/SMS-API-Status")]
        [Route("SMS-API-Status")]
        public IActionResult ISSMSAPIActive(int ID, bool IsActive, bool IsDefault)
        {
            IResponseStatus _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = "Incomplete request!"
            };
            if (_lr.LoginTypeID.In(LoginType.ApplicationUser, LoginType.CustomerCare))
            {
                if (ID != 0)
                {
                    ISMSAPIML aPIML = new APIML(_accessor, _env);
                    _res = aPIML.ISSMSAPIActive(ID, IsActive, IsDefault);
                }
            }
            return Json(_res);
        }

        #endregion

        #region DepartmentMaster
        [HttpGet]
        [Route("DepartmentRoleMaster")]
        public IActionResult DepartmentRoleMaster()
        {
            return View();
        }
        [HttpPost]
        [Route("Home/DepartmentRoles")]
        [Route("DepartmentRoles")]
        public IActionResult _DepartmentRoles()
        {
            IDepartmentML departmentML = new DepartmentML(_accessor, _env);
            var deps = departmentML.GetDepartmentRoles();
            return PartialView("Partial/_DepartmentRoles", deps);
        }
        [HttpPost]
        [Route("Department-Master/{did}")]
        public IActionResult DepartmentMaster(int did)
        {
            IDepartmentML departmentML = new DepartmentML(_accessor, _env);
            var deps = departmentML.GetDepartment(did);
            return PartialView("Partial/_Department", deps);
        }
        [HttpPost]
        [Route("Home/DepartmentRole/{drid}")]
        [Route("DepartmentRole/{drid}")]
        public IActionResult DepartmentRole(int drid)
        {
            IDepartmentML departmentML = new DepartmentML(_accessor, _env);
            var role = departmentML.GetDepartmentRole(drid);
            var departmentModel = new DepartmentEntity
            {
                departmentRole = role
            };
            var dept = departmentML.GetDepartment();
            departmentModel.selectDepartment = new SelectList(dept, "ID", "Name", role.DepartmentID);
            return PartialView("Partial/_DepartmentRole", departmentModel);
        }
        [HttpPost]
        [Route("Home/Department-c")]
        [Route("Department-c")]
        public IActionResult DepartmentC([FromBody] Department department)
        {
            IDepartmentML departmentML = new DepartmentML(_accessor, _env);
            var res = departmentML.SaveDepartment(department);
            return Json(res);
        }
        [HttpPost]
        [Route("Home/DepartmentRole-c")]
        [Route("DepartmentRole-c")]
        public IActionResult DepartmentRoleC([FromBody] DepartmentRole departmentRole)
        {
            IDepartmentML departmentML = new DepartmentML(_accessor, _env);
            var res = departmentML.SaveDepartmentRole(departmentRole);
            return Json(res);
        }
        [HttpPost]
        [Route("Home/asd-menu")]
        [Route("asd-menu")]
        public IActionResult _AssignedMenu(int r)
        {
            ICustomercareML ml = new CustomercareML(_accessor, _env);
            var lst = ml.CCOperationAssigned(r);
            return PartialView("Partial/_AssignedMenu", lst);
        }
        [HttpPost]
        [Route("Home/uasd-menu")]
        [Route("uasd-menu")]
        public IActionResult _UpdateAssignedMenu(int r, int mn, int o, bool ia)
        {
            ICustomercareML ml = new CustomercareML(_accessor, _env);
            var res = ml.UpdateOperationAssigned(r, mn, o, ia);
            return Json(res);

        }
        #endregion

        #region CustomerCareSection
        [HttpGet]
        [Route("Admin/CustomerCares")]
        [Route("CustomerCares")]
        public IActionResult CustomerCares()
        {
            return View();
        }
        [HttpPost]
        [Route("Admin/ccares")]
        [Route("ccares")]
        public async Task<IActionResult> CustomerCareList(int uid, string mob, int r, int d)
        {
            ICustomercareML ml = new CustomercareML(_accessor, _env);
            var req = new CommonReq
            {
                CommonInt = uid,
                CommonStr = mob,
                CommonInt2 = r,
                CommonInt3 = d
            };
            var res = await ml.GetCustomerCares(req);
            return PartialView("Partial/_CustomerCare", res);
        }
        [HttpPost]
        [Route("Home/_CustomerCare")]
        [Route("_CustomerCare")]
        public async Task<IActionResult> _CusomerCare(int ID)
        {
            ICustomercareML ml = new CustomercareML(_accessor, _env);
            var mdl = new CustomerCareEdit
            {
                CustomercareInfo = await ml.GetCustomerCare(ID)
            };
            IDepartmentML dml = new DepartmentML(_accessor, _env);
            mdl.DDLDepartment = new SelectList(dml.GetDepartment(), "ID", "Name", mdl.CustomercareInfo.DeptID);
            return PartialView("Partial/_CustomerCareEdit", mdl);
        }
        [HttpPost]
        [Route("Home/ddlDeptRl")]
        [Route("ddlDeptRl")]
        public IActionResult DDLForDepartmentRole(int did, int id)
        {
            IDepartmentML ml = new DepartmentML(_accessor, _env);
            var ddl = did > 0 ? ml.GetDepartmentRoles(did) : new List<DepartmentRole>();
            var listItems = new SelectList(ddl, "ID", "Name", id);
            return PartialView("Partial/_DropdownRoleDept", listItems);
        }
        [HttpPost]
        [Route("Admin/CCCU")]
        [Route("CCCU")]
        public IActionResult CustomerCareCU([FromBody] Customercare customercare)
        {
            ICustomercareML ml = new CustomercareML(_accessor, _env);
            var resp = ml.CustomerCareCU(customercare);
            return Json(resp);
        }
        [HttpPost]
        [Route("Admin/ToggleStatusCC")]
        [Route("ToggleStatusCC")]
        public IActionResult ChangeStatusCC(int ID, int type)
        {
            ICustomercareML ml = new CustomercareML(_accessor, _env);
            var _res = ml.ChangeOTPStatusCC(ID, type);
            return Json(_res);
        }
        [HttpPost]
        [Route("Admin/ShowPassCus/{id}")]
        [Route("ShowPassCus/{id}")]
        public IActionResult ShowPassCus(int ID)
        {
            IAdminML userML = new UserML(_accessor, _env);
            return Ok(userML.ShowPasswordCustomer(ID));
        }
        #endregion

        #region ApiOperatorOptionalMapping
        [HttpPost]
        [Route("Home/AOPMapping")]
        [Route("AOPMapping")]
        public IActionResult AOPMapping(int A, int O)
        {
            IOperatorML opml = new OperatorML(_accessor, _env);
            return PartialView("Partial/_APIOperatorOptionalKeyValuePair", opml.AOPMapping(A, O));
        }
        [HttpPost]
        [Route("Home/SaveAOPMapping")]
        [Route("SaveAOPMapping")]
        public IActionResult SaveAOPMapping([FromBody] ApiOperatorOptionalMappingModel model)
        {
            IOperatorML opml = new OperatorML(_accessor, _env);
            return Json(opml.SaveAOPMapping(model));
        }
        #endregion        

        #region IPAddressSection
        [HttpGet]
        [Route("Home/IPMaster")]
        [Route("IPMaster")]
        public IActionResult IPMaster()
        {
            IUserML userML = new UserML(_accessor, _env, false);
            if (_lr.RoleID.In(Role.Admin, Role.APIUser) || userML.IsCustomerCareAuthorised(ActionCodes.AddEditIP))
                return View((LoginType.CustomerCare == _lr.LoginTypeID ? Role.Admin : _lr.RoleID));
            return BadRequest();
        }
        [HttpPost]
        [Route("Home/IPMaster")]
        [Route("IPMaster")]
        public IActionResult _IPMaster(string MobileNo, int ID)
        {
            var userML = new UserML(_accessor, _env);
            if (_lr.RoleID.In(Role.Admin, Role.APIUser) || userML.IsCustomerCareAuthorised(ActionCodes.AddEditIP))
            {
                var showIPAddressModel = new ShowIPAddressModel
                {
                    IsAdmin = _lr.LoginTypeID == LoginType.ApplicationUser && _lr.UserID == 1 || userML.IsCustomerCareAuthorised(ActionCodes.AddEditIP),
                    IPAddressModels = userML.GetIPAddress(MobileNo, ID)
                };
                return PartialView("Partial/_IPMaster", showIPAddressModel);
            }
            return BadRequest();
        }
        [HttpPost]
        [Route("Home/IP-Master")]
        [Route("IP-Master")]
        public IActionResult _IPMasterInput()
        {
            return PartialView("Partial/_IPMasterInput", _lr.RoleID);
        }
        [HttpPost]
        [Route("Home/save-ip")]
        [Route("save-ip")]
        public IActionResult SaveIPAddress([FromBody] IPAddressModel iPAddressModel)
        {
            IIPAddressML userML = new UserML(_accessor, _env);
            var res = userML.SaveIPAddress(iPAddressModel);
            return Json(res);
        }
        [HttpPost]
        [Route("/RemoveIp")]
        public IActionResult RemoveIp(int ID)
        {
            IIPAddressML userML = new UserML(_accessor, _env);
            var res = (ResponseStatus)userML.RemoveIp(ID);
            return Json(res);
        }
        [HttpPost]
        [Route("/send-ip-otp")]
        public IActionResult IPMasterSendOTP()
        {
            IIPAddressML userML = new UserML(_accessor, _env);
            var res = userML.SendIPOTP();
            if (res.Statuscode == ErrorCodes.One)
            {
                return PartialView("OTP/_OTP");
            }
            return Json(res);
        }

        [HttpPost]
        [Route("/u-ip-sts")]
        public IActionResult UpdateIPStatus(int ID, bool Sts)
        {
            IIPAddressML userML = new UserML(_accessor, _env);
            var res = (ResponseStatus)userML.UpdateIpStatus(ID, Sts);
            return Json(res);
        }
        #endregion

        #region Range
        [HttpGet]
        [Route("Home/range-master")]
        [Route("range-master")]
        public IActionResult RangeMaster()
        {
            var AdminOrAPIUser = _lr.LoginTypeID == LoginType.ApplicationUser ? (_lr.RoleID == Role.Admin ? 1 : 0) : (_lr.RoleID == Role.APIUser ? 2 : 0);
            return View(AdminOrAPIUser);
        }
        [HttpPost]
        [Route("Home/Range-Master")]
        [Route("Range-Master")]
        public IActionResult _RangeMaster()
        {
            IOperatorML IOML = new OperatorML(_accessor, _env);
            var list = IOML.GetRange();
            ViewBag.AdminOrAPIUser = _lr.LoginTypeID == LoginType.ApplicationUser ? (_lr.RoleID == Role.Admin ? 1 : 0) : (_lr.RoleID == Role.APIUser ? 2 : 0);
            return PartialView("Partial/_RangeMaster", list);
        }

        [HttpPost]
        [Route("Home/Range-Edit/{id}")]
        [Route("Range-Edit/{id}")]
        public IActionResult _RangeEdit(int ID)
        {
            IOperatorML IOML = new OperatorML(_accessor, _env);
            var rangeDetail = IOML.GetRange(ID);
            rangeDetail.OpTypes = IOML.GetOptypes().Where(x => !x.ID.In(OPTypes.Primary, OPTypes.Secondary)).ToList();
            return PartialView("Partial/_RangeCU", rangeDetail);
        }
        [HttpPost]
        [Route("Home/Range-Edit")]
        [Route("Range-Edit")]
        public IActionResult RangeEdit([FromBody] RangeModel rangeDetail)
        {
            IOperatorML IOML = new OperatorML(_accessor, _env);
            IResponseStatus _resp = IOML.SaveRange(rangeDetail);
            return Json(_resp);
        }

        #endregion

        #region Target
        [HttpPost]
        [Route("Admin/Target")]
        [Route("Target")]
        public IActionResult _showTarget(TargetModel param)
        {
            IUserML userML = new UserML(_lr);
            ITargetML opml = new OperatorML(_accessor, _env);
            var Model = new TargetModel
            {
                TargetTypeID = ApplicationSetting.TargetType,
                SlabID = param.SlabID,
                IsAdminDefined = param.IsAdminDefined
            };
            var _List = new _TargetModel
            {
                TargetModelList = opml.GetTarget(Model),
                TargetType = Model.TargetTypeID
            };
            return PartialView("Partial/_TargetChanel", _List);
        }

        [HttpPost]
        [Route("Admin/Target_Edit")]
        [Route("Target_Edit")]
        public IActionResult _Target_Edit(TargetModel param)
        {
            ITargetML IOML = new OperatorML(_accessor, _env);
            param.TargetTypeID = ApplicationSetting.TargetType;
            IResponseStatus _resp = IOML.SaveTarget(param);
            return Json(_resp);
        }

        [HttpPost]
        [Route("Admin/upload-Gift-Img")]
        [Route("upload-Gift-Img")]
        public IActionResult UploadGiftImg(IFormFile file, string fileName)
        {
            ITargetML _targetML = new OperatorML(_accessor, _env);
            var _res = _targetML.UploadGift(file, fileName);
            return Json(_res);
        }


        [HttpPost]
        [Route("Admin/Target-ByRole")]
        [Route("Target-ByRole")]
        public IActionResult showTargetByRole(TargetModel param)
        {
            IUserML userML = new UserML(_accessor, _env);
            IOperatorML opml = new OperatorML(_accessor, _env);
            TargetModelLvl Model = new TargetModelLvl();

            if (ApplicationSetting.TargetType == TargetType.Servicewise)
            {
                Model.Services = opml.GetServices().ToList();
            }
            if (ApplicationSetting.TargetType == TargetType.OpTypewise)
            {
                Model.OpTypes = opml.GetOptypes().ToList();
            }
            if (ApplicationSetting.TargetType == TargetType.Operatorwise)
            {
                Model.Operators = opml.GetOperators().ToList();
            }
            Model.Roles = userML.GetRoleSlab().Roles.Where(x => x.ID != Role.FOS).ToList();
            Model.TargetType = ApplicationSetting.TargetType;
            Model.SlabID = param.SlabID;
            return PartialView("Partial/_TargetRole", Model);
        }


        [HttpPost]
        [Route("Admin/Target-ByRole-Edit")]
        [Route("Target-ByRole-Edit")]
        public IActionResult _TargetByRole_Edit(TargetModel param)
        {
            ITargetML _targetML = new OperatorML(_accessor, _env);
            param.TargetTypeID = ApplicationSetting.TargetType;
            TargetModel model = _targetML.GetTargetByRole(param);
            model.RoleName = param.RoleName;
            model.OpName = param.OpName;
            return PartialView("Partial/_TargetRoleEdit", model);
        }
        #endregion

        #region EmailAPIRegion
        [HttpGet("EmailAPI")]
        [Route("Home/EmailAPI")]
        public IActionResult EmailAPI()
        {
            return View();
        }
        [HttpPost]
        [Route("Home/EmailAPI/{id}")]
        [Route("EmailAPI/{id}")]
        public IActionResult EmailAPI(int id)
        {
            var model = new EmailAPIDetail();
            IEmailAPIML ML = new APIML(_accessor, _env);
            model = ML.GetEmailAPIDetailByID(id);
            model.Provider = ML.GetEmailProviderDetail();
            return PartialView("Partial/_EmailAPI", model);

        }
        [HttpPost]
        [Route("Home/EmailAPI")]
        [Route("Sendmail")]
        public IActionResult Sendmail(int EmailID)
        {
            var model = new EmailAPIDetail();
            IEmailAPIML ML = new APIML(_accessor, _env);
            model = ML.GetEmailAPIDetailByID(EmailID);
            return PartialView("Partial/_SendEmail", model);

        }
        [HttpPost]
        [Route("Home/EmailAPI")]
        [Route("Sendingmail")]
        public IActionResult Sendingmail(string ToMail, int EmailID)
        {

            IResponseStatus _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = "Incomplete request!"
            };
            // var model = new EmailAPIDetail();
            if (ToMail != null && Validate.O.IsEmail(ToMail ?? ""))
            {
                IEmailAPIML ML = new APIML(_accessor, _env);
                _res = ML.SendEmailToId(EmailID, ToMail);
                return Json(_res);
            }
            return Json(_res);

        }
        [HttpPost]
        [Route("Home/EmailAPIs")]
        [Route("EmailAPIs")]
        public IActionResult EmailAPIList()
        {
            IEmailAPIML ML = new APIML(_accessor, _env);
            return PartialView("Partial/_EmailAPIs", ML.GetEmailAPIDetail());
        }
        [HttpPost]
        [Route("Home/Email-API")]
        [Route("Email-API")]
        public IActionResult EmailAPI([FromBody] EmailAPIDetail aPIDetail)
        {
            IResponseStatus _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = "Incomplete request!"
            };
            if (_lr.LoginTypeID.In(LoginType.ApplicationUser, LoginType.CustomerCare))
            {
                if (aPIDetail != null)
                {
                    IEmailAPIML aPIML = new APIML(_accessor, _env);
                    _res = aPIML.SaveEmailAPI(aPIDetail);
                }
            }
            return Json(_res);
        }
        #endregion

        #region Fund
        [Route("Admin/FundRequestToRole")]
        [Route("FundRequestToRole")]
        public IActionResult FundRequestToRole()
        {
            return View();
        }

        [Route("Admin/FundRequest-To-Role")]
        [Route("FundRequest-To-Role")]
        public IActionResult _FundRequestToRole()
        {
            IFundProcessML fundProcessML = new UserML(_accessor, _env);
            var res = fundProcessML.GetFundRequestToRole();
            return PartialView("Partial/_FundRequestToRole", res);
        }

        [Route("Admin/UpdateFundRequestToRole")]
        [Route("UpdateFundRequestToRole")]
        public IActionResult _UpdateFundRequestToRole([FromBody] FundRequestToRole req)
        {
            IFundProcessML fundProcessML = new UserML(_accessor, _env);
            var res = fundProcessML.UpdateFundRequestToRole(req);
            return Json(res);
        }
        [HttpPost("mbox.jsf")]
        public IActionResult MessageBox([FromBody] MessageBoxModel messageBoxModel)
        {
            return PartialView("Common/_MsgBox", messageBoxModel);
        }
        #endregion

        #region WebsiteList
        [HttpGet]
        [Route("Admin/WebsiteList")]
        [Route("WebsiteList")]
        public IActionResult WebsiteList()
        {
            return View();
        }
        [HttpPost]
        [Route("Admin/Website_List")]
        [Route("Website_List")]
        public IActionResult WebsiteListNew()
        {
            IWebsiteML websiteML = new WebsiteML(_accessor, _env);
            var deps = websiteML.GetWebsite();
            return PartialView("Partial/_WebsiteList", deps);
        }

        [HttpPost]
        [Route("Admin/UpdateWebsiteList")]
        [Route("UpdateWebsiteList")]
        public IActionResult UpdateWebsiteList([FromBody] WebsiteModel req)
        {

            //var WebInfo = loginML.GetWebsiteInfo();
            //req.WID = _lr.WID;
            IWebsiteML _userWebsite = new WebsiteML(_accessor, _env);
            var _res = _userWebsite.UpdateWebsiteList(req);
            return Json(_res);
        }
        [HttpPost]
        [Route("Admin/UpdateIsWLAPIAllowed")]
        [Route("UpdateIsWLAPIAllowed")]
        public IActionResult UpdateIsWLAPIAllowed([FromBody] WebsiteModel req)
        {
            IWebsiteML _userWebsite = new WebsiteML(_accessor, _env);
            var _res = _userWebsite.UpdateIsWLAPIAllowed(req);
            return Json(_res);
        }

        #endregion

        #region PopUp

        [HttpPost]
        [Route("Admin/ResendPassword")]
        [Route("ResendPassword")]
        public IActionResult ResendPassword(int ID, string MobileNo)
        {

            LoginDetail loginDetail = new LoginDetail
            {
                LoginMobile = MobileNo,
                LoginTypeID = _lr.LoginTypeID
            };
            var responseStatus = new ForgetPasss
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvalidLogin
            };
            if (loginDetail == null)
            {
                return Json(responseStatus);
            }

            if (!Validate.O.IsMobile(loginDetail.LoginMobile))
            {
                loginDetail.Prefix = Validate.O.Prefix(loginDetail.LoginMobile);
                if (Validate.O.IsNumeric(loginDetail.Prefix))
                    return Json(responseStatus);
                string loginID = Validate.O.LoginID(loginDetail.LoginMobile);
                if (!Validate.O.IsNumeric(loginID))
                {
                    return Json(responseStatus);
                }
                loginDetail.LoginID = Convert.ToInt32(loginID);
                loginDetail.LoginMobile = "";
            }
            if (_lr.operationsAssigned != null)
            {
                if (_lr.operationsAssigned.Any(x => x.OperationCode == ActionCodes.ShowUser && x.IsActive))
                {
                    loginDetail.LoginID = 0;
                    loginDetail.LoginTypeID = 1;
                }
            }
            loginDetail.RequestMode = RequestMode.PANEL;
            return Json(loginML.Forget(loginDetail));
        }

        [HttpPost]
        [Route("UpdatePassword")]
        public IActionResult UpdatePassword(int UserID)
        {

            IUserML userML = new UserML(_accessor, _env);
            return Json(userML.Regeneratepassword(UserID));
        }
        [HttpGet]
        [Route("Admin/Popup")]
        [Route("Popup")]
        public IActionResult Popup()
        {
            return View("Popup/Popup");
        }

        [HttpPost]
        [Route("_Popup")]
        public IActionResult _Popup()
        {
            var WebInfo = loginML.GetPopupInfo();
            WebInfo.WID = _lr.WID;
            return PartialView("Popup/Partial/_Popup", WebInfo);
        }

        [HttpPost]
        [Route("Admin/Web-Popup")]
        [Route("Web-Popup")]
        public IActionResult WebsitePopup(IFormFile file)
        {
            IBannerML _bannerML = new ResourceML(_accessor, _env);
            var _res = _bannerML.UploadPopup(file, _lr.WID.ToString(), _lr, FolderType.Website, FolderType.WebsitePopup);
            return Json(_res);
        }

        [HttpPost]
        [Route("Admin/Before-Login-Popup")]
        [Route("Before-Login-Popup")]
        public IActionResult BeforeLoginPopup(IFormFile file)
        {
            IBannerML _bannerML = new ResourceML(_accessor, _env);
            var _res = _bannerML.UploadPopup(file, _lr.WID.ToString(), _lr, FolderType.Website, FolderType.BeforeLoginPopup);
            return Json(_res);
        }

        [HttpPost]
        [Route("Admin/After-Login-Popup")]
        [Route("After-Login-Popup")]
        public IActionResult AfterLoginPopup(IFormFile file)
        {
            IBannerML _bannerML = new ResourceML(_accessor, _env);
            var _res = _bannerML.UploadPopup(file, _lr.WID.ToString(), _lr, FolderType.Website, FolderType.AfterLoginPopup);
            return Json(_res);
        }
        [HttpPost]
        [Route("remove-Popup")]
        public IActionResult RemoveBanner(string id)
        {
            IBannerML bannerML = new ResourceML(_accessor, _env);
            var _res = bannerML.RemovePopup(id, _lr.WID.ToString(), _lr);
            return Json(_res);
        }

        [HttpPost]
        [Route("/AfterLoginPopup")]
        public IActionResult AfterWebsitePopUp()
        {
            var WebInfo = loginML.GetPopupInfo();
            if (WebInfo.IsAfterLoginPopup)
            {
                return PartialView("Partial/_AfterLoginPopup", WebInfo);
            }
            else
            {
                return Ok();
            }
        }
        [Route("Admin/AppAfter-Login-Popup")]
        [Route("AppAfter-Login-Popup")]
        public IActionResult AppAfterLoginPopup(IFormFile file)
        {
            IBannerML _bannerML = new ResourceML(_accessor, _env);
            var _res = _bannerML.UploadPopup(file, _lr.WID.ToString(), _lr, FolderType.Website, FolderType.AppAfterLoginPopup);
            return Json(_res);
        }
        [HttpPost]
        [Route("Admin/AppBefore-Login-Popup")]
        [Route("AppBefore-Login-Popup")]
        public IActionResult AppBeforeLoginPopup(IFormFile file)
        {
            IBannerML _bannerML = new ResourceML(_accessor, _env);
            var _res = _bannerML.UploadPopup(file, _lr.WID.ToString(), _lr, FolderType.Website, FolderType.AppBeforeLoginPopup);
            return Json(_res);
        }
        #endregion

        #region Support
        [HttpGet]
        [Route("Support")]
        public IActionResult Support()
        {

            IUserWebsite _userML = new UserML(_accessor, _env);
            var resp = _userML.GetCompanyProfile(_lr.WID);
            return View(resp);
        }
        [HttpPost]
        [Route("bank-List")]
        public IActionResult _BankList()
        {
            IBankML bankML = new BankML(_accessor, _env);
            return PartialView("Partial/_BankList", bankML.BankList());
        }

        [HttpPost]
        [Route("Home/Mobile-Tollfree")]
        [Route("Mobile-Tollfree")]
        public IActionResult _MobileTollFree()
        {
            IOperatorML opml = new OperatorML(_accessor, _env);

            IEnumerable<OperatorDetail> list = opml.GetMobileTollFree();
            return PartialView("Partial/_MobileTollFree", list);
        }

        [HttpPost]
        [Route("Home/DTH-Tollfree")]
        [Route("DTH-Tollfree")]
        public IActionResult _DTHTollFree()
        {
            IOperatorML opml = new OperatorML(_accessor, _env);
            IEnumerable<OperatorDetail> list = opml.GetDTHTollFree();
            return PartialView("Partial/_DTHTollFree", list);
        }
        #endregion

        #region CallMe
        [HttpGet]
        [Route("callmerequest")]
        public IActionResult CallMeRequest()
        {
            return View();
        }
        [HttpPost]
        [Route("c-m-r")]
        public IActionResult CallMeReqList(int t)
        {
            IUserML userML = new UserML(_accessor, _env);
            var callmereq = userML.GetCallMeRequests(t);
            return PartialView("Partial/_CallMeRequest", callmereq);
        }

        [HttpPost]
        [Route("update-cmr")]
        public ResponseStatus UpdateCallMeReq(UserCallMeModel data)
        {
            IUserML userML = new UserML(_accessor, _env);
            var res = userML.UpdateCallMeHistory(data);
            return res;
        }

        [HttpPost]
        [Route("Admin/callMeCount")]
        [Route("callMeCount")]
        public IActionResult callMeCount()
        {
            IUserML userML = new UserML(_accessor, _env);
            return Json(userML.CallMeRequestCount());
        }

        #endregion

        [HttpPost]
        [Route("/PendingViews")]
        public IActionResult PendingViews()
        {
            var ReportML = new ReportML(_accessor, _env);
            return Json(ReportML.TotalPendingViews());
        }
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (loginML.IsInValidSession() && (context.RouteData.Values["Action"].ToString() != "Index" || context.RouteData.Values["Controller"].ToString() != "Admin"))
            {
                context.Result = new RedirectResult("~/");
            }
            else
            {
                base.OnActionExecuting(context);
            }
        }

        [HttpPost]
        [Route("Admin/invoice-by")]
        [Route("invoice-by")]
        public IActionResult ChangeInvoiceByAdmin(int ID, bool Is)
        {
            IAdminML userML = new UserML(_accessor, _env);
            IResponseStatus _res = userML.UpdateInvoiceByAdmin(ID, Is);
            return Json(_res);
        }
        [HttpPost]
        [Route("Admin/mark-rg")]
        [Route("mark-rg")]
        public IActionResult ChangeMark_RG(int ID, bool Is)
        {
            IAdminML userML = new UserML(_accessor, _env);
            IResponseStatus _res = userML.UpdateMarkRG(ID, Is);
            return Json(_res);
        }
        #region UserSubcriptionAssign
        [HttpGet]
        [Route("Home/UserSubcriptionAssign")]
        [Route("UserSubcriptionAssign")]
        public async Task<IActionResult> UserSubcriptionAssign()
        {
            IUserML UML = new UserML(_accessor, _env);
            var cus = new GetinTouctListModel
            {
                CustomerCareDetails = UML.GetCustomercare()
            };
            return View(cus);
        }
        [HttpPost]
        [Route("Home/_UserSubcriptionAssign")]
        [Route("_UserSubcriptionAssign")]
        public IActionResult _UserSubcriptionAssign(int TopRows, string Request, string MobileNo, int CustomerID, string Date)
        {
            IUserML UserML = new UserML(_accessor, _env);
            var _req = new CommonReq
            {
                LoginID = _lr.UserID,
                CommonInt = TopRows,
                CommonStr = Request,
                CommonStr2 = MobileNo,
                CommonInt2 = CustomerID,
                CommonStr3 = Date,
            };
            GetinTouctListModel var = UserML.GetUserSubcription(_req);
            var.CustomerCareList = new SelectList(var.CustomerCareDetail, "ID", "Name");
            return PartialView("Partial/_UserSubcriptionAssign", var);
        }
        [HttpPost]
        [Route("Home/AssignCustomer/{id}/{itemid}")]
        [Route("AssignCustomer/{id}/{itemid}")]
        public IActionResult AssignCustomer(int id, int itemid)
        {
            CommonReq _req = new CommonReq
            {
                CommonInt = itemid,
                CommonInt2 = id,
                LoginID = _lr.UserID,
                LoginTypeID = _lr.LoginTypeID
            };
            IUserML userML = new UserML(_accessor, _env);
            IResponseStatus _res = userML.AssignuserSubscription(_req);
            return Json(_res);

        }
        [HttpPost]
        [Route("RemoveRepeatedData/{itemid}")]
        public IActionResult RemoveRepeatedData(int itemid)
        {
            CommonReq _req = new CommonReq
            {
                CommonInt = itemid,
                LoginID = _lr.UserID,
                LoginTypeID = _lr.LoginTypeID
            };
            IUserML userML = new UserML(_accessor, _env);
            IResponseStatus _res = userML.RemoveUserSubscription(_req);
            return Json(_res);
        }
        #endregion
        #region SignUp Referral
        [HttpPost]
        [Route("Admin/ref-setting")]
        [Route("ref-setting")]
        public IActionResult UpdateReferralSetting(bool r, bool u)
        {
            //r --> (IsReferralToDownline),1.1 To Downline - True, 1.2 Any Role - False
            //u --> (IsUplineUnderAdmin), 2.1 Under Admin - True, 2.2 Under Upline - False
            ISettingML settings = new SettingML(_accessor, _env);
            var _res = settings.UpdateReferralSetting(r, u);
            return Json(_res);
        }
        [HttpPost]
        [Route("Admin/get-ref-setting")]
        [Route("get-ref-setting")]
        public IActionResult GetReferralSetting()
        {
            ISettingML settings = new SettingML(_accessor, _env);
            var _res = settings.GetReferralSetting();
            return Json(_res);
        }
        #endregion
        [HttpPost]
        [Route("upload-Apk")]
        public IActionResult UploadApk(IFormFile file, string FileName, bool IsTest)
        {
            IResourceML resourceML = new ResourceML(_accessor, _env);
            var _res = resourceML.UpLoadApk(file, FileName, IsTest, _lr);
            return Json(_res);
        }

        #region MAster-role

        [HttpGet]
        [Route("GetMasterRole")]
        public IActionResult GetMasterRole()
        {
            return View("MasterRole/MasterRole");
        }

        [HttpPost]
        [Route("/_GetMasterRole")]
        public IActionResult _GetMasterRole()
        {
            IUserML mL = new UserML(_accessor, _env);
            var res = mL.GetMasterRole();
            return PartialView("MasterRole/Partial/_MasterRole", res);
        }

        [HttpPost]
        [Route("/UpdateMasterRole")]
        public IActionResult UpdateMasterRole(int Id, int RegCharge)
        {
            IUserML mL = new UserML(_accessor, _env);
            var res = mL.UpdateMasterRoel(Id, RegCharge);
            return Json(res);
        }
        #endregion



        [HttpGet]
        [Route("Admin/API-Document")]
        [Route("API-Document")]
        public IActionResult APIDocument()
        {
            var model = new GetApiDocument();
            IWebsiteML settings = new WebsiteML(_accessor, _env);
            model = settings.GetAPIDocument();
            ViewBag.UserId = _lr.UserID;
            return View(model);
        }


        #region PinCodeArea
        [HttpGet]
        [Route("Home/PinCodeArea")]
        [Route("PinCodeArea")]
        public IActionResult PinCodeArea()
        {
            return View();
        }

        [HttpPost]
        [Route("Home/_PinCodeArea")]
        [Route("_PinCodeArea")]
        public IActionResult PinCodeArea(int PinCode)
        {
            var _req = new CommonReq
            {
                LoginID = _lr.UserID,
                LoginTypeID = _lr.LoginTypeID,
                CommonInt2 = PinCode
            };
            IUserML UML = new UserML(_accessor, _env);
            IEnumerable<PincodeDetail> list = UML.GetPincodeArea(_req);
            return PartialView("Partial/_PinCodeArea", list);
        }
        [HttpPost]
        [Route("Home/_UpdatePincodehour")]
        [Route("_UpdatePincodehour")]
        public IActionResult UpdatePincodehour(CommonReq req)
        {
            IResponseStatus _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvalidParam
            };
            req.LoginID = _lr.UserID;
            req.LoginTypeID = _lr.LoginTypeID;

            IUserML opml = new UserML(_accessor, _env);
            _resp = opml.SaveHour(req);
            return Json(_resp);
        }
        #endregion
        [HttpPost]
        [Route("/_SocialAlertSetting")]
        public IActionResult _SocialAlertSetting()
        {
            if (ApplicationSetting.IsSocialAlert && _lr.LoginTypeID == LoginType.ApplicationUser)
            {
                IUserML ml = new UserML(_accessor, _env);
                var res = ml.GetSetting();
                if (string.IsNullOrEmpty(res.WhatsappNo) || string.IsNullOrEmpty(res.TelegramNo) || string.IsNullOrEmpty(res.HangoutId))
                    return PartialView("UserEdit/_LowBalanceSetting", res);
                else
                    return Json(false);
            }
            else
            {
                return Json(false);
            }
        }
        [HttpGet]
        [Route("BulkSocialAlert")]
        public IActionResult BulkSocialAlert()
        {
            var userML = new UserML(_accessor, _env);
            var modal = userML.GetBulkSmsSocial();
            return View("BulkAction/BulkSocialAlert", modal);
        }

        [HttpPost]
        [Route("SendBulkSocial")]
        public IActionResult SendBulkSocial(List<string> APIIDs, string SocialIDs, string Message, int SocialAlertType)
        {
            IUserML ml = new UserML(_accessor, _env);
            var data = ml.GetUserDeatilForAlert(_lr.UserID);
            data.Msg = Message;
            data.SocialIDs = SocialIDs;
            data.SocialAlertType = SocialAlertType;
            var AlertML = new AlertML(_accessor, _env);
            var res = AlertML.SendBulkSocialAlert(data, APIIDs);
            return Json(res);
        }
        #region Video master
        [HttpGet("Videomaster")]
        [Route("Home/Videomaster")]
        public IActionResult Videomaster()
        {
            return View();
        }
        [HttpPost]
        [Route("Home/_Videomaster")]
        [Route("_Videomaster")]
        public IActionResult _Videomaster()
        {
            IResourceML ML = new ResourceML(_accessor, _env);
            return PartialView("Partial/_Videomaster", ML.Getvideolink(_lr));
        }
        [HttpPost]
        [Route("Home/VideoADD")]
        [Route("VideoADD")]
        public IActionResult VideoADD()
        {
            return PartialView("Partial/_VideoADD");

        }
        [HttpPost]
        [Route("RemoveVideo")]
        public IActionResult RemoveVideo(int URlID)
        {
            IResponseStatus _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = "Fail request!"
            };

            IResourceML ML = new ResourceML(_accessor, _env);
            _res = ML.RemoveVideo(_lr, URlID);
            return Json(_res);
        }
        [HttpPost]
        [Route("Home/Save-Video")]
        [Route("Save-Video")]
        public IActionResult SaveVideo([FromBody] Video VideoDetail)
        {
            IResponseStatus _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = "Fail request!"
            };
            if (_lr.LoginTypeID.In(LoginType.ApplicationUser, LoginType.CustomerCare))
            {
                if (VideoDetail != null)
                {
                    IResourceML ML = new ResourceML(_accessor, _env);
                    _res = ML.SaveVideo(_lr, VideoDetail.ID, VideoDetail.URL, VideoDetail.Title);
                }
            }
            return Json(_res);
        }
        #endregion

        //[HttpPost]
        //[Route("GetLapuAPI")]
        //public IActionResult GetLapuAPI()
        //{
        //    var userML = new UserML(_accessor, _env);
        //    var APIs = userML.GetBulkSms().smsApi.Where(x => x.APIType == 2);
        //    return Json(APIs);
        //}
        [HttpPost]
        [Route("GetAPIs")]
        public IActionResult GetAPIs()
        {
            var userML = new UserML(_accessor, _env);
            var APIs = userML.GetBulkSms().smsApi;
            return Json(APIs);
        }
        [HttpPost]
        [Route("Home/Lead-summary")]
        [Route("Lead-summary")]
        public async Task<IActionResult> _LeadSummary(int CustomerID)
        {
            IUserML ml = new UserML(_accessor, _env);
            LeadSummary _transactionSummary = await ml.GetLeadSummary(CustomerID);
            return Json(_transactionSummary);
        }
        [HttpPost]
        [Route("/DeleteWebNotification")]
        public IActionResult DeleteWebNotification(string Ids, int Action)
        {
            IUserML userMl = new UserML(_accessor, _env);
            var res = userMl.DeleteWebNotification(Ids, Action);
            return Json(res);
        }

        [HttpPost]
        [Route("Admin/updateAppPackID")]
        [Route("updateAppPackID")]
        public IActionResult UpdateAppPackageID(int wid, string appPackage)
        {
            IWebsiteML _userWebsite = new WebsiteML(_accessor, _env);
            var _res = _userWebsite.UpdateAppPackageID(wid, appPackage);
            return Json(_res);
        }

        public IActionResult APICircleCode()
        {
            return View();
        }

        [HttpPost]
        [Route("/_APICircleCode")]
        public IActionResult _APICircleCode()
        {
            IOperatorAppML ml = new OperatorML(_accessor, _env);
            IAPIML apiml = new APIML(_accessor, _env);
            var response = new APICircleCodeModel
            {
                Circles = ml.CircleList().Result,
                APICircleCode = ml.APICircleCode().Result,
                APIs = apiml.GetAPIDetail()
            };
            return PartialView("Partial/_APICircleCode", response);
        }

        [HttpPost]
        [Route("/SaveAPICircleCode")]
        public IActionResult SaveAPICircleCode(APICircleCode req)
        {
            IOperatorAppML ml = new OperatorML(_accessor, _env);
            var response = ml.SaveAPICircleCode(req);
            return Json(response);
        }
        public IActionResult ModifyTemplate()
        {
            IWebsiteML ml = new WebsiteML(_accessor, _env);
            var modal = new ModifyTemplate
            {
                Themes = ml.GetThemes()
            };
            return View(modal);
        }

        public IActionResult _ModifyTemplate(int ThemeID, int SectionID)
        {
            IWebsiteML ml = new WebsiteML(_accessor, _env);
            var modal = ml.Template(ThemeID);
            modal.SectionID = SectionID;
            return PartialView("Partial/_ModifyTemplate", modal);
        }

        [HttpPost]
        [Route("/UpdateTemplate")]
        public IActionResult UpdateTemplate(HomeDisplayRequest req)
        {
            IWebsiteML ml = new WebsiteML(_accessor, _env);
            var res = ml.UpdateDisplayHtml(req);
            return Json(res);
        }

        [HttpPost]
        [Route("uploadTinyMCEImage")]
        public IActionResult uploadTinyMCEImage(IFormFile file)
        {
            IBannerML _bannerML = new ResourceML(_accessor, _env);
            var _res = _bannerML.uploadTinyMCEImage(file, _lr.WID);
            return Json(_res.CommonStr);
        }
        [HttpPost]
        [Route("RegisterasVendor")]
        public IActionResult RegisterasVendor(int UserID)
        {
            IUserML mL = new UserML(_accessor, _env);
            var _res = mL.RegisterasVendor(UserID);
            return Json(_res);
        }

        [HttpGet]
        [Route("Home/UserWiseLimit")]
        [Route("UserWiseLimit")]
        public IActionResult UserWiseLimit()
        {
            ILoginML ml = new LoginML(_accessor, _env);
            WebsiteInfo _winfo = ml.GetWebsiteInfo();
            return View(_winfo.IsMultipleMobileAllowed);
        }
        [HttpPost]
        [Route("Home/User-Wise-Limit")]
        [Route("User-Wise-Limit")]
        public IActionResult _UserWiseLimit(int UserID)
        {
            ISwitchingML sml = new SwitchingML(_accessor, _env);
            IEnumerable<UserWiseLimitResp> list = sml.GetUserLimitByUser(UserID);
            ViewBag.UserID = UserID;
            return PartialView("Partial/_UserWiseLimit", list);
        }

        [HttpPost]
        [Route("Home/UserWiseLimitCU")]
        [Route("UserWiseLimitCU")]
        public IActionResult UserSwitchAPI([FromBody] UserLimitCUReq userlimitCUReq)
        {
            ISwitchingML swml = new SwitchingML(_accessor, _env);
            IResponseStatus _res = swml.UserwiseLimitCU(userlimitCUReq);
            return Json(_res);
        }

        [HttpGet]
        [Route("Master/Vendor")]
        public IActionResult MasterVendor()
        {
            return View();
        }

        [HttpPost]
        [Route("GetVendorList")]
        public IActionResult _MasterVendor()
        {
            IDeviceML ml = new DeviceML(_accessor, _env);
            MasterVendorModel req = new MasterVendorModel
            {
                ID = 0
            };
            var res = ml.VendorMaster(req);
            return PartialView("Partial/_MasterVendor", res);
        }

        [HttpPost]
        [Route("MasterVendorCU")]
        public IActionResult MasterVendorCU(int id)
        {
            var res = new MasterVendorModel();
            if (id > 0)
            {
                IDeviceML ml = new DeviceML(_accessor, _env);
                MasterVendorModel req = new MasterVendorModel
                {
                    ID = id
                };
                var response = ml.VendorMaster(req).ToList();
                if (response.Count == 1)
                {
                    res = response.ElementAt(0);
                }
            }
            return PartialView("Partial/_MasterVendorCU", res);
        }

        [HttpPost]
        [Route("MasterVendorCUP")]
        public IActionResult MasterVendorCU(MasterVendorModel req)
        {
            IDeviceML ml = new DeviceML(_accessor, _env);
            var res = ml.VendorMasterCU(req);
            return Json(res);
        }

        [HttpPost]
        [Route("ToggleVendor")]
        public IActionResult MasterVendorToggle(MasterVendorModel req)
        {
            IDeviceML ml = new DeviceML(_accessor, _env);
            var res = ml.VendorMasterToggle(req);
            return Json(res);
        }

        [HttpGet]
        [Route("Master/DeviceModel")]
        public IActionResult MasterDeviceModel()
        {
            return View();
        }

        [HttpPost]
        [Route("GetDeviceModelList")]
        public IActionResult _MasterDeviceModel()
        {
            IDeviceML ml = new DeviceML(_accessor, _env);
            MasterDeviceModel req = new MasterDeviceModel
            {
                ID = 0
            };
            var res = ml.DeviceModelMaster(req);
            return PartialView("Partial/_MasterDeviceModel", res);
        }

        [HttpPost]
        [Route("MasterDeviceModelCU")]
        public IActionResult MasterDeviceModelCU(int id)
        {
            var res = new MasterDeviceModel();
            IDeviceML ml = new DeviceML(_accessor, _env);
            res.VendorDdl = ml.VendorDDl();
            if (id > 0)
            {
                MasterDeviceModel req = new MasterDeviceModel
                {
                    ID = id
                };
                var response = ml.DeviceModelMaster(req).ToList();
                if (response.Count == 1)
                {
                    res.ID = response.ElementAt(0).ID;
                    res.ModelName = response.ElementAt(0).ModelName;
                    res.VendorId = response.ElementAt(0).VendorId;
                    res.VendorName = response.ElementAt(0).VendorName;
                    res.ServiceId = response.ElementAt(0).ServiceId;
                    res.ServiceName = response.ElementAt(0).ServiceName;
                    res.Remark = response.ElementAt(0).Remark;
                    res.IsActive = response.ElementAt(0).IsActive;
                }
            }
            return PartialView("Partial/_MasterDeviceModelCU", res);
        }

        [HttpPost]
        [Route("MasterDeviceModelCUP")]
        public IActionResult MasterDeviceModelCU(MasterDeviceModel req)
        {
            IDeviceML ml = new DeviceML(_accessor, _env);
            var res = ml.DeviceModelMasterCU(req);
            return Json(res);
        }

        [HttpPost]
        [Route("ToggleDeviceModel")]
        public IActionResult MasterDeviceModelToggle(MasterDeviceModel req)
        {
            IDeviceML ml = new DeviceML(_accessor, _env);
            var res = ml.DeviceModelMasterToggle(req);
            return Json(res);
        }

        [HttpPost]
        [Route("OperatorAllocationVendor")]
        public IActionResult OperatorAllocation(int ID)
        {
            IDeviceML ml = new DeviceML(_accessor, _env);
            var res = ml.VendorOperatorAllocation(ID);
            IOperatorML opml = new OperatorML(_accessor, _env);
            res.OperatorDdl = opml.GetOptypeInSlab().ToList();
            return PartialView("Partial/_MasterVendorOperator", res);
        }

        [HttpPost]
        [Route("OperatorAllocate")]
        public IActionResult OperatorAlloacte(int ID, string[] arrOp)
        {
            string op = string.Join(((char)160), arrOp);
            VendorBindOperators req = new VendorBindOperators
            {
                ID = ID,
                SelectOps = op
            };
            IDeviceML ml = new DeviceML(_accessor, _env);
            var res = ml.VendorOperatorUpdate(req);
            return Json(res);
        }

        [HttpPost]
        [Route("GetVendorOperator")]
        public IActionResult GetVendorOperator(int id)
        {
            IOperatorML ml = new OperatorML(_accessor, _env);
            var res = ml.GetOptypeVendor(id);
            return Json(res);
        }

        [HttpGet]
        [Route("MPosDeviceInventory")]
        public IActionResult MPosDeviceInventory()
        {
            return View();
        }

        [HttpPost]
        [Route("GetMPosDeviceList")]
        public IActionResult _MPosDeviceInventory()
        {
            IDeviceML ml = new DeviceML(_accessor, _env);
            MPosDeviceInventoryModel req = new MPosDeviceInventoryModel
            {
                ID = 0
            };
            var res = ml.MPosDevice(req);
            return PartialView("Partial/_MPosDeviceInventory", res);
        }

        [HttpPost]
        [Route("GetMPosDeviceById")]
        public IActionResult MPosDeviceInventoryCU(int id)
        {
            IDeviceML ml = new DeviceML(_accessor, _env);
            var res = new MPosDeviceInventoryModel();
            if (id > 0)
            {
                MPosDeviceInventoryModel req = new MPosDeviceInventoryModel
                {
                    ID = id
                };
                var response = ml.MPosDevice(req).ToList();
                if (response.Count == 1)
                {
                    res = response.ElementAt(0);
                }
            }
            res.VendorDdl = ml.VendorDDl();
            return PartialView("Partial/_MPosDeviceInventoryCU", res);
        }

        [HttpPost]
        [Route("MPosDeviceCUP")]
        public IActionResult MPosDeviceInventoryCU(MPosDeviceInventoryModel req)
        {
            IDeviceML ml = new DeviceML(_accessor, _env);
            var res = ml.MPosDeviceCU(req);
            return Json(res);
        }

        [HttpPost]
        [Route("GetMasterDeviceList")]
        public IActionResult GetMasterDeviceList(int id)
        {
            IDeviceML ml = new DeviceML(_accessor, _env);
            var res = ml.DeviceModelDDl(id);
            return Json(res);
        }

        [HttpPost]
        [Route("MPosDeviceAssignMap")]
        public IActionResult MPosDeviceAssignment(int id, bool isMap = false)
        {
            IDeviceML ml = new DeviceML(_accessor, _env);
            var res = new MPosDeviceInventoryModel();

            MPosDeviceInventoryModel req = new MPosDeviceInventoryModel
            {
                ID = id
            };
            var response = ml.MPosDevice(req).ToList();
            if (response.Count == 1)
            {
                res = response.ElementAt(0);
                if (isMap) { res.UserId = 0; } else { res.OutletId = 0; }
            }
            //if (res.UserId <= 1)
            //{
            return PartialView("Partial/_MPosDeviceAssignment", res);
            //}
            //else
            //    return null;
        }

        [HttpPost]
        [Route("MPosDeviceAssign")]
        public IActionResult MPosDeviceAssignment(MPosDeviceInventoryModel req)
        {
            IDeviceML ml = new DeviceML(_accessor, _env);
            var res = ml.MPosDeviceAssignMap(req);
            return Json(res);
        }

        [HttpPost]
        [Route("MPosDeviceMap")]
        public IActionResult MPosDeviceMap(MPosDeviceInventoryModel req)
        {
            IDeviceML ml = new DeviceML(_accessor, _env);
            var res = ml.MPosDeviceAssignMap(req, true);
            return Json(res);
        }

        [HttpPost]
        [Route("MPosDeviceUnAssignUnMap")]
        public IActionResult MPosDeviceUnAssignUnMap(MPosDeviceInventoryModel req)
        {
            IDeviceML ml = new DeviceML(_accessor, _env);
            var res = ml.MPosDeviceUnAssignUnMap(req);
            return Json(res);
        }

        [HttpPost]
        [Route("ToggleMPos")]
        public IActionResult MPosToggle(MPosDeviceInventoryModel req)
        {
            IDeviceML ml = new DeviceML(_accessor, _env);
            var res = ml.MPosToggle(req);
            return Json(res);
        }

        [HttpPost]
        [Route("AutoBillingDetail")]
        public IActionResult AutoBillingDetail(int uid)
        {
            IUserML ml = new UserML(_accessor, _env);
            var res = ml.GetAutoBillingDetail(uid);
            return PartialView("Partial/_AutobillingDetail", res);
        }
        [HttpPost]
        [Route("UpdateAutoBilling")]
        public IActionResult UpdateAutoBilling(AutoBillingModel req)
        {
            IUserML ml = new UserML(_accessor, _env);
            req.LoginID = _lr.UserID;
            req.LT = _lr.LoginTypeID;
            var res = ml.UpdateAutoBilling(req);
            return Json(res);
        }
        [HttpPost]
        [Route("AutoBillingDetailBulk")]
        public IActionResult AutoBillingDetail(string uidList)
        {
            IUserML ml = new UserML(_accessor, _env);
            AutoBillingModel res = new AutoBillingModel
            {
                Id = 0,
                UserIdBulk = uidList
            };
            ViewBag.UserList
= uidList;
            return PartialView("Partial/_AutobillingDetail", res);
        }
        #region GetAllUplines
        [HttpPost]
        [Route("_get-All-Uplines")]
        public IActionResult _GetAllUplines(int userid)
        {
            IUserML ml = new UserML(_accessor, _env);
            var res = ml.GetAllUplines(userid);
            return PartialView("UserList/Partial/_GetAllUplines", res);
        }
        #endregion

        #region Holiday
        [HttpPost]
        [Route("Home/Getholiday")]
        [Route("Getholiday")]
        public IActionResult Getholiday()
        {
            IBankML bankML = new BankML(_accessor, _env);
            var res = bankML.GetHoliday();
            return PartialView("Partial/_HolidayDetail", res);
        }
        [HttpPost]
        [Route("Home/SaveHoliday")]
        [Route("SaveHoliday")]
        public IActionResult SaveHoliday(int ID, string holidayDate, string Remark, bool IsDeleted)
        {
            IBankML bankML = new BankML(_accessor, _env);
            var resp = bankML.SaveHolidayMaster(ID, holidayDate, Remark, IsDeleted);
            return Json(resp);
        }
        [HttpPost]
        [Route("Home/Addholiday")]
        [Route("Addholiday")]
        public IActionResult Addholiday()
        {
            return PartialView("Partial/_AddHoliday");
        }
        #endregion
        #region MTWRSetting
        [HttpPost]
        [Route("_mtwr-setting")]
        public IActionResult MTWRSettlSetting(int uid)
        {
            IUserML ml = new UserML(_accessor, _env);
            var res = ml.GetSettlementSetting(uid);
            return PartialView("Partial/_SettlementSetting", res);
        }
        [HttpPost]
        [Route("update-mtwr-set")]
        public IActionResult UpdateMTWRSettlSetting(UserwiseSettSetting userwiseSettSetting)
        {
            IUserML ml = new UserML(_accessor, _env);
            var res = ml.GetSettlementSetting(userwiseSettSetting);
            return Json(res);
        }
        #endregion
        #region PromoCodeMaster
        [HttpGet]
        [Route("Home/promocode-master")]
        [Route("promocode-master")]
        public IActionResult PromoCode()
        {
            return View("PromoCode/PromoCode");
        }
        [HttpPost]
        [Route("Home/GetOperatorsByOpType")]
        [Route("/GetOperatorsByOpType")]
        public IActionResult GetOperatorsByOpType(int OpTypeID)
        {
            IOperatorML opml = new OperatorML(_accessor, _env);
            PromoCode promoCode = new PromoCode();
            promoCode.Operator = opml.GetOperators(OpTypeID).ToList();
            return Json(promoCode);
        }

        [HttpPost]
        [Route("Home/SavePromocode")]
        [Route("/SavePromocode")]
        public IActionResult SavePromocode([FromForm] PromoCode PromocodeObj)
        {
            IPromoCodeML promocodeml = new PromoCodeML(_accessor, _env);
            var resp = promocodeml.SavePromoCode(PromocodeObj);
            return Json(resp);
        }
        [HttpPost]
        [Route("Home/promoCode-lst")]
        [Route("/promoCode-lst")]
        public IActionResult GetPromoCodeDetail()
        {
            IPromoCodeML promocodeml = new PromoCodeML(_accessor, _env);
            PromoCode promoCode = new PromoCode();
            promoCode.lstPromoCode = promocodeml.GetPromoCodeDetail();
            return PartialView("PromoCode/Partial/_PromoCode", promoCode.lstPromoCode);
        }
        [HttpPost]
        [Route("Home/pmc-cu")]
        [Route("pmc-cu")]
        public IActionResult PromoCodeCU(int ID)
        {
            IOperatorML opml = new OperatorML(_accessor, _env);
            IPromoCodeML promocodeml = new PromoCodeML(_accessor, _env);
            PromoCode promoCode = new PromoCode();
            promoCode.OpTypes = opml.GetOptypes().Where(e => e.IsB2CVisible == true).ToList();

            promoCode.Operator = opml.GetOperators(0).ToList();
            if (ID > 0)
            {
                promoCode = promocodeml.GetPromoCodeByID(ID);
                promoCode.OpTypes = opml.GetOptypes().Where(e => e.IsB2CVisible == true).ToList();
                promoCode.Operator = opml.GetOperators(0).ToList();
            }
            return PartialView("PromoCode/Partial/_PromoCodeCU", promoCode);
        }
        [HttpPost]
        [Route("Home/Upload-PromoImage")]
        [Route("/Upload-PromoImage")]
        public IActionResult UploadPromoImage(int ID, IFormFile PromoImage)
        {
            IPromoCodeML promocodeml = new PromoCodeML(_accessor, _env);
            var resp = promocodeml.UploadPromoImage(ID, PromoImage);
            return Json(resp);
        }
        [HttpPost]
        [Route("Home/ShowPreview")]
        [Route("/ShowPreview")]
        public IActionResult ShowPreview(int ID)
        {
            IPromoCodeML promocodeml = new PromoCodeML(_accessor, _env);
            PromoCode promoCode = new PromoCode();
            promoCode = promocodeml.GetPromoCodeByID(ID);
            return PartialView("PromoCode/Partial/_Preview", promoCode);
        }

        #endregion

        [HttpPost]
        [Route("Admin/Cal-Comm-Cir")]
        [Route("Cal-Comm-Cir")]
        public IActionResult ChangeComm_Cir(int ID, bool Is)
        {
            IAdminML userML = new UserML(_accessor, _env);
            IResponseStatus _res = userML.UpdatCalCommCir(ID, Is);
            return Json(_res);
        }
        [HttpPost]
        [Route("Admin/chg-sts-pg")]
        [Route("chg-sts-pg")]
        public IActionResult PGStsChange(int ID)
        {
            IAdminML userML = new UserML(_accessor, _env);
            var _res = userML.PGStsChange(ID);
            return Json(_res);
        }
        [HttpPost]
        [Route("Admin/chg-sts-dwnpg")]
        [Route("chg-sts-dwnpg")]
        public IActionResult PGDownLineStsChange(int ID)
        {
            IAdminML userML = new UserML(_accessor, _env);
            var _res = userML.PGDownLineStsChange(ID);
            return Json(_res);
        }
        [HttpPost]
        [Route("/rpadcount")]
        public IActionResult RPADCount()
        {
            var ReportML = new ReportML(_accessor, _env);
            var res = ReportML.RPADCount();
            return Json(new { res.DisputeCount, res.PCount });
        }
        [HttpPost]
        [Route("/rpadcountbyu")]
        public IActionResult RPADCountByU()
        {
            var ReportML = new ReportML(_accessor, _env);
            var res = ReportML.RPADCountByUserID();
            return Json(new { res.DisputeCount, res.PCount });
        }

        #region special-APID
        [HttpPost]
        [Route("Home/Special-APIID-Detail")]
        [Route("Special-APIID-Detail")]
        public IActionResult _SpecialAPIDDetail(int APIID, int OpTypeID)
        {
            IOperatorML opml = new OperatorML(_accessor, _env);
            var opList = opml.GetOperators().Where(x => x.OpType == OpTypeID);
            IOperatorAppML ml = new OperatorML(_accessor, _env);
            var circleList = ml.CircleListWithAll().Result;
            var Model = new DenominationDetailModal
            {
                APIID = APIID,
                Operator = opList.ToList(),
                CirlceList = circleList
            };
            return PartialView("Partial/_SpecialAPIIDDetail", Model);
        }

        [HttpPost]
        [Route("SpecialAPIIDDetailRole")]
        public async Task<IActionResult> _SpecialAPIDDetailRole(CircleWithDomination param)
        {
            IOperatorML opml = new OperatorML(_accessor, _env);
            var circleDomList = await opml.GetCircleWithDominationsAPI(param).ConfigureAwait(false);

            return PartialView("Partial/_SpecialAPIIDDetailRole", circleDomList);
        }

        [HttpPost]
        [Route("SpecialAPIIDDetailSaving")]
        public async Task<IActionResult> _SpecialAPIDDetailSaving(CircleWithDomination param)
        {
            IOperatorML opml = new OperatorML(_accessor, _env);
            var circleDomList = await opml.GetRemainDominationsSpecialSlabAPI(param).ConfigureAwait(false);
            var model = new DenominationDetailModal
            {
                circleWithDominations = circleDomList
            };
            return PartialView("Partial/_SpecialAPIIDDetailSaving", model);
        }

        [HttpPost]
        [Route("Home/Special-Incentive-Edit-APIID")]
        [Route("Special-Incentive-Edit-APIID")]
        public IActionResult _SpecialIncentiveEditAPID(CircleWithDomination param)
        {
            IOperatorML IOML = new OperatorML(_accessor, _env);
            IResponseStatus _resp = IOML.SaveSpecialSlabDetailAPI(param);
            return Json(_resp);
        }

        [HttpPost]
        [Route("Home/Special-Domination-Update-APIID")]
        [Route("Special-Domination-Update-APIID")]
        public IActionResult _SpecialDominationUpdateAPID(CircleWithDomination param)
        {
            IOperatorML IOML = new OperatorML(_accessor, _env);
            IResponseStatus _resp = IOML.UpdateSpecialAPIIDDomID(param);
            return Json(_resp);
        }

        [HttpPost]
        [Route("Home/Special-Group-Domination-Update-APIID")]
        [Route("Special-Group-Domination-Update-APIID")]
        public IActionResult _SpecialGroupDominationUpdateAPID(CircleWithDomination param)
        {
            IOperatorML IOML = new OperatorML(_accessor, _env);
            IResponseStatus _resp = IOML.UpdateGroupSpecialAPIIDDomID(param);
            return Json(_resp);
        }
        #endregion

        [HttpPost]
        [Route("/_ShowSocialPopup")]
        public IActionResult ShowSocialPopup()
        {
            if (((ApplicationSetting.IsSocialAlert || ApplicationSetting.IsEmailVefiricationRequired) && _lr.LoginTypeID == LoginType.ApplicationUser) && (_lr.RoleID != Role.Admin))
            {
                IUserML ml = new UserML(_accessor, _env);
                bool isEmail = ml.IsEMailVerified();
                var _uSetting = ml.GetSetting();

                var resp = new LowBalanceSetting
                {
                    IsEmailVerified = isEmail,
                    WhatsappNo = _uSetting.WhatsappNo,
                    TelegramNo = _uSetting.TelegramNo,
                    HangoutId = _uSetting.HangoutId
                };
                if (isEmail && !string.IsNullOrEmpty(_uSetting.WhatsappNo) && !string.IsNullOrEmpty(_uSetting.TelegramNo) && !string.IsNullOrEmpty(_uSetting.HangoutId))
                    return Json(false);
                else
                    return PartialView("UserEdit/_ShowSocialPopUp", resp);
            }
            else
            {
                return Json(false);
            }
        }

        #region MNP

        [Route("MNPUserView")]
        public IActionResult MNPUser(string s)
        {
            if (loginML.IsInValidSession())
            {
                return RedirectToAction("Index", "Login");
            }
            if (s == "MNPUSER")
            {
                return View("MNPUser", s);
            }
            return View();
        }
        [HttpPost]
        [Route("MNPUserList")]
        public IActionResult _MNPUserView(string s)
        {
            IUserML partnerML = new UserML(_accessor, _env);
            int UserID = _lr.UserID;
            var model = partnerML.GetMNPUser(UserID, s);

            model.RoleID = _lr.RoleID;
            return PartialView("Partial/_MNPUser", model);

        }

        [HttpPost]
        [Route("MNPUserListView")]
        public IActionResult MNPUserListView(int id)
        {
            if (loginML.IsInValidSession())
            {
                return RedirectToAction("Index", "Login");
            }
            IUserML partnerML = new UserML(_accessor, _env);
            var model = partnerML.GetMNPUserByID(new MNPUser { ID = id, UserID = _lr.UserID });

            model.RoleID = _lr.RoleID;
            return PartialView("Partial/_ViewMNPData", model);
        }

        [HttpPost]
        [Route("Admin/UpdateMNPStatus")]
        [Route("UpdateMNPStatus")]
        public IActionResult UpdateStatus(int ID, int status, string Username, string Password, string Remark, string Demo)
        {
            if (loginML.IsInValidSession())
            {
                return RedirectToAction("Index", "Login");
            }
            IUserML partnerML = new UserML(_accessor, _env);
            IResponseStatus _res = partnerML.UpdateMNPStatus(status, ID, Username, Password, Remark, Demo);
            return Json(_res);
        }



        [Route("MNPClaim")]
        public IActionResult MNPClaim(string s)
        {
            if (loginML.IsInValidSession())
            {
                return RedirectToAction("Index", "Login");
            }
            if (s == "MnpClaim")
            {
                return View("MNPClaim", s);
            }
            return View();
        }
        [HttpPost]
        [Route("MNPClaim")]
        public IActionResult _MNPClaim(string s)
        {
            IUserML partnerML = new UserML(_accessor, _env);
            int UserID = _lr.UserID;
            var model = partnerML.GetMNPCliam(UserID, s);

            model.RoleID = _lr.RoleID;
            return PartialView("Partial/_MNPClaim", model);

        }
        [HttpPost]
        [Route("MNPClaimByID")]
        public IActionResult MNPClaimByID(int id)
        {
            if (loginML.IsInValidSession())
            {
                return RedirectToAction("Index", "Login");
            }
            IUserML partnerML = new UserML(_accessor, _env);
            var model = partnerML.GetMNPClaimByID(new MNPUser { ID = id, UserID = _lr.UserID });

            model.RoleID = _lr.RoleID;
            return PartialView("Partial/_MNPClaimData", model);
        }
        [HttpPost]
        [Route("Admin/UpdateClaimMNPStatus")]
        [Route("UpdateClaimMNPStatus")]
        public IActionResult UpdateClaimMNPStatus(int ID, int status, decimal Amount, string Remark, string FRCDate, string FRCDemoNo, string FRCType, string FRCDoneDate)
        {
            if (loginML.IsInValidSession())
            {
                return RedirectToAction("Index", "Login");
            }

            IUserML partnerML = new UserML(_accessor, _env);
            IResponseStatus _res = partnerML.UpdateMNPClaiimStatus(status, ID, Remark, Amount, FRCDate, FRCDemoNo, FRCType, FRCDoneDate);
            return Json(_res);
        }

        #endregion

        [HttpPost]
        [Route("GetAPIsSocial")]
        public IActionResult GetAPIsSocial(int FormatID)
        {
            var userML = new APIML(_accessor, _env);
            var APIs = userML.GetSMSAPIDetail();
            if (FormatID == 5)
            {
                APIs = APIs.Where(w => w.IsWhatsApp);
            }
            if (FormatID == 6)
            {
                APIs = APIs.Where(w => w.IsHangout);
            }
            if (FormatID == 7)
            {
                APIs = APIs.Where(w => w.IsWhatsApp);
            }
            if (FormatID == 8)
            {
                APIs = APIs.Where(w => w.IsTelegram);
            }
            return Json(APIs);
        }

        [HttpPost]
        [Route("Admin/Toggle-Slab-Status")]
        [Route("Toggle-Slab-Status")]
        public IActionResult ChangeSlabStatus(int ID)
        {
            IUserML userML = new UserML(_accessor, _env);
            IResponseStatus _res = userML.ChangeSlabStatus(ID);
            return Json(_res);
        }


        [HttpPost]
        [Route("upload-AppImage")]
        public IActionResult AppLogoImage(IFormFile file)
        {
            IBannerML _bannerML = new ResourceML(_accessor, _env);

            var _res = _bannerML.UploadImages(file, _lr, FolderType.Website, FolderType.AppImage);
            return Json(_res);
        }


        #region AdditionalService

        [HttpPost]
        [Route("Admin/get-add-serv")]
        [Route("get-add-serv")]
        public IActionResult GetAddService(int UID, int outletID)
        {
            var userML = new UserML(_accessor, _env);
            var res = userML.GetAddServiceSts(_lr.LoginTypeID, UID, outletID);
            res.OutletID = outletID;
            return PartialView("Partial/_GetAdditionalService", res);
        }

        [HttpPost]
        [Route("Admin/act-add-serv")]
        [Route("act-add-serv")]
        public IActionResult ActAddService(int uid, int opTypeId, int OID, int outletID)
        {
            var userML = new UserML(_accessor, _env);
            var res = userML.ActivateAddServiceSts(_lr.LoginTypeID, _lr.UserID, uid, opTypeId, OID, outletID);
            return Json(res);
        }
        #endregion
        #region MembershipMaster
        [HttpGet]
        [Route("Home/MembershipMaster")]
        [Route("MembershipMaster")]
        public IActionResult MembershipMaster()
        {
            IUserML userML = new UserML(_lr);
            bool IsAdmin = _lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.AddEditSLAB);
            if ((IsAdmin || !_lr.IsAdminDefined) && !userML.IsEndUser())
            {
                var res = new Membership();
                res.IsAdmin = IsAdmin;
                res.IsWebsite = _lr.IsWebsite;
                return View(res);
            }
            return Ok();
        }

        [HttpPost]
        [Route("Home/MembershipMaster")]
        [Route("MembershipMaster")]
        public IActionResult _MembershipMaster()
        {
            IUserML userML = new UserML(_lr);
            var memModel = new Membership
            {
                IsAdmin = _lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.AddEditSLAB),
                IsWebsite = _lr.IsWebsite
            };
            if ((memModel.IsAdmin || !_lr.IsAdminDefined) && !userML.IsEndUser())
            {
                IMembershipML ML = new MembershipML(_accessor, _env);
                memModel.membershipMasters = ML.GetMembershipMaster();
                return PartialView("Partial/_MembershipMaster", memModel);
            }
            return Ok();
        }

        [HttpPost]
        [Route("Home/Membership-Edit/{id}")]
        [Route("Membership-Edit/{id}")]
        public IActionResult _MembershipEdit(int ID)
        {
            IUserML userML = new UserML(_accessor, _env);
            var membershipModel = new Membership
            {
                IsAdmin = _lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.AddEditSLAB),
                IsWebsite = _lr.IsWebsite
            };

            if ((membershipModel.IsAdmin || !_lr.IsAdminDefined) && !userML.IsEndUser())
            {
                IMembershipML ML = new MembershipML(_accessor, _env);
                membershipModel.membershipMaster = ML.GetMembershipMaster(ID);
                ISlabML slabML = new SlabML(_accessor, _env);

                membershipModel.slabMasters = slabML.GetSlabMaster();

                return PartialView("Partial/_MembershipCU", membershipModel);
            }
            return Ok();
        }


        [HttpPost]
        [Route("Home/Membership-Edit")]
        [Route("Membership-Edit")]
        public IActionResult MembershipEdit([FromBody] MembershipMaster memMaster)
        {
            IUserML userML = new UserML(_lr);
            ViewBag.IsAdmin = _lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.AddEditSLAB);
            if ((ViewBag.IsAdmin || !_lr.IsAdminDefined) && !userML.IsEndUser())
            {
                IMembershipML ML = new MembershipML(_accessor, _env);
                var resp = ML.UpdateMembershipMaster(memMaster);
                return Json(resp);
            }
            return Ok();
        }

        [HttpPost]
        [Route("Admin/Toggle-Mmeber-Status")]
        [Route("Toggle-Membership-Status")]
        public IActionResult ChangeMembershipStatus(int ID)
        {
            IMembershipML ML = new MembershipML(_accessor, _env);
            IResponseStatus res = ML.ChangeMemberShipStatus(ID);
            return Json(res);
        }

        [HttpPost]
        [Route("Admin/Toggle-Membership-CouponAllowed")]
        [Route("Toggle-Membership-CouponAllowed")]
        public IActionResult ToggleMembershipCouponAllowed(int ID)
        {
            IMembershipML ML = new MembershipML(_accessor, _env);
            IResponseStatus res = ML.ChangeCouponAllowedStatus(ID);
            return Json(res);
        }



        #endregion

        [HttpPost]
        [Route("Home/SenderNumberMapping/{id}")]
        [Route("SenderNumberMapping/{id}")]
        public IActionResult SenderNumberMapping(int id)
        {
            IWhatsappSenderNoML ML = new APIML(_accessor, _env);
            ViewData["DeptID"] = id;
            return PartialView("Partial/_SenderNumberMapping", ML.GetWhatsappSenderNoList(0));
        }


        [HttpPost]
        [Route("Home/map-number")]
        [Route("map-number")]
        public IActionResult UpdateMapNumber(string dept, string mn, bool ia, int id)
        {
            IWhatsappSenderNoML ml1 = new APIML(_accessor, _env);
            var res = ml1.UpdateMapNumber(dept, mn, ia, id);
            return Json(res);

        }

        [HttpPost]
        [Route("AccountOpeningSetting")]
        public IActionResult AccountOpeningSetting(int ID)
        {
            IOperatorML operatorML = new OperatorML(_accessor, _env);
            IWebsiteML ml = new WebsiteML(_accessor, _env);
            ReqAccOpenSetting retreq = new ReqAccOpenSetting();
            retreq.AccList = operatorML.AccountOpeningSetting(ID).ToList().FirstOrDefault();
            retreq._path = ml.GetWebsiteInfo();
            return PartialView("Partial/_AccountOpeningSetting", retreq);
        }

        [HttpPost]
        [Route("AccOpen_Setting_Edit")]
        public IActionResult AccountOpeningSettingEdit(int OID, string Content, string URI)
        {
            IOperatorML operatorML = new OperatorML(_accessor, _env);
            return Json(operatorML.UpdateAccountOpeningSetting(OID, Content, URI));
        }

        [HttpPost]
        [Route("upload-BankBanner")]
        public IActionResult uploadBankBanner(IFormFile file, int OID)
        {
            IResourceML mL = new ResourceML(_accessor, _env);
            var _res = mL.UploadBankBanner(file, OID);
            return Json(_res);
        }

        #region AXISCDM
        [HttpPost]
        [Route("MapCardTouser")]
        public IActionResult MapCardTouser(int ID)
        {
            ViewBag.Id = ID;
            ICardMapping ml = new CardMappingML(_accessor, _env);
            List<CardAccountMapping> cardaccountmapping = new List<CardAccountMapping>();
            return PartialView("Partial/_CardAccountList", ml.GetCardAccount(ID));
        }

        [HttpPost]
        [Route("MapCardTouserList")]
        public IActionResult MapCardTouserList(int ID)
        {
            ICardMapping ml = new CardMappingML(_accessor, _env);
            List<CardAccountMapping> cardaccountmapping = new List<CardAccountMapping>();
            return PartialView("Partial/__CardAccountList", ml.GetCardAccount(ID));
        }

        [HttpPost]
        [Route("UpdateCardDetail")]
        public IActionResult UpdateCardDetail([FromBody] CardAccountMapping data)
        {
            ICardMapping ml = new CardMappingML(_accessor, _env);

            return Json(ml.UpdateCardAccount(data));
        }

        [HttpPost]
        [Route("DeleteCardDetail")]
        public IActionResult DeleteCardDetail([FromBody] CardAccountMapping data)
        {
            ICardMapping ml = new CardMappingML(_accessor, _env);

            return Json(ml.DeleteCardAccount(data));
        }
        #endregion

        #region WhatsappAutoReplyBot

        [Route("WhatsappBotSetting")]
        public IActionResult WhatsappBotSetting()
        {
            if (ApplicationSetting.IsWhatsappAgent)
                return View("whatsapp/WhatsappBotSetting");
            else
                return Ok("Either requested page is not exists or you are not permitted to visit this page");
        }

        [HttpPost, Route("WhatsappBotDicList")]
        public IActionResult WhatsappBotDicList()
        {
            var ml = new WhatsappML(_accessor, _env);
            var res = ml.GetWhatsappBotDicList(new CommonReq());
            var response = new List<WhatsappBotDicViewModel>();
            if (res != null)
            {
                var formatTypes = res.Select(x => x.FormatType).Distinct();
                var dict = new List<WhatsappBotDic>();
                foreach (var format in formatTypes)
                {
                    var botKeys = new List<WhatsappBotKey>();
                    var keys = res.Where(y => y.FormatType == format).ToList();
                    foreach (var k in keys)
                    {
                        botKeys.Add(new WhatsappBotKey
                        {
                            KeyId = k.KeyId,
                            Key = k.Key
                        });
                    }
                    var modal = res.Where(y => y.FormatType == format).FirstOrDefault();
                    response.Add(new WhatsappBotDicViewModel
                    {
                        FormatType = format,
                        ReplyType = modal.ReplyType,
                        IsActive = modal.IsActive,
                        ReplyText1 = modal.ReplyText1,
                        ReplyText2 = modal.ReplyText2,
                        ReplyText3 = modal.ReplyText3,
                        Keys = botKeys
                    });
                }
            }
            return PartialView("whatsapp/_WhatsappBotDicList", response);
        }

        [HttpPost, Route("WhatsappKeyphrase")]
        public IActionResult _WhatsappKeyphrase(string formatType, int Id = 0)
        {
            var ml = new WhatsappML(_accessor, _env);
            var response = ml.GetWhatsappBotDicList(new CommonReq { CommonStr = formatType }).FirstOrDefault();
            response = response == null ? new WhatsappBotDic() : response;
            if (Id == 0)
                response.KeyId = Id;
            return PartialView("whatsapp/_WhatsappKeyphrase", response);
        }

        [HttpPost, Route("UpdateWhatsappKeyphrase")]
        public IActionResult UpdateWhatsappKeyphrase(WhatsappBotDic req)
        {
            var response = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            var mL = new WhatsappML(_accessor, _env);
            response = mL.UpdateWhatsappBotDic(req);
            return Json(response);
        }

        [HttpPost, Route("deletekeyFromWADic")]
        public IActionResult DeletekeyFromWADictionary(int keyId)
        {
            var response = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            var mL = new WhatsappML(_accessor, _env);
            response = mL.DeletekeyFromWADictionary(keyId);
            return Json(response);
        }
        #endregion

        #region Pakage ID limit 
        [HttpPost]
        [Route("Admin/pkg-lmt-trf")]
        [Route("pkg-lmt-trf")]
        public IActionResult PackageLimitTransfer(int UID)
        {
            var userML = new UserML(_accessor, _env);
            var res = userML.GetAddServiceIDLimittransfer(_lr.LoginTypeID, UID);
            return PartialView("Partial/_PackageLimitTransfer", res);
        }


        [HttpPost]
        [Route("Admin/IDlimitTransfer")]
        [Route("IDlimitTransfer")]
        public IActionResult IDlimitTransfer(GetAddService Req)
        {
            IResponseStatus _res = new ResponseStatus()
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvalidParam
            };
            if (Req != null)
            {
                IUserML userML = new UserML(_accessor, _env);
                _res = userML.IDLimit(Req);
            }
            return Json(_res);
        }

        [HttpPost]
        [Route("Admin/PackageLimitbalance")]
        [Route("PackageLimitbalance")]
        public IActionResult PackageLimitTransfer()
        {
            var userML = new UserML(_accessor, _env);
            var res = userML.GetAddServiceIDLimittransfer(_lr.LoginTypeID, _lr.UserID);
            return PartialView("Partial/_PackageLimitBalance", res);
        }
        #endregion

        #region PaymentGatwaySetting
        [HttpGet]
        [Route("/MasterPaymentGateway")]
        public IActionResult MasterPaymentGateway()
        {
            return View();
        }

        [HttpPost]
        [Route("/_MasterPG")]
        public async Task<IActionResult> _MasterPG(int id = 0)
        {
            IReportML ml = new ReportML(_accessor, _env);
            var resp = await ml.GetMasterPaymentGateway(id).ConfigureAwait(false);
            return PartialView("Partial/_MasterPG", resp.FirstOrDefault());
        }

        [HttpPost]
        [Route("/Admin/UpdateMPGateway")]
        [Route("UpdateMPGateway")]
        public IActionResult UpdateMPGateway(MasterPaymentGateway AddData)
        {
            IReportML ml = new ReportML(_accessor, _env);
            var resp = ml.UpdateMasterPaymentGateway(AddData);
            return Json(resp);

        }
        [HttpPost]
        [Route("/_MasterPaymentGateway")]
        public async Task<IActionResult> _MasterPaymentGateway(int id = 0)
        {
            IReportML ml = new ReportML(_accessor, _env);
            var _resp = await ml.GetMasterPaymentGateway(id).ConfigureAwait(false);
            return PartialView("Partial/_MasterPaymentGateway", _resp);
        }

        [HttpPost]
        [Route("/Admin/UpdatePaymentGateway")]
        [Route("UpdatePaymentGateway")]
        public IActionResult UpdatePaymentGateway(PaymentGateway AddData)
        {
            IReportML ml = new ReportML(_accessor, _env);
            var resp = ml.UpdatePaymentGateway(AddData);
            return Json(resp);

        }

        [HttpGet]
        [Route("/PaymentGateway")]
        public IActionResult PaymentGateway()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> _PaymentGateway()
        {
            CommonReq filter = new CommonReq
            {
                LoginID = _lr.UserID,
                LoginTypeID = _lr.LoginTypeID,
                CommonInt2 = _lr.WID
            };
            IReportML ml = new ReportML(_accessor, _env);
            var _resp = await ml.GetPaymentGateway(filter).ConfigureAwait(false);
            return PartialView("Partial/_PaymentGateway", _resp);
        }


        [HttpPost]
        public async Task<IActionResult> _PaymentGatewayById(int id = -1)
        {
            CommonReq filter = new CommonReq
            {
                CommonInt = id,
                LoginID = _lr.UserID,
                LoginTypeID = _lr.LoginTypeID,
                CommonInt2 = _lr.WID
            };
            IReportML ml = new ReportML(_accessor, _env);
            var _resp = await ml.GetPaymentGateway(filter).ConfigureAwait(false);
            return PartialView("Partial/_PaymentGatewayById", _resp.FirstOrDefault());
        }
        #endregion


        //[Route("_AgreementApprovalNotification")]
        //[HttpPost]
        //public IActionResult AgreementApprovalNotification()
        //{
        //    IReportML ml = new ReportML(_accessor, _env);
        //    var _resp = ml.GetAgreementApprovalNotification(_lr.UserID);
        //    if (!_resp.Status)
        //        return PartialView("Partial/_AgreementApprovalNotification", _resp);
        //    else
        //        return Json(new { status = true });
        //}
    }
}
