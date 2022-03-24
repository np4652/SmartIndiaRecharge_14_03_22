using Fintech.AppCode.HelperClass;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using RoundpayFinTech.AppCode;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.MiddleLayer;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Report;
using RoundpayFinTech.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.Controllers
{
    public partial class ReportController
    {
        #region GSTSummary
        [HttpGet("Home/GSTSummary")]
        [Route("GSTSummary")]
        public IActionResult GSTSummary()
        {
            IUserML userML = new UserML(_lr);
            var IsAdmin = (_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser) || userML.IsCustomerCareAuthorised(ActionCodes.ShowGSTReport);
            return View(IsAdmin);
        }
        [HttpPost("Home/GST-Summary")]
        [Route("GST-Summary")]
        public IActionResult _GSTSummary(string M, string D, int IA, string bModel)
        {
            var gSTReportFilter = new GSTReportFilter
            {
                MobileNo = M,
                GSTMonth = D,
                IsGSTVerified = IA,
                BillingModel = bModel
            };
            IInvoiceReportML operation = new ReportML(_accessor, _env);
            var calculatedGSTEntries = operation.GetGSTSummary(gSTReportFilter);
            return PartialView("PartialView/_GSTSummary", calculatedGSTEntries);
        }
        [HttpGet("Home/TDSSummary")]
        [Route("TDSSummary")]
        public IActionResult TDSSummary()
        {
            IUserML userML = new UserML(_lr);
            var IsAdmin = (_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser) || userML.IsCustomerCareAuthorised(ActionCodes.ShowGSTReport);
            return View(IsAdmin);
        }

        [HttpPost("Home/TDS-Summary")]
        [Route("TDS-Summary")]
        public IActionResult _TDSSummary(string M, string D)
        {
            IInvoiceReportML operation = new ReportML(_accessor, _env);
            var calculatedGSTEntries = operation.GetTDSSummary(new GSTReportFilter
            {
                MobileNo = M,
                GSTMonth = D
            });
            return PartialView("PartialView/_TDSSummary", calculatedGSTEntries);
        }
        [HttpGet]
        [Route("TDS-Summary-Excel")]
        public  IActionResult _TDSSummaryExport(string M, string D)
        {
            IInvoiceReportML operation = new ReportML(_accessor, _env);
            var _report = operation.GetTDSSummary(new GSTReportFilter
            {
                MobileNo = M,
                GSTMonth = D
            });
            DataTable dataTable = ConverterHelper.O.ToDataTable(_report.ToList());
           
            string[] removableColumns = {"InvoiceID","State","GSTIN","IsGSTVerified","RequestedAmount","Amount","GSTTaxAmount","NetAmount","CompanyState","InvoiceMonth","BillingModel","IsHoldGST","ByAdminUser" };
            DataColumnCollection columns = dataTable.Columns;
            foreach (string col in removableColumns)
            {
                if (columns.Contains(col))
                {
                    dataTable.Columns.Remove(col);
                }
            }
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("TDSSummary");
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
                    FileName = "TDSSummary.xlsx"
                };
                return File(exportToExcel.Contents, DOCType.XlsxContentType, exportToExcel.FileName);
            }
        }
        [HttpGet]
        [Route("GST-Summary-Excel")]
        public async Task<IActionResult> _GSTSummaryExport(string M, string D, int IA, string bModel)
        {
            IInvoiceReportML operation = new ReportML(_accessor, _env);
            var gSTReportFilter = new GSTReportFilter
            {
                MobileNo = M,
                GSTMonth = D,
                IsGSTVerified = IA,
                BillingModel = bModel
            };
            var _report = await Task.FromResult(operation.GetGSTSummary(gSTReportFilter));
            DataTable dataTable = ConverterHelper.O.ToDataTable(_report.ToList());
            dataTable.Columns.Add("SGST", typeof(System.Decimal));
            dataTable.Columns.Add("CGST", typeof(System.Decimal));
            dataTable.Columns.Add("IGST", typeof(System.Decimal));
            dataTable.Columns.Add("Total", typeof(System.Decimal));
            foreach (DataRow row in dataTable.Rows)
            {
                row["SGST"] = Math.Round((Convert.ToString(row["State"]) == Convert.ToString(row["CompanyState"]) ? Convert.ToDecimal(row["NetAmount"]) * 0.09M : 0), 2, MidpointRounding.AwayFromZero);
                row["CGST"] = Math.Round((Convert.ToString(row["State"]) == Convert.ToString(row["CompanyState"]) ? Convert.ToDecimal(row["NetAmount"]) * 0.09M : 0), 2, MidpointRounding.AwayFromZero);
                row["IGST"] = Math.Round((Convert.ToString(row["State"]) == Convert.ToString(row["CompanyState"]) ? 0 : Convert.ToDecimal(row["NetAmount"]) * 0.18M), 2, MidpointRounding.AwayFromZero);
                row["Total"] = Math.Round(Convert.ToDecimal(row["NetAmount"]) + Convert.ToDecimal(row["SGST"]) + Convert.ToDecimal(row["CGST"]) + Convert.ToDecimal(row["IGST"]));
            }
            string[] removableColumns = { };
            DataColumnCollection columns = dataTable.Columns;
            foreach (string col in removableColumns)
            {
                if (columns.Contains(col))
                {
                    dataTable.Columns.Remove(col);
                }
            }
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("GSTSummary");
                worksheet.Cells["A1"].LoadFromDataTable(dataTable, PrintHeaders: true);
                worksheet.Row(1).Height = 20;
                worksheet.Row(1).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Row(1).Style.Font.Bold = true;
                int rowindex = 2;
                foreach (DataRow row in dataTable.Rows)
                {
                    if (rowindex == 2)
                    {
                        //worksheet.Cells[1, dataTable.Columns["Type"].Ordinal + 1].Value = "Status";
                        //worksheet.Cells[1, dataTable.Columns["EntryDate"].Ordinal + 1].Value = "RechargeDate";
                    }
                    rowindex++;
                }
                for (var col = 1; col < dataTable.Columns.Count + 1; col++)
                {
                    worksheet.Column(col).AutoFit();
                }
                ExportToExcel exportToExcel = new ExportToExcel
                {
                    Contents = package.GetAsByteArray(),
                    FileName = "GSTSummary.xlsx"
                };
                return File(exportToExcel.Contents, DOCType.XlsxContentType, exportToExcel.FileName);
            }
        }

        [HttpGet("Home/InvoiceList")]
        [Route("InvoiceList")]
        public IActionResult InvoiceList()
        {
            IUserML userML = new UserML(_lr);
            var IsAdmin = (_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser) || userML.IsCustomerCareAuthorised(ActionCodes.ShowGSTReport);
            return View(IsAdmin);
        }
        [HttpGet("Home/P2ASummary")]
        [Route("P2ASummary")]
        public IActionResult P2ASummary()
        {
            IUserML userML = new UserML(_lr);
            var IsAdmin = (_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser) || userML.IsCustomerCareAuthorised(ActionCodes.ShowGSTReport);
            return View(IsAdmin);
        }
        [HttpPost("Home/GST-SummaryP2A")]
        [Route("GST-SummaryP2A")]
        public IActionResult _P2ASummary(string M, string D)
        {
            var gSTReportFilter = new GSTReportFilter
            {
                MobileNo = M,
                GSTMonth = D,
                BillingModel = "P2A"
            };
            IInvoiceReportML operation = new ReportML(_accessor, _env);
            var calculatedGSTEntries = operation.GetGSTSummary(gSTReportFilter);
            return PartialView("PartialView/_P2ASummary", calculatedGSTEntries);
        }
        [HttpGet]
        [Route("GST-SummaryP2A-Excel")]
        public async Task<IActionResult> _GSTSummaryP2AExport(string M, string D)
        {
            IInvoiceReportML operation = new ReportML(_accessor, _env);
            var gSTReportFilter = new GSTReportFilter
            {
                MobileNo = M,
                GSTMonth = D,
                BillingModel = "P2A"
            };
            var _report = await Task.FromResult(operation.GetGSTSummary(gSTReportFilter));
            DataTable dataTable = ConverterHelper.O.ToDataTable(_report.ToList());
            dataTable.Columns.Add("SGST", typeof(System.Decimal));
            dataTable.Columns.Add("CGST", typeof(System.Decimal));
            dataTable.Columns.Add("IGST", typeof(System.Decimal));
            dataTable.Columns.Add("Total", typeof(System.Decimal));
            foreach (DataRow row in dataTable.Rows)
            {
                row["SGST"] = Math.Round((Convert.ToString(row["State"]) == Convert.ToString(row["CompanyState"]) ? Convert.ToDecimal(row["NetAmount"]) * 0.09M : 0), 2, MidpointRounding.AwayFromZero);
                row["CGST"] = Math.Round((Convert.ToString(row["State"]) == Convert.ToString(row["CompanyState"]) ? Convert.ToDecimal(row["NetAmount"]) * 0.09M : 0), 2, MidpointRounding.AwayFromZero);
                row["IGST"] = Math.Round((Convert.ToString(row["State"]) == Convert.ToString(row["CompanyState"]) ? 0 : Convert.ToDecimal(row["NetAmount"]) * 0.18M), 2, MidpointRounding.AwayFromZero);
                row["Total"] = Math.Round(Convert.ToDecimal(row["NetAmount"]) + Convert.ToDecimal(row["SGST"]) + Convert.ToDecimal(row["CGST"]) + Convert.ToDecimal(row["IGST"]));
            }
            string[] removableColumns = { };
            DataColumnCollection columns = dataTable.Columns;
            foreach (string col in removableColumns)
            {
                if (columns.Contains(col))
                {
                    dataTable.Columns.Remove(col);
                }
            }
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("GSTSummaryP2A");
                worksheet.Cells["A1"].LoadFromDataTable(dataTable, PrintHeaders: true);
                worksheet.Row(1).Height = 20;
                worksheet.Row(1).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Row(1).Style.Font.Bold = true;
                int rowindex = 2;
                foreach (DataRow row in dataTable.Rows)
                {
                    if (rowindex == 2)
                    {
                        //worksheet.Cells[1, dataTable.Columns["Type"].Ordinal + 1].Value = "Status";
                        //worksheet.Cells[1, dataTable.Columns["EntryDate"].Ordinal + 1].Value = "RechargeDate";
                    }
                    rowindex++;
                }
                for (var col = 1; col < dataTable.Columns.Count + 1; col++)
                {
                    worksheet.Column(col).AutoFit();
                }
                ExportToExcel exportToExcel = new ExportToExcel
                {
                    Contents = package.GetAsByteArray(),
                    FileName = "GSTSummaryP2A.xlsx"
                };
                return File(exportToExcel.Contents, DOCType.XlsxContentType, exportToExcel.FileName);
            }
        }

        [HttpGet("Home/SurchargeSummary")]
        [Route("SurchargeSummary")]
        public IActionResult SurSummary()
        {
            IUserML userML = new UserML(_lr);
            var IsAdmin = (_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser) || userML.IsCustomerCareAuthorised(ActionCodes.ShowGSTReport);
            return View(IsAdmin);
        }
        [HttpPost("Home/GST-SummarySUR")]
        [Route("GST-SummarySUR")]
        public IActionResult _SurSummary(string M, string D)
        {
            var gSTReportFilter = new GSTReportFilter
            {
                MobileNo = M,
                GSTMonth = D,
                BillingModel = "SUR"
            };
            IInvoiceReportML operation = new ReportML(_accessor, _env);
            var calculatedGSTEntries = operation.GetGSTSummary(gSTReportFilter);
            return PartialView("PartialView/_SURSummary", calculatedGSTEntries);
        }

        [HttpGet]
        [Route("GST-SummarySUR-Excel")]
        public async Task<IActionResult> _GSTSurSummaryExport(string M, string D)
        {
            IInvoiceReportML operation = new ReportML(_accessor, _env);
            var gSTReportFilter = new GSTReportFilter
            {
                MobileNo = M,
                GSTMonth = D,
                BillingModel = "SUR"
            };
            var _report = await Task.FromResult(operation.GetGSTSummary(gSTReportFilter));
            DataTable dataTable = ConverterHelper.O.ToDataTable(_report.ToList());
            dataTable.Columns.Add("SGST", typeof(System.Decimal));
            dataTable.Columns.Add("CGST", typeof(System.Decimal));
            dataTable.Columns.Add("IGST", typeof(System.Decimal));
            dataTable.Columns.Add("Total", typeof(System.Decimal));
            foreach (DataRow row in dataTable.Rows)
            {
                row["SGST"] = Math.Round((Convert.ToString(row["State"]) == Convert.ToString(row["CompanyState"]) ? Convert.ToDecimal(row["NetAmount"]) * 0.09M : 0), 2, MidpointRounding.AwayFromZero);
                row["CGST"] = Math.Round((Convert.ToString(row["State"]) == Convert.ToString(row["CompanyState"]) ? Convert.ToDecimal(row["NetAmount"]) * 0.09M : 0), 2, MidpointRounding.AwayFromZero);
                row["IGST"] = Math.Round((Convert.ToString(row["State"]) == Convert.ToString(row["CompanyState"]) ? 0 : Convert.ToDecimal(row["NetAmount"]) * 0.18M), 2, MidpointRounding.AwayFromZero);
                row["Total"] = Math.Round(Convert.ToDecimal(row["NetAmount"]) + Convert.ToDecimal(row["SGST"]) + Convert.ToDecimal(row["CGST"]) + Convert.ToDecimal(row["IGST"]));
            }
            string[] removableColumns = { };
            DataColumnCollection columns = dataTable.Columns;
            foreach (string col in removableColumns)
            {
                if (columns.Contains(col))
                {
                    dataTable.Columns.Remove(col);
                }
            }
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("GSTSummaryP2A");
                worksheet.Cells["A1"].LoadFromDataTable(dataTable, PrintHeaders: true);
                worksheet.Row(1).Height = 20;
                worksheet.Row(1).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Row(1).Style.Font.Bold = true;
                int rowindex = 2;
                foreach (DataRow row in dataTable.Rows)
                {
                    if (rowindex == 2)
                    {
                        //worksheet.Cells[1, dataTable.Columns["Type"].Ordinal + 1].Value = "Status";
                        //worksheet.Cells[1, dataTable.Columns["EntryDate"].Ordinal + 1].Value = "RechargeDate";
                    }
                    rowindex++;
                }
                for (var col = 1; col < dataTable.Columns.Count + 1; col++)
                {
                    worksheet.Column(col).AutoFit();
                }
                ExportToExcel exportToExcel = new ExportToExcel
                {
                    Contents = package.GetAsByteArray(),
                    FileName = "GSTSummaryP2A.xlsx"
                };
                return File(exportToExcel.Contents, DOCType.XlsxContentType, exportToExcel.FileName);
            }
        }

        [HttpPost("Home/invoice-list")]
        [Route("invoice-list")]
        public IActionResult _InvoiceList(string M)
        {
            IUserML userML = new UserML(_lr);
            var model = new InvoiceListModel
            {
                IsAdmin = (_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser) || userML.IsCustomerCareAuthorised(ActionCodes.ShowGSTReport),
                MobileNo = M
            };
            IInvoiceReportML reportML = new ReportML(_accessor, _env);
            model.invoiceDetails = reportML.GetInvoiceMonths(M);
            return PartialView("PartialView/_InvoiceList", model);
        }
        [HttpPost]
        [Route("Home/InvoiceSetting")]
        [Route("InvoiceSetting")]
        public IActionResult _InvoiceSetting()
        {
            IInvoiceReportML reportML = new ReportML(_accessor, _env);
            var list = reportML.InvoiceSettings(false, 0, 0);
            return PartialView("PartialView/_InvoiceSetting", list);
        }
        [HttpPost]
        [Route("Home/update-invoice-status")]
        [Route("update-invoice-status")]
        public IActionResult UpdateInvoiceStatus(bool s, int i)
        {
            IInvoiceReportML reportML = new ReportML(_accessor, _env);
            var _res = reportML.InvoiceSettings(s, i, 1);
            IResponseStatus astatus = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = "An error has been occured!"
            };
            if (_res.Count() > 0)
            {
                astatus.Statuscode = _res[0].StatusCode;
                astatus.Msg = _res[0].Msg;
            }
            return Json(astatus);
        }
        #endregion
    }
}
