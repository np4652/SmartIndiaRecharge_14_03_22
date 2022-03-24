using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Fintech.AppCode.HelperClass;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OfficeOpenXml;
using RoundpayFinTech.AppCode;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.MiddleLayer;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Report;
using RoundpayFinTech.Models;

namespace RoundpayFinTech.Controllers
{
    public partial class ReportController
    {
        #region SalesSummary
        [HttpGet]
        [Route("Home/SalesSummary")]
        [Route("SalesSummary")]
        public IActionResult SalesSummary()
        {
            return View();
        }

        [HttpPost]
        [Route("Home/sales-summary")]
        [Route("sales-summary")]
        public IActionResult _SalesSummary(string f, string t, string m)
        {
            IUserML userML = new UserML(_lr);
            IReportML operation = new ReportML(_accessor, _env);
            SalesSummaryModel salesSummaryModel = new SalesSummaryModel
            {
                IsAdmin = _lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowAdminSalesSummary),
                CanMarkDispute = _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.MarkDispute),
                CanFail = _lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.PageFailed),
                CanSuccess = _lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.PageSuccess),
                IsAPIUser = _lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.APIUser,
                IsEndUser = userML.IsEndUser(),
                Report = operation.GetSalesSummary(f, t, m)
            };
            return PartialView("PartialView/_SalesSummary", salesSummaryModel);
        }

        [HttpGet]
        [Route("Home/sales-summary")]
        [Route("sales-summary")]
        public IActionResult SalesSummaryExport(string f, string t, string m)
        {
            IUserML userML = new UserML(_lr);
            IReportML operation = new ReportML(_accessor, _env);
            SalesSummaryModel salesSummaryModel = new SalesSummaryModel
            {
                IsAdmin = _lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowAdminSalesSummary),
                CanMarkDispute = _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.MarkDispute),
                CanFail = _lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.PageFailed),
                CanSuccess = _lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.PageSuccess),
                IsAPIUser = _lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.APIUser,
                IsEndUser = userML.IsEndUser(),
                Report = operation.GetSalesSummary(f, t, m)
            };
            DataTable dataTable = ConverterHelper.O.ToDataTable(salesSummaryModel.Report);
            dataTable.Columns.Remove("_OID");
            dataTable.Columns["_Operator"].ColumnName = "Operator";
            dataTable.Columns["TAmount"].ColumnName = "Debited";
            dataTable.Columns["TAmountR"].ColumnName = "Requested";
            dataTable.Columns["TLoginCom"].ColumnName = "SelfComm";
            dataTable.Columns.Remove("OpSalesSummary");
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("SalesSummary1");
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
                    FileName = "SalesSummary_" + f.Replace(" ", "") + "To" + t.Replace(" ", "") + ".xlsx"
                };
                return File(exportToExcel.Contents, DOCType.XlsxContentType, exportToExcel.FileName);
            }
        }

        [HttpPost]
        [Route("Home/d-tran-detail")]
        [Route("d-tran-detail")]
        public IActionResult _DenominationTransactionDetail(string fd, string td, int u, int o)
        {
            IReportML operation = new ReportML(_accessor, _env);
            var _res = operation.DenominationTransactions(fd, td, u, o);
            return PartialView("PartialView/_DenominationTransaction", _res);
        }

        [HttpGet]
        [Route("Home/USalesSummary")]
        [Route("USalesSummary")]
        public IActionResult USalesSummary()
        {
            var loginResp = chkAlternateSession();
            return View(loginResp.RoleID);
        }

        [HttpPost]
        [Route("Home/u-sales-summary")]
        [Route("u-sales-summary")]
        public async Task<IActionResult> _USalesSummary(string f, string t, string m)
        {
            var loginResp = chkAlternateSession();
            ViewBag.RoleID = loginResp.RoleID;
            IReportML operation = new ReportML(_accessor, _env);
            List<SalesSummaryUserDateWise> _res = await operation.GetAPIUserSalesSummary(f, t, m).ConfigureAwait(false);
            return PartialView("PartialView/_USalesSummary", _res);
        }

        [HttpGet]
        [Route("Home/USalesSummaryDate")]
        [Route("USalesSummaryDate")]
        public IActionResult USalesSummaryDate()
        {
            var loginResp = chkAlternateSession();
            return View(loginResp.RoleID);
        }

        [HttpPost]
        [Route("Home/u-sales-summary-d")]
        [Route("u-sales-summary-d")]
        public async Task<IActionResult> _USalesSummaryDate(string f, string t, string m)
        {
            var loginResp = chkAlternateSession();
            ViewBag.RoleID = loginResp.RoleID;
            IReportML operation = new ReportML(_accessor, _env);
            List<SalesSummaryUserDateWise> _res = await operation.GetAPIUserSalesSummaryDate(f, t, m).ConfigureAwait(false);
            return PartialView("PartialView/_USalesSummaryDate", _res);
        }

        [HttpGet]
        [Route("Home/USalesSummaryR")]
        [Route("USalesSummaryR")]
        public IActionResult USalesSummaryR()
        {
            IUserML userML = new UserML(_accessor, _env);
            IReportML reportML = new ReportML(_accessor, _env);
            var loginResp = chkAlternateSession();
            var salesSummaryModel = new SalesSummaryModel
            {
                IsAdmin = loginResp.RoleID == Role.Admin && loginResp.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowUserSalesSummary),
                UserMobile = loginResp.LoginTypeID == LoginType.ApplicationUser ? loginResp.MobileNo : "AN1",
                UserID = loginResp.LoginTypeID == LoginType.ApplicationUser ? loginResp.UserID : 1,
            };
            var roles = userML.GetChildRole();
            salesSummaryModel.Roles = new SelectList(roles, "ID", "Role");
            return View(salesSummaryModel);
        }

        [HttpPost]
        [Route("Home/u-sales-summaryr")]
        [Route("u-sales-summaryr")]
        public async Task<IActionResult> _USalesSummaryR(string f, string t, int r, int i)
        {
            IUserML userML = new UserML(_accessor, _env);
            IReportML reportML = new ReportML(_accessor, _env);
            var salesSummaryModel = new SalesSummaryModel
            {
                IsAdmin = _lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowUserSalesSummary),
                userRolewiseTransactions = await reportML.GetUSalesSummary(f, t, r, i)
            };
            return PartialView("PartialView/_USalesSummaryR", salesSummaryModel);
        }

        [HttpGet]
        [Route("Home/USalesSummaryRD")]
        [Route("USalesSummaryRD")]
        public IActionResult USalesSummaryRDatewise()
        {
            IUserML userML = new UserML(_accessor, _env);
            IReportML reportML = new ReportML(_accessor, _env);
            var salesSummaryModel = new SalesSummaryModel
            {
                IsAdmin = _lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowUserSalesSummary),
                UserMobile = _lr.LoginTypeID == LoginType.ApplicationUser ? _lr.MobileNo : "AN1",
                UserID = _lr.LoginTypeID == LoginType.ApplicationUser ? _lr.UserID : 1,
            };
            return View(salesSummaryModel);
        }

        [HttpPost]
        [Route("Home/u-sales-summaryrd")]
        [Route("u-sales-summaryrd")]
        public async Task<IActionResult> _USalesSummaryRDate(string f, string t, int r, int i, bool slf)
        {
            IUserML userML = new UserML(_accessor, _env);
            IReportML reportML = new ReportML(_accessor, _env);
            var salesSummaryModel = new SalesSummaryModel
            {
                IsAdmin = _lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowUserSalesSummary),
                userRolewiseTransactions = await reportML.GetUSalesSummaryDate(f, t, r, i, slf).ConfigureAwait(false)
            };
            return PartialView("PartialView/_USalesSummaryRDate", salesSummaryModel);
        }

        [HttpGet]
        [Route("sales-summaryrd-Export")]
        public async Task<IActionResult> _SalesSummaryDExport(string f, string t, int r, int i, bool slf)
        {
            IReportML reportML = new ReportML(_accessor, _env);
            var userRolewiseTransactions = await reportML.GetUSalesSummaryDate(f, t, r, i, slf).ConfigureAwait(false);
            DataTable dataTable = ConverterHelper.O.ToDataTable(userRolewiseTransactions);
            string[] removableCol = { "OID", "Operator", "OpTypeID", "OpType", "UserID", "Prefix", "Name", "MobileNo", "SAName", "SAID", "MDID", "MDName", "DTID", "DTName" };
            var Contents = EportExcel.o.GetFile(dataTable, removableCol);
            return File(Contents, DOCType.XlsxContentType, "USalesSummaryRDate.xlsx");
        }

        [HttpGet]
        [Route("salessummaryRole-Export")]
        public async Task<IActionResult> SalesSummaryRExport(string f, string t, int r, int i)
        {
            IReportML reportML = new ReportML(_accessor, _env);
            var userRolewiseTransactions = await reportML.GetUSalesSummary(f, t, r, i).ConfigureAwait(false);
            DataTable dataTable = ConverterHelper.O.ToDataTable(userRolewiseTransactions);
            string[] removableCol = { "OID", "Operator", "OpTypeID", "OpType", "UserID", "Prefix", "Name", "MobileNo", "SAName", "SAID", "MDID", "MDName", "DTID", "DTName" };
            var Contents = EportExcel.o.GetFile(dataTable, removableCol);
            return File(Contents, DOCType.XlsxContentType, "SalesSummaryRole.xlsx");
        }

        [HttpGet]
        [Route("salessummaryop-Export")]
        public async Task<IActionResult> salessummaryopExport(string f, string t, int i, int o, int Ot)
        {
            IUserML userML = new UserML(_accessor, _env);
            IReportML reportML = new ReportML(_accessor, _env);
            var userRolewiseTransactions = await reportML.GetUSalesSummaryOperator(f, t, i, o, Ot).ConfigureAwait(false);
            DataTable dataTable = ConverterHelper.O.ToDataTable(userRolewiseTransactions);
            string[] removableCol = { "OID", "Operator", "OpTypeID", "OpType", "UserID", "Prefix", "Name", "MobileNo", "SAName", "SAID", "MDID", "MDName", "DTID", "DTName" };
            var Contents = EportExcel.o.GetFile(dataTable, removableCol);
            return File(Contents, DOCType.XlsxContentType, "SalesSummaryOperator.xlsx");
        }

        [HttpGet]
        [Route("salessummaryopd-Export")]
        public async Task<IActionResult> salessummaryopdExport(string f, string t, int i, int o, int Ot)
        {
            IUserML userML = new UserML(_accessor, _env);
            IReportML reportML = new ReportML(_accessor, _env);
            var userRolewiseTransactions = await reportML.GetUSalesSummaryOperator(f, t, i, o, Ot, true).ConfigureAwait(false);
            DataTable dataTable = ConverterHelper.O.ToDataTable(userRolewiseTransactions);
            string[] removableCol = { "OID", "Operator", "OpTypeID", "OpType", "UserID", "Prefix", "Name", "MobileNo", "SAName", "SAID", "MDID", "MDName", "DTID", "DTName" };
            var Contents = EportExcel.o.GetFile(dataTable, removableCol);
            return File(Contents, DOCType.XlsxContentType, "SalesSummaryOperatorDate.xlsx");
        }

        [HttpGet]
        [Route("Home/USalesSummaryOP")]
        [Route("USalesSummaryOP")]
        public IActionResult USalesSummaryOperator()
        {
            IUserML userML = new UserML(_accessor, _env);
            IReportML reportML = new ReportML(_accessor, _env);
            IOperatorML opML = new OperatorML(_accessor, _env);
            var salesSummaryModel = new SalesSummaryModel
            {
                IsAdmin = _lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowUserSalesSummary),
                UserMobile = _lr.LoginTypeID == LoginType.ApplicationUser ? _lr.MobileNo : "AN1",
                UserID = _lr.LoginTypeID == LoginType.ApplicationUser ? _lr.UserID : 1,
                Operators = opML.GetOperators(),
                OpTypes = opML.GetOptypes()
            };
            return View(salesSummaryModel);
        }

        [HttpPost]
        [Route("Home/u-sales-summaryop")]
        [Route("u-sales-summaryop")]
        public async Task<IActionResult> _USalesSummaryOP(string f, string t, int i, int o, int Ot)
        {
            IUserML userML = new UserML(_accessor, _env);
            IReportML reportML = new ReportML(_accessor, _env);
            var salesSummaryModel = new SalesSummaryModel
            {
                IsAdmin = _lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowUserSalesSummary),
                userRolewiseTransactions = await reportML.GetUSalesSummaryOperator(f, t, i, o, Ot).ConfigureAwait(false)
            };
            return PartialView("PartialView/_USalesSummaryOperator", salesSummaryModel);
        }

        [HttpGet]
        [Route("Home/USalesSummaryOPD")]
        [Route("USalesSummaryOPD")]
        public IActionResult USalesSummaryOperatorDate()
        {
            IUserML userML = new UserML(_accessor, _env);
            IReportML reportML = new ReportML(_accessor, _env);
            IOperatorML opML = new OperatorML(_accessor, _env);
            var salesSummaryModel = new SalesSummaryModel
            {
                IsAdmin = _lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowUserSalesSummary),
                UserMobile = _lr.LoginTypeID == LoginType.ApplicationUser ? _lr.MobileNo : "AN1",
                UserID = _lr.LoginTypeID == LoginType.ApplicationUser ? _lr.UserID : 1,
                Operators = opML.GetOperators(),
                OpTypes = opML.GetOptypes()
            };
            return View(salesSummaryModel);
        }
        [HttpPost]
        [Route("Home/u-sales-summaryopd")]
        [Route("u-sales-summaryopd")]
        public async Task<IActionResult> _USalesSummaryOPD(string f, string t, int i, int o, int Ot)
        {
            IUserML userML = new UserML(_accessor, _env);
            IReportML reportML = new ReportML(_accessor, _env);
            var salesSummaryModel = new SalesSummaryModel
            {
                IsAdmin = _lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowUserSalesSummary),
                userRolewiseTransactions = await reportML.GetUSalesSummaryOperator(f, t, i, o, Ot, true)
            };
            return PartialView("PartialView/_USalesSummaryOperatorDate", salesSummaryModel);
        }

        [HttpGet]
        [Route("Home/TransactionOPD")]
        [Route("TransactionOPD")]
        public IActionResult TransactionUOpDateWise()
        {
            return View();
        }

        [HttpPost]
        [Route("Home/_TransactionOPD")]
        [Route("_TransactionOPD")]
        public IActionResult _TransactionUOpDateWise(string f, string t, string m)
        {
            IReportML reportML = new ReportML(_accessor, _env);
            var _res = reportML.GetUOpDateWiseTransaction(f, t, m);
            return PartialView("PartialView/_TransactionUOpDateWise", _res);
        }
        #endregion
    }
}
