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
using Microsoft.AspNetCore.Routing;
using RoundpayFinTech.AppCode.Configuration;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.MiddleLayer;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.Employee;
using RoundpayFinTech.AppCode.StaticModel;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class EmployeeController : Controller
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly ISession _session;
        private readonly LoginResponse _lrEmp;
        private readonly ILoginML loginML;
        private readonly LoginResponse _lr;

        public EmployeeController(IHttpContextAccessor accessor, IHostingEnvironment env)
        {
            _accessor = accessor;
            _env = env;
            _session = _accessor.HttpContext.Session;
            _lrEmp = _session.GetObjectFromJson<LoginResponse>(SessionKeys.LoginResponseEmp);
            loginML = new LoginML(_accessor, _env);
            _lr = _session.GetObjectFromJson<LoginResponse>(SessionKeys.LoginResponse);
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

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            CookieHelper cookie = new CookieHelper(_accessor);
            cookie.Remove(SessionKeys.AppSessionID);
            return RedirectToAction("Index", "Login");
        }

        [Route("/Employee")]
        public IActionResult Index()
        {
            if (loginML.IsInValidSession())
            {
                return RedirectToAction("Index", "Login");
            }
            if (LoginType.Employee == _lrEmp.LoginTypeID)
                return View();
            return Ok();
        }

        #region ChangePasswordSection

        [HttpPost]
        [Route("Employee/_ChangePassword")]
        public IActionResult _ChangePassword(bool IsMandate)
        {
            return PartialView("Partial/_ChangePassword", IsMandate);
        }

        [HttpPost]
        [Route("Employee/ChangePassword")]
        public IActionResult ChangePassword(ChangePassword UserData)
        {
            IEmpML ml = new EmpML(_accessor, _env);
            return Json(ml.ChangePassword(UserData));
        }

        #endregion

        #region Employee
        [HttpGet]
        [Route("Employee/List")]
        public IActionResult Employees()
        {
            return View();
        }

        [HttpPost]
        [Route("/Get-Employee")]
        public IActionResult _Employees(CommonFilter filter)
        {
            IEmpML mL = new EmpML(_accessor, _env);
            var res = mL.GetEmployee(filter);
            return PartialView("Partial/_Employees", res);
        }

        //[HttpPost]
        //[Route("Admin/emp")]
        //[Route("emp")]
        //public IActionResult _AddEmployee(string m)
        //{
        //    m = string.IsNullOrEmpty(m) ? _lr.UserID.ToString() : m;
        //    IEmpML empML = new EmpML(_accessor, _env);
        //    return PartialView("Partial/_AddEmployee", empML.GetEmpReffDeatil(m));
        //}        

        [HttpPost]
        [Route("/Edit-Employee")]
        public IActionResult EditEmployee(int EmpID)
        {
            IEmpML mL = new EmpML(_accessor, _env);
            var res = mL.GetEmployeeByID(EmpID);
            return PartialView("Partial/_AddEmployee", res);
        }

        [HttpPost]
        [Route("cemp")]
        public IActionResult AddEmp([FromBody] EmpInfo EmpCreate)
        {
            EmpCreate.RequestModeID = RequestMode.PANEL;
            IEmpML _CreateEmpML = new EmpML(_accessor, _env);
            IResponseStatus _resp = _CreateEmpML.CallCreateEmp(EmpCreate);
            return Json(_resp);
        }

        [HttpPost]
        [Route("showEmpPass")]
        public IActionResult showEmpPass(int Id)
        {
            IEmpML empML = new EmpML(_accessor, _env);
            return Json(empML.ShowEmpPass(Id));
        }

        [HttpPost]
        [Route("resendEmpPass")]
        public IActionResult resendEmpPass(int Id)
        {
            IEmpML empML = new EmpML(_accessor, _env);
            return Json(empML.ResendEmpPass(Id));
        }

        [HttpPost]
        [Route("ChangeEmpSts")]
        public IActionResult ChangeEmpSts(int Id, int Is)
        {
            IEmpML empml = new EmpML(_accessor, _env);
            return Json(empml.ChangeEmpStatus(Id, Is));
        }

        [HttpGet]
        [Route("EmpBulkAction")]
        public IActionResult EmpBulkAction()
        {
            return View();
        }

        [HttpPost]
        [Route("/_EmpBulkAction")]
        public IActionResult _EmpBulkAction(CommonFilter filter)
        {
            IEmpML mL = new EmpML(_accessor, _env);
            var res = mL.GetEmployee(filter);
            return PartialView("Partial/_EmpBulkAction", res);
        }

        [HttpPost]
        [Route("/SelectEmpByroleInBulk")]
        public IActionResult SelectEmpByroleInBulk(int Id, bool OnlyChild)
        {
            IEmpML mL = new EmpML(_accessor, _env);
            return Json(mL.SelectEmpByRoleBulk(Id, OnlyChild));
        }

        [HttpPost]
        [Route("/PossibleAMForEmp")]
        public IActionResult PossibleAMForEmp(int EmpId)
        {
            IEmpML mL = new EmpML(_accessor, _env);
            var list = mL.PossibleAMForEmp(EmpId);
            return Json(list);
        }

        [HttpPost]
        [Route("Admin/ChangeReportingTo")]
        [Route("ChangeReportingTo")]
        public IActionResult ChangeReportingTo(int Id, int ReportingTo)
        {
            IEmpML empml = new EmpML(_accessor, _env);
            return Json(empml.ChangeEmpAssignee(Id, ReportingTo));
        }

        [HttpPost]
        [Route("/AssignUserToEmp")]
        public IActionResult AssignUserToEmp(int EmpID, string mobileNo)
        {
            IEmpML empml = new EmpML(_accessor, _env);
            var res = empml.AssignUserToEmp(EmpID, mobileNo);
            return Json(res);
        }

        [HttpPost]
        [Route("Admin/ChangeOtpSts")]
        [Route("ChangeOtpSts")]
        public IActionResult ChangeOtpSts(int Id, int Is)
        {
            IEmpML empml = new EmpML(_accessor, _env);
            return Json(empml.ChangeEmpOtpStatus(Id, Is));
        }

        #endregion

        [HttpPost]
        [Route("_EmpTarget")]
        public IActionResult _EmpTarget(int EmpID)
        {
            IEmpML mL = new EmpML(_accessor, _env);
            var res = mL.GetEmpTarget(EmpID, 0);
            return PartialView("Partial/_EmpTarget", res);
        }

        [HttpPost]
        [Route("_EmpTargetEdit")]
        public IActionResult _EmpTargetEdit(EmpTarget param)
        {
            IEmpML mL = new EmpML(_accessor, _env);
            IResponseStatus _resp = mL.SaveEmpTarget(param);
            return Json(_resp);
        }

        [HttpPost]
        [Route("Employee/upload-GiftImg")]
        public IActionResult UploadEmployeeGiftImg(IFormFile file, string fileName)
        {
            IResourceML ml = new ResourceML(_accessor, _env);
            var _res = ml.UploadEmployeeGift(file, fileName);
            return Json(_res);
        }

        [HttpGet]
        [Route("EmpUserList")]
        public IActionResult EmpUserList()
        {
            return View();
        }

        [HttpPost]
        [Route("/_EmpUserList")]
        [Route("Employee/_EmpUserList")]
        public IActionResult _EmpUserList(CommonFilter filter)
        {
            IEmpML mL = new EmpML(_accessor, _env);
            var res = mL.GetEmployeeeUser(filter);
            return PartialView("Partial/_EmpUserList", res);
        }


        [HttpPost]
        [Route("/EmpUserListChild")]
        public IActionResult EmpUserListChild(int ReffId, bool IsUp)
        {
            IEmpML ml = new EmpML(_accessor, _env);
            var res = ml.GetEmployeeUserChild(ReffId, IsUp);
            return PartialView("Partial/_EmpUserList", res);
        }

        #region Reports
        [HttpGet]
        [Route("Employee/PSTReport")]
        public IActionResult PSTReport()
        {
            return View(_lrEmp.UserID);
        }

        [HttpPost]
        [Route("Employee/_PSTReport")]
        public async Task<IActionResult> _PSTReport([FromBody] CommonFilter filter)
        {
            IEmpML ml = new EmpML(_accessor, _env);
            var _resp = await ml.GetPSTReportEmp(filter).ConfigureAwait(false);
            return PartialView("Partial/_PSTReport", _resp);
        }

        [HttpGet]
        [Route("Employee/PSTReport-Excel")]
        public async Task<IActionResult> _PSTReportExport(CommonFilter filter)
        {
            IEmpML ml = new EmpML(_accessor, _env);
            var _report = await ml.GetPSTReportEmp(filter).ConfigureAwait(false);
            var dataTable = ConverterHelper.O.ToDataTable(_report);
            string[] removableCol = { "Statuscode", "Msg", "ID", "SHID", "CHID", "ZID", "AID", "URoleID", "AMID", "AMRoleID", "UserID", "EntryDate" };
            var Contents = EportExcel.o.GetFile(dataTable, removableCol);
            return File(Contents, DOCType.XlsxContentType, "PSTReportForEmployee.xlsx");
        }

        [HttpPost]
        [Route("Employee/PSTDeatilReport")]
        public async Task<IActionResult> _PSTDeatilReport(CommonFilter filter)
        {
            IEmpML ml = new EmpML(_accessor, _env);
            var _resp = await Task.FromResult(ml.PSTDeatilReport(filter)).ConfigureAwait(false);
            return PartialView("Partial/_PSTDeatilReport", _resp);
        }



        [HttpGet]
        [Route("Employee/TertiaryReport")]
        public IActionResult TertiaryReport()
        {
            return View(_lrEmp.UserID);
        }

        [HttpPost]
        [Route("Employee/_TertiaryReport")]
        public async Task<IActionResult> _TertiaryReport(CommonReq filter)
        {
            IEmpML ml = new EmpML(_accessor, _env);
            var _resp = await ml.GetTertiaryReportEmp(filter).ConfigureAwait(false);
            return PartialView("Partial/_TertiaryReport", _resp);
        }

        [HttpGet]
        [Route("Employee/TertiaryReport-Excel")]
        public async Task<IActionResult> _TertiaryReportExcel(CommonReq filter)
        {
            IEmpML ml = new EmpML(_accessor, _env);
            var _report = await ml.GetTertiaryReportEmp(filter).ConfigureAwait(false);
            var dataTable = ConverterHelper.O.ToDataTable(_report);
            string[] removableCol = { "Statuscode", "Msg", "ID", "SHID", "CHID", "ZID", "AID", "URoleID", "AMID", "AMRoleID", "UserID", "EntryDate", "ServiceID", "OpTypeID" };
            var Contents = EportExcel.o.GetFile(dataTable, removableCol);
            return File(Contents, DOCType.XlsxContentType, "TertiaryReportForEmployee.xlsx");
        }

        [HttpGet]
        [Route("Employee/EmpTargetReport")]
        public IActionResult EmpTargetReport()
        {
            return View(_lrEmp.UserID);
        }

        [HttpPost]
        [Route("Employee/_EmpTargetReport")]
        public async Task<IActionResult> _EmpTargetReport(CommonReq filter)
        {
            IEmpML ml = new EmpML(_accessor, _env);
            var _resp = await ml.GetEmpTargetReport(filter).ConfigureAwait(false);
            return PartialView("Partial/_EmpTargetReport", _resp);
        }

        [HttpPost]
        [Route("Employee/ComparisionChart")]
        public IActionResult ComparisionChart(int UserID)
        {
            IEmpML ml = new EmpML(_accessor, _env);
            var _resp = ml.GetComparisionChart(UserID);
            return Json(_resp);
        }

        [HttpPost]
        [Route("Employee/LastdayVsTodayData")]
        public IActionResult LastdayVsTodayData()
        {
            IEmpML ml = new EmpML(_accessor, _env);
            var _resp = ml.GetLastdayVsTodayChart();
            return Json(_resp);
        }

        [HttpPost]
        [Route("Employee/TargetSegment")]
        public IActionResult TargetSegment()
        {
            IEmpML ml = new EmpML(_accessor, _env);
            var _resp = ml.GetTargetSegment();
            return Json(_resp);
        }

        [HttpPost]
        [Route("Employee/TodayLivePST")]
        public IActionResult TodayLivePST()
        {
            IEmpML ml = new EmpML(_accessor, _env);
            var _resp = ml.GetEmpTodayLivePST();
            return Json(_resp);
        }

        [HttpPost]
        [Route("Employee/TodayOutletsListForEmp")]
        public IActionResult TodayOutletsListForEmp()
        {
            IEmpML ml = new EmpML(_accessor, _env);
            var _resp = ml.TodayOutletsListForEmp();
            return Json(_resp);
        }

        [HttpPost]
        [Route("Employee/TodaySellPackages")]
        public IActionResult TodaySellPackages()
        {
            IEmpML ml = new EmpML(_accessor, _env);
            var _resp = ml.TodaySellPackages();
            return Json(_resp);
        }

        [HttpPost]
        [Route("Employee/GetUserCommitment")]
        public IActionResult GetUserCommitment()
        {
            IEmpML ml = new EmpML(_accessor, _env);
            var _resp = ml.GetUserCommitment();
            return PartialView("Partial/_EditUserCommitment", _resp);
        }

        
        [Route("Employee/ExportUserCommitment")]
        public IActionResult ExportUserCommitment()
        {
            IEmpML ml = new EmpML(_accessor, _env);
            var _report = ml.GetUserCommitment();
            var dataTable = ConverterHelper.O.ToDataTable(_report);
            string[] removableCol = { "CommitmentID", "EmpID", "EntryDate" };
            var Contents = EportExcel.o.GetFile(dataTable, removableCol);
            return File(Contents, DOCType.XlsxContentType, "TodayCommitment.xlsx");
            //return PartialView("Partial/_EditUserCommitment", _resp);
        }

        [HttpPost]
        [Route("Employee/GetCommitmentSummary")]
        public IActionResult GetCommitmentSummary()
        {
            IEmpML ml = new EmpML(_accessor, _env);
            var _resp = ml.GetUserCommitment();
            int TotalCommitment = 0, TotalAchieved = 0;
            foreach (var item in _resp)
            {
                TotalCommitment += item.Commitment;
                TotalAchieved += item.Achieved;
            }
            var summary = new CommitmentSummary
            {
                TotalCommitment = TotalCommitment,
                TotalAchieved = TotalAchieved
            };
            return Json(summary);
        }

        [HttpPost]
        [Route("Employee/CommitmentSummarychart")]
        public IActionResult CommitmentSummarychart(int UserID=0)
        {
            IEmpML ml = new EmpML(_accessor, _env);
            var _resp = ml.GetUserCommitment(UserID);
            int TotalCommitment = 0, TotalAchieved = 0;
            foreach (var item in _resp)
            {
                TotalCommitment += item.Commitment;
                TotalAchieved += item.Achieved;
            }

            var list = new List<CommitmentSummaryChart>();
            //list.Add(new CommitmentSummaryChart
            //{
            //    Service = "Commitment",
            //    Amount = TotalCommitment
            //});
            list.Add(new CommitmentSummaryChart
            {
                Service = "Acheived",
                Amount = TotalAchieved
            });            
            list.Add(new CommitmentSummaryChart
            {
                Service = "Remain",
                Amount = (TotalCommitment - TotalAchieved) > 0 ? (TotalCommitment - TotalAchieved) : 0
            });
            return Json(list);
        }

        [HttpPost]
        [Route("Employee/SetUserCommitment")]
        public IActionResult SetUserCommitment(UserCommitment req)
        {
            IEmpML ml = new EmpML(_accessor, _env);
            var _resp = ml.SetUserCommitment(req);
            return Json(_resp);
        }


        [HttpPost]
        [Route("Employee/GetEmpDownlineUser")]
        public IActionResult GetEmpDownlineUser()
        {
            IEmpML ml = new EmpML(_accessor, _env);
            var _resp = ml.GetEmpDownlineUser();
            return Json(_resp);
        }

        [HttpGet]
        [Route("Employee/LeadDetail")]
        public IActionResult LeadDetail()
        {
            if (ApplicationSetting.IsLead)
                return View();
            else
                return Json(null);
        }

        [HttpPost]
        [Route("Employee/_LeadDetail")]
        public async Task<IActionResult> _LeadDetail(int TopRows, string Request, string MobileNo, string AssignDate, bool OnlyTodayFollowup)
        {
            IEmpML ml = new EmpML(_accessor, _env);
            var _req = new CommonReq
            {
                CommonInt = TopRows,
                CommonStr = Request,
                CommonStr2 = MobileNo,
                CommonStr3 = AssignDate,
                CommonBool = OnlyTodayFollowup
            };
            GetinTouctListModel res = ml.GetUserSubscription(_req);
            return PartialView("Partial/_LeadDetail", res);
        }

        [HttpPost]
        public async Task<IActionResult> LeadStats()
        {
            IEmpML ml = new EmpML(_accessor, _env);
            var res = await ml.GetLeadStats().ConfigureAwait(true);
            return Json( res);
        }

        [HttpPost]
        [Route("Employee/UpdateAgainstLead")]
        public IActionResult UpdateAgainstLead(LeadDetail req)
        {
            if (ApplicationSetting.IsLead == true)
            {
                IEmpML ml = new EmpML(_accessor, _env);
                IResponseStatus _res = ml.UpdationAgainstLead(req);
                return Json(_res);
            }
            else
            {
                return Json(null);
            }

        }

        #endregion


        [HttpPost]
        [Route("/_AssignEmployee")]
        public IActionResult _AssignEmployee(CommonFilter filter)
        {
            filter.TopRows = 1000;
            filter.btnID = 1;
            filter.EmployeeRole = 2;
            IEmpML mL = new EmpML(_accessor, _env);
            var res = mL.GetEmployee(filter);
            return PartialView("Partial/_AssignEmployee", res);
        }

        [HttpPost]
        [Route("/UnAssignEmployee")]
        public IActionResult UnAssignEmployee(int UserID)
        {
            IEmpML mL = new EmpML(_accessor, _env);
            var res = mL.UnAssignEmployee(UserID);
            return Json(res);
        }

        [HttpPost]
        [Route("/TodayTransactors")]
        public IActionResult TodayTransactors(int type = 3)
        {
            IEmpML mL = new EmpML(_accessor, _env);
            var res = mL.TodayTransactors(type);
            ViewBag.Type = type;
            return PartialView("Partial/_TodayTransactors", res);
        }

        [HttpGet]
        [Route("/TodayTransactorsExcel")]
        public async Task<IActionResult> TodayTransactorsExcel(int type = 3)
        {
            IEmpML ml = new EmpML(_accessor, _env);
            var res = ml.TodayTransactors(type).ToList();
            var dataTable = ConverterHelper.O.ToDataTable(res);
            string[] removableCol = { "Id", "UserId" };
            var Contents = EportExcel.o.GetFile(dataTable, removableCol);
            return File(Contents, DOCType.XlsxContentType, "ActiveUsersExport.xlsx");
        }


        [HttpGet]
        [Route("/PSTComparisionTable")]
        public IActionResult PSTComparisionTable()
        {
            return View();
        }

        [HttpPost]
        [Route("/GetPSTComparisionTable")]
        public IActionResult GetPSTComparisionTable()
        {
            IEmpML mL = new EmpML(_accessor, _env);
            var res = mL.GetPSTComparisionTable();
            return PartialView("Partial/_PSTComparisionTable", res);
        }

        [HttpGet]
        [Route("/GetPSTComparisionTableExcel")]
        public async Task<IActionResult> GetPSTComparisionTableExcel()
        {
            IEmpML ml = new EmpML(_accessor, _env);
            var res = ml.GetPSTComparisionTable().ToList();
            var dataTable = ConverterHelper.O.ToDataTable(res);
            string[] removableCol = { "Id", "UserId" };
            var Contents = EportExcel.o.GetFile(dataTable, removableCol);
            return File(Contents, DOCType.XlsxContentType, "PSTCalendarReport.xlsx");
        }

        [HttpPost]
        [Route("Employee/TransferLead")]
        public IActionResult TransferLead(int Id, int TransferTo)
        {
            IEmpML mL = new EmpML(_accessor, _env);
            var res = mL.TransferLead(Id, TransferTo);
            return Json(res);
        }

        [HttpPost]
        [Route("Employee/GetLastSevenDayPST")]
        public IActionResult GetLastSevenDayPST()
        {
            IEmpML mL = new EmpML(_accessor, _env);
            var res = mL.GetLastSevenDayPSTDataForEmp();
            return Json(res);
        }
        [HttpPost]
        [Route("Employee/CreateLead")]
        public IActionResult _CreateLead(CreateLead param)
        {

            return PartialView("Partial/_CreateLead");
        }

        [HttpPost]
        [Route("Employee/CreateLead1")]
        public IActionResult CreateLead(CreateLead param)
        {
            IEmpML ml = new EmpML(_accessor, _env);
            IResponseStatus _resp = ml.CreateLead(param);
            return Json(_resp);
        }

        [HttpPost]
        [Route("Employee/_CreateMeeting")]
        public IActionResult _CreateMeeting()
        {
            IEmpML ml = new EmpML(_accessor, _env);
            if (!ml.IsClosingDone())
            {
                var res = ml.ReasonandPurpuse();
                res.Id = 0;
                return PartialView("Partial/_CreateMeeting", res);
            }
            else
            {
                var res = ml.ReasonandPurpuse();
                res.Id = -1;
                return Json(res); 
            }
        }

        [HttpPost]
        [Route("Employee/CreateMeeting1")]
        public IActionResult CreateMeeting(Meetingdetails param)
        {
            IEmpML ml = new EmpML(_accessor, _env);
            IResponseStatus _resp = ml.CreateMeeting(param);
            return Json(_resp);
        }

        [HttpPost]
        [Route("Employee/GetAreabyPincode")]
        public IActionResult GetAreabyPin(int Pincode)
        {
            IEmpML ml = new EmpML(_accessor, _env);
            var _resp = ml.GetAreabypincode(Pincode);
            return Json(_resp);
        }

        [HttpPost]
        [Route("Employee/GetUserdetailsbyNo")]
        public IActionResult GetUserDetailbyNo(string MobileNo)
        {
            IEmpML ml = new EmpML(_accessor, _env);
            var _resp = ml.GetUserdatabyMobileNo(MobileNo);
            return Json(_resp);
        }

        [HttpGet]
        [Route("/Employee/MeetingDetail")]
        [Route("Admin/EmployeeMeetingDetail")]
        public IActionResult MeetingDetail()
        {
            return View();
        }

        [HttpPost]
        [Route("/Employee/_MeetingDetail")]
        public IActionResult _MeetingDetail()
        {
            IEmpML ml = new EmpML(_accessor, _env);
            var res = ml.GetMeetingdetails();
            return PartialView("Partial/_MeetingDetail", res);
        }

        [HttpPost]
        [Route("/Employee/_MeetingDetail1")]
        public IActionResult _MeetingDetail(CommonReq filter)
        {
            IEmpML ml = new EmpML(_accessor, _env);
            var res = ml.GetMeetingdetails(filter);
            return PartialView("Partial/_MeetingDetail", res);
        }

        [HttpGet]
        [Route("Employee/_MeetingDetailExcel-Excel")]
        public async Task<IActionResult> _MeetingDetailExcel(CommonReq filter)
        {
            IEmpML ml = new EmpML(_accessor, _env);
            var _report = ml.GetMeetingdetails(filter);
            var dataTable = ConverterHelper.O.ToDataTable(_report);
            string[] removableCol = { "LoginID", "LoginTypeID", "Id", "PurposeId", "ReasonId", "AttandanceId", "Latitute", "Longitute", "StatusCode" };
            var Contents = EportExcel.o.GetFile(dataTable, removableCol);
            return File(Contents, DOCType.XlsxContentType, "MeetingReportForEmployee.xlsx");
        }

        [HttpPost]
        [Route("Employee/_DailyClosing")]
        public IActionResult _DailyClosing()
        {
            return PartialView("Partial/_DailyClosing");
        }

        [HttpPost]
        [Route("Employee/DailyClosing")]
        public IActionResult DailyClosing(DailyClosingModel param)
        {
            IEmpML ml = new EmpML(_accessor, _env);
            IResponseStatus _resp = ml.DailyClosing(param);
            return Json(_resp);
        }

        [HttpGet]
        [Route("Employee/MeetingReport")]
        [Route("Admin/EmployeeMeetingReport")]
        public IActionResult MeetingReport()
        {
            var loginRes = IsAdmin();
            return View(loginRes.UserID);
        }

        [HttpPost]
        [Route("Employee/_MeetingReport")]
        public async Task<IActionResult> _MeetingReport(CommonReq filter)
        {
            IEmpML ml = new EmpML(_accessor, _env);
            var _resp = await ml.GetMeetingReport(filter).ConfigureAwait(false);
            return PartialView("Partial/_MeetingReport", _resp);
        }

        [HttpPost]
        [Route("Employee/_MeetingDetailReport")]
        public async Task<IActionResult> _MeetingDetailReport(CommonReq filter)
        {
            IEmpML ml = new EmpML(_accessor, _env);
            var _resp = await ml.GetMeetingDetailReport(filter).ConfigureAwait(false);
            return Json(_resp);
        }

        [HttpGet]
        [Route("Employee/_MeetingReportExcel-Excel")]
        public async Task<IActionResult> _MeetingReportExcel(CommonReq filter)
        {
            IEmpML ml = new EmpML(_accessor, _env);
            var _report = await ml.GetMeetingReport(filter).ConfigureAwait(false);
            var dataTable = ConverterHelper.O.ToDataTable(_report);
            string[] removableCol = { "Id", "UserId", "StatusCode" };
            var Contents = EportExcel.o.GetFile(dataTable, removableCol);
            return File(Contents, DOCType.XlsxContentType, "MeetingDetailReportForEmployee.xlsx");
        }

        [HttpPost]
        [Route("Employee/_GetMapPoints")]
        public async Task<IActionResult> _GetMapPoints(CommonReq filter)
        {
            IEmpML ml = new EmpML(_accessor, _env);
            var _resp = await ml.GetMapPoints(filter).ConfigureAwait(false);
            return Json(_resp);
        }

        [HttpGet]
        [Route("Employee/AttendanceReport")]
        [Route("Admin/EmployeeAttendanceReport")]
        public IActionResult AttendanceReport()
        {
            var loginRes = IsAdmin();
            return View(loginRes.UserID);
        }

        [HttpPost]
        [Route("Employee/_AttendanceReport")]
        public async Task<IActionResult> _AttendanceReport(CommonReq filter)
        {
            IEmpML ml = new EmpML(_accessor, _env);
            var _resp = await ml.GetAttendanceReport(filter).ConfigureAwait(false);
            return PartialView("Partial/_AttendanceReport", _resp);
        }

        [HttpGet]
        [Route("Employee/_AttendanceReport-Excel")]
        public async Task<IActionResult> __AttendanceReportExcel(CommonReq filter)
        {
            IEmpML ml = new EmpML(_accessor, _env);
            var _report = await ml.GetAttendanceReport(filter).ConfigureAwait(false);
            string[] removableCol = { "Id", "EmpId" };
            var Contents = EportExcel.o.GetFile(_report.dtReport, removableCol);
            return File(Contents, DOCType.XlsxContentType, "EmployeeAttendanceReport.xlsx");
        }

        [HttpPost]
        [Route("Admin/Employee-upload-shopImage")]
        [Route("Employee-upload-shopImage")]
        public IActionResult EmployeeUploadShopImage(IFormFile file, string Mobile)
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
            if(file == null || Mobile.Length < 9)
            {
                return Json(_res);
            }
            else
            {
                IBannerML _bannerML = new ResourceML(_accessor, _env);

                _res = _bannerML.UploadEmployeeImage(file, _lrEmp, Mobile, FolderType.ShopImage);
                if (_res.Statuscode == ErrorCodes.One)
                { _res.CommonStr = "Image/Employee/ShopImage/" + _lrEmp.UserID.ToString() + Mobile + ".png"; }
                else { _res.CommonStr = _res.Msg; _res.Msg = _res.Msg; }
                return Json(_res);
            }
        }

        private LoginResponse IsAdmin()
        {
            var result = new LoginResponse();
            if (_lrEmp != null)
            {
                result = _lrEmp;
            }
            if (_lr != null && _lr.RoleID == Role.Admin)
            {
                result = _lr;

            }
            return result;
        }
    }
}
