using Fintech.AppCode.HelperClass;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using RoundpayFinTech.AppCode;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.MiddleLayer;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Report;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.Controllers
{
    public partial class ReportController
    {
        #region Daybook
        [HttpGet]
        [Route("Home/Daybook")]
        [Route("Daybook")]
        public async Task<IActionResult> Daybook()
        {
            IUserML userML = new UserML(_lr);
            if (_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowAdminDaybook))
            {
                IOperatorML operation = new OperatorML(_accessor, _env);
                IEnumerable<OperatorDetail> oplist = await operation.GetOPListBYServices(ServiceType.DayBookReportServices);
                ViewBag.OPs = oplist;

                IAPIML aPIML = new APIML(_accessor, _env);
                ViewBag.APIs = aPIML.GetAPIDetail();
                return View();
            }
            return Ok();
        }
        [HttpPost]
        [Route("Home/day-book")]
        [Route("day-book")]
        public IActionResult _Daybook(string FromDate, string ToDate, int a, int o, string Mobile_F)
        {
            IReportML operation = new ReportML(_accessor, _env);
            List<Daybook> _res = operation.AdminDayBook(FromDate, ToDate, a, o, Mobile_F);
            return PartialView("PartialView/_Daybook", _res);
        }
        [HttpGet]
        [Route("Home/DaybookDatewise")]
        [Route("DaybookDatewise")]
        public IActionResult DaybookDatewise()
        {
            IUserML userML = new UserML(_lr);
            if (_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowAdminDaybook))
            {
                return View();
            }
            return Ok();
        }
        [HttpPost]
        [Route("Home/day-book-d")]
        [Route("day-book-d")]
        public IActionResult _DaybookD(string FromDate, string ToDate)
        {
            IReportML operation = new ReportML(_accessor, _env);
            var _res = operation.AdminDayBookDaywise(FromDate, ToDate);
            return PartialView("PartialView/_DaybookD", _res);
        }
        [HttpGet]
        [Route("Home/DaybookAPIDatewise")]
        [Route("DaybookAPIDatewise")]
        public IActionResult DaybookAPIDatewise()
        {
            IUserML userML = new UserML(_lr);
            if (_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowAdminDaybook))
            {
                IAPIML aPIML = new APIML(_accessor, _env);
                return View(aPIML.GetAPIDetail());
            }
            return Ok();
        }
        [HttpPost]
        [Route("Home/Daybook-APIDatewise")]
        [Route("Daybook-APIDatewise")]
        public IActionResult _DaybookAPIDatewise(string FromDate, string ToDate, int a)
        {
            IReportML operation = new ReportML(_accessor, _env);
            IEnumerable<APIDaybookDatewise> _res = operation.AdminDayBookDateAPIwise(FromDate, ToDate, a);
            return PartialView("PartialView/_DaybookAPIDatewise", _res);
        }

        [HttpGet]
        [Route("Home/DaybookDateAPIwise")]
        [Route("DaybookDateAPIwise")]
        public IActionResult DaybookDateAPIwise()
        {
            IUserML userML = new UserML(_lr);
            if (_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowAdminDaybook))
            {
                IAPIML aPIML = new APIML(_accessor, _env);
                return View(aPIML.GetAPIDetail());
            }
            return Ok();
        }
        [HttpPost]
        [Route("Home/Daybook-DateAPIwise")]
        [Route("Daybook-DateAPIwise")]
        public IActionResult _DaybookDateAPIwise(string FromDate, string ToDate, int a)
        {
            IReportML operation = new ReportML(_accessor, _env);
            IEnumerable<APIDaybookDatewise> _res = operation.AdminDayBookDateAPIwiseNew(FromDate, ToDate, a);
            return PartialView("PartialView/_DaybookDateAPIwise", _res);
        }

        [HttpGet]
        [Route("Home/AdminDaybookDMR")]
        [Route("AdminDaybookDMR")]
        public IActionResult DaybookAdminDMR()
        {
            IUserML userML = new UserML(_lr);
            if (_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowAdminDaybook))
            {
                return View();
            }
            return Ok();
        }
        [HttpPost]
        [Route("Home/day-book-dmr")]
        [Route("day-book-dmr")]
        public IActionResult _AdminDaybookDMR(string FromDate, string ToDate)
        {
            IReportML operation = new ReportML(_accessor, _env);
            var _res = operation.AdminDayBookDMR(FromDate, ToDate);
            return PartialView("PartialView/_AdminDaybookDMR", _res);
        }
        [HttpGet]
        [Route("Home/UDaybook/{MobileNo}")]
        [Route("UDaybook/{MobileNo}")]
        public IActionResult UserDaybook(string MobileNo)
        {
            IUserML userML = new UserML(_lr);
            var IsEndUser = userML.IsEndUser();
            ViewBag.MobileNo = MobileNo ?? "";
            return View(IsEndUser);
        }
        [HttpGet]
        [Route("Home/UDaybook")]
        [Route("UDaybook")]
        public IActionResult UserDaybook()
        {
            IUserML userML = new UserML(_lr);
            var IsEndUser = userML.IsEndUser();
            ViewBag.MobileNo = "";
            return View(IsEndUser);
        }
        [HttpPost]
        [Route("Home/u-day-book")]
        [Route("u-day-book")]
        public IActionResult _UserDaybook(string fd, string td, string m)
        {
            IReportML operation = new ReportML(_accessor, _env);
            var _res = operation.UserDaybook(fd, td, m);
            IUserML userML = new UserML(_lr);
            ViewBag.IsEndUser = userML.IsEndUser();
            return PartialView("PartialView/_UserDaybook", _res);
        }
        [HttpPost]
        [Route("Home/u-day-book-I")]
        [Route("u-day-book-I")]
        public IActionResult _UserDaybookIncentive(string fd, string td, string m, int i)
        {
            IReportML operation = new ReportML(_accessor, _env);
            var _res = operation.UserDaybookIncentive(fd, td, m, i);
            return PartialView("PartialView/_UserDaybookIncentive", _res);
        }
        [HttpPost]
        [Route("Home/u-day-book-C")]
        [Route("u-day-book-C")]
        public IActionResult _UserDaybookCircleIncentive(string fd, string td, string m, int i)
        {
            IReportML operation = new ReportML(_accessor, _env);
            var _res = operation.CircleIncentive(fd, td, m, i);
            return PartialView("PartialView/_UserDaybookIncentiveCircle", _res);
        }

        [HttpGet]
        [Route("Home/u-day-book")]
        [Route("u-day-book")]
        public IActionResult _UserDaybookExport(string fd, string td, string m)
        {
            IReportML operation = new ReportML(_accessor, _env);
            var daybooks = operation.UserDaybook(fd, td, m);

            DataTable dataTable = ConverterHelper.O.ToDataTable(daybooks);
            dataTable.Columns.Remove("API");
            dataTable.Columns.Remove("OID");
            dataTable.Columns.Remove("APICommission");
            dataTable.Columns.Remove("TDSAmount");
            dataTable.Columns.Remove("GSTTaxAmount");
            dataTable.Columns.Remove("Profit");
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("UserDaybook1");
                worksheet.Cells["A1"].LoadFromDataTable(dataTable, PrintHeaders: true);

                for (var col = 1; col < dataTable.Columns.Count + 1; col++)
                {
                    worksheet.Column(col).AutoFit();
                }
                ExportToExcel exportToExcel = new ExportToExcel
                {
                    Contents = package.GetAsByteArray(),
                    FileName = "UserDaybook.xlsx"
                };
                return File(exportToExcel.Contents, DOCType.XlsxContentType, exportToExcel.FileName);
            }
        }

        [HttpGet]
        [Route("Home/UDaybookDMR")]
        [Route("UDaybookDMR")]
        public IActionResult UserDaybookDMR()
        {
            IUserML userML = new UserML(_lr);
            var IsEndUser = userML.IsEndUser();
            ViewBag.MobileNo = "";
            return View(IsEndUser);
        }
        [HttpPost]
        [Route("Home/u-day-book-dmr")]
        [Route("u-day-book-dmr")]
        public IActionResult _UserDaybookDMR(string fd, string td, string m)
        {
            IReportML ml = new ReportML(_accessor, _env);
            var _res = ml.UserDaybookDMR(fd, td, m);
            IUserML userML = new UserML(_lr);
            ViewBag.IsEndUser = userML.IsEndUser();
            return PartialView("PartialView/_UserDaybookDMR", _res);
        }

        [HttpGet]
        [Route("DaybookAPIDatewise-Export")]
        public IActionResult DaybookAPIDatewiseExport(string FromDate, string ToDate, int a)
        {
            IReportML operation = new ReportML(_accessor, _env);
            var result = operation.AdminDayBookDateAPIwise(FromDate, ToDate, a);
            DataTable dataTable = ConverterHelper.O.ToDataTable(result.ToList());
            string[] removableCol = { "EntryDate", "Daybooks","API" };
            var Contents = EportExcel.o.GetFile(dataTable, removableCol);
            return File(Contents, DOCType.XlsxContentType, "DaybookAPIDatewise.xlsx");
        }
        [HttpGet]
        [Route("Daybook-DateAPIwise-Export")]
        public IActionResult DaybookDateAPIwiseExport(string FromDate, string ToDate, int a)
        {
            IReportML operation = new ReportML(_accessor, _env);
            var result = operation.AdminDayBookDateAPIwiseNew(FromDate, ToDate, a);
            DataTable dataTable = ConverterHelper.O.ToDataTable(result.ToList());
            string[] removableCol = { "EntryDate", "Daybooks", };
            var Contents = EportExcel.o.GetFile(dataTable, removableCol);
            return File(Contents, DOCType.XlsxContentType, "DaybookAPIDatewise.xlsx");
        }
        #endregion
    }
}
