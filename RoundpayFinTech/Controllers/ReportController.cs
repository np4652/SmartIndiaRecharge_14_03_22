using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fintech.AppCode;
using Fintech.AppCode.Configuration;
using Fintech.AppCode.DB;
using Fintech.AppCode.HelperClass;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.Model.Reports;
using Fintech.AppCode.StaticModel;
using GoogleAuthenticatorService.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using OfficeOpenXml;
using QRCoder;
using RoundpayFinTech.AppCode;
using RoundpayFinTech.AppCode.Classes;
using RoundpayFinTech.AppCode.Configuration;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.HelperClass;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.MiddleLayer;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.Paymentgateway;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Report;
using RoundpayFinTech.AppCode.Model.Reports.Filter;
using RoundpayFinTech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.ThirdParty.HotelAPI;
using RoundpayFinTech.Models;
using Syncfusion.HtmlConverter;
using Syncfusion.Pdf;

using Validators;

namespace RoundpayFinTech.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public partial class ReportController : Controller
    {
        #region Global Variables
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly ISession _session;
        private readonly LoginResponse _lr;
        private readonly ILoginML loginML;
        private readonly LoginResponse _lrEmp;
        private readonly IUserML userML;
        private readonly IDAL _dal;
        private readonly IConnectionConfiguration _c;
        #endregion

        public ReportController(IHttpContextAccessor accessor, IHostingEnvironment env)
        {
            _accessor = accessor;
            _env = env;
            _c = new ConnectionConfiguration(_accessor, _env);
            _dal = new DAL(_c.GetConnectionString());
            _session = _accessor.HttpContext.Session;
            _lr = _session.GetObjectFromJson<LoginResponse>(SessionKeys.LoginResponse);
            loginML = new LoginML(_accessor, _env);
            _lrEmp = _session.GetObjectFromJson<LoginResponse>(SessionKeys.LoginResponseEmp);
            userML = new UserML(_lr);
        }

        #region SendSMSReport
        [HttpGet]
        [Route("Home/SentSMSReport")]
        [Route("SentSMSReport")]
        public IActionResult SentSMSReport()
        {
            return View();
        }
        [HttpPost]
        [Route("Home/SentSMS-Report")]
        [Route("SentSMS-Report")]
        public IActionResult SentSMSReport([FromBody] SentSMSRequest req)
        {
            IReportML rmL = new ReportML(_accessor, _env);
            var list = rmL.GetSentSMSreport(req);
            return PartialView("PartialView/_SentSMSReport", list);
        }
        #endregion

        #region DMRReportSection
        #region DMRTransactionReport
        [HttpGet]
        [Route("Home/DMRReport")]
        [Route("DMRReport")]
        public async Task<IActionResult> DMRTransactionReport()
        {

            IOperatorML opML = new OperatorML(_accessor, _env);
            var loginResp = chkAlternateSession();
            IUserML userML = new UserML(loginResp);
            var opTypes = opML.GetOptypes();
            IBankML bankML = new BankML(_accessor, _env);
            IAPIML aPIML = new APIML(_accessor, _env);
            var rechApi = aPIML.GetAPIDetail();
            var rep = new DMRReportModel
            {
                RechargeAPI = rechApi.Where(x => x.IsDMT),
                OpTypes = opTypes.Where(x => x.ServiceTypeID.ToString().In(ServiceType.DMRReportServices.Split(',')) && !x.ID.In(79, 78, 77, 68, 67)),
                Banks = bankML.DMRBanks(),
                Operators = await opML.GetOPListBYServices(ServiceType.DMRReportServices).ConfigureAwait(false),
                IsAdmin = loginResp.RoleID == Role.Admin && loginResp.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowRechargeReport),
                IsAPIUser = loginResp.LoginTypeID == LoginType.ApplicationUser && loginResp.RoleID == Role.APIUser,
                IsEndUser = userML.IsEndUser()
            };
            return View(rep);
        }


        [HttpPost]
        [Route("Home/DMR-Report")]
        [Route("DMR-Report")]
        public async Task<IActionResult> _DMRTransactionReport([FromBody] RechargeReportFilter filter)
        {
            var loginResp = chkAlternateSession();
            IUserML userML = new UserML(loginResp);
            IDMRReportML dmlML = new ReportML(_accessor, _env);
            var dMRReportModel = new DMRReportModel
            {
                CanMarkDispute = loginResp.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.MarkDispute),
                CanFail = loginResp.RoleID == Role.Admin && loginResp.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.PageFailed),
                CanSuccess = loginResp.RoleID == Role.Admin && loginResp.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.PageSuccess),
                IsAdmin = loginResp.RoleID == Role.Admin && loginResp.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowDMTReport),
                IsAPIUser = loginResp.LoginTypeID == LoginType.ApplicationUser && loginResp.RoleID == Role.APIUser,
                IsEndUser = userML.IsEndUser(),
                Report = await dmlML.GetDMRReport(filter).ConfigureAwait(false)
            };
            return PartialView("PartialView/_DMRTransactionReport", dMRReportModel);
        }
        [HttpGet]
        [Route("Home/DMR-Report")]
        [Route("DMR-Report")]
        public async Task<IActionResult> _DMRTransactionReportExport(RechargeReportFilter filter)
        {
            IUserML userML = new UserML(_accessor, _env);
            IDMRReportML ml = new ReportML(_accessor, _env);
            filter.IsExport = true;
            var IsAdmin = _lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowDMTReport);
            List<DMRReportResponse> _reportRole;
            List<ProcDMRTransactionResponse> _report;

            _report = await ml.GetDMRReport(filter).ConfigureAwait(false);
            DataTable dataTable = ConverterHelper.O.ToDataTable(_report);

            dataTable.Columns["TransactionID"].SetOrdinal(19);
            dataTable.Columns["SenderMobile"].SetOrdinal(21);
            dataTable.Columns["Optional1"].SetOrdinal(22);
            dataTable.Columns["Opening"].SetOrdinal(25);
            dataTable.Columns["CCF"].SetOrdinal(28);
            dataTable.Columns["Surcharge"].SetOrdinal(28);
            dataTable.Columns["RefundGST"].SetOrdinal(29);
            dataTable.Columns["AmtWithTDS"].SetOrdinal(30);
            dataTable.Columns["Credited_Amount"].SetOrdinal(31);
            dataTable.Columns.Remove("ResultCode");
            dataTable.Columns.Remove("Msg");
            dataTable.Columns.Remove("UserID");
            dataTable.Columns.Remove("OID");
            dataTable.Columns.Remove("_Type");
            dataTable.Columns.Remove("OutletUserMobile");
            dataTable.Columns.Remove("OutletUserCompany");
            dataTable.Columns.Remove("GroupID");
            dataTable.Columns.Remove("Display1");
            dataTable.Columns.Remove("Display2");
            dataTable.Columns.Remove("Display3");
            dataTable.Columns.Remove("Display4");
            dataTable.Columns.Remove("Prefix");
            dataTable.Columns.Remove("Role");
            dataTable.Columns.Remove("GSTAmount");
            dataTable.Columns.Remove("TDSAmount");
            dataTable.Columns.Remove("CCMobile");
            dataTable.Columns.Remove("APICode");
            dataTable.Columns.Remove("ExtraParam");
            dataTable.Columns.Remove("O9");
            dataTable.Columns.Remove("O10");
            dataTable.Columns.Remove("O11");
            dataTable.Columns.Remove("RequestModeID");
            dataTable.Columns.Remove("SwitchingName");
            dataTable.Columns.Remove("SwitchingID");
            dataTable.Columns.Remove("CircleName");
            dataTable.Columns.Remove("CustomerNo");
            dataTable.Columns.Remove("_ServiceID");
            dataTable.Columns.Remove("SlabCommType");
            dataTable.Columns.Remove("LastBalance");
            if (!IsAdmin)
            {
                dataTable.Columns.Remove("API");
                dataTable.Columns.Remove("VendorID");
                dataTable.Columns.Remove("TID");
            }

            dataTable.Columns.Remove("Optional2");
            dataTable.Columns.Remove("Optional3");
            dataTable.Columns.Remove("Optional4");
            dataTable.Columns.Remove("RefundStatus");
            dataTable.Columns.Remove("RefundStatus_");
            dataTable.Columns.Remove("IsWTR");
            dataTable.Columns.Remove("ModifyDate");
            dataTable.Columns.Remove("CommType");
            dataTable.Columns["Balance"].ColumnName = "ClosingBalance";
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("RechargeReport1");
                worksheet.Cells["A1"].LoadFromDataTable(dataTable, PrintHeaders: true);
                worksheet.Row(1).Height = 20;
                worksheet.Row(1).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Row(1).Style.Font.Bold = true;
                int rowindex = 2;
                foreach (DataRow row in dataTable.Rows)
                {
                    if (rowindex == 2)
                    {
                        worksheet.Cells[1, dataTable.Columns["Type_"].Ordinal + 1].Value = "Status";
                        worksheet.Cells[1, dataTable.Columns["EntryDate"].Ordinal + 1].Value = "RechargeDate";
                    }
                    var Type_ = worksheet.Cells[rowindex, dataTable.Columns["Type_"].Ordinal + 1].Value;
                    if (Type_.ToString() == RechargeRespType._SUCCESS)
                    {
                        worksheet.Cells[rowindex, dataTable.Columns["Type_"].Ordinal + 1].Style.Font.Color.SetColor(Color.Green);
                    }
                    else
                    if (Type_.ToString() == RechargeRespType._FAILED)
                    {
                        worksheet.Cells[rowindex, dataTable.Columns["Type_"].Ordinal + 1].Style.Font.Color.SetColor(Color.Red);
                    }
                    else
                    {
                        worksheet.Cells[rowindex, dataTable.Columns["Type_"].Ordinal + 1].Style.Font.Color.SetColor(Color.Gray);
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
                    FileName = "DMTReport.xlsx"
                };
                return File(exportToExcel.Contents, DOCType.XlsxContentType, exportToExcel.FileName);
            }
        }
        [HttpGet]
        [Route("DMRReceipt")]
        public IActionResult DMRReceipt()
        {
            return View();
        }
        [HttpPost]
        [Route("DMR-Receipt")]
        public IActionResult _DMRReceipt(string GroupID, bool IsInvoice, decimal convenientFee)
        {
            var dmr = new SellerML(_accessor, _env);
            var dmrRec = dmr.DMRReceipt(GroupID, convenientFee);
            //ViewBag.IsInvoice = IsInvoice;
            dmrRec.IsInvoice = IsInvoice;
            if (IsInvoice)
            {
                return PartialView("PartialView/_DMRReceipt", dmrRec);
            }
            else
            {
                return PartialView("PartialView/_DMRReceiptPrint", dmrRec);
            }
        }
        [HttpGet]
        [Route("TransactionReceipt")]
        public IActionResult TransactionReceipt()
        {
            return View();
        }

        [HttpPost]
        [Route("Transaction-Receipt")]
        public IActionResult _TransactionReceipt(string TransactionID, int TID, bool IsInvoice, decimal convenientFee)

        {
            var dmr = new SellerML(_accessor, _env);
            var dmrRec = dmr.TransactionReceipt(TID, TransactionID, convenientFee);
            dmrRec.IsInvoice = IsInvoice;
            if (IsInvoice)
            {
                return PartialView("PartialView/_TransactionReceipt", dmrRec);
            }
            else
            {
                return PartialView("PartialView/_TransactionReceiptPrint", dmrRec);
            }
        }
        #endregion
        #region DMRPending
        [HttpGet]
        [Route("Home/DMRPendings")]
        [Route("DMRPendings")]
        public IActionResult DMRPendingTransaction()
        {
            return View();
        }
        [HttpPost]
        [Route("Home/dmrpending")]
        [Route("dmrpending")]
        public async Task<IActionResult> _DMRPendingTransaction()
        {
            IReportML rml = new ReportML(_accessor, _env);
            IUserML userML = new UserML(_lr);
            var pendingTransactionModel = new PendingTransactionModel
            {
                CanSuccess = userML.IsCustomerCareAuthorised(ActionCodes.PageSuccess) || (_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.Admin),
                CanFail = userML.IsCustomerCareAuthorised(ActionCodes.PageFailed) || (_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.Admin),
                Report = await rml.PendingTransaction(0, 0, ReportType.DMR).ConfigureAwait(false)
            };
            return PartialView("PartialView/_DMRPendingTransaction", pendingTransactionModel);
        }
        #endregion
        #endregion

        #region AEPS

        #region AEPSReportSection
        [HttpGet]
        [Route("Home/AEPSReport")]
        [Route("AEPSReport")]
        public IActionResult AEPSReport()
        {
            IUserML userML = new UserML(_lr);
            IOperatorML opML = new OperatorML(_accessor, _env);
            var opTypes = opML.GetOptypes();
            IAPIML aPIML = new APIML(_accessor, _env);
            var rechApi = aPIML.GetAPIDetail();
            var rep = new RechargeReportModel
            {
                RechargeAPI = rechApi.Where(x => x.IsAEPS),
                OpTypes = opTypes.Where(x => x.ServiceTypeID.In(ServiceType.AEPS, ServiceType.MiniBank)),
                IsAdmin = _lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowRechargeReport),
                IsAPIUser = _lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.APIUser,
                Operators = opML.GetOPListBYServices(ServiceType.AEPSResportService).Result,
                IsEndUser = userML.IsEndUser()
            };
            return View(rep);
        }
        [HttpGet]
        [Route("Home/AEPS-Report")]
        [Route("AEPS-Report")]
        public async Task<IActionResult> _AEPSReportExport(RechargeReportFilter filter)
        {
            IUserML userML = new UserML(_accessor, _env);
            IRechargeReportML ml = new ReportML(_accessor, _env);
            filter.IsExport = true;
            var IsAdmin = _lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowAEPSReport);
            List<ProcRechargeReportResponse> _report;
            if (ApplicationSetting.IsRoleFixed)
            {
                _report = await ml.GetAEPSReport(filter).ConfigureAwait(false);
                DataTable dataTable = ConverterHelper.O.ToDataTable(_report);
                dataTable.Columns.Remove("ResultCode");
                dataTable.Columns.Remove("Msg");

                dataTable.Columns.Remove("UserID");
                dataTable.Columns.Remove("OID");
                dataTable.Columns.Remove("_Type");

                if (!IsAdmin)
                {
                    dataTable.Columns.Remove("TID");
                    dataTable.Columns.Remove("API");
                    dataTable.Columns.Remove("VendorID");
                    dataTable.Columns.Remove("CCName");
                    dataTable.Columns.Remove("CCMobile");
                }
                else
                {
                    dataTable.Columns.Remove("TransactionID");
                }
                dataTable.Columns.Remove("SwitchingName");
                dataTable.Columns.Remove("CircleName");
                dataTable.Columns.Remove("CustomerNo");
                dataTable.Columns.Remove("APICode");
                dataTable.Columns.Remove("RequestMode");
                dataTable.Columns.Remove("Optional1");
                dataTable.Columns.Remove("Optional2");
                dataTable.Columns.Remove("Optional3");
                dataTable.Columns.Remove("Optional4");
                dataTable.Columns.Remove("Display1");
                dataTable.Columns.Remove("Display2");
                dataTable.Columns.Remove("Display3");
                dataTable.Columns.Remove("Display4");
                dataTable.Columns.Remove("Prefix");
                dataTable.Columns.Remove("Role");
                dataTable.Columns.Remove("GSTAmount");
                dataTable.Columns.Remove("TDSAmount");

                dataTable.Columns.Remove("RefundStatus");
                dataTable.Columns.Remove("RefundStatus_");
                dataTable.Columns.Remove("IsWTR");
                dataTable.Columns.Remove("ModifyDate");
                dataTable.Columns.Remove("CommType");
                dataTable.Columns["LastBalance"].ColumnName = "OpeningBalance";
                dataTable.Columns["Balance"].ColumnName = "ClosingBalance";
                dataTable.Columns["Amount"].ColumnName = "Credit";
                dataTable.Columns["RequestedAmount"].ColumnName = "Amount";
                bool IsRemoveSA = _lr.RoleID.In(FixedRole.MasterDistributer, FixedRole.Distributor, FixedRole.Retailor, FixedRole.APIUser);
                bool IsRemoveMD = _lr.RoleID.In(FixedRole.Distributor, FixedRole.Retailor, FixedRole.APIUser);
                bool IsRemoveDT = _lr.RoleID.In(FixedRole.Retailor, FixedRole.APIUser);
                if (!_lr.RoleID.In(FixedRole.APIUser, FixedRole.Admin))
                {
                    dataTable.Columns.Remove("ApiRequestID");
                }
                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("AEPSReport");
                    worksheet.Cells["A1"].LoadFromDataTable(dataTable, PrintHeaders: true);
                    worksheet.Row(1).Height = 20;
                    worksheet.Row(1).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    worksheet.Row(1).Style.Font.Bold = true;
                    int rowindex = 2;
                    foreach (DataRow row in dataTable.Rows)
                    {
                        if (rowindex == 2)
                        {
                            worksheet.Cells[1, dataTable.Columns["Type_"].Ordinal + 1].Value = "Status";
                            worksheet.Cells[1, dataTable.Columns["EntryDate"].Ordinal + 1].Value = "RechargeDate";
                        }
                        var Type_ = worksheet.Cells[rowindex, dataTable.Columns["Type_"].Ordinal + 1].Value;
                        if (Type_.ToString() == RechargeRespType._SUCCESS)
                        {
                            worksheet.Cells[rowindex, dataTable.Columns["Type_"].Ordinal + 1].Style.Font.Color.SetColor(Color.Green);
                        }
                        else
                        if (Type_.ToString() == RechargeRespType._FAILED)
                        {
                            worksheet.Cells[rowindex, dataTable.Columns["Type_"].Ordinal + 1].Style.Font.Color.SetColor(Color.Red);
                        }
                        else
                        {
                            worksheet.Cells[rowindex, dataTable.Columns["Type_"].Ordinal + 1].Style.Font.Color.SetColor(Color.Gray);
                        }
                        rowindex++;
                    }
                    for (var col = 1; col < dataTable.Columns.Count + 1; col++)
                    {
                        worksheet.Column(col).AutoFit();
                    }
                    var exportToExcel = new ExportToExcel
                    {
                        Contents = package.GetAsByteArray(),
                        FileName = "AEPSReport.xlsx"
                    };
                    return File(exportToExcel.Contents, DOCType.XlsxContentType, exportToExcel.FileName);
                }
            }

        }

        [HttpPost]
        [Route("Home/AEPS-Report")]
        [Route("AEPS-Report")]
        public async Task<IActionResult> _AEPSReport([FromBody] RechargeReportFilter filter)
        {
            IUserML userML = new UserML(_lr);
            IRechargeReportML ml = new ReportML(_accessor, _env);
            var rechargeReportModel = new RechargeReportModel
            {
                IsAdmin = _lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowAEPSReport),
                CanMarkDispute = _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.MarkDispute),
                CanFail = _lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.PageFailed),
                CanSuccess = _lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.PageSuccess),
                IsAPIUser = _lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.APIUser,
                IsEndUser = userML.IsEndUser(),
                Report = await ml.GetAEPSReport(filter).ConfigureAwait(false)
            };
            return PartialView("PartialView/_AEPSReport", rechargeReportModel);
        }
        #endregion
        #region AEPSReceipt
        [HttpGet]
        [Route("AEPSReceipt")]
        public IActionResult AEPSReceipt()
        {
            return View();
        }
        [HttpPost]
        [Route("AEPS-Receipt")]
        public IActionResult _AEPSReceipt(string TransactionID, int TID, bool IsInvoice, decimal convenientFee)
        {
            var dmr = new SellerML(_accessor, _env);
            var dmrRec = dmr.AEPSReceipt(TID, TransactionID, convenientFee);
            dmrRec.IsInvoice = IsInvoice;
            if (IsInvoice)
            {
                return PartialView("PartialView/_AEPSReceipt", dmrRec);
            }
            else
            {
                return PartialView("PartialView/_AEPSReceiptPrint", dmrRec);
            }
        }
        #endregion
        #region AEPSPending
        [HttpGet]
        [Route("Home/AEPSPendings")]
        [Route("AEPSPendings")]
        public IActionResult AEPSPendingTransaction()
        {
            if (userML.IsCustomerCareAuthorised(ActionCodes.ShowPending) || (_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser))
            {
                return View();
            }
            else
            {
                return BadRequest();
            }
        }
        [HttpPost]
        [Route("Home/aepspending")]
        [Route("aepspending")]
        public async Task<IActionResult> _AEPSPendingTransaction()
        {
            if (userML.IsCustomerCareAuthorised(ActionCodes.ShowPending) || (_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser))
            {
                IReportML rml = new ReportML(_accessor, _env);
                IUserML userML = new UserML(_lr);
                var pendingTransactionModel = new PendingTransactionModel
                {
                    CanSuccess = userML.IsCustomerCareAuthorised(ActionCodes.PageSuccess) || (_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.Admin),
                    CanFail = userML.IsCustomerCareAuthorised(ActionCodes.PageFailed) || (_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.Admin),
                    Report = await rml.PendingTransaction(0, 0, ReportType.AEPS).ConfigureAwait(false)
                };
                return PartialView("PartialView/_AEPSPendingTransaction", pendingTransactionModel);
            }
            else
            {
                return BadRequest();
            }
        }
        #endregion

        [HttpPost]
        [Route("Home/aepsStsUpdate")]
        [Route("aepsStsUpdate")]
        public async Task<IActionResult> _UpdateAEPSStatus(char Status, string ApiCode, int TID, string VenderId, string rrn, string TransactionID)
        {
            IAEPSML aEPSML = new AEPSML(_accessor, _env, false);
            if (TransactionID.Contains((char)160))
            {
                //RequestPage = "PendingPage";
                TransactionID = TransactionID.Replace(((char)160) + "", "");
            }
            var req = new AEPSTransactionServiceReq
            {
                LiveID = rrn,
                TID = TID,
                APICode = ApiCode,
                VendorID = VenderId,
                TransactionID = TransactionID,
            };
            if (Status.In('S', 's'))
                req.Status = RechargeRespType.SUCCESS;
            if (Status.In('F', 'f'))
                req.Status = RechargeRespType.FAILED;
            var res = await aEPSML.UpdateAEPSTransaction(req).ConfigureAwait(false);

            return Json(new { res.Statuscode, res.Msg });
        }
        [HttpPost]
        [Route("Home/chk-sts-aeps")]
        [Route("chk-sts-aeps")]
        public async Task<IActionResult> _StatusCheckAEPS(int TID, string TransactionID)
        {
            IResponseStatus _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = "Incomplete Request!"
            };
            if (TID > 0)
            {
                IRechargeReportML rml = new ReportML(_accessor, _env);
                _res = await rml.CheckAEPSStatusAsync(TID, TransactionID ?? string.Empty).ConfigureAwait(false);
            }
            ViewData["Heading"] = "Status Check Detail";
            return PartialView("PartialView/_StatusCheck", _res);
        }

        #endregion

        #region RefundRequestReport Section

        [HttpGet("RefundRequest")]
        public async Task<IActionResult> RefundRequest()
        {
            if ((_lr.IsWLAPIAllowed && ApplicationSetting.IsWLAPIAllowed) || (_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.Admin || userML.IsCustomerCareAuthorised(ActionCodes.ShowRefundRequestHistory))
                )
            {
                IAPIML aPIML = new APIML(_accessor, _env);
                IOperatorML opML = new OperatorML(_accessor, _env);
                var rep = new RechargeReportModel
                {
                    IsWLAPIAllowed = _lr.IsWLAPIAllowed && ApplicationSetting.IsWLAPIAllowed,
                    RechargeAPI = aPIML.GetAPIDetail(),
                    Operators = _lr.IsWLAPIAllowed && ApplicationSetting.IsWLAPIAllowed ? opML.GetOperators(string.Join(',', OPTypes.AllowToWhitelabel)) : await opML.GetOPListBYServices(ServiceType.RechargeReportServices).ConfigureAwait(false)
                };
                return View(rep);
            }
            else
            {
                return Ok();
            }
        }
        [HttpGet("DMRRefundRequest")]
        public IActionResult DMRRefundRequest()
        {
            if ((_lr.IsWLAPIAllowed && ApplicationSetting.IsWLAPIAllowed) || (_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.Admin || userML.IsCustomerCareAuthorised(ActionCodes.ShowRefundRequestHistory))
                )
            {
                return View();
            }
            else
            {
                return Ok();
            }
        }
        [HttpPost]
        [Route("Home/refundrequest")]
        [Route("refundrequest")]
        [Route("Home/dmr-refundrequest")]
        [Route("dmr-refundrequest")]
        public IActionResult RefundRequestTransaction(int APIID, int OID)
        {
            if ((_lr.IsWLAPIAllowed && ApplicationSetting.IsWLAPIAllowed) || (_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.Admin || userML.IsCustomerCareAuthorised(ActionCodes.ShowRefundRequestHistory)))
            {
                string routePath = Request.Path.Value;
                IRefundReportML rml = new ReportML(_accessor, _env);
                var filter = new RefundLogFilter
                {
                    TopRows = 0,
                    APIID = APIID,
                    OID = OID,
                    IsDMR = routePath.Contains("dmr-refundrequest")
                };
                ViewBag.routePath = routePath;
                var transactios = rml.GetRefundLog(filter);
                return PartialView("PartialView/_RefundRequest", transactios);
            }
            else
            {
                return Ok();
            }
        }

        [HttpGet]
        [Route("refundrequest-Export")]
        [Route("dmr-refundrequest")]
        public IActionResult RefundrequestExcel(int APIID, int OID)
        {
            if ((_lr.IsWLAPIAllowed && ApplicationSetting.IsWLAPIAllowed) || (_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.Admin || userML.IsCustomerCareAuthorised(ActionCodes.ShowRefundRequestHistory))
               )
            {
                string routePath = Request.Path.Value;
                IRefundReportML rml = new ReportML(_accessor, _env);
                var filter = new RefundLogFilter
                {
                    TopRows = 0,
                    APIID = APIID,
                    OID = OID,
                    IsDMR = routePath.Contains("dmr-refundrequest")
                };
                var _report = rml.GetRefundLog(filter);
                var dataTable = ConverterHelper.O.ToDataTable(_report.RefundTransaction.ToList());
                string[] removableCol = { "TID", "RefundType_", "APIID", "VendorID", "OID", "Response", "RefundRemark", "RequestMode", "LiveID" };
                dataTable.Columns["EntryDate"].ColumnName = "RequestDate";
                var Contents = EportExcel.o.GetFile(dataTable, removableCol);
                return File(Contents, DOCType.XlsxContentType, "Dispute-Request.xlsx");
            }
            else
            {
                return BadRequest();
            }
        }
        [HttpPost]
        [Route("Home/U-A-R")]
        [Route("U-A-R")]
        public async Task<IActionResult> _UpdateRefundStatus(char Status, int TID, string TransactionID, string Remark, decimal amount)
        {
            IResponseStatus _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvalidParam
            };
            RefundRequestData refundRequestData = new RefundRequestData
            {
                Remark = Remark ?? "",
                TID = TID,
                TransactionID = TransactionID ?? "",
                Amount = amount,
                RequestMode = RequestMode.PANEL
            };
            if (Status == 'A' || Status == 'a')
            {
                refundRequestData.RefundStatus = RefundType.REFUNDED;
            }
            if (Status == 'R' || Status == 'r')
            {
                refundRequestData.RefundStatus = RefundType.REJECTED;
            }
            if (refundRequestData.RefundStatus > 0)
            {
                IRefundReportML rml = new ReportML(_accessor, _env);
                _res = await rml.AcceptOrRejectRefundRequest(refundRequestData).ConfigureAwait(false);
            }
            return Json(_res);
        }
        [HttpGet]
        [Route("Home/RefundHistory")]
        [Route("RefundHistory")]
        public async Task<IActionResult> RefundLog(int f = 0)
        {
            IUserML _userML = new UserML(_lr);
            IAPIML aPIML = new APIML(_accessor, _env);
            IOperatorML opML = new OperatorML(_accessor, _env);
            var rep = new RechargeReportModel
            {
                Flag = f,
                RechargeAPI = aPIML.GetAPIDetail(),
                Operators = await opML.GetOPListBYServices(ServiceType.RechargeReportServices).ConfigureAwait(false),
                IsAdmin = _lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser || _userML.IsCustomerCareAuthorised(ActionCodes.ShowRefundRequestHistory)
            };
            return View(rep);
        }
        [HttpPost]
        [Route("Home/Refund-History")]
        [Route("Refund-History")]
        public IActionResult _RefundLog([FromBody] RefundLogFilter filter)
        {
            IRefundReportML rml = new ReportML(_accessor, _env);
            IUserML _userML = new UserML(_lr);
            filter.IsReport = true;
            var commonModel = new CommonModel
            {
                obj = rml.GetRefundLog(filter),
                IsAdmin = _lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser || _userML.IsCustomerCareAuthorised(ActionCodes.ShowRefundRequestHistory)
            };
            return PartialView("PartialView/_RefundLog", commonModel);
        }
        [HttpGet]
        [Route("Refund-History-Export")]
        public IActionResult _RefundLogExcel(RefundLogFilter filter)
        {

            IRefundReportML rml = new ReportML(_accessor, _env);
            var _report = rml.GetRefundLog(filter);
            var dataTable = ConverterHelper.O.ToDataTable(_report.RefundTransaction.ToList());
            string[] removableCol = { "_RefundType", "RefundType_", "RStatus", "APIName", "TID", "APIID", "VendorID", "OID", "Response", "RefundRemark", "RequestMode", "LiveID" };
            dataTable.Columns["EntryDate"].ColumnName = "RequestDate";
            dataTable.Columns["RefundRemark"].ColumnName = "Refund Status";
            var Contents = EportExcel.o.GetFile(dataTable, removableCol);
            return File(Contents, DOCType.XlsxContentType, "Dispute-Report.xlsx");
        }
        [HttpGet]
        [Route("WTRRequest")]
        public IActionResult WTRRequest()
        {
            IOperatorML operation = new OperatorML(_accessor, _env);
            IEnumerable<OperatorDetail> oplist = operation.GetOperators(OPTypes.DTH);
            ViewBag.OPs = oplist.ToList();
            IAPIML aPIOperation = new APIML(_accessor, _env);
            ViewBag.APIs = aPIOperation.GetAPIDetail();
            return View();
        }
        [HttpPost]
        [Route("/bind-DTH-Package")]
        public IActionResult bindDTHPackage(int OID)
        {

            IOperatorML ml = new OperatorML(_accessor, _env);
            var _res = ml.GetDTHPackage(0, OID);
            return Json(_res);
        }
        [HttpPost]
        [Route("Home/WTRRequest")]
        [Route("WTRRequest")]
        public IActionResult _WTRRequest(int APIID, int OID)
        {
            IReportML reportML = new ReportML(_accessor, _env);
            var filter = new RefundLogFilter
            {
                TopRows = 0,
                APIID = APIID,
                OID = OID
            };
            var transactios = reportML.GetWTRLog(filter);
            return PartialView("PartialView/_WTRRequest", transactios);
        }

        [HttpPost]
        [Route("Home/W2R-A-R")]
        [Route("W2R-A-R")]
        public IActionResult _UpdateW2RStatus(char Status, int TID, string TransactionID, string Remark)
        {
            IResponseStatus _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                ErrorMsg = ErrorCodes.TempError
            };
            var refundRequestData = new RefundRequestData
            {
                Remark = Remark ?? "",
                TID = TID,
                TransactionID = TransactionID ?? ""
            };
            if (Status == 'A' || Status == 'a')
            {
                refundRequestData.RefundStatus = RefundType.REFUNDED;
            }
            if (Status == 'R' || Status == 'r')
            {
                refundRequestData.RefundStatus = RefundType.REJECTED;
            }
            IReportML reportML = new ReportML(_accessor, _env);
            _res = reportML.UpdateWTRStatus(refundRequestData);
            return Json(new { statuscode = _res.Statuscode, status = _res.Msg });
        }

        [HttpGet]
        [Route("Home/WTRHistory")]
        [Route("APIUser/WTRHistory")]
        [Route("WTRHistory")]
        public IActionResult WTRLog()
        {
            ViewBag.RoleID = _lr.RoleID;
            var rep = new ReportModelCommon();
            IAPIML aPIML = new APIML(_accessor, _env);
            rep.IsAdmin = _lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser;
            rep.RechargeAPI = aPIML.GetAPIDetail();
            IOperatorML operatorOperation = new OperatorML(_accessor, _env);
            rep.Operators = operatorOperation.GetOperators(OPTypes.DTH);
            return View(rep);
        }
        [HttpPost]
        [Route("Home/WTR-History")]
        [Route("APIUser/WTR-History")]
        [Route("WTR-History")]
        public IActionResult _WTRLog([FromBody] RefundLogFilter filter)
        {
            ViewBag.RoleID = _lr.RoleID;
            IReportML reportML = new ReportML(_accessor, _env);
            var _report = reportML.GetWTRLog(filter);
            return PartialView("PartialView/_WTRLog", _report);
        }
        #endregion

        #region AdminLedger
        [HttpGet]
        [Route("Home/AdminLedger")]
        [Route("AdminLedger")]
        public IActionResult AdminLedger()
        {
            IUserML userML = new UserML(_accessor, _env);
            var ledgerReportModel = new LedgerReportModel
            {
                IsAdmin = _lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser,
                userBalnace = userML.GetUserBalnace(0)
            };
            return View(ledgerReportModel);
        }
        [HttpPost]
        [Route("Home/Admin-Ledger")]
        [Route("Admin-Ledger")]
        public IActionResult _AdminLedger([FromBody] ALedgerReportFilter filter)
        {
            IReportML rml = new ReportML(_accessor, _env);
            IEnumerable<ProcAdminLedgerResponse> _report = rml.GetAdminLedgerList(filter);
            return PartialView("PartialView/_AdminLedger", _report);
        }
        [HttpGet]
        [Route("Home/Admin-Ledger")]
        [Route("Admin-Ledger")]
        public IActionResult _AdminLedgerExport(ALedgerReportFilter filter)
        {
            ViewBag.RoleID = _lr.RoleID;
            IReportML rml = new ReportML(_accessor, _env);
            var _report = rml.GetAdminLedgerList(filter);

            var dataTable = ConverterHelper.O.ToDataTable(_report);
            dataTable.Columns.Remove("ID");
            dataTable.Columns.Remove("Msg");
            dataTable.Columns.Remove("TopRows");
            dataTable.Columns.Remove("ResultCode");
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("AdminLedger1");
                worksheet.Cells["A1"].LoadFromDataTable(dataTable, PrintHeaders: true);
                int rowindex = 2;
                foreach (DataRow row in dataTable.Rows)
                {
                    if (rowindex == 2)
                    {
                        worksheet.Cells[1, dataTable.Columns["TID"].Ordinal + 1].Value = "TransactionID";
                        worksheet.Cells[1, dataTable.Columns["EntryDate"].Ordinal + 1].Value = "Date Time";
                        worksheet.Cells[1, dataTable.Columns["LastBalance"].Ordinal + 1].Value = "Old";
                        worksheet.Cells[1, dataTable.Columns["CurentBalance"].Ordinal + 1].Value = "Current";
                    }
                    rowindex++;
                }
                for (var col = 1; col < dataTable.Columns.Count + 1; col++)
                {
                    worksheet.Column(col).AutoFit();
                }
                var exportToExcel = new ExportToExcel
                {
                    Contents = package.GetAsByteArray(),
                    FileName = "AdminLedger.xlsx"
                };
                return File(exportToExcel.Contents, DOCType.XlsxContentType, exportToExcel.FileName);
            }
        }
        #endregion

        #region UserLedger
        [Route("Home/UserLedger/{MobileNo}")]
        [Route("UserLedger/{MobileNo}")]
        public IActionResult UserLedger(string MobileNo)
        {
            IUserML userml = new UserML(_accessor, _env);
            var ledgerReportModel = new LedgerReportModel
            {
                HaveChild = !userml.IsEndUser() || userml.IsCustomerCareAuthorised(ActionCodes.ShowUserLedger),
                SelfMobile = MobileNo ?? _lr.MobileNo ?? "",
                userBalnace = userml.GetUserBalnace(0)
            };
            return View(ledgerReportModel);
        }
        [Route("Home/UserLedger")]
        [Route("UserLedger")]
        public IActionResult UserLedger()
        {
            IUserML userml = new UserML(_accessor, _env);
            var loginResp = chkAlternateSession();
            var ledgerReportModel = new LedgerReportModel();
            if (_lr != null)
            {
                ledgerReportModel.HaveChild = !userml.IsEndUser() || userml.IsCustomerCareAuthorised(ActionCodes.ShowUserLedger);
            }
            if (_lrEmp != null)
            {
                ledgerReportModel.HaveChild = _lrEmp.LoginTypeID == LoginType.Employee ? true : false;
            }

            ledgerReportModel.SelfMobile = loginResp.MobileNo ?? "";
            ledgerReportModel.userBalnace = userml.GetUserBalnace(0);
            ledgerReportModel.IsEmployee = loginResp.LoginTypeID == LoginType.Employee ? true : false;

            return View(ledgerReportModel);
        }

        [HttpPost]
        [Route("Home/User-Ledger")]
        [Route("User-Ledger")]
        public async Task<IActionResult> _UserLedger([FromBody] ULedgerReportFilter filter)
        {
            var loginResp = chkAlternateSession();
            IUserML userml = new UserML(loginResp);
            if (_lr != null)
            {
                ViewBag.HaveChild = !userml.IsEndUser() || userml.IsCustomerCareAuthorised(ActionCodes.ShowUserLedger);
            }
            if (_lrEmp != null)
            {
                ViewBag.HaveChild = _lrEmp.LoginTypeID == LoginType.Employee ? true : false;
            }
            //ViewBag.HaveChild = !userml.IsEndUser() || userml.IsCustomerCareAuthorised(ActionCodes.ShowUserLedger) || _lrEmp.LoginTypeID == LoginType.Employee;
            ViewBag.SelfMobile = loginResp.MobileNo ?? "";
            IReportML ml = new ReportML(_accessor, _env);
            IEnumerable<ProcUserLedgerResponse> _report = await ml.GetUserLedgerList(filter);
            return PartialView("PartialView/_UserLedger", _report);
        }
        [HttpGet]
        [Route("Home/User-Ledger")]
        [Route("User-Ledger")]
        public async Task<IActionResult> _UserLedgerExport(ULedgerReportFilter filter)
        {
            var loginResp = chkAlternateSession();
            IUserML userml = new UserML(loginResp);
            ViewBag.IsEndUser = userml.IsEndUser();
            filter.Mobile_F = filter.Mobile_F == null ? "" : filter.Mobile_F;
            filter.TransactionId_F = filter.TransactionId_F == null ? "" : filter.TransactionId_F;
            IReportML ml = new ReportML(_accessor, _env);
            var _report = await ml.GetUserLedgerList(filter);
            DataTable dataTable = ConverterHelper.O.ToDataTable(_report);
            dataTable.Columns.Remove("ResultCode");
            dataTable.Columns.Remove("Msg");
            dataTable.Columns.Remove("TopRows");
            dataTable.Columns.Remove("ID");
            dataTable.Columns.Remove("MobileNo");
            if (ViewBag.IsEndUser)
            {
                dataTable.Columns.Remove("User");
            }
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("UserLedger1");
                worksheet.Cells["A1"].LoadFromDataTable(dataTable, PrintHeaders: true);
                worksheet.Row(1).Height = 20;
                worksheet.Row(1).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Row(1).Style.Font.Bold = true;
                int rowindex = 2;
                foreach (DataRow row in dataTable.Rows)
                {
                    if (rowindex == 2)
                    {
                        worksheet.Cells[1, dataTable.Columns["TID"].Ordinal + 1].Value = "TransactionID";
                        worksheet.Cells[1, dataTable.Columns["EntryDate"].Ordinal + 1].Value = "Date Time";
                        worksheet.Cells[1, dataTable.Columns["LastAmount"].Ordinal + 1].Value = "Old";
                        worksheet.Cells[1, dataTable.Columns["CurentBalance"].Ordinal + 1].Value = "Current";
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
                    FileName = "UserLedger.xlsx"
                };
                return File(exportToExcel.Contents, DOCType.XlsxContentType, exportToExcel.FileName);
            }
        }
        [HttpPost]
        [Route("Home/ASum")]
        [Route("APIUser/ASum")]
        [Route("ASum")]
        public IActionResult _AccountSummary([FromBody] ULedgerReportFilter filter)
        {
            IReportML ml = new ReportML(_accessor, _env);
            return Json(ml.GetAccountSummary(filter));
        }
        [HttpPost]
        [Route("Home/A-Summary")]
        [Route("A-Summary")]
        public IActionResult _ASummary([FromBody] ULedgerReportFilter filter)
        {
            IReportML ml = new ReportML(_accessor, _env);
            ViewBag.WalletID = filter.WalletTypeID;
            return PartialView("PartialView/_AccountSummary", ml.GetAccountSummary(filter));
        }
        #endregion

        #region DashboardSummary
        [HttpPost]
        [Route("Home/DSum")]
        [Route("APIUser/DSum")]
        [Route("DSum")]
        public IActionResult _SummaryDashboard()
        {
            IReportML ml = new ReportML(_accessor, _env);
            return PartialView("PartialView/_DashboardSummary", ml.GetAccountSummaryDashboard());
        }
        [HttpPost]
        [Route("Home/dsummary")]
        [Route("APIUser/dsummary")]
        [Route("dsummary")]
        public async Task<IActionResult> _dsummary()
        {
            IRechargeReportML ml = new ReportML(_accessor, _env);
            return PartialView("PartialView/_RechargeSummary", await ml.GetTransactionSummary(0));
        }
        [HttpPost]
        [Route("s-g-i")]
        public IActionResult _ShowGiftImage()
        {
            ITargetML _targetML = new OperatorML(_accessor, _env);
            var res = _targetML.ShowGiftImages();
            return PartialView("PartialView/_ShowGiftImages", res);
        }
        #endregion

        #region FundReceiveReport
        [Route("FundReceiveStatement")]
        public IActionResult FundReceiveStatement()
        {
            var loginRes = chkAlternateSession();
            IUserML userML = new UserML(loginRes);
            if (loginRes.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowFundDebitCredit) || loginRes.LoginTypeID == LoginType.Employee)
            {
                IAppReportML ml = new ReportML(_accessor, _env, false);
                var res = new FundReciveModel
                {
                    LoginMob = loginRes.LoginTypeID == LoginType.CustomerCare ? "AN1" : loginRes.MobileNo,
                    Walletes = ml.GetWalletTypes()
                };
                return View(res);
            }
            return Ok();
        }
        [HttpPost]
        [Route("FundReceive-Statement")]
        public IActionResult _FundReceiveStatement([FromBody] ULFundReceiveReportFilter filter)
        {

            IReportML operation = new ReportML(_accessor, _env);
            var res = new FundReciveModel
            {
                ServiceID = filter.ServiceID,
                ProcFundReceiveStatementResponses = operation.GetUserFundReceive(filter)
            };
            return PartialView("PartialView/_FundReceiveStatement", res);
        }
        [HttpGet]
        [Route("APIUser/FundReceiveStatementInvoice")]
        public IActionResult FundReceiveStatementInvoice(string Invoice)
        {
            IReportML rml = new ReportML(_accessor, _env);
            ProcFundReceiveInvoiceResponse _report = rml.GetUserFundReceiveInvoice(Invoice);
            return View(_report);
        }
        [HttpGet]
        [Route("FundReceive-Statement-Excel")]
        public IActionResult _FundReceiveStatementExcel(ULFundReceiveReportFilter filter)
        {
            IReportML operation = new ReportML(_accessor, _env);
            var res = operation.GetUserFundReceive(filter);
            var dataTable = ConverterHelper.O.ToDataTable(res);
            string[] removableCol = { "Id", "UserId", "StatusCode", "TransactionID", "ServiceTypeID", "WalletID" };
            var Contents = EportExcel.o.GetFile(dataTable, removableCol);
            return File(Contents, DOCType.XlsxContentType, "FundReceiveStatementExcel.xlsx");
        }
        #endregion

        #region PaymentRequestReport Section
        [HttpGet]
        [Route("FundRequestReport")]
        public IActionResult FundRequestReport()
        {
            //This report is for parent only

            var IsAdmin = _lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.Admin;
            IUserML userml = new UserML(_accessor, _env);
            var ledgerReportModel = new LedgerReportModel
            {
                HaveChild = !userml.IsEndUser() || userml.IsCustomerCareAuthorised(ActionCodes.ShowUserLedger),
                userBalnace = userml.GetUserBalnace(0),
                IsAdmin = IsAdmin
            };
            return View(ledgerReportModel);
        }

        [HttpPost]
        [Route("Fund-Report")]
        public IActionResult _FundRequestReport(string f, string t, int tm, int sts, int tp, int C, string CText, int wid)
        {
            //This report is for parent only
            IReportML rml = new ReportML(_accessor, _env);
            var fundOrderFilter = new FundOrderFilter
            {
                FromDate = f,
                ToDate = t,
                TMode = tm,
                RSts = sts,
                Top = tp,
                Criteria = C,
                CriteriaText = CText,
                WalletTypeID = wid
            };
            IEnumerable<FundRequetResp> list = rml.GetUserFundReport(fundOrderFilter);
            return PartialView("PartialView/_FundRequestReport", list);
        }

        [HttpGet]
        [Route("Fund-Report")]
        public IActionResult _FundRequestReportExport(string f, string t, int tm, int sts, int tp, int C, string CText, int wid)
        {
            //This report is for parent only
            IReportML rml = new ReportML(_accessor, _env);
            var fundOrderFilter = new FundOrderFilter
            {
                FromDate = f,
                ToDate = t,
                TMode = tm,
                RSts = sts,
                Top = tp,
                Criteria = C,
                CriteriaText = CText,
                WalletTypeID = wid
            };
            var list = rml.GetUserFundReport(fundOrderFilter);
            var dataTable = ConverterHelper.O.ToDataTable(list);
            dataTable.Columns.Remove("IsSelf");
            dataTable.Columns.Remove("PaymentId");
            dataTable.Columns.Remove("UserId");
            dataTable.Columns.Remove("LT");
            dataTable.Columns.Remove("StatusCode");
            dataTable.Columns.Remove("Description");
            dataTable.Columns.Remove("ToDate");
            dataTable.Columns.Remove("CommRate");
            dataTable.Columns.Remove("_TMode");
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("FundOrders");
                worksheet.Cells["A1"].LoadFromDataTable(dataTable, PrintHeaders: true);
                worksheet.Row(1).Height = 20;
                worksheet.Row(1).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Row(1).Style.Font.Bold = true;
                int rowindex = 2;
                foreach (DataRow row in dataTable.Rows)
                {
                    worksheet.Cells[1, dataTable.Columns["EntryDate"].Ordinal + 1].Value = "RequestDate";
                    var Type_ = worksheet.Cells[rowindex, dataTable.Columns["Status"].Ordinal + 1].Value;
                    if (Type_.ToString() == "Accepted")
                    {
                        worksheet.Cells[rowindex, dataTable.Columns["Status"].Ordinal + 1].Style.Font.Color.SetColor(Color.Green);
                    }
                    else
                    if (Type_.ToString() == "Rejected")
                    {
                        worksheet.Cells[rowindex, dataTable.Columns["Status"].Ordinal + 1].Style.Font.Color.SetColor(Color.Red);
                    }
                    else
                    {
                        worksheet.Cells[rowindex, dataTable.Columns["Status"].Ordinal + 1].Style.Font.Color.SetColor(Color.Gray);
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
                    FileName = "FundOrder.xlsx"
                };
                return File(exportToExcel.Contents, DOCType.XlsxContentType, exportToExcel.FileName);
            }
        }
        [HttpGet]
        [Route("FundRequestReportSelf")]
        public IActionResult FundRequestReportSelf()
        {
            IUserML userml = new UserML(_accessor, _env);
            var ledgerReportModel = new LedgerReportModel
            {
                HaveChild = !userml.IsEndUser() || userml.IsCustomerCareAuthorised(ActionCodes.ShowUserLedger),
                userBalnace = userml.GetUserBalnace(0),
            };
            return View(ledgerReportModel);
        }
        [HttpPost]
        [Route("Fund-Report-self")]
        public IActionResult _FundRequestReportSelf(string f, string t, string um, int tm, int sts, string acn, string tranid, int tp, int wid)
        {
            IReportML rml = new ReportML(_accessor, _env);
            FundOrderFilter fundOrderFilter = new FundOrderFilter
            {
                FromDate = f,
                ToDate = t,
                UMobile = um,
                TMode = tm,
                RSts = sts,
                AccountNo = acn,
                TransactionID = tranid,
                Top = tp,
                IsSelf = true,
                WalletTypeID = wid
            };
            IEnumerable<FundRequetResp> list = rml.GetUserFundReport(fundOrderFilter);
            return PartialView("PartialView/_FundRequestReportSelf", list);
        }
        #endregion

        #region PaymentRequest Approval Section
        [HttpGet]
        [Route("FundRequestApproval")]
        public IActionResult FundRequestApproval()
        {
            if (!_lr.RoleID.In(Role.Customer, Role.APIUser, Role.Retailor_Seller, Role.FOS) && _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowFundRequest))
            {
                return View(_lr.RoleID);
            }
            else
            {
                return BadRequest();
            }
        }
        [HttpPost]
        [Route("Fund-Report-Approval")]
        public IActionResult _FundRequestApproval()
        {
            if (!_lr.RoleID.In(Role.Customer, Role.APIUser, Role.Retailor_Seller, Role.FOS) && _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowFundRequest))
            {
                IReportML rml = new ReportML(_accessor, _env);
                //IEnumerable<FundRequetResp> list = 
                var data = new FundRequestShow
                {
                    UserRoleID = _lr.RoleID,
                    FundRequests = (List<FundRequetResp>)rml.GetUserFundReportApproval()
                };
                return PartialView("PartialView/_FundRequestApproval", data);
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpPost]
        [Route("Home/F-R")]
        [Route("F-R")]
        public IActionResult _FundRejectPanel(int PaymentId)
        {
            IFundProcessML userML = new UserML(_accessor, _env);

            var list = new FundRequetResp();
            list.MasterRR = userML.MASTERRR();
            list.FundReject = userML.GetUserFundTransferData(PaymentId);
            return PartialView("PartialView/_FundRejectPanel", list);
        }
        [HttpPost]
        [Route("Home/FR")]
        [Route("FR")]
        public IActionResult FundReject([FromBody] FundProcess req)
        {
            req.RequestMode = RequestMode.PANEL;
            IFundProcessML userML = new UserML(_accessor, _env);
            var res = userML.FundReject(req);
            return Json(res);
        }
        #endregion

        #region PGTransaction Report
        [HttpGet]
        [Route("PGTransactionReport")]
        public IActionResult PGTransactionReport()
        {
            IReportML ml = new ReportML(_accessor, _env);
            var rechargeReportModel = new TransactionPGReportModel
            {
                PGActive = ml.GetActivePaymentGateway()
            };
            return View(rechargeReportModel);
        }

        [HttpPost]
        [Route("Home/PGTransaction-Report")]
        [Route("PGTransaction-Report")]
        public async Task<IActionResult> _GetPGTransactionReport([FromBody] TransactionPG filter)
        {
            IUserML userML = new UserML(_lr);
            IReportML ml = new ReportML(_accessor, _env);
            var rechargeReportModel = new TransactionPGReportModel
            {
                IsAdmin = _lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowRechargeReport),
                CanMarkDispute = _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.MarkDispute),
                CanFail = _lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.PageFailed),
                CanSuccess = _lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.PageSuccess),
                IsAPIUser = _lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.APIUser,
                IsEndUser = userML.IsEndUser(),
                Report = await ml.GetPGTransactionReport(filter).ConfigureAwait(false)
            };
            return PartialView("PartialView/_PGTransactionReport", rechargeReportModel);
        }

        [HttpGet]
        [Route("Home/PGTransaction-Report")]
        [Route("PGTransaction-Report")]
        public async Task<IActionResult> _PGTransactionReportExport(TransactionPG filter)
        {
            IUserML userML = new UserML(_accessor, _env);
            IReportML ml = new ReportML(_accessor, _env);
            filter.IsExport = true;
            var IsAdmin = _lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowRechargeReport);
            var _report = await ml.GetPGTransactionReport(filter).ConfigureAwait(false);
            DataTable dataTable = ConverterHelper.O.ToDataTable(_report);
            string[] removableColumns = { "LT", "LoginID", "UserID", "PGID", "FromDate", "ToDate", "OID", "OpTypeID", "ServiceID", "RequestIP", "ChargeAmtType", "UPGID", "TopRows", "Criteria", "CriteriaText", "IsExport", "Statuscode", "Msg", "CommonInt", "CommonInt2", "CommonStr", "CommonStr2", "CommonStr3", "CommonStr4", "CommonInt3", "CommonBool", "Flag", "ErrorCode", "ErrorMsg", "ReffID" };
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
                var worksheet = package.Workbook.Worksheets.Add("PGTransaction");
                worksheet.Cells["A1"].LoadFromDataTable(dataTable, PrintHeaders: true);
                worksheet.Row(1).Height = 20;
                worksheet.Row(1).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Row(1).Style.Font.Bold = true;
                int rowindex = 2;
                foreach (DataRow row in dataTable.Rows)
                {
                    if (rowindex == 2)
                    {
                        worksheet.Cells[1, dataTable.Columns["Type"].Ordinal + 1].Value = "Status";
                        worksheet.Cells[1, dataTable.Columns["EntryDate"].Ordinal + 1].Value = "RechargeDate";
                    }
                    var Type_ = worksheet.Cells[rowindex, dataTable.Columns["Type"].Ordinal + 1].Value;
                    if (Convert.ToInt32(Type_) == RechargeRespType.SUCCESS)
                    {
                        worksheet.Cells[rowindex, dataTable.Columns["Type"].Ordinal + 1].Style.Font.Color.SetColor(Color.Green);
                    }
                    else if (Convert.ToInt32(Type_) == RechargeRespType.FAILED)
                    {
                        worksheet.Cells[rowindex, dataTable.Columns["Type"].Ordinal + 1].Style.Font.Color.SetColor(Color.Red);
                    }
                    else if (Convert.ToInt32(Type_) == RechargeRespType.PENDING || Convert.ToInt32(Type_) == RechargeRespType.REQUESTSENT)
                    {
                        worksheet.Cells[rowindex, dataTable.Columns["Type"].Ordinal + 1].Style.Font.Color.SetColor(Color.DarkOrange);
                        worksheet.Cells[rowindex, dataTable.Columns["Type"].Ordinal + 1].Value = nameof(RechargeRespType.PENDING);
                    }

                    var RequestMode_ = worksheet.Cells[rowindex, dataTable.Columns["RequestedMode"].Ordinal + 1].Value;
                    if (Convert.ToInt32(RequestMode_) == RequestMode.PANEL)
                    {
                        worksheet.Cells[rowindex, dataTable.Columns["RequestedMode"].Ordinal + 1].Value = nameof(RequestMode.PANEL);
                    }
                    else if (Convert.ToInt32(RequestMode_) == RequestMode.API)
                    {
                        worksheet.Cells[rowindex, dataTable.Columns["RequestedMode"].Ordinal + 1].Value = nameof(RequestMode.API);
                    }
                    else if (Convert.ToInt32(RequestMode_) == RequestMode.APPS)
                    {
                        worksheet.Cells[rowindex, dataTable.Columns["RequestedMode"].Ordinal + 1].Value = nameof(RequestMode.APPS);
                    }
                    else if (Convert.ToInt32(RequestMode_) == RequestMode.SMS)
                    {
                        worksheet.Cells[rowindex, dataTable.Columns["RequestedMode"].Ordinal + 1].Value = nameof(RequestMode.SMS);
                    }
                    else if (Convert.ToInt32(RequestMode_) == RequestMode.WEBAPPS)
                    {
                        worksheet.Cells[rowindex, dataTable.Columns["RequestedMode"].Ordinal + 1].Value = nameof(RequestMode.WEBAPPS);
                    }
                    var WalletType_ = worksheet.Cells[rowindex, dataTable.Columns["WalletID"].Ordinal + 1].Value;
                    if (Convert.ToInt32(WalletType_) == Wallet.Prepaid)
                    {
                        worksheet.Cells[rowindex, dataTable.Columns["WalletID"].Ordinal + 1].Value = nameof(Wallet.Prepaid);
                    }
                    else if (Convert.ToInt32(WalletType_) == Wallet.Utility)
                    {
                        worksheet.Cells[rowindex, dataTable.Columns["WalletID"].Ordinal + 1].Value = nameof(Wallet.Utility);
                    }
                    else if (Convert.ToInt32(WalletType_) == Wallet.Bank)
                    {
                        worksheet.Cells[rowindex, dataTable.Columns["WalletID"].Ordinal + 1].Value = nameof(Wallet.Bank);
                    }
                    else if (Convert.ToInt32(WalletType_) == Wallet.Card)
                    {
                        worksheet.Cells[rowindex, dataTable.Columns["WalletID"].Ordinal + 1].Value = nameof(Wallet.Card);
                    }
                    else if (Convert.ToInt32(WalletType_) == Wallet.RegID)
                    {
                        worksheet.Cells[rowindex, dataTable.Columns["WalletID"].Ordinal + 1].Value = nameof(Wallet.RegID);
                    }
                    else if (Convert.ToInt32(WalletType_) == Wallet.Package)
                    {
                        worksheet.Cells[rowindex, dataTable.Columns["WalletID"].Ordinal + 1].Value = nameof(Wallet.Package);
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
                    FileName = "PGTransactionReport.xlsx"
                };
                return File(exportToExcel.Contents, DOCType.XlsxContentType, exportToExcel.FileName);
            }
        }


        [HttpPost]
        [Route("Home/PGTransaction-log")]
        [Route("PGTransaction-log")]
        public IActionResult _TransactionmoreDetail(TransactionPGLogDetail param)
        {
            IUserML userML = new UserML(_lr);
            IReportML ml = new ReportML(_accessor, _env);
            var _log = ml.GetTransactionPGLog(param);
            return PartialView("PartialView/_TransactionmoreDetail", _log);
        }

        [HttpPost]
        [Route("_ChangePGTransactionStatus")]
        public IActionResult _ChangePGTransactionStatus(TransactionPG param)
        {
            IReportML ml = new ReportML(_accessor, _env);
            var res = ml.ChangeTransactionPGStatus(param);
            return Json(res);
        }
        [HttpPost]
        [Route("_CheckPGTransactionStatus_")]
        public IActionResult _CheckPGTransactionStatus_(int TID)
        {
            IPaymentGatewayML ml = new PaymentGatewayML(_accessor, _env);
            var param = new CommonReq
            {
                LoginTypeID = _lr.LoginTypeID,
                LoginID = _lr.UserID,
                CommonInt = TID
            };
            var res = ml.CheckPGTransactionStatus(param);
            return Json(res);
        }
        [HttpGet]
        [Route("BulkStatusCheckPG")]
        public IActionResult BulkStatusCheckPG()
        {
            if (_lr.LoginTypeID == LoginType.ApplicationUser)
            {
                IPaymentGatewayML ml = new PaymentGatewayML(_accessor, _env);
                ml.LoopPGTransactionStatus();
                return Ok("success");
            }
            else
            {
                return Ok("success");
            }
        }
        [HttpGet]
        [Route("/PendingPGTransactionReport")]
        public IActionResult PendingPGTransactionReport()
        {
            return View();
        }
        [HttpPost]
        [Route("/_PendingPGTransactionReport")]
        public async Task<IActionResult> _PendingPGTransactionReport()
        {
            IUserML userML = new UserML(_lr);
            IReportML ml = new ReportML(_accessor, _env);
            var _Model = new TransactionPGReportModel
            {
                IsAdmin = _lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowRechargeReport),
                CanMarkDispute = _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.MarkDispute),
                CanFail = _lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.PageFailed),
                CanSuccess = _lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.PageSuccess),
                IsAPIUser = _lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.APIUser,
                IsEndUser = userML.IsEndUser(),
                Report = await ml.GetPendingPGTransactionReport()
            };
            return PartialView("PartialView/_PendingPgTransactionReport", _Model);
        }
        #endregion



        #region CustomerCareReports
        [HttpGet]
        [Route("Home/CustomerCareActivity")]
        [Route("CustomerCareActivity")]
        public IActionResult ActivityLog()
        {
            if (_lr.RoleID == Role.Admin)
            {
                return View();
            }
            return Ok();
        }
        [HttpPost]
        [Route("Home/cc-al")]
        [Route("cc-al")]
        public async Task<IActionResult> _ActivityLog(string f, string t, string m, int o)
        {
            if (_lr.RoleID != Role.Admin)
                return Ok();
            IReportML operation = new ReportML(_accessor, _env);
            List<UserActivityLog> _res = await operation.GetUserActivity(f, t, m).ConfigureAwait(false);
            return PartialView("PartialView/_ActivityLog", _res);
        }
        [HttpGet]
        [Route("Home/cc-al-export")]
        [Route("cc-al-export")]
        public async Task<IActionResult> _ActivityLogExport(string f, string t, string m, int o)
        {
            if (_lr.RoleID != Role.Admin)
                return Ok();
            IReportML operation = new ReportML(_accessor, _env);
            List<UserActivityLog> _res = await operation.GetUserActivity(f, t, m).ConfigureAwait(false);
            DataTable dt = ConverterHelper.O.ToDataTable(_res);
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("ActivityLog");
                worksheet.Cells["A1"].LoadFromDataTable(dt, PrintHeaders: true);
                worksheet.Row(1).Height = 20;
                worksheet.Row(1).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Row(1).Style.Font.Bold = true;

                for (var col = 1; col < dt.Columns.Count + 1; col++)
                {
                    worksheet.Column(col).AutoFit();
                }
                var exportToExcel = new ExportToExcel
                {
                    Contents = package.GetAsByteArray(),
                    FileName = "ActivityLog.xlsx"
                };
                return File(exportToExcel.Contents, DOCType.XlsxContentType, exportToExcel.FileName);
            }
        }
        #endregion

        [HttpPost]
        [Route("/DenomCommissionDetail")]
        public IActionResult DenomCommissionDetail(int OID)
        {
            ISlabML ml = new SlabML(_accessor, _env);
            var res = ml.GetDenomCommissionDetail(OID);
            return PartialView("PartialView/_DenomCommissionDetail", res);
        }
        [HttpPost]
        [Route("/CircleSlab-Display")]
        public IActionResult CircleSlabDisplay(int OID)
        {
            ISlabML ml = new SlabML(_accessor, _env);
            var res = ml.CircleSlabGet(_lr.SlabID, OID);
            return PartialView("PartialView/_CircleSlabDisplay", res);
        }

        [HttpPost]
        [Route("/SpecialSlab-Display")]
        public async Task<IActionResult> SpecialSlabDisplay(int OID)
        {
            CircleWithDomination param = new CircleWithDomination
            {
                OID = OID
            };

            IOperatorML opml = new OperatorML(_accessor, _env);
            var circleDomList = await opml.GetCircleWithDominations(param).ConfigureAwait(false);
            return PartialView("PartialView/_SpecialSlabDisplay", circleDomList);
        }

        #region SlabCommission
        [HttpGet("SlabCommission")]
        public IActionResult SlabCommission()
        {
            var _info = new UserInfo();
            _info.RoleID = _lr.RoleID;
            _info.IsRealAPI = _lr.IsRealAPI;
            return View(_info);
        }

        [HttpPost]
        [Route("RealAPI-Status")]
        public IActionResult RealAPIStatusUpdate(bool Status)
        {
            ISlabML slabML = new SlabML(_accessor, _env);
            var res = slabML.RealAPIStatusUpdate(Status);
            if (res.Statuscode == ErrorCodes.One)
            {
                ILoginML lML = new LoginML(_accessor, _env);
                var PreLDetail = _lr;
                PreLDetail.IsRealAPI = Status;
                lML.ResetLoginSession(_lr);
            }
            return Json(res);
        }

        [HttpPost]
        [Route("rslab-detail")]
        public IActionResult _RSlab_Detail()
        {
            ISlabML sML = new SlabML(_accessor, _env);
            IOperatorML opml = new OperatorML(_accessor, _env);
            if (ApplicationSetting.IsRoleCommissionDisplay)
            {
                var sdml = new SlabDetailDisplayModel
                {
                    slbModel = sML.GetSlabDetailForDisplay(),
                    IsAPIUser = _lr.RoleID == Role.APIUser && _lr.LoginTypeID == LoginType.ApplicationUser,
                    IsCircleSlabAllowed = _lr.IsCalculateCommissionFromCircle,
                    Optionals = _lr.RoleID == Role.APIUser ? opml.OperatorOptionalStuff() : null
                };
                return PartialView("PartialView/_SlabCommissionRoleDisplay", sdml);
            }
            else
            {
                var sdml = sML.GetSlabCommission(_lr.SlabID);
                return PartialView("PartialView/_SlabCommission", sdml);
            }
        }
        [HttpPost]
        [Route("rslab-detail-Range")]
        public IActionResult _RSlabRange_Detail(int OID)
        {
            ISlabML sML = new SlabML(_accessor, _env);
            var sdml = sML.GetSlabRangeDetail(OID);
            return PartialView("PartialView/_RSlabRangeDetail", sdml);
        }
        [HttpPost]
        [Route("sops-comm-detail")]
        public IActionResult SpecialOpsCommissionDetail(int OID)
        {
            ISlabML sML = new SlabML(_accessor, _env);
            var sdml = sML.GetSpecialSlabDetail(OID);
            return PartialView("PartialView/_SpecialSlabDetail", sdml);
        }

        [HttpPost]
        [Route("AFslab-detail")]
        public IActionResult _AFSlabDetail(int OID)
        {
            ISlabML sML = new SlabML(_accessor, _env);
            if (ApplicationSetting.IsRoleCommissionDisplay)
            {
                var sdml = sML.GetAFSlabDetailForDisplay(OID);
                return PartialView("PartialView/_AFSlabDetailRole", sdml);
            }
            else
            {
                var sdml = sML.GetAFSlabCommission(OID);
                return PartialView("PartialView/_AFSlabDetail", sdml);
            }
        }



        [HttpGet("DTHSlabCommissionDisplay")]
        public IActionResult DTHSlabCommissionDisplay()
        {
            var _info = new UserInfo
            {
                RoleID = _lr.RoleID,
                IsRealAPI = _lr.IsRealAPI
            };
            return View(_info);
        }
        [HttpPost]
        [Route("R-DTH-slab-detail")]
        public IActionResult _RDTHSlab_Detail(int OID)
        {
            ISlabML sML = new SlabML(_accessor, _env);
            if (ApplicationSetting.IsRoleCommissionDisplay)
            {
                var sdml = sML.GetDTHSlabDetailForDisplay(OID);
                return PartialView("PartialView/_DTHSlabCommissionDisplay", sdml);
            }
            return Ok();
        }
        public IActionResult CheckSlab()
        {
            ISlabML slabML = new SlabML(_accessor, _env);
            return Json(slabML.GetSlabDetailForDisplay());
        }
        [HttpGet("RangeSlabCommission")]
        public IActionResult RangeCommissionSlab()
        {
            return View();
        }
        [HttpPost]
        [Route("rangeslab-detail")]
        public IActionResult _RangeSlab_Detail()
        {
            ISlabML sML = new SlabML(_accessor, _env);
            if (ApplicationSetting.IsRoleCommissionDisplay)
            {
                var sdml = sML.GetSlabDetailForDisplayRange();
                return PartialView("PartialView/_RangeSlabCommissionRoleDisplay", sdml);
            }
            else
            {
                var sdml = sML.GetSlabCommission(_lr.SlabID);
                return PartialView("PartialView/_RangeSlabCommission", sdml);
            }

        }
        #endregion

        #region OutletSection

        [HttpGet]
        [Route("Home/OutletList")]
        [Route("OutletList")]
        public IActionResult OutletsOfUser()
        {
            IUserML userML = new UserML(_lr);
            var IsAdmin = _lr.LoginTypeID == LoginType.ApplicationUser && _lr.UserID == 1 || userML.IsCustomerCareAuthorised(ActionCodes.ShowOutlets);
            if (IsAdmin || _lr.RoleID == Role.APIUser)
                return View(IsAdmin);
            return Ok();
        }

        [HttpPost]
        [Route("Home/OutletList")]
        [Route("OutletList")]
        public async Task<IActionResult> _OutletsOfUsersList([FromBody] OuletOfUsersListFilter filter)
        {
            IUserML userML = new UserML(_lr);
            IOutletML ml = new ReportML(_accessor, _env);
            var OutletUserListModel = new OutletUserListModel
            {
                IsAdmin = _lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowRechargeReport),
                CanMarkDispute = _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.MarkDispute),
                CanFail = _lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.PageFailed),
                CanSuccess = _lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.PageSuccess),
                IsAPIUser = _lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.APIUser,
                IsEndUser = userML.IsEndUser(),
                Report = await ml.GetOutletUserList(filter).ConfigureAwait(false)
            };
            return PartialView("PartialView/_OutletsOfUser", OutletUserListModel);
        }

        [HttpGet]
        [Route("Home/OutletList-Export")]
        [Route("OutletList-Export")]
        public async Task<IActionResult> _OutletUserListExport(OuletOfUsersListFilter filter)
        {
            IUserML userML = new UserML(_accessor, _env);
            IOutletML ml = new ReportML(_accessor, _env);
            filter.IsExport = true;
            var IsAdmin = _lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowRechargeReport);
            List<OutletsOfUsersList> _report;
            _report = await ml.GetOutletUserList(filter);
            DataTable dataTable = ConverterHelper.O.ToDataTable(_report);
            var removableCol = new List<string> { "ResultCode", "Msg", "_ServiceID", "_Prefix", "_UserID", "_VerifyStatus", "_RoleID", "_KYCStatus", "_OType", "_EntryBy", "_ModifyBy", "_ModifyDate", "BBPSStatus", "AEPSStatus", "PSAStatus", "DMTStatus" };
            if (!IsAdmin)
            {
                removableCol.Add("DisplayUserID");
                removableCol.Add("UserName");
                removableCol.Add("UserMobile");
                removableCol.Add("Role");
                removableCol.Add("_IsOutsider");
                removableCol.Add("_latlong");
            }
            foreach (string str in removableCol)
            {
                dataTable.Columns.Remove(str);
            }
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("OutletUserList1");
                worksheet.Cells["A1"].LoadFromDataTable(dataTable, PrintHeaders: true);
                worksheet.Row(1).Height = 20;
                worksheet.Row(1).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Row(1).Style.Font.Bold = true;
                for (var col = 1; col < dataTable.Columns.Count + 1; col++)
                {
                    worksheet.Column(col).AutoFit();
                }
                var exportToExcel = new ExportToExcel
                {
                    Contents = package.GetAsByteArray(),
                    FileName = "OutletUserList.xlsx"
                };
                return File(exportToExcel.Contents, DOCType.XlsxContentType, exportToExcel.FileName);
            }
        }

        [HttpGet]
        [Route("Report/bindApiList")]
        [Route("bindApiList")]
        public IActionResult _BindApiList()
        {
            IAPIML aPIML = new APIML(_accessor, _env);
            var _list = aPIML.GetAPIDetail().ToList();
            _list = _list.Where(c => c.IsOutletRequired && c.IsOutletManual).ToList();
            var rep = new RechargeReportModel
            {
                RechargeAPI = _list
            };
            return PartialView("PartialView/_BindApiList", rep);
        }

        [HttpGet]
        [Route("Report/OutletList-Tamplate")]
        [Route("OutletList-Tamplate")]
        public IActionResult _OutletUserListTamplate()
        {
            List<_OutletsOfUsersList> _report = new List<_OutletsOfUsersList>();
            DataTable dataTable = ConverterHelper.O.ToDataTable(_report);
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("OutletUserList");
                worksheet.Cells["A1"].LoadFromDataTable(dataTable, PrintHeaders: true);
                worksheet.Row(1).Height = 20;
                worksheet.Row(1).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Row(1).Style.Font.Bold = true;
                for (var col = 1; col < dataTable.Columns.Count + 1; col++)
                {
                    worksheet.Column(col).AutoFit();
                }
                var exportToExcel = new ExportToExcel
                {
                    Contents = package.GetAsByteArray(),
                    FileName = "OutletUserListTamplate.xlsx"
                };
                return File(exportToExcel.Contents, DOCType.XlsxContentType, exportToExcel.FileName);
            }
        }

        [HttpPost]
        [Route("Report/upload-OutletUserList")]
        [Route("upload-OutletUserList")]
        public async Task<IActionResult> UploadOutletUserListExcelAsync(IFormFile file, int id)
        {
            IOutletML ml = new ReportML(_accessor, _env);
            IResponseStatus res = new ResponseStatus();
            if (file == null || file.Length <= 0)
            {
                res.Statuscode = -1;
                res.Msg = "No file found.";
                return Json(res);
            }
            if (id < 1)
            {
                res.Statuscode = -1;
                res.Msg = "API Id not found.";
                return Json(res);
            }
            if (!Path.GetExtension(file.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                res.Statuscode = -1;
                res.Msg = "Uploaded file is not valid.Please upload .xlsx file only.";
                return Json(res);
            }
            var list = new List<_OutletsOfUsersList>();
            try
            {
                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    using (var package = new ExcelPackage(stream))
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                        var OltIndex = new OutletsOfUsersListIndex
                        {
                            OutletID = worksheet.GetColumnByName("OutletID"),
                            APIOutletID = worksheet.GetColumnByName("APIOutletID"),
                            verifyStatus = worksheet.GetColumnByName("verifyStatus"),
                            DocVerifyStatus = worksheet.GetColumnByName("DocVerifyStatus"),
                            BBPSID = worksheet.GetColumnByName("BBPSID"),
                            BBPSStatus = worksheet.GetColumnByName("BBPSStatus"),
                            AEPSID = worksheet.GetColumnByName("AEPSID"),
                            AEPSStatus = worksheet.GetColumnByName("AEPSStatus"),
                            PANRequestID = worksheet.GetColumnByName("PANRequestID"),
                            PANID = worksheet.GetColumnByName("PANID"),
                            PANStatus = worksheet.GetColumnByName("PANStatus"),
                            DMTID = worksheet.GetColumnByName("DMTID"),
                            DMTStatus = worksheet.GetColumnByName("DMTStatus"),
                            Password = worksheet.GetColumnByName("Password"),
                            Pin = worksheet.GetColumnByName("Pin")
                        };
                        var rowCount = worksheet.Dimension.Rows;
                        for (int row = 2; row <= rowCount; row++)
                        {
                            var olt = new _OutletsOfUsersList
                            {
                                OutletID = Convert.ToInt32(worksheet.Cells[row, OltIndex.OutletID].Value),
                                APIOutletID = Convert.ToString(worksheet.Cells[row, OltIndex.APIOutletID].Value).Trim(),
                                verifyStatus = Convert.ToInt32(worksheet.Cells[row, OltIndex.verifyStatus].Value),
                                DocVerifyStatus = Convert.ToInt32(worksheet.Cells[row, OltIndex.DocVerifyStatus].Value),
                                BBPSID = Convert.ToString(worksheet.Cells[row, OltIndex.BBPSID].Value).Trim(),
                                BBPSStatus = Convert.ToInt32(worksheet.Cells[row, OltIndex.BBPSStatus].Value),
                                AEPSID = Convert.ToString(worksheet.Cells[row, OltIndex.AEPSID].Value).Trim(),
                                AEPSStatus = Convert.ToInt32(worksheet.Cells[row, OltIndex.AEPSStatus].Value),
                                PANRequestID = Convert.ToInt32(worksheet.Cells[row, OltIndex.PANRequestID].Value),
                                PANID = Convert.ToString(worksheet.Cells[row, OltIndex.PANID].Value).Trim(),
                                PANStatus = Convert.ToInt32(worksheet.Cells[row, OltIndex.PANStatus].Value),
                                DMTID = Convert.ToString(worksheet.Cells[row, OltIndex.DMTID].Value).Trim(),
                                DMTStatus = Convert.ToInt32(worksheet.Cells[row, OltIndex.DMTStatus].Value),
                                Password = Convert.ToString(worksheet.Cells[row, OltIndex.Password].Value),
                                Pin = Convert.ToString(worksheet.Cells[row, OltIndex.Pin].Value)
                            };
                            list.Add(olt);
                        }
                    }
                }

                var ReqData = new OutletsReqData
                {
                    OutletsUserList = list,
                    APIId = id
                };
                res = await ml.UploadOutletUsersExcel(ReqData).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                res.Statuscode = ErrorCodes.Minus1;
                res.Msg = ErrorCodes.AnError;
            }
            return Json(res);
        }

        [HttpPost]
        [Route("Report/ApiWiseDetail")]
        [Route("/ApiWiseDetail")]
        public IActionResult _ApiWiseDetail(int oid = 0)
        {
            IOutletML outletML = new ReportML(_accessor, _env);
            var list = outletML.GetApiWiseDetail(oid);
            return PartialView("PartialView/_ApiWiseDetail", list);
        }

        [HttpGet]
        [Route("Report/ApiWiseDetail-Export")]
        [Route("ApiWiseDetail-Export")]
        public IActionResult ApiWiseDetailExport()
        {
            IUserML userML = new UserML(_accessor, _env);
            IOutletML ml = new ReportML(_accessor, _env);
            var IsAdmin = _lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowRechargeReport);
            int oid = -1;
            var _report = ml.GetApiWiseDetail(oid);
            DataTable dataTable = ConverterHelper.O.ToDataTable(_report.ToList());
            string[] removableCol = { "ResultCode", "Msg", "_ServiceID", "ID", "VerifyStatus", "BBPSStatus", "AEPSStatus", "DMTStatus", "PSAStatus", "APIID" };
            foreach (string str in removableCol)
            {
                dataTable.Columns.Remove(str);
            }
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("OutletUserList1");
                worksheet.Cells["A1"].LoadFromDataTable(dataTable, PrintHeaders: true);
                worksheet.Row(1).Height = 20;
                worksheet.Row(1).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Row(1).Style.Font.Bold = true;
                for (var col = 1; col < dataTable.Columns.Count + 1; col++)
                {
                    worksheet.Column(col).AutoFit();
                }
                var exportToExcel = new ExportToExcel
                {
                    Contents = package.GetAsByteArray(),
                    FileName = "APIWISEDETAIL.xlsx"
                };
                return File(exportToExcel.Contents, DOCType.XlsxContentType, exportToExcel.FileName);
            }
        }

        [HttpGet]
        [Route("Report/ApiList")]
        [Route("ApiList")]
        public IActionResult ApiList()
        {
            IReportML ml = new ReportML(_accessor, _env);
            var res = ml.GetApiList();
            return Json(res);
        }
        #endregion

        #region MATMRelated
        [HttpPost]
        [Route("APIUser/AddmAtmRequest")]
        public IActionResult _AddmAtm(int id)
        {
            AddmAtmModel model = new AddmAtmModel()
            {
                UserID = id
            };
            return PartialView("PartialView/_AddmAtm", model);
        }

        [HttpPost]
        [Route("APIUser/_AddmAtm")]
        public IActionResult _AddmAtm(AddmAtmModel model)
        {
            IAPIUserMiddleLayer ml = new APIUserML(_accessor, _env);
            model.LT = _lr.LoginTypeID;
            model.LoginID = _lr.UserID;
            var res = ml.AddMAtm(model);
            return Json(res);
        }

        [HttpGet]
        [Route("Home/mAtmRequests")]
        [Route("mAtmRequests")]
        public IActionResult mAtmRequests()
        {
            IUserML userML = new UserML(_lr);
            var IsAdmin = _lr.LoginTypeID == LoginType.ApplicationUser && _lr.UserID == 1 || userML.IsCustomerCareAuthorised(ActionCodes.ShowMaTMRequest);
            if (IsAdmin)
                return View("MAtmRequest", IsAdmin);
            return Ok();
        }

        [HttpPost]
        [Route("Home/mAtmRequestsList")]
        [Route("mAtmRequestList")]
        public async Task<IActionResult> _mAtmRequests([FromBody] MAtmFilterModel filter)
        {
            IUserML userML = new UserML(_lr);
            IOutletML ml = new ReportML(_accessor, _env);
            var resp = new MAtmModelResp
            {
                IsAdmin = _lr.RoleID == Role.Admin,
                MAtmModelR = await ml.GetmAtmRequestList(filter)
            };
            return PartialView("PartialView/_mAtmRequests", resp);
        }


        [HttpGet]
        [Route("Home/_mAtmRequestsListExport")]
        [Route("_mAtmRequestsListExport")]
        public async Task<IActionResult> _mAtmRequestsListExport(MAtmFilterModel filter)
        {
            IUserML userML = new UserML(_accessor, _env);
            IOutletML ml = new ReportML(_accessor, _env);
            filter.IsExport = true;
            var IsAdmin = _lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser;
            List<MAtmModel> _report;
            _report = await ml.GetmAtmRequestList(filter);
            DataTable dataTable = ConverterHelper.O.ToDataTable(_report);
            var removableCol = new List<string> { "StatusCode", "Msg", "UserID", "RoleID", "RoleName", "PartnerName", "mAtamStatus", "ID", "Name", "Company", "EmailId", "OutletName" };
            if (!IsAdmin)
            {
                removableCol.Add("DisplayUserID");
                removableCol.Add("UserName");
                removableCol.Add("UserMobile");
                removableCol.Add("Role");
                removableCol.Add("_IsOutsider");
                removableCol.Add("_latlong");
            }
            foreach (string str in removableCol)
            {
                dataTable.Columns.Remove(str);
            }

            //Change column sequence
            dataTable.Columns["ExID"].SetOrdinal(0);
            dataTable.Columns["mAtamSerialNo"].SetOrdinal(1);
            dataTable.Columns["ExName"].SetOrdinal(2);
            dataTable.Columns["MobileNo"].SetOrdinal(3);
            dataTable.Columns["Address"].SetOrdinal(4);
            dataTable.Columns["State"].SetOrdinal(5);
            dataTable.Columns["City"].SetOrdinal(6);
            dataTable.Columns["Pincode"].SetOrdinal(7);
            dataTable.Columns["Pan"].SetOrdinal(8);
            dataTable.Columns["KYCDoc"].SetOrdinal(9);

            //Change column name
            dataTable.Columns["ExID"].ColumnName = "Existing Merchant login Id (Its should be already registered via api/Bulk)";
            dataTable.Columns["mAtamSerialNo"].ColumnName = "Micro ATM Serial Number";
            dataTable.Columns["ExName"].ColumnName = "Merchant Name";
            dataTable.Columns["MobileNo"].ColumnName = "Merchant phone Number";
            dataTable.Columns["Address"].ColumnName = "Merchant address";
            dataTable.Columns["City"].ColumnName = "City/District";
            dataTable.Columns["Pan"].ColumnName = "Pan card Number";
            dataTable.Columns["KYCDoc"].ColumnName = "Kyc documents";

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("OutletUserList1");
                worksheet.Cells["A1"].LoadFromDataTable(dataTable, PrintHeaders: true);
                worksheet.Row(1).Height = 20;
                worksheet.Row(1).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Row(1).Style.Font.Bold = true;
                for (var col = 1; col < dataTable.Columns.Count + 1; col++)
                {
                    worksheet.Column(col).AutoFit();
                }
                var exportToExcel = new ExportToExcel
                {
                    Contents = package.GetAsByteArray(),
                    FileName = "miniATMRequestExcel.xlsx"
                };
                return File(exportToExcel.Contents, DOCType.XlsxContentType, exportToExcel.FileName);
            }
        }

        [HttpPost]
        [Route("Home/UpdatemAtmRequests")]
        [Route("UpdatemAtmRequests")]
        public async Task<IActionResult> UpdatemAtmRequests(int id, int status)
        {
            IUserML userML = new UserML(_lr);
            IOutletML ml = new ReportML(_accessor, _env);

            var Report = await ml.UpdatemAtmRequestList(id, status);
            return Json(Report);
        }
        #endregion

        #region CallbackReport
        [HttpGet]
        [Route("Report/CallbackData")]
        [Route("CallbackData")]
        public IActionResult CallbackData()
        {
            if (_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowCallbackRequests))
            {
                return View();
            }
            else
            {
                return BadRequest();
            }
        }
        [HttpPost]
        [Route("Report/_CallbackData")]
        [Route("_CallbackData")]
        public IActionResult _CallbackData(int t, string s)
        {
            if (_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowCallbackRequests))
            {
                IReportML rMl = new ReportML(_accessor, _env);
                var lst = rMl.CallbackReport(t, s);
                return PartialView("PartialView/_CallbackData", lst);
            }
            else
            {
                return BadRequest();
            }
        }
        #endregion

        #region fundrequest
        [HttpPost]
        [Route("Fund-Request")]
        public IActionResult _FundRequest(int U)
        {
            IBankML operation = new BankML(_accessor, _env);
            var userML = new UserML(_accessor, _env);
            var model = new PaymentRequestModel
            {
                BankList = operation.Banks(U),
                BonafideAccountList = userML.GetBonafideAccount(new CommonReq
                {
                    CommonInt = 50,
                    LoginID = _lr.UserID,
                    LoginTypeID = _lr.LoginTypeID
                })
            };
            model.userBalnace = userML.GetUserBalnace(0);
            return PartialView("PartialView/_FundRequest", model);
        }
        [HttpPost]
        [Route("Payment-Mode")]
        public IActionResult _BindPaymentMode(int BankID)
        {
            var userML = new UserML(_accessor, _env);
            var model = new PaymentRequestModel
            {
                PaymentModeList = userML.PaymentModes(BankID)
            };
            return Json(model);
        }
        [HttpPost]
        [Route("upload-Receipt")]
        public IActionResult UploadReceipt(IFormFile file, int BankID)
        {
            IBannerML bannerML = new ResourceML(_accessor, _env);
            var _res = bannerML.UploadReceipt(file, BankID, _lr);
            return Json(_res);
        }
        [HttpGet]
        [Route("fund-request")]
        public IActionResult FundRequest()
        {
            IFundProcessML fundProcessML = new UserML(_accessor, _env);
            return View(fundProcessML.FundRequestToUser());
        }
        [HttpPost]
        [Route("fund-lst")]
        public IActionResult _FundRequestBank(int U)
        {
            IBankML operation = new BankML(_accessor, _env);
            var smartCollectML = new SmartCollectML(_accessor, _env);
            var fundPageModel = new FundRequestPageModel
            {
                bankList = operation.Banks(U),
                userSmartDetail = smartCollectML.GetUserSmartDetails(_lr.UserID, _lr.UserID).USDList
            };
            return PartialView("PartialView/_FundRequestBank", fundPageModel);
        }
        [HttpPost]
        [Route("Update-SmartCollect")]
        public IActionResult UpdateSmartCollect()
        {
            SmartCollectML smartCollectML = new SmartCollectML(_accessor, _env);
            var res = smartCollectML.UpdateSmartCollectDetailOfUser(_lr.UserID, _lr.UserID);
            return Json(res);
        }
        [HttpPost]
        [Route("Fund-Request-Submit")]
        public IActionResult FundRequestSubmit([FromBody] FundRequest fr)
        {
            IFundProcessML operation = new UserML(_accessor, _env);
            return Json(operation.FundRequestOperation(fr));
        }
        #endregion

        #region ChangePasswordSection
        [HttpPost]
        [Route("Change-Password")]
        public IActionResult _ChangePassword(bool IsMandate)
        {
            return PartialView("PartialView/_ChangePassword", IsMandate);
        }
        [HttpPost]
        [Route("ChangePassword")]
        public IActionResult ChangePassword([FromBody] ChangePassword obj)
        {
            IUserML userML = new UserML(_accessor, _env);
            return Json(userML.ChangePassword(obj));
        }
        [HttpPost]
        [Route("Change-Pin")]
        public IActionResult _ChangePin(bool IsMandate)
        {
            return PartialView("PartialView/_ChangePin", IsMandate);
        }
        [HttpPost]
        [Route("ChangePin")]
        public IActionResult ChangePin([FromBody] ChangePassword obj)
        {
            IUserML userML = new UserML(_accessor, _env);
            return Json(userML.ChangePin(obj));
        }

        #endregion

        #region BalanceSection

        [HttpPost]
        [Route("mybal")]
        public IActionResult MyBalance()
        {
            IUserML userML = new UserML(_accessor, _env);
            var ub = userML.GetUserBalnace(0);
            if (ApplicationSetting.IsAutoBilling)
            {
                userML.GetAutoBillingProcess(_lr.UserID);
            }
            StringBuilder sb = new StringBuilder();
            sb.Append("[");
            sb.AppendFormat("PREPAID : {0}", ub.Balance);
            if (ub.IsBBalance && _lr.RoleID != Role.Admin)
            {
                sb.AppendFormat(" | MINIBANK : {0}", ub.BBalance);
            }
            if (ub.IsUBalance)
            {
                sb.AppendFormat(" | UTILITY : {0}", ub.UBalance);
            }
            if (ub.IsPacakgeBalance)
            {
                sb.AppendFormat(" | PACKAGE : {0}", ub.PacakgeBalance);
            }
            if (ub.IsCBalance)
            {
                sb.AppendFormat(" | CARD : {0}", ub.CBalance);
            }
            if (ub.IsIDBalance)
            {
                sb.AppendFormat(" | ID : {0}", ub.IDBalnace);
            }
            if (ApplicationSetting.IsAccountStatement)
            {
                sb.AppendFormat(" | Out Standing : {0}", ub.OSBalance);
            }
            sb.Append("]");
            return Json(new { sb = sb.ToString(), ub.Balance, ub.IsP, ub.IsPN, ub.IsLowBalance, IsAddMoneyEnable = ApplicationSetting.IsAddMoneyEnable && _lr.WID == ErrorCodes.One, ub.IsShowLBA });
        }
        [HttpPost]
        [Route("my-bal")]
        public IActionResult _MyBalance()
        {
            if (_lr.RoleID != Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser)
            {
                IOperatorML opML = new OperatorML(_accessor, _env);
                IUserML userML = new UserML(_accessor, _env);
                var mdl = new MyBalanceViewModel
                {
                    userBalnace = userML.GetUserBalnace(0),
                    transactionModes = opML.GetOperatorsActive(OPTypes.RealTimeBank),
                    moveToWalletMappings = userML.GetMoveToWalletsMap()
                };
                return PartialView("PartialView/_MyBalance", mdl);
            }
            return Ok();
        }
        [HttpPost]
        [Route("move-to-wallet")]
        public async Task<IActionResult> MoveToWallet([FromBody] CommonReq commonReq)
        {
            IFundProcessML userML = new UserML(_accessor, _env);
            var res = await userML.MoveToWallet(commonReq).ConfigureAwait(false);
            return Json(res);
        }
        [HttpPost]
        [Route("Trans-Mode")]
        public IActionResult TransMode([FromBody] CommonReq commonReq)
        {
            IUserML userML = new UserML(_accessor, _env);
            var res = userML.GetTransactionMode(commonReq);
            return Json(res);
        }


        #endregion

        #region WalletRequest Approval Section

        [HttpGet]
        [Route("WalletRequestReport")]
        public IActionResult WalletRequestReport()
        {
            //This report is for parent only
            var IsAdmin = _lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.Admin;
            IUserML userML = new UserML(_accessor, _env);
            IOperatorML opML = new OperatorML(_accessor, _env);
            ViewBag.TransModes = opML.GetOperatorsActive(OPTypes.RealTimeBank);
            return View(IsAdmin);
        }
        [HttpPost]
        [Route("Wallet-Request-Report")]
        public IActionResult _WallRequestReport(WalletRequest req)
        {
            //This report is for parent only
            if (_lr.RoleID != Role.Admin)
            {
                req.Mobile = _lr.MobileNo;
            }
            IReportML rml = new ReportML(_accessor, _env);
            IEnumerable<WalletRequest> list = rml.GetWalletRequestReport(req);
            return PartialView("PartialView/_WalletRequestReport", list);
        }
        [HttpGet]
        [Route("Wallet-Bene-List")]
        public IActionResult _GetBeneListExcel(WalletRequest req)
        {
            //This report is for parent only
            if (_lr.RoleID != Role.Admin)
            {
                req.Mobile = _lr.MobileNo;
            }
            req.CommonInt = 2;
            IReportML rml = new ReportML(_accessor, _env);
            DataTable dt = rml.GetBeneficieryList(req);
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("BeneList");
                worksheet.Cells["A1"].LoadFromDataTable(dt, PrintHeaders: true);
                worksheet.Row(1).Height = 20;
                worksheet.Row(1).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Row(1).Style.Font.Bold = true;

                for (var col = 1; col < dt.Columns.Count + 1; col++)
                {
                    worksheet.Column(col).AutoFit();
                }
                var exportToExcel = new ExportToExcel
                {
                    Contents = package.GetAsByteArray(),
                    FileName = "BeneList_" + DateTime.Now.ToString("dd/MM/yyyy") + ".xlsx"
                };
                return File(exportToExcel.Contents, DOCType.XlsxContentType, exportToExcel.FileName);
            }
        }

        [HttpGet]
        [Route("WalletRequestApproval")]
        public IActionResult WalletRequestApproval()
        {
            if (_lr.RoleID == Role.Admin)
                return View();
            return Ok();
        }
        [HttpPost]
        [Route("Wallet-Request-Approval")]
        public IActionResult _WalletRequestApproval()
        {
            if (_lr.RoleID == Role.Admin)
            {
                WalletRequest req = new WalletRequest();
                req.CommonInt = 1;
                IReportML rml = new ReportML(_accessor, _env);
                IEnumerable<WalletRequest> list = rml.GetWalletRequestReport(req);
                return PartialView("PartialView/_WalletRequestApproval", list);
            }
            return Ok();
        }
        [HttpPost]
        [Route("wra-realtime")]
        public async Task<IActionResult> WRARealTime(int id)
        {
            IUserML userML = new UserML(_accessor, _env);
            if (_lr.RoleID == Role.Admin)
            {
                var res = await userML.CallBankTransfer(new BankServiceReq
                {
                    LoginID = _lr.UserID,
                    RequestModeID = RequestMode.PANEL,
                    WalletRequestID = id
                }).ConfigureAwait(false);
                return Json(res);
            }
            else
            {
                return Json(new ResponseStatus { Statuscode = ErrorCodes.Minus1, Msg = ErrorCodes.AuthError });
            }
        }
        #endregion

        #region UpdateKYCSection
        [Route("/CheckKycStatus")]
        public IActionResult CheckKycStatus(int UserID)
        {
            IKYCML userML = new UserML(_accessor, _env);
            var _res = userML.CheckKycStatus(UserID);
            return Json(_res);
        }

        [HttpPost]
        [Route("KYC/DocumentDetails")]
        public IActionResult _DocumentDetails(int uid, int oid = 0)
        {
            IKYCML userML = new UserML(_accessor, _env);

            if (oid > 0)
            {
                var list = userML.GetDocumentsForApproval(uid, oid);
                return PartialView("PartialView/_showDocuments", list);
            }
            else
            {
                var list = userML.GetDocuments(uid);
                return PartialView("PartialView/_DocumentsDetail", list);
            }
        }
        [HttpPost]
        [Route("KYC/Upload-File")]
        public IActionResult UploadFile(int dtype, IFormFile file, int uid)
        {
            IKYCML userML = new UserML(_accessor, _env);
            var _res = userML.UploadDocuments(file, dtype, uid);
            return Json(_res);
        }
        [HttpPost]
        [Route("KYC/updatekycsts")]
        public IActionResult UpdateKYCStatus(int outletid, int sts)
        {
            var kYCStatusReq = new KYCStatusReq
            {
                OutletID = outletid,
                KYCStatus = sts
            };
            IKYCML userML = new UserML(_accessor, _env);
            var _res = userML.ChangeKYCStatus(kYCStatusReq);
            return Json(_res);
        }
        #endregion

        #region NewsSection     
        [Route("GetNewsByRole")]
        [HttpPost]
        public IActionResult GetNewsByRole()
        {
            var model = new List<News>();
            INewsML settings = new NewsML(_accessor, _env);
            if (_lr.RoleID > 0)
            {
                model = settings.GetNewsByRole(_lr.RoleID);
            }
            string NewsDetail = "";
            if (model.Any())
            {
                NewsDetail = string.Join(" || ", model.Select(x => x.NewsDetail)).Replace("</p> || <p>", " || ").Replace("<p>", "<span>").Replace("</p>", "</span>").Replace("<span>&nbsp;</span>", "");
            }

            return Json(new { NewsDetail });
        }

        [Route("GetDownOP")]
        [HttpPost]
        public async Task<IActionResult> GetDownOP()
        {
            IOperatorML operatorML = new OperatorML(_accessor, _env);
            var dops = await operatorML.GetDowns();
            return Json(dops);
        }
        #endregion

        #region Display
        [HttpGet]
        [Route("DisplayReport/{Id}")]
        public IActionResult Display(int id)
        {
            if ((_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.Admin || userML.IsCustomerCareAuthorised(ActionCodes.ShowRefundRequestHistory)))
            {
                return View("DisplayPending");
            }
            else
            {
                return BadRequest();
            }
        }
        [HttpPost]
        [Route("Home/DisplayPending")]
        [Route("DisplayPending")]
        public async Task<IActionResult> DisplayPending(int id)
        {
            if ((_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.Admin || userML.IsCustomerCareAuthorised(ActionCodes.ShowRefundRequestHistory)))
            {
                IRechargeReportML ml = new ReportML(_accessor, _env);
                var lst = await ml.GetDisplayLive(new RechargeReportFilter
                {
                    Status = id
                }).ConfigureAwait(false);
                if (id == 1)
                    lst = lst.OrderBy(x => x.TID).ToList();
                return PartialView("PartialView/_DisplayPending", lst);
            }
            else
            {
                return BadRequest();
            }
        }
        [HttpGet]
        [Route("DisplayLive")]
        public IActionResult DisplayLive()
        {
            if ((_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.Admin || userML.IsCustomerCareAuthorised(ActionCodes.ShowRefundRequestHistory)))
            {
                return View();
            }
            else
            {
                return BadRequest();
            }
        }
        [HttpPost]
        [Route("Home/DisplayLive")]
        [Route("DisplayLive")]
        public async Task<IActionResult> _DisplayLive()
        {
            if ((_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.Admin || userML.IsCustomerCareAuthorised(ActionCodes.ShowRefundRequestHistory)))
            {
                IRechargeReportML ml = new ReportML(_accessor, _env);
                return PartialView("PartialView/_DisplayLive", await ml.GetDisplayLive(new RechargeReportFilter
                {
                    Status = 0
                }).ConfigureAwait(false));
            }
            else
            {
                return BadRequest();
            }
        }
        #endregion

        #region BannerSection
        [HttpGet]
        [Route("BannerMaster")]
        public IActionResult BannerMaster()
        {
            IOperatorML mL = new OperatorML(_accessor, _env);
            var OpTypeList = mL.GetOptypes().Where(x => x.IsB2CVisible == true);
            var rep = new RechargeReportModel
            {
                Operators = mL.GetOperators(OPTypes.DTHConnection),
                OpTypes = OpTypeList,

            };
            return View(rep);
        }
        [HttpPost]
        [Route("_BannerMaster")]
        public IActionResult _BannerMaster()
        {

            IBannerML bannerML = new ResourceML(_accessor, _env);
            var resp = bannerML.GetBanners(_lr.WID.ToString());
            return PartialView("PartialView/_BannerMaster", resp);
        }
        [HttpPost]
        [Route("_SiteBannerMaster")]
        public IActionResult _SiteBannerMaster()
        {

            IBannerML bannerML = new ResourceML(_accessor, _env);
            var resp = bannerML.SiteGetBanners(_lr.WID.ToString());
            return PartialView("PartialView/_SiteBannerMaster", resp);
        }



        [HttpPost]
        [Route("upload-web-banner")]
        public IActionResult UploadWebBanner(IFormFile file)
        {
            IBannerML bannerML = new ResourceML(_accessor, _env);
            var _res = bannerML.SiteUploadBanners(file, _lr.WID.ToString(), _lr);
            return Json(_res);
        }
        [HttpPost]
        [Route("upload-banner")]
        public IActionResult UploadBanner(IFormFile file, string Url)
        {
            IBannerML bannerML = new ResourceML(_accessor, _env);
            var _res = bannerML.UploadBanners(file, _lr.WID.ToString(), Url, _lr);
            return Json(_res);
        }

        [HttpPost]
        [Route("upload-b2cbanner")]
        public IActionResult UploadB2CBanner(IFormFile file, int opType)
        {
            IBannerML bannerML = new ResourceML(_accessor, _env);
            var _res = bannerML.UploadB2CBanners(file, _lr.WID.ToString(), opType, _lr);
            return Json(_res);
        }


        [HttpPost]
        [Route("rem-banr")]
        public IActionResult RemoveBanner(string id)
        {
            IBannerML bannerML = new ResourceML(_accessor, _env);
            var _res = bannerML.RemoveBanners(id, _lr.WID.ToString(), _lr);
            return Json(_res);
        }

        [Route("rem-Sitebanr")]
        public IActionResult RemoveSiteBanner(string id)
        {
            IBannerML bannerML = new ResourceML(_accessor, _env);
            var _res = bannerML.SiteRemoveBanners(id, _lr.WID.ToString(), _lr);
            return Json(_res);
        }

        #region Amit
        //[HttpGet]
        //[Route("B2CBanner")]
        //public IActionResult B2CBanner()
        //{
        //    IOperatorML mL = new OperatorML(_accessor, _env);
        //    var OpTypeList = mL.GetOptypeInSlab();
        //    return View(OpTypeList);
        //}

        [HttpPost]
        [Route("get-B2CBanner")]
        public IActionResult _B2CBannerMaster(int opType)
        {
            IBannerML bannerML = new ResourceML(_accessor, _env);
            var resp = bannerML.GetB2CBanners(_lr.WID.ToString(), opType);
            return PartialView("PartialView/_B2CBannerMaster", resp);
        }

        [HttpPost]
        [Route("rm-B2Cbanner")]
        public IActionResult RemoveB2CBanner(string id, int opType)
        {
            IBannerML bannerML = new ResourceML(_accessor, _env);
            var _res = bannerML.RemoveB2CBanners(id, opType, _lr.WID.ToString(), _lr);
            return Json(_res);
        }
        #endregion
        #endregion

        #region SettlementRegion
        [HttpGet]
        [Route("AdminSettlement")]
        public IActionResult AdminSettlement()
        {
            IUserML userml = new UserML(_accessor, _env);
            var Model = new LedgerReportModel
            {
                HaveChild = !userml.IsEndUser() || userml.IsCustomerCareAuthorised(ActionCodes.ShowUserLedger),
                SelfMobile = _lr.MobileNo ?? "",
                userBalnace = userml.GetUserBalnace(0)
            };
            return View(Model);
        }


        [HttpPost]
        [Route("Report/admin-sett")]
        [Route("admin-sett")]
        public IActionResult _AdminSettlement(string f, string t, int w)
        {
            IReportML reportML = new ReportML(_accessor, _env);
            var res = reportML.GetAdminSettlement(f, t, w);
            return PartialView("PartialView/_AdminSettlement", res);
        }

        [HttpGet]
        [Route("/get-AdminSettlement-Excel")]
        public async Task<IActionResult> _AdminSettlementExport(string f, string t, int w)
        {
            IReportML ml = new ReportML(_accessor, _env);
            var _report = await Task.FromResult(ml.GetAdminSettlement(f, t, w));
            var dataTable = ConverterHelper.O.ToDataTable(_report);
            string[] removableCol = { };
            foreach (string str in removableCol)
            {
                if (dataTable.Columns.Contains(str))
                {
                    dataTable.Columns.Remove(str);
                }
            }
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("AdminSettlement");
                worksheet.Cells["A1"].LoadFromDataTable(dataTable, PrintHeaders: true);
                int rowindex = 2;
                foreach (DataRow row in dataTable.Rows)
                {
                    if (rowindex == 2)
                    {

                    }
                    rowindex++;
                }
                for (var col = 1; col < dataTable.Columns.Count + 1; col++)
                {
                    worksheet.Column(col).AutoFit();
                }
                var exportToExcel = new ExportToExcel
                {
                    Contents = package.GetAsByteArray(),
                    FileName = "AdminSettlement.xlsx"
                };
                return File(exportToExcel.Contents, DOCType.XlsxContentType, exportToExcel.FileName);
            }
        }

        [Route("UserSettlement")]
        public IActionResult UserSettlement()
        {
            IUserML userml = new UserML(_accessor, _env);
            var Model = new LedgerReportModel
            {
                HaveChild = !userml.IsEndUser() || userml.IsCustomerCareAuthorised(ActionCodes.ShowUserLedger),
                SelfMobile = _lr.MobileNo ?? "",
                IsAdmin = _lr.RoleID == Role.Admin ? true : false,
                userBalnace = userml.GetUserBalnace(0)
            };
            return View(Model);
        }

        [HttpPost]
        [Route("Get-UserSettlement")]
        public async Task<IActionResult> GetUserSettlement([FromBody] SettlementFilter filter)
        {
            IReportML ml = new ReportML(_accessor, _env);
            var _report = await ml.GetUserSettlement(filter);
            return PartialView("PartialView/_UserSettlement", _report);
        }

        [HttpGet]
        [Route("/get-UserSettlement-Excel")]
        public async Task<IActionResult> _UserSettlementExport(SettlementFilter filter)
        {
            IReportML ml = new ReportML(_accessor, _env);
            var _report = await ml.GetUserSettlement(filter);
            var dataTable = ConverterHelper.O.ToDataTable(_report);
            string[] removableCol = { "Prefix", "UserID", "EntryDate" };
            var Contents = EportExcel.o.GetFile(dataTable, removableCol);
            return File(Contents, DOCType.XlsxContentType, "userSettlement.xlsx");
        }
        #endregion

        #region LogDetails
        [HttpGet]
        [Route("Report/APIURLHitting")]
        [Route("APIURLHitting")]
        [Route("Report/LogDetails")]
        [Route("LogDetails")]
        public IActionResult LogDetails()
        {
            return View();
        }

        #endregion

        #region Others

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
        [Route("Login/Logout")]
        [Route("Logout")]
        public async Task<IActionResult> Logout(int ULT, int UserID, int SType)
        {
            IResponseStatus responseStatus = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.RoleID > 0)
            {
                LogoutReq logoutReq = new LogoutReq
                {
                    LT = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    ULT = ULT == 0 ? _lr.LoginTypeID : ULT,
                    UserID = UserID == 0 ? _lr.UserID : UserID,
                    SessID = _lr.SessID,
                    SessionType = SType == 0 ? SessionType.Single : SType,
                    RequestMode = RequestMode.PANEL
                };
                ILoginML loginML = new LoginML(_accessor, _env);
                responseStatus = await loginML.DoLogout(logoutReq);
                if (ClearCurrentSession())
                {
                    return Json(responseStatus);
                }
                else
                {
                    return Json(responseStatus);
                }
            }
            return Json(responseStatus);
        }

        [HttpPost]
        [Route("Report/ServiceChartData")]
        [Route("ServiceChartData")]
        public IActionResult GetTodaySummaryChart()
        {
            IReportML user = new ReportML(_accessor, _env);
            return Json(user.GetTodaySummaryChart());
        }

        private bool ClearCurrentSession()
        {
            try
            {
                HttpContext.Session.Clear();
                CookieHelper cookie = new CookieHelper(_accessor);
                cookie.Remove(SessionKeys.AppSessionID);
                return true;
            }
            catch (Exception)
            {
            }
            return false;
        }
        #endregion

        #region NumberSeries Section
        [HttpGet]
        [Route("Home/NumberSeries")]
        [Route("NumberSeries")]
        public IActionResult NumberSeries()
        {
            IOperatorML opML = new OperatorML(_accessor, _env);
            return View(opML.GetOperatorsByGroup(1));
        }
        [HttpPost]
        [Route("Home/_NumberSeries")]
        [Route("_NumberSeries")]
        public IActionResult _NumberSeries(int OID)
        {
            IReportML rML = new ReportML(_accessor, _env);
            NumberSeriesListWithCircle _res = rML.GetNumberSeriesListWithCircles(OID);
            return PartialView("PartialView/_NumberSeries", _res);
        }
        [HttpPost]
        [Route("Home/num-u")]
        [Route("num-u")]
        public IActionResult _UpdateNumberSeries(int o, int c, string n, string _n, char f, int i)
        {
            IReportML rML = new ReportML(_accessor, _env);
            ResponseStatus _res = rML.UpdateNumberSeries(o, c, n, _n, f, i);
            if (_res.Flag.In('D', 'R', 'U') && f == 48)
                return Json(new { _res.Statuscode, _res.Msg, _res.Flag, eyed = _res.CommonInt });
            else
                return Json(new { _res.Statuscode, _res.Msg });
        }

        [HttpPost]
        [Route("Home/check-num-s")]
        [Route("check-num-s")]
        public IActionResult CheckNumberSeries(string n)
        {
            IReportML rML = new ReportML(_accessor, _env);
            var _res = rML.CheckNumberSeriesExist(n);
            return Json(new { _res.Statuscode, _res.Msg, o = _res.CommonInt, c = _res.CommonInt2 });
        }
        #endregion

        #region AddMoney
        [HttpGet]
        [Route("addmoney")]
        public IActionResult AddMoney()
        {
            if (_lr.RoleID != Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser && ApplicationSetting.IsAddMoneyEnable)
            {
                IOperatorML opML = new OperatorML(_accessor, _env);
                var userML = new UserML(_accessor, _env);
                var pGDisplayModel = new PGDisplayModel
                {
                    UB = userML.GetUserBalnace(_lr.UserID),
                    modes = opML.GetPaymentModesOp(_lr.UserID)
                };
                return View(pGDisplayModel);
            }
            return Ok();
        }
        [HttpPost]
        [Route("choose-pg")]
        public IActionResult _ChoosePaymentGateway(int id, int a, int w)
        {
            if (_lr.RoleID != Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser && ApplicationSetting.IsAddMoneyEnable)
            {
                IPaymentGatewayML gatewayML = new PaymentGatewayML(_accessor, _env);
                var pGDisplayModel = new PGDisplayModel
                {
                    OID = id,
                    Amount = a,
                    WalletID = w,
                    PGs = (id > 0 && a > 0 && w > 0 ? gatewayML.GetPGDetailsUser(_lr.WID, false) : null)
                };
                return PartialView("PartialView/_ChoosePaymentGateway", pGDisplayModel);
            }
            return Ok();
        }
        [HttpPost]
        [Route("choose-pg-upi")]
        public IActionResult _ChoosePaymentGatewayUPI(int id, int a, int w, string vpa)
        {
            if (_lr.RoleID != Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser && ApplicationSetting.IsAddMoneyEnable)
            {
                IPaymentGatewayML gatewayML = new PaymentGatewayML(_accessor, _env);
                var pGDisplayModel = new PGDisplayModel
                {
                    OID = id,
                    Amount = a,
                    WalletID = w,
                    VPA = vpa,
                    PGs = (id > 0 && a > 0 && w > 0 ? gatewayML.GetPGDetailsUser(_lr.WID, true) : null)
                };
                if (pGDisplayModel.PGs.Count() > 1)
                {
                    return PartialView("PartialView/_ChoosePaymentGatewayUPI", pGDisplayModel);
                }
                else
                {
                    return Json(pGDisplayModel);
                }
            }
            return Ok();
        }
        [HttpGet]
        [Route("redirect-pg")]
        public IActionResult PGRedirect(int id, int a, int w, int pg)
        {
            if (_lr.RoleID != Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser && ApplicationSetting.IsAddMoneyEnable)
            {
                var res = new PGModelForRedirection();
                try
                {
                    ILoginML loginML = new LoginML(_accessor, _env);
                    var _WInfo = loginML.GetWebsiteInfo();
                    IPaymentGatewayML gatewayML = new PaymentGatewayML(_accessor, _env);
                    res = gatewayML.IntiatePGTransactionForWeb(_lr.UserID, a, pg, id, w, _WInfo.WID == 1 ? _WInfo.AbsoluteHost : _WInfo.MainDomain, string.Empty);
                }
                catch (Exception ex)
                {
                    var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                    {
                        ClassName = GetType().Name,
                        FuncName = "ReportController -->IntiatePGTransactionForWeb",
                        Error = ex.Message,
                        LoginTypeID = LoginType.ApplicationUser,
                        UserId = 1
                    });
                    res = new PGModelForRedirection
                    {
                        Statuscode = ErrorCodes.Minus1,
                        Msg = "Error occured in redirect-pg",
                    };
                }
                return View(res);
            }
            return BadRequest(new { msg = "Unautorised request" });
        }
        [HttpGet]
        [Route("redirect-upi")]
        public IActionResult PGRedirectUPI(int id, int a, int w, int pg, string vpa)
        {
            if (_lr.RoleID != Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser && ApplicationSetting.IsAddMoneyEnable && ApplicationSetting.IsUPI)
            {
                ILoginML loginML = new LoginML(_accessor, _env);
                var _WInfo = loginML.GetWebsiteInfo();
                IPaymentGatewayML gatewayML = new PaymentGatewayML(_accessor, _env);
                var res = gatewayML.IntiatePGTransactionForWeb(_lr.UserID, a, pg, id, w, _WInfo.WID == 1 ? _WInfo.AbsoluteHost : _WInfo.MainDomain, vpa);
                return View(res);
            }
            return BadRequest(new { msg = "Unautorised request" });
        }
        [HttpGet]
        [Route("addmoneyUPI")]
        public IActionResult AddMoneyUPI()
        {
            if (_lr.RoleID != Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser && ApplicationSetting.IsAddMoneyEnable)
            {
                IOperatorML opML = new OperatorML(_accessor, _env);
                var userML = new UserML(_accessor, _env);

                var pGDisplayModel = new PGDisplayModel
                {
                    UB = userML.GetUserBalnace(_lr.UserID),
                    modes = opML.GetOperators(OPTypes.UPI)
                };
                //IPaymentGatewayML gatewayML = new PaymentGatewayML(_accessor, _env);
                //var gtres = gatewayML.GetPGDetailsUser(_lr.WID, true);
                //if (gtres.Any())
                //{
                //    pGDisplayModel.UPGID = gtres.Select(x => x.ID).FirstOrDefault();
                //}
                return View(pGDisplayModel);
            }
            return Ok();
        }
        [HttpPost]
        [Route("paywith-upi")]
        public IActionResult PaywithUPI(int w, int a, string upiid, int o, int upgid)
        {
            IUPIPaymentML uPIPaymentML = new PaymentGatewayML(_accessor, _env);
            var res = uPIPaymentML.InitiateUPIPaymentForWeb(_lr.UserID, a, upgid, o, w, upiid);
            res.VendorID = string.Empty;
            res.BankRRN = string.Empty;
            return Json(res);
            //return Json(new CollectUPPayResponse
            //{
            //    Statuscode=ErrorCodes.One,
            //    Msg=nameof(ErrorCodes.Transaction_Successful)
            //});
        }
        [HttpPost]
        [Route("upi-status-check")]
        public IActionResult UPIStatusCheck(int TID)
        {
            IUPIPaymentML uPIPaymentML = new PaymentGatewayML(_accessor, _env);
            var res = uPIPaymentML.GetUPIStatusFromDB(TID);
            return Json(res);
        }
        [HttpPost]
        [Route("verify-upi")]
        public IActionResult VerifyUPI(string UPIID)
        {
            if (ApplicationSetting.IsAddMoneyEnable && ApplicationSetting.IsUPI)
            {
                IVerificationML verification = new VerificationML(_accessor, _env);
                var req = new VerificationInput
                {
                    UserID = _lr.UserID,
                    AccountNo = UPIID,
                    SPKey = SPKeys.UpiVerification,
                    RequestModeID = _lr.LoginTypeID
                };
                var resp = verification.UPIVerification(req);
                return Json(new { AccHold = resp.AccountHolder ?? string.Empty });
            }
            return Ok(new { AccHold = "" });
        }
        #endregion

        #region UserReports
        [HttpGet]
        [Route("/GetUsersList")]
        public IActionResult GetUsersList(CommonReq req)
        {
            return View();
        }
        [HttpPost]
        [Route("/_Get-Users-List")]
        public IActionResult _GetUsersList(CommonReq req)
        {
            IReportML ml = new ReportML(_accessor, _env);
            var _res = ml._GetUserList(req);
            return PartialView("PartialView/_GetUsersList", _res);
        }
        [HttpPost]
        [Route("/_update-IsViewd")]
        public IActionResult _UpdateUser(int UserID)
        {
            IReportML ml = new ReportML(_accessor, _env);
            var _res = ml.UpdateUser(UserID);
            return Json(_res);
        }
        #endregion

        #region TopPerformer
        [HttpGet]
        [Route("/TopPerformer")]
        public IActionResult TopPerformer()
        {
            IOperatorML ml = new OperatorML(_accessor, _env);
            var _services = ml.GetServices().Where(x => x.IsVisible == true);
            return View(_services);
        }

        [HttpPost]
        [Route("/_TopPerformer")]
        public IActionResult _TopPerformer(CommonReq req)
        {
            IReportML ml = new ReportML(_accessor, _env);
            var _res = ml.GetTopPerformer(req);
            return PartialView("PartialView/_TopPerformer", _res);
        }

        #endregion


        #region PESApproval
        [HttpPost]
        [Route("upload-PESDocument")]
        public IActionResult UploadPESDocument(List<IFormFile> file, int TID)
        {
            IBannerML bannerML = new ResourceML(_accessor, _env);
            var _res = bannerML.UploadPESDocument(file, _lr.WID, _lr, TID);
            return Json(_res);
        }
        #endregion



        #region PrimarySecondaryTertiary
        [HttpGet]
        [Route("PSTReport")]
        public IActionResult PrimarySecoundaryReport()
        {
            IUserML userML = new UserML(_lr);
            var salesSummaryModel = new SalesSummaryModel
            {
                IsAdmin = _lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowUserSalesSummary),
                UserMobile = _lr.LoginTypeID == LoginType.ApplicationUser ? _lr.MobileNo : "AN1",
                UserID = _lr.LoginTypeID == LoginType.ApplicationUser ? _lr.UserID : 1,
            };
            return View(salesSummaryModel);
        }

        [HttpPost]
        [Route("Primary-Secoundary-Report")]
        public async Task<IActionResult> _GetPrimarySecoundaryReport([FromBody] CommonFilter filter)
        {
            IReportML ml = new ReportML(_accessor, _env);
            var _resp = await ml.GetPrimarySecoundaryReport(filter).ConfigureAwait(false);
            return PartialView("PartialView/_GetPrimarySecoundaryReport", _resp);
        }

        [HttpGet]
        [Route("/get-PrimarySecoundary-Excel")]
        public async Task<IActionResult> _PrimarySecoundaryExport(CommonFilter filter)
        {
            IReportML ml = new ReportML(_accessor, _env);
            var _report = await ml.GetPrimarySecoundaryReport(filter);
            var dataTable = ConverterHelper.O.ToDataTable(_report);
            var removableCol = GetProperties.o.GetPropertiesNameOfClass(new ResponseStatus());
            foreach (string str in removableCol)
            {
                if (dataTable.Columns.Contains(str))
                {
                    dataTable.Columns.Remove(str);
                }
            }
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("PrimarySecoundary");
                worksheet.Cells["A1"].LoadFromDataTable(dataTable, PrintHeaders: true);
                int rowindex = 2;
                foreach (DataRow row in dataTable.Rows)
                {
                    if (rowindex == 2)
                    {

                    }
                    rowindex++;
                }
                for (var col = 1; col < dataTable.Columns.Count + 1; col++)
                {
                    worksheet.Column(col).AutoFit();
                }
                var exportToExcel = new ExportToExcel
                {
                    Contents = package.GetAsByteArray(),
                    FileName = "PrimarySecoundary.xlsx"
                };
                return File(exportToExcel.Contents, DOCType.XlsxContentType, exportToExcel.FileName);
            }
        }

        [HttpPost]
        [Route("PSTDeatilReport")]
        public async Task<IActionResult> _PSTDeatilReport(CommonFilter filter)
        {
            IReportML ml = new ReportML(_accessor, _env);
            var _resp = await Task.FromResult(ml.PSTDeatilReport(filter)).ConfigureAwait(false);
            return PartialView("PartialView/_PSTDeatilReport", _resp);
        }
        #endregion

        [HttpGet]
        [Route("Approval-Group")]
        public IActionResult _ApprovalGroup(CommonReq req)
        {
            IReportML ml = new ReportML(_accessor, _env);
            var _list = ml.getRequestApproval(req);
            return PartialView("PartialView/_ApprovalGroup", _list);
        }

        [HttpGet]
        [Route("Wallet-Request-Report")]
        public IActionResult _WallRequestReportExcel(WalletRequest req)
        {
            //This report is for parent only
            if (_lr.RoleID != Role.Admin)
            {
                req.Mobile = _lr.MobileNo;
            }
            req.CommonInt = 2;
            IReportML rml = new ReportML(_accessor, _env);
            DataTable dt = rml.GetSettlementExcel(req);
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("MPOS");
                worksheet.Cells["A1"].LoadFromDataTable(dt, PrintHeaders: true);
                worksheet.Row(1).Height = 20;
                worksheet.Row(1).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Row(1).Style.Font.Bold = true;

                for (var col = 1; col < dt.Columns.Count + 1; col++)
                {
                    worksheet.Column(col).AutoFit();
                }
                var exportToExcel = new ExportToExcel
                {
                    Contents = package.GetAsByteArray(),
                    FileName = "Mpos_" + DateTime.Now.ToString("dd/MM/yyyy") + "-G-" + req.ShowGroupID + ".xlsx"
                };
                return File(exportToExcel.Contents, DOCType.XlsxContentType, exportToExcel.FileName);
            }
        }

        [HttpPost]
        [Route("download-pdf")]
        public IActionResult downloads()
        {
            IResourceML ml = new ResourceML(_accessor, _env);
            var res = ml.GetOutletData(_lr.LoginTypeID, _lr.UserID);

            return PartialView("PartialView/_downloads", res);
        }
        [HttpGet]
        [Route("downloadpdfFile")]
        public IActionResult _downloadpdf(string fileName)
        {
            HtmlToPdfConverter converter = new HtmlToPdfConverter();
            WebKitConverterSettings settings = new WebKitConverterSettings();
            settings.WebKitPath = DOCType.WebKitPath;
            settings.PdfPageSize = PdfPageSize.A4;
            converter.ConverterSettings = settings;
            string FileName = "unknown.pdf";
            string content = string.Empty;
            CertificateTemplate template = new CertificateTemplate();
            IResourceML ml = new ResourceML(_accessor, _env);
            var res = ml.downloadPdf(_lr.LoginTypeID, _lr.UserID, _lr.WID);
            if (res.Statuscode == ErrorCodes.Minus1)
                return Json(res);

            res.OutletID = _lr.OutletID;
            res.OutletName = _lr.OutletName;
            res.WID = _lr.WID;
            if (fileName == "ESC")
            {
                content = template.EsseintialServices(res);
                FileName = "Essential Services Certificate.pdf";
            }
            else if (fileName == "MRC")
            {
                settings.Orientation = PdfPageOrientation.Landscape;

                content = template.MerchantCertificate(res);
                FileName = "Merchant Certificate.pdf";
            }

            if (res.KYCStatus != KYCStatusType.COMPLETED && fileName.Equals("MRC"))
            {
                return Json(new ResponseStatus
                {
                    Statuscode = ErrorCodes.Minus1,
                    Msg = "KYC not completed"
                });
            }
            else
            {
                PdfDocument document = converter.Convert(content, string.Empty);
                MemoryStream ms = new MemoryStream();
                document.Save(ms);
                document.Close(true);
                ms.Position = 0;
                FileStreamResult fsr = new FileStreamResult(ms, "application/pdf");
                fsr.FileDownloadName = FileName;
                return fsr;
            }

        }

        #region DTHSubscription

        [HttpGet]
        [Route("DTHSubscriptionPending")]
        public async Task<IActionResult> DTHSubscriptionPending()
        {
            IAPIML aPIML = new APIML(_accessor, _env);
            IOperatorML opML = new OperatorML(_accessor, _env);
            IUserML userML = new UserML(_lr);

            var rep = new RechargeReportModel
            {
                RechargeAPI = aPIML.GetAPIDetail(),
                Operators = await opML.GetOPListBYServices(ServiceType.RechargeReportServices).ConfigureAwait(false),

                IsAdmin = _lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowRechargeReport),
                IsAPIUser = _lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.APIUser,
                IsEndUser = userML.IsEndUser()
            };
            return View(rep);
        }

        [HttpPost]
        [Route("DTHSubscription-Pending")]
        public async Task<IActionResult> _DTHSubscriptionPending()
        {
            IUserML userML = new UserML(_lr);
            IReportML ml = new ReportML(_accessor, _env);
            var res = new DTHSubscriptionModel
            {
                IsAdmin = _lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowRechargeReport),
                CanMarkDispute = _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.MarkDispute),
                CanFail = _lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.PageFailed),
                CanSuccess = _lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.PageSuccess),
                IsAPIUser = _lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.APIUser,
                IsEndUser = userML.IsEndUser(),
                Report = await ml.GetDthsubscriptionPendings().ConfigureAwait(false)
            };
            return PartialView("PartialView/_DTHSubscriptionPending", res);
        }

        [HttpGet]
        [Route("DTHSubscriptionReport")]
        public IActionResult DTHSubscriptionReport()
        {
            IAPIML aPIML = new APIML(_accessor, _env);
            IOperatorML opML = new OperatorML(_accessor, _env);
            IUserML userML = new UserML(_lr);
            var rep = new RechargeReportModel
            {
                RechargeAPI = aPIML.GetAPIDetail(),
                Operators = opML.GetOperatorsByService(ServiceCode.DTHSubscription),
                IsAdmin = _lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowRechargeReport),
                IsAPIUser = _lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.APIUser,
                IsEndUser = userML.IsEndUser()
            };
            return View(rep);
        }

        [HttpPost]
        [Route("DTHSubscription-Report")]
        public async Task<IActionResult> _DTHSubscriptionReport(_RechargeReportFilter filter)
        {
            IUserML userML = new UserML(_lr);
            IReportML ml = new ReportML(_accessor, _env);
            var res = new DTHSubscriptionModel
            {
                IsAdmin = _lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowRechargeReport),
                CanMarkDispute = _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.MarkDispute),
                CanFail = _lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.PageFailed),
                CanSuccess = _lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.PageSuccess),
                IsAPIUser = _lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.APIUser,
                IsEndUser = userML.IsEndUser(),
                Report = await ml.GetDthsubscriptionReport(filter).ConfigureAwait(false)
            };
            return PartialView("PartialView/_DTHSubscriptionReport", res);
        }

        [HttpPost]
        [Route("Home/U-booking")]
        [Route("U-booking")]
        public IActionResult UpdateBookingStatus(DTHSubscriptionReport dth)
        {
            var callbackData = new _CallbackData
            {
                TID = dth.TID,
                TransactionID = dth.TransactionID,
                Msg = dth.Remark,
                RequestPage = "PendingPage",
                BookingStatus = dth.BookingStatus,
                TechnicianName = dth.TechnicianName,
                TechnicianMobile = dth.TechnicianMobile,
                CustomerID = dth.CustomerID,
                STBID = dth.STBID,
                VCNO = dth.VCNO,
                InstallationTime = dth.InstallationTime,
                InstalltionCharges = dth.InstalltionCharges,
                ApprovalTime = dth.ApprovalTime
            };
            IReportML rml = new ReportML(_accessor, _env);
            return Json(rml.UpdateBookingStatus(callbackData));
        }

        [HttpPost]
        [Route("Home/DTH-Deatils")]
        [Route("DTH-Deatils")]
        public IActionResult DTHDeatils(int TID, string TransactionID)
        {
            IUserML IuML = new UserML(_accessor, _env);
            DTHSubscriptionReport data = IuML.GetBookingStatus(TID, TransactionID);
            return PartialView("PartialView/_DTHDetails", data);
        }

        #endregion

        #region WebNotification
        [HttpPost]
        [Route("/WebNotification")]
        public async Task<IActionResult> GetWebNotification(bool IsShowAll)
        {
            IReportML ml = new ReportML(_accessor, _env);
            var list = await ml.GetWebNotification(IsShowAll);
            var modal = new WebNotificationModel
            {
                Notification = list,
                IsWebNotification = _lr.RoleID == Role.Admin ? false : ApplicationSetting.IsWebNotification
            };
            return Json(modal);
        }

        [HttpPost]
        [Route("/CloseNotification")]
        public IActionResult CloseNotification(int id, int userID, string EntryDate)
        {
            IReportML ml = new ReportML(_accessor, _env);
            var res = ml.CloseNotification(id, userID, EntryDate);
            return Json(res);
        }
        [HttpGet]
        [Route("Report/WebNotification")]
        public IActionResult WebNotification()
        {
            if (_lr.RoleID == Role.Admin)
                return View();
            else
                return Ok();
        }

        [HttpPost]
        [Route("/Report/_WebNotification")]
        public IActionResult _WebNotification(CommonFilter req)
        {
            IReportML ml = new ReportML(_accessor, _env);
            var res = ml.WebNotificationsReport(req);
            return PartialView("PartialView/_WebNotification", res);
        }

        [HttpPost]
        [Route("/DeactiveNotification")]
        public IActionResult DeactiveNotification(int id, bool IsActive)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = "You havn't permit for this action"
            };
            if (_lr.RoleID == Role.Admin)
            {
                IReportML ml = new ReportML(_accessor, _env);
                res = ml.DeactiveNotification(id, IsActive);
            }
            return Json(res);
        }
        #endregion
        [HttpPost]
        [Route("/markallread")]
        public async Task markallread()
        {
            await Task.Delay(0);
            IReportML ml = new ReportML(_accessor, _env);
            ml.markallread();
        }

        [HttpPost]
        [Route("Home/show-pes-doc")]
        [Route("show-pes-doc")]
        public async Task<IActionResult> _ShowPESDocument(int ID)
        {
            IRechargeReportML ml = new ReportML(_accessor, _env);
            var _res = ml.GetPESApprovedDocument(ID);
            return PartialView("PartialView/_showPESDocuments", _res);
        }
        [HttpPost]
        [Route("IsVendor")]
        public async Task<IActionResult> IsVendor(int ID)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError,
                CommonBool = false
            };
            if (!_lr.RoleID.In(Role.Admin, Role.APIUser, Role.Customer, Role.Retailor_Seller))
            {
                IUserML ml = new UserML(_accessor, _env);
                res = await ml.IsVendor().ConfigureAwait(false);
            }
            return Json(res);
        }
        private LoginResponse chkAlternateSession()
        {
            var result = new LoginResponse();
            if (_lr != null)
            {
                result = _lr;
            }
            if (_lrEmp != null)
            {
                result = _lrEmp;
            }
            return result;
        }


        #region LeadService
        [Route("Home/LeadServiceRequests")]
        [Route("LeadServiceRequests")]
        public async Task<IActionResult> LeadServiceRequests()
        {
            IUserML userML = new UserML(_lr);
            IAPIML aPIML = new APIML(_accessor, _env);
            var loginResp = chkAlternateSession();
            IOperatorML OpML = new OperatorML(_accessor, _env);
            LeadServiceRequest Leadreq = new LeadServiceRequest();
            Leadreq.IsAdmin = loginResp.RoleID == Role.Admin && loginResp.LoginTypeID == LoginType.ApplicationUser;
            Leadreq.IsAPIUser = loginResp.LoginTypeID == LoginType.ApplicationUser && loginResp.RoleID == Role.APIUser;
            Leadreq.Operatorlst = await OpML.GetOPListBYServices(ServiceType.LeadService.ToString()).ConfigureAwait(false);
            return View(Leadreq);
        }
        [HttpPost]
        [Route("Home/LeadServiceRequest")]
        [Route("LeadServiceRequest")]
        public async Task<IActionResult> LeadServiceRequests([FromBody] LeadServiceRequest LeadReq)
        {
            IUserML userML = new UserML(_lr);
            IAPIML aPIML = new APIML(_accessor, _env);
            LeadServiceRequest leadServiceRequestModel = new LeadServiceRequest();
            var loginResp = chkAlternateSession();
            ILeadServiceML ml = new LeadServiceML(_accessor, _env);

            if (loginResp != null)
            {
                leadServiceRequestModel.IsAdmin = LeadReq.IsAdmin = loginResp.RoleID == Role.Admin && loginResp.LoginTypeID == LoginType.ApplicationUser;
                leadServiceRequestModel.IsAPIUser = LeadReq.IsAPIUser = loginResp.LoginTypeID == LoginType.ApplicationUser && loginResp.RoleID == Role.APIUser;
                LeadReq.IsEndUser = userML.IsEndUser();
                LeadReq.UserID = loginResp.UserID;

                leadServiceRequestModel.Leadservicelst = ml.GetLeadServiceRequest(LeadReq);
            }
            return PartialView("PartialView/_LeadServiceRequests", leadServiceRequestModel);

            //return Ok();
        }
        [HttpGet]
        [Route("Home/Lead-Report")]
        [Route("Lead-Report")]
        public IActionResult _LeadReport(LeadServiceRequest LeadReq)
        {
            IUserML userML = new UserML(_accessor, _env);
            ILeadServiceML ml = new LeadServiceML(_accessor, _env);
            LeadReq.IsExport = true;
            var loginResp = chkAlternateSession(); ;
            List<LeadServiceRequest> _report;
            DataTable dataTable = new DataTable();
            _report = ml.GetLeadServiceRequest(LeadReq);
            dataTable = ConverterHelper.O.ToDataTable(_report);
            //string[] col = { "Display1", "Display2", "Display3", "Display4" };
            List<string> removableCols = new List<string> { "Leadservicelst", "FromDate", "ToDate", "IsAdmin", "IsAPIUser", "IsEndUser", "Criteria", "CriteriaText", "TopRows", "Operatorlst", "IsExport", "LT", "ID", "LoanTypeID", "InsuranceTypeID", "CustomerTypeID", "EntryBy", "ModifyBy", "RequestModeID", "OID", "LoanType", "InsuranceType", "OpTypeID", "RequestMode", "LoginID", "UserID", "BankID", "RequestIP", "Browser", "LeadStatus", "Operator", "HaveLoan" };
            List<string> ColSequence = new List<string> { "Outlet", "Name", "Mobile", "OpType", "LeadSubType", "Email", "Age", "PAN", "Amount", "PinCode", "CustomerType", "RequiredFor", "OccupationType", "BankName", "ActiveLoan" };
            int columnIndex = 0;
            foreach (string colName in ColSequence)
            {
                dataTable.Columns[colName].SetOrdinal(columnIndex);
                columnIndex++;
            }
            foreach (string str in removableCols)
            {
                if (dataTable.Columns.Contains(str))
                {
                    dataTable.Columns.Remove(str);
                }
            }

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("LeadReport");
                worksheet.Cells["A1"].LoadFromDataTable(dataTable, PrintHeaders: true);
                worksheet.Row(1).Height = 20;
                worksheet.Row(1).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Row(1).Style.Font.Bold = true;
                int rowindex = 2;

                for (var col = 1; col < dataTable.Columns.Count + 1; col++)
                {
                    worksheet.Column(col).AutoFit();
                }
                var exportToExcel = new ExportToExcel
                {
                    Contents = package.GetAsByteArray(),
                    FileName = "LeadReport.xlsx"
                };
                return File(exportToExcel.Contents, DOCType.XlsxContentType, exportToExcel.FileName);
            }
        }
        [HttpPost]
        [Route("Home/UpdateLeadServiceRequest")]
        [Route("UpdateLeadServiceRequest")]
        public IActionResult UpdateLeadServiceRequest(int ID, string Remark, int LeadStatus)
        {
            ILeadServiceML ml = new LeadServiceML(_accessor, _env);
            return Json(ml.UpdateLeadServiceRequest(ID, Remark, LeadStatus));

        }
        [HttpPost]
        [Route("Home/GetLeadDetailById")]
        [Route("GetLeadDetailById")]
        public IActionResult GetLeadDetailById(int ID)
        {
            LeadServiceRequest leadServiceRequestModel = new LeadServiceRequest();
            ILeadServiceML ml = new LeadServiceML(_accessor, _env);
            leadServiceRequestModel.Leadservicelst = ml.GetLeadDetailById(ID);
            return PartialView("PartialView/_LeadRequestDetail", leadServiceRequestModel);
        }
        #endregion
        #region P2AInvoiceUploaded
        [HttpGet]
        [Route("Home/P2AInvoiceUploaded")]
        [Route("P2AInvoiceUploaded")]
        public IActionResult P2AInvoiceUploaded(string m)
        {
            return View();
        }
        [HttpPost]
        [Route("Home/_P2AInvoiceUploaded")]
        [Route("_P2AInvoiceUploaded")]
        public IActionResult _P2AInvoiceUploaded(string M)
        {
            IInvoiceReportML reportML = new ReportML(_accessor, _env);
            var res = reportML.P2AInvoiceApprovalList(M);
            return PartialView("PartialView/_P2AInvoiceUploaded", res);
        }
        #endregion

        [HttpPost]
        [Route("Doc/_UserDocumentDetails")]
        public IActionResult _UserDocumentDetails(int uid)
        {
            IKYCML userML = new UserML(_accessor, _env);
            var list = userML.GetUserDocumentsList(uid);
            return PartialView("PartialView/_showDocumentsDetail", list);
        }
        [HttpPost]
        [Route("View-Settelment-cycle")]
        public IActionResult ViewSettelment()
        {
            IUserML ml = new UserML(_accessor, _env);
            var res = ml.GetSettlementSettingSeller();
            return PartialView("PartialView/_ViewSettelmentCycle", res);
        }
        #region AutoClearance
        [HttpGet]
        [Route("Home/AutoClr")]
        [Route("AutoClr")]
        public async Task<IActionResult> AutoClearance()
        {
            if (ApplicationSetting.IsAutoClearancePage)
                return View();
            return Ok();
        }

        [HttpPost]
        [Route("Home/_AutoClr-rech")]
        [Route("_AutoClr-rech")]
        public async Task<IActionResult> AutoClrRech()
        {
            IReportML ml = new ReportML(_accessor, _env);
            await ml.LoopRechTransactionStatus().ConfigureAwait(false);
            return Ok("success");
        }
        [HttpPost]
        [Route("Home/_AutoClr-dmt")]
        [Route("_AutoClr-dmt")]
        public async Task<IActionResult> AutoClrDMT()
        {
            IReportML ml = new ReportML(_accessor, _env);
            await ml.LoopDMTTransactionStatus().ConfigureAwait(false);
            return Ok("success");
        }
        [HttpPost]
        [Route("Home/_AutoClr-aeps")]
        [Route("_AutoClr-aeps")]
        public async Task<IActionResult> AutoClrAEPS()
        {
            IReportML ml = new ReportML(_accessor, _env);
            ml.LoopAEPSTransactionStatus();
            return Ok("success");
        }

        #endregion

        #region AccountStatement
        [Route("Home/AccStmt/{MobileNo}")]
        [Route("AccStmt/{MobileNo}")]
        public IActionResult AccountStatement(string MobileNo)
        {
            IUserML userml = new UserML(_accessor, _env);
            var ledgerReportModel = new LedgerReportModel
            {
                HaveChild = !userml.IsEndUser() || userml.IsCustomerCareAuthorised(ActionCodes.ShowUserLedger),
                SelfMobile = MobileNo ?? _lr.MobileNo ?? "",
                userBalnace = userml.GetUserBalnace(0)
            };
            return View(ledgerReportModel);
        }
        [Route("Home/AccStmt")]
        [Route("AccStmt")]
        public IActionResult AccountStatement()
        {
            IUserML userml = new UserML(_accessor, _env);
            var loginResp = chkAlternateSession();
            var ledgerReportModel = new LedgerReportModel();
            if (_lr != null)
            {
                ledgerReportModel.HaveChild = !userml.IsEndUser() || userml.IsCustomerCareAuthorised(ActionCodes.ShowUserLedger);
            }
            if (_lrEmp != null)
            {
                ledgerReportModel.HaveChild = _lrEmp.LoginTypeID == LoginType.Employee ? true : false;
            }

            ledgerReportModel.SelfMobile = loginResp.MobileNo ?? "";
            ledgerReportModel.userBalnace = userml.GetUserBalnace(0);
            ledgerReportModel.IsEmployee = loginResp.LoginTypeID == LoginType.Employee ? true : false;
            return View(ledgerReportModel);
        }

        [HttpPost]
        [Route("Home/Acc-Stmt")]
        [Route("Acc-Stmt")]
        public async Task<IActionResult> _AccountStatement([FromBody] ULedgerReportFilter filter)
        {
            var loginResp = chkAlternateSession();
            IUserML userml = new UserML(loginResp);
            if (_lr != null)
            {
                ViewBag.HaveChild = !userml.IsEndUser() || userml.IsCustomerCareAuthorised(ActionCodes.ShowUserLedger);
            }
            if (_lrEmp != null)
            {
                ViewBag.HaveChild = _lrEmp.LoginTypeID == LoginType.Employee ? true : false;
            }
            ViewBag.SelfMobile = loginResp.MobileNo ?? "";
            IReportML ml = new ReportML(_accessor, _env);
            IEnumerable<ProcUserLedgerResponse> _report = await ml.GetAccountStatement(filter);
            return PartialView("PartialView/_AccountStatement", _report);
        }
        [HttpGet]
        [Route("Home/Acc-Stmt")]
        [Route("Acc-Stmt")]
        public async Task<IActionResult> _AccStmtExport(ULedgerReportFilter filter)
        {
            var loginResp = chkAlternateSession();
            IUserML userml = new UserML(loginResp);
            if (_lr != null)
            {
                ViewBag.HaveChild = !userml.IsEndUser() || userml.IsCustomerCareAuthorised(ActionCodes.ShowUserLedger);
            }
            if (_lrEmp != null)
            {
                ViewBag.HaveChild = _lrEmp.LoginTypeID == LoginType.Employee ? true : false;
            }
            ViewBag.SelfMobile = loginResp.MobileNo ?? "";
            IReportML ml = new ReportML(_accessor, _env);
            var _report = await ml.GetAccountStatement(filter);
            DataTable dataTable = ConverterHelper.O.ToDataTable(_report);
            string[] remItemList = new string[] { "ResultCode", "Msg", "UserID", "ID", "TopRows", "ServiceID", "LType", "LastAmount", "MobileNo" };
            foreach (var item in remItemList)
            {
                dataTable.Columns.Remove(item);
            }
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("AccountStatementSummary");
                worksheet.Cells["A1"].LoadFromDataTable(dataTable, PrintHeaders: true);
                worksheet.Row(1).Height = 20;
                worksheet.Row(1).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Row(1).Style.Font.Bold = true;
                for (var col = 1; col < dataTable.Columns.Count + 1; col++)
                {
                    worksheet.Column(col).AutoFit();
                }
                var exportToExcel = new ExportToExcel
                {
                    Contents = package.GetAsByteArray(),
                    FileName = "AccountStatementSummary.xlsx"
                };
                return File(exportToExcel.Contents, DOCType.XlsxContentType, exportToExcel.FileName);
            }
        }

        [HttpPost]
        [Route("Home/AS-Summary")]
        [Route("AS-Summary")]
        public IActionResult _ASSummary([FromBody] ULedgerReportFilter filter)
        {
            IReportML ml = new ReportML(_accessor, _env);
            return PartialView("PartialView/_ASSummary", ml.GetASSummary(filter));
        }

        [Route("Home/ASC/{id}")]
        [Route("ASC/{id}")]
        public IActionResult ASC(int id)
        {
            IUserML userML = new UserML(_accessor, _env);
            var ud = userML.GetUserBalnace(0);
            var com = new CommonReq()
            {
                UserID = _lr.UserID,
                CommonInt = id,
                CommonBool = ud.IsBalance,
                CommonBool2 = ud.IsUBalance,
                CommonBool1 = _lr.RoleID == Role.FOS ? false : true
            };
            return View(com);
        }
        [HttpPost]
        [Route("Home/as-c")]
        [Route("as-c")]
        public IActionResult _ASC([FromBody] ULedgerReportFilter filter)
        {
            filter.UType = _lr.RoleID == Role.FOS ? 0 : filter.UType;
            IReportML ml = new ReportML(_accessor, _env);
            return PartialView("PartialView/_ASC", ml.GetASCSummary(filter));
        }
        [HttpGet]
        [Route("Home/as-c")]
        [Route("as-c")]
        public async Task<IActionResult> _ASCExport(ULedgerReportFilter filter)
        {
            filter.UType = _lr.RoleID == Role.FOS ? 0 : filter.UType;
            IReportML ml = new ReportML(_accessor, _env);
            var _data = ml.GetASCSummary(filter);
            DataTable dataTable = ConverterHelper.O.ToDataTable(_data);
            string[] remItemList = new string[] { "StatusCode", "Status", "Purchase", "Return", "Requested", "Debited", "Debited2202", "Refunded", "Commission", "CCFCommission", "Surcharge", "FundTransfered", "OtherCharge", "CCFCommDebited", "Expected", "CCF", "RoleID", "UserID", "SetTarget", "TargetTillDate", "IsGift", "DCommission", "FundDeducted" };
            foreach (var item in remItemList)
            {
                dataTable.Columns.Remove(item);
            }
            if (!Convert.ToBoolean(dataTable.Rows[0]["IsUtility"]))
                dataTable.Columns.Remove("UBalance");
            dataTable.Columns.Remove("IsUtility");
            dataTable.Columns.Remove("IsPrepaid");
            if (!ApplicationSetting.IsAreaMaster)
                dataTable.Columns.Remove("Area");
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("BalanceSheet");
                worksheet.Cells["A1"].LoadFromDataTable(dataTable, PrintHeaders: true);
                worksheet.Row(1).Height = 20;
                worksheet.Row(1).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Row(1).Style.Font.Bold = true;
                int rowindex = 2;
                worksheet.Cells[1, dataTable.Columns["Sales"].Ordinal + 1].Value = "Purchase";
                worksheet.Cells[1, dataTable.Columns["Lsale"].Ordinal + 1].Value = "Last Purchase";
                worksheet.Cells[1, dataTable.Columns["LSDate"].Ordinal + 1].Value = "Last Purchase Date";
                worksheet.Cells[1, dataTable.Columns["CCollection"].Ordinal + 1].Value = "Collection";
                worksheet.Cells[1, dataTable.Columns["LCollection"].Ordinal + 1].Value = "Last Collection";
                worksheet.Cells[1, dataTable.Columns["LCDate"].Ordinal + 1].Value = "Last Collection Date";

                for (var col = 1; col < dataTable.Columns.Count + 1; col++)
                {
                    worksheet.Column(col).AutoFit();
                }
                var exportToExcel = new ExportToExcel
                {
                    Contents = package.GetAsByteArray(),
                    FileName = "BalanceSheet.xlsx"
                };
                return File(exportToExcel.Contents, DOCType.XlsxContentType, exportToExcel.FileName);
            }
        }
        [HttpPost]
        [Route("Home/as-collect")]
        [Route("as-collect")]
        public IActionResult _ASCollect(CommonReq commonReq)
        {
            IReportML ml = new ReportML(_accessor, _env);
            var res = new ASCollection
            {
                UserID = commonReq.UserID,
                OutletName = commonReq.CommonStr,
                Mobile = commonReq.CommonStr2,
                Banks = ml.GetASBanks(_lr.UserID)
            };
            return PartialView("PartialView/_ASCollection", res);
        }
        [HttpPost]
        [Route("Home/a-s-c")]
        [Route("a-s-c")]
        public IActionResult _ASCollectF(ASCollectionReq aSCollectionReq)
        {
            IReportML ml = new ReportML(_accessor, _env);
            var res = ml.ASPaymentCollection(aSCollectionReq);
            return Json(res);
        }
        [Route("Home/area-master")]
        [Route("area-master")]
        public IActionResult AreaMaster()
        {
            return View();
        }
        [HttpPost]
        [Route("Home/am")]
        [Route("am")]
        public IActionResult _AreaMaster()
        {
            IReportML ml = new ReportML(_accessor, _env);
            var res = ml.GetAreaMaster(_lr.UserID);
            return PartialView("PartialView/_AreaMaster", res);
        }
        [HttpPost]
        [Route("Home/amc")]
        [Route("amc")]
        public IActionResult _AreaUserWise(ASAreaMaster aSAreaMaster)
        {
            return PartialView("PartialView/_AreaUserWise", aSAreaMaster);
        }

        [HttpPost]
        [Route("Home/amcu")]
        [Route("amcu")]
        public IActionResult _AreaMasterCU(ASAreaMaster aSAreaMaster)
        {
            IReportML ml = new ReportML(_accessor, _env);
            var res = ml.AreaMasterCU(aSAreaMaster);
            return Json(res);
        }
        [HttpPost]
        [Route("Home/mua")]
        [Route("mua")]
        public IActionResult _MAU(CommonReq commonReq)
        {
            return PartialView("PartialView/_MAU", commonReq);
        }
        [HttpPost]
        [Route("Home/gm")]
        [Route("gm")]
        public IActionResult _GetMap(int uid)
        {
            uid = uid == 0 ? _lr.UserID : uid;
            IReportML ml = new ReportML(_accessor, _env);
            var res = ml.GetAreaMaster(uid);
            return Json(res);
        }

        [HttpPost]
        [Route("Home/uma")]
        [Route("uma")]
        public IActionResult _UMA(CommonReq commonReq)
        {
            IReportML ml = new ReportML(_accessor, _env);
            var res = ml.MapUserArea(commonReq);
            return Json(res);
        }
        [Route("Home/pmcaw")]
        [Route("pmcaw")]
        public IActionResult PMCAW()
        {
            return View();
        }
        [HttpPost]
        [Route("Home/gapc")]
        [Route("gapc")]
        public IActionResult _GAPC()
        {
            IReportML ml = new ReportML(_accessor, _env);
            var res = ml.GetAreaFPC(_lr.UserID);
            return Json(res);
        }
        [HttpPost]
        [Route("Home/paccstmt")]
        [Route("paccstmt")]
        public async Task<IActionResult> _PartialAccStmt([FromBody] ULedgerReportFilter filter)
        {
            IReportML ml = new ReportML(_accessor, _env);
            IEnumerable<ProcUserLedgerResponse> _report = await ml.GetAccountStatement(filter);
            return PartialView("PartialView/_ParAccStmt", _report);
        }
        [HttpPost]
        [Route("Home/vch-Entry")]
        [Route("vch-Entry")]
        public IActionResult _VoucherEntry()
        {
            IReportML ml = new ReportML(_accessor, _env);
            var res = new ASCollection
            {
                Banks = ml.GetASBanks(_lr.UserID),
                userList = ml.GetUListForVoucherEntry(_lr.UserID)
            };
            return PartialView("PartialView/_VoucherEntry", res);
        }
        #endregion



        #region Shared-Components
        [HttpPost]
        [Route("Home/GetNewsSummary")]
        [Route("/GetNewsSummary")]
        public IActionResult NewsSummary()
        {
            var model = new List<News>();
            INewsML settings = new NewsML(_accessor, _env);
            if (_lr.RoleID > 0)
            {
                model = settings.GetNewsByRole(_lr.RoleID);
            }
            return PartialView(GetPartialView_SharedComponents("__NewsSummary"), model);
        }

        [HttpPost]
        [Route("Home/GetTSummaryTable")]
        [Route("/GetTSummaryTable")]
        public async Task<IActionResult> TransactionSummaryTable()
        {
            IReportML ml = new ReportML(_accessor, _env);
            PSTReportUser _transactionSummary = await ml.GetPriSecTerUserData().ConfigureAwait(false);
            return PartialView(GetPartialView_SharedComponents("__TSummaryTable"), _transactionSummary);
        }

        [HttpPost]
        [Route("Home/GetASummaryTable")]
        [Route("/GetASummaryTable")]
        public IActionResult AccountSummaryTable()
        {
            IReportML ml = new ReportML(_accessor, _env);
            var res = ml.GetAccountSummaryTable();
            return PartialView(GetPartialView_SharedComponents("__ASummaryTable"), res);
        }

        [HttpPost]
        [Route("Home/GetDSummaryTable")]
        [Route("/GetDSummaryTable")]
        public IActionResult DealerSummaryTable()
        {
            ULedgerReportFilter filter = new ULedgerReportFilter();
            IReportML ml = new ReportML(_accessor, _env);
            var res = ml.GetDealerSummary();
            return PartialView(GetPartialView_SharedComponents("__DSummaryTable"), res);
        }

        [HttpPost]
        [Route("Home/GetBannerSlider")]
        [Route("/GetBannerSlider")]
        public IActionResult BannerSlider()
        {
            IBannerML bannerML = new ResourceML(_accessor, _env);
            var res = bannerML.GetBanners(_lr.WID.ToString());
            return PartialView(GetPartialView_SharedComponents("__BannerSlider"), res);
        }

        [HttpPost]
        [Route("Home/GetHelpInfoSection")]
        [Route("/GetHelpInfoSection")]
        public IActionResult HelpInfoSection()
        {
            IUserWebsite _userML = new UserML(_accessor, _env);
            var resp = _userML.GetCompanyProfile(_lr.WID);
            return PartialView(GetPartialView_SharedComponents("__HelpInfoSection"), resp);
        }

        private string GetPartialView_SharedComponents(string PartialViewName)
        {
            return ("PartialView/" + loginML.GetWebsiteInfo().ThemeId + "/" + PartialViewName);

        }
        [HttpPost]
        [Route("Home/GetWISection")]
        [Route("/GetWISection")]
        public IActionResult WISection()
        {
            IUserWebsite _userML = new UserML(_accessor, _env);
            var resp = _userML.GetCompanyProfile(_lr.WID);
            return PartialView("PartialView/_WhatsAppIcon", resp);
            // return PartialView(GetPartialView_SharedComponents("_WhatsAppIcon"), resp);
        }

        [HttpPost]
        [Route("Home/GetBCSummaryTable")]
        [Route("/GetBCSummaryTable")]
        public IActionResult BCSummaryTable()
        {
            IReportML ml = new ReportML(_accessor, _env);
            var res = ml.GetBCAgentSummary();
            return PartialView(GetPartialView_SharedComponents("__BCSummaryTable"), res);
        }

        [HttpPost]
        [Route("Home/GetOpTypeList")]
        [Route("/GetOpTypeList")]
        public IActionResult GetOpTypeList()
        {
            IOperatorML opML = new OperatorML(_accessor, _env);
            var opTypes = opML.GetOptypes();

            return Json(opTypes);
        }

        [HttpPost]
        [Route("Home/GetUsedServiceList")]
        [Route("/GetUsedServiceList")]
        public async Task<IActionResult> GetUsedServiceList()
        {
            IReportML ML = new ReportML(_accessor, _env);
            var _ser = await ML.GetUsedServicesList();

            return Json(_ser);
        }
        [HttpPost]
        [Route("Home/most-used-services")]
        [Route("APIUser/most-used-services")]
        [Route("most-used-services")]
        public async Task<IActionResult> MostUsedServices()
        {
            IReportML ml = new ReportML(_accessor, _env);
            return Json(await ml.GetMostUsedServices().ConfigureAwait(false));
        }

        [HttpPost]
        [Route("Home/month-week-day-transactions")]
        [Route("APIUser/month-week-day-transactions")]
        [Route("month-week-day-transactions")]
        public async Task<IActionResult> MonthWeekDaysTransactions(string ActivityType, int RequestedDataType, int ServiceTypeID)
        {
            IReportML ml = new ReportML(_accessor, _env);
            return Json(await ml.GetMonthWeekDaysTransactions(ActivityType, RequestedDataType, ServiceTypeID).ConfigureAwait(false));
        }

        [HttpPost]
        [Route("Home/today-transactions-status")]
        [Route("APIUser/today-transactions-status")]
        [Route("/today-transactions-status")]
        public async Task<IActionResult> TodayTransactionStatus(int RequestedDataType)
        {
            IReportML ml = new ReportML(_accessor, _env);
            var res = await ml.GetTodayTransactionStatus(RequestedDataType).ConfigureAwait(false);
            return Json(res);
        }

        [HttpPost]
        [Route("Home/recent-outletofuser-list")]
        [Route("APIUser/recent-outletofuser-list")]
        [Route("recent-outletofuser-list")]
        public async Task<IActionResult> RecentOutletOfUserList(int TopRow)
        {
            IOutletML ml = new ReportML(_accessor, _env);
            var res = await ml.GetRecentOutletUserList(TopRow).ConfigureAwait(false);
            return PartialView(GetPartialView_SharedComponents("__RecentOutletUserTable"), res);
        }


        [HttpPost]
        [Route("Home/recent-login-activity")]
        [Route("APIUser/recent-login-activity")]
        [Route("recent-login-activity")]
        public async Task<IActionResult> RecentLoginActivity(int TopRow, bool outputInjson = false)
        {
            IReportML ml = new ReportML(_accessor, _env);
            var res = await ml.GetRecentLoginActivity(TopRow).ConfigureAwait(false);
            if (!outputInjson)
                return PartialView(GetPartialView_SharedComponents("__RecentLoginActivityTable"), res);
            else
                return Json(res);
        }



        [HttpPost]
        [Route("Home/recent-transaction-list")]
        [Route("APIUser/recent-transaction-list")]
        [Route("/recent-transaction-list")]
        public async Task<IActionResult> RecentTransactionActivity()
        {
            IReportML ml = new ReportML(_accessor, _env);
            var res = await ml.GetRecentTransactionActivity().ConfigureAwait(false);
            return PartialView(GetPartialView_SharedComponents("__RecentTransactionActivityTable"), res);
        }

        [HttpPost]
        [Route("Home/date-optype-transactions-status")]
        [Route("APIUser/date-optype-transactions-status")]
        [Route("/date-optype-transactions-status")]
        public async Task<IActionResult> DateOpTypeWiseTransactionStatus(string RequestedDate, int OpTypeID, int RequestedDataType)
        {
            IReportML ml = new ReportML(_accessor, _env);
            var res = await ml.GetDateOpTypeWiseTransactionStatus(RequestedDate, OpTypeID, RequestedDataType).ConfigureAwait(false);
            return Json(res);
        }
        #region RecentDaysTransactions
        [HttpPost]
        [Route("Home/recent-days-transactions")]
        [Route("APIUser/recent-days-transactions")]
        [Route("recent-days-transactions")]
        public async Task<IActionResult> RecentDaysTransactions(int ServiceTypeID, int OpTypeID)
        {
            IReportML ml = new ReportML(_accessor, _env);
            return Json(await ml.GetRecentDaysTransactions(OpTypeID, ServiceTypeID).ConfigureAwait(false));
        }

        [HttpPost]
        [Route("Home/recent-days-prisecter")]
        [Route("APIUser/recent-days-prisecter")]
        [Route("recent-days-prisecter")]
        public async Task<IActionResult> RecentDaysPriSecTer(int PriSecTerType)
        {
            IReportML ml = new ReportML(_accessor, _env);
            return Json(await ml.GetRecentDaysPriSecTer(PriSecTerType).ConfigureAwait(false));
        }

        #endregion

        #endregion
        [HttpPost]
        [Route("Report/Log-Details")]
        [Route("Log-Details")]
        public IActionResult _LogDetails(CommonReq req)
        {
            IReportML rMl = new ReportML(_accessor, _env);
            var lst = rMl.GetLogDetails(req);
            string PView = "";
            if (lst.GetType() == typeof(List<ROfferLog>))
            {
                PView = "_ROfferLog";
            }
            else if (lst.GetType() == typeof(List<FetchBillLog>))
            {
                PView = "_FetchBillLog";
            }
            else if (lst.GetType() == typeof(List<LookUpLog>))
            {
                PView = "_LookUpLog";
            }
            else if (lst.GetType() == typeof(List<APIUrlHittingLog>))
            {
                PView = "_APIURLLog";
            }
            return PartialView("PartialView/" + PView, lst);
        }
        [HttpPost]
        [Route("Home/GetHolidaySection")]
        [Route("GetHolidaySection")]
        public IActionResult HolidaySection()
        {
            IBankML bankML = new BankML(_accessor, _env);
            var res = bankML.GetUpcomingHolidays();
            return PartialView(GetPartialView_SharedComponents("__UpcomingHolidays"), res);
        }


        [Route("AutoLowBalance")]
        public IActionResult AutoLowBalance()
        {
            return View();
        }

        #region BBPS-Complaints-Status-Check
        [Route("/BBPSComplaintsStatusCheck")]
        public IActionResult BBPSComplaintsStatusCheck()
        {
            if (_lr.RoleID.In(Role.Retailor_Seller, Role.APIUser))
            {
                return View();
            }
            return Ok();
        }

        [HttpPost]
        [Route("/bbps-complaint-status-check")]
        public IActionResult BBPSTranStatusCheck(string liveid, int comType)
        {
            if (_lr.RoleID.In(Role.Retailor_Seller, Role.APIUser))
            {
                var ml = new BBPSML(_accessor, _env);
                var res = ml.BBPSComplainStatusCheck(liveid, comType);

                return Json(new
                {
                    status = res.Statuscode == ErrorCodes.Minus1 ? "Invalid Complain ID" : BBPSComplaintRespType.GetComplaintStatusText(res.ComplainStatus)
                });
            }

            return Ok();
        }
        #endregion
        #region BBPS-Transaction-Status-Check
        [Route("/BBPSTranStatusCheck")]
        public IActionResult BBPSTranStatusCheck()
        {
            if (_lr.RoleID.In(Role.Retailor_Seller, Role.APIUser))
            {
                return View();
            }
            return Ok();
        }
        [HttpPost]
        [Route("/bbps-generateOTP")]
        public IActionResult GenerateBBPSComplainOTP(bool IsResend, string mob)
        {
            IResponseStatus _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = "OTP could not be generated"
            };
            if (!Validate.O.IsMobile(mob ?? string.Empty))
            {
                _res.Msg = "Invalid MobileNo";
                return Json(_res);
            }
            var o = IsResend ? _session.GetString(SessionKeys.CommonOTP) : HashEncryption.O.CreatePasswordNumeric(6);
            if (IsResend == false)
            {
                _session.SetString(SessionKeys.CommonOTP, o);
            }
            IUserML uml = new UserML(_accessor, _env);
            var alertData = uml.GetUserDeatilForAlert(_lr.UserID);
            if (alertData.Statuscode == ErrorCodes.One)
            {
                alertData.UserMobileNo = mob;
                IAlertML alertMl = new AlertML(_accessor, _env);
                alertData.OTP = o;
                _res = alertMl.OTPSMS(alertData);
            }
            return Json(_res);
        }
        [HttpPost]
        [Route("/bbps-transaction-status-check")]
        public IActionResult BBPSTranStatusCheck(string rpid, bool IsTran, string OTP, string FromDate, string ToDate)
        {
            if (IsTran == false)
            {
                if (_session.GetString(SessionKeys.CommonOTP) != OTP || string.IsNullOrEmpty(OTP))
                {
                    return Json(new
                    {

                        Statuscode = ErrorCodes.Minus1,
                        Msg = "Invalid OTP"
                    });
                }
            }
            if (_lr.RoleID.In(Role.Retailor_Seller, Role.APIUser))
            {
                var ml = new ReportML(_accessor, _env);
                var res = ml.GetBBPSTransactionStatusCheck((IsTran ? rpid : string.Empty), FromDate, ToDate, (!IsTran ? rpid : string.Empty));
                return Json(new
                {
                    Statuscode = ErrorCodes.One,
                    Msg = "",
                    billerName = res.Operator,
                    caNumber = res.ACCOUNT,
                    amount = res.AMOUNT,
                    refNumber = res.OPID,
                    status = RechargeRespType.GetRechargeStatusText(res.STATUS)
                });
            }

            return Ok();
        }
        #endregion

        #region InternalProcess
        [HttpPost]
        [Route("Report/InLogout")]
        [Route("InLogout")]
        public async Task<IActionResult> _InLogout()
        {
            IResponseStatus responseStatus = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.RoleID > 0)
            {
                LogoutReq logoutReq = new LogoutReq
                {
                    LT = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    ULT = _lr.LoginTypeID,
                    UserID = _lr.UserID,
                    SessID = _lr.SessID,
                    SessionType = SessionType.Single,
                    RequestMode = RequestMode.PANEL
                };
                ILoginML loginML = new LoginML(_accessor, _env);
                responseStatus = await loginML.DoLogout(logoutReq);
                if (ClearCurrentSession())
                {
                    return Json(responseStatus);
                }
                else
                {
                    return Json(responseStatus);
                }
            }
            return Json(responseStatus);
        }


        [HttpPost]
        [Route("InForget")]
        public IActionResult _InForget()
        {
            var responseStatus = new ForgetPasss
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvalidLogin
            };
            var loginDetail = new LoginDetail
            {
                LoginID = _lr.UserID,
                LoginTypeID = _lr.LoginTypeID,
            };
            if (_lr.RoleID == Role.Admin)
                loginDetail.Prefix = "AN";
            if (_lr.RoleID == Role.APIUser)
                loginDetail.Prefix = "AU";
            if (_lr.RoleID == Role.FOS)
                loginDetail.Prefix = "FS";
            if (_lr.RoleID == Role.MasterWL)
                loginDetail.Prefix = "SA";
            if (_lr.RoleID == Role.Retailor_Seller)
                loginDetail.Prefix = "RT";
            if (_lr.RoleID == Role.Distributor)
                loginDetail.Prefix = "DT";
            if (_lr.RoleID == Role.Master_Distributor)
                loginDetail.Prefix = "MD";
            if (_lr.RoleID == Role.Customer)
                loginDetail.Prefix = "CT";
            if (loginDetail == null)
            {
                return Json(responseStatus);
            }
            if (!loginDetail.LoginTypeID.In(LoginType.ApplicationUser, LoginType.CustomerCare, LoginType.Employee))
            {
                responseStatus.Msg = "Choose user login type!";
                return Json(responseStatus);
            }
            var _loginML = new LoginML(_accessor, _env);
            loginDetail.RequestMode = RequestMode.PANEL;
            return Json(_loginML.Forget(loginDetail));
        }
        #endregion
        #region BillFetchReport
        [HttpPost]
        [Route("Home/BillFetchOperator")]
        [Route("BillFetchOperator")]
        public async Task<IActionResult> BillFetchOperator(int opTypeID)
        {
            IOperatorML opML = new OperatorML(_accessor, _env);
            var Operators = opML.GetOperators(opTypeID);
            return Json(Operators);
        }

        [HttpGet]
        [Route("Home/BillFetchReport")]
        [Route("BillFetchReport")]
        public async Task<IActionResult> BillFetchReport()
        {
            var loginRes = chkAlternateSession();
            IAPIML aPIML = new APIML(_accessor, _env);
            IOperatorML opML = new OperatorML(_accessor, _env);
            IUserML userML = new UserML(loginRes);
            IReportML rml = new ReportML(_accessor, _env);
            var opTypes = opML.GetOptypes();
            if (opTypes.Any())
            {
                if (ApplicationSetting.IsWLAPIAllowed && _lr.IsWLAPIAllowed)
                {
                    opTypes = opTypes.Where(x => x.ID.In(OPTypes.AllowToWhitelabel));
                }
                else
                {
                    opTypes = opTypes.Where(x => x.ServiceTypeID.In(ServiceType.BillPayment));
                }
            }
            IEnumerable<OperatorDetail> Ops = null;
            if (ApplicationSetting.IsWLAPIAllowed && _lr.IsWLAPIAllowed)
            {
                Ops = opML.GetOperators(string.Join(',', OPTypes.AllowToWhitelabel));
            }
            else
            {
                Ops = await opML.GetOPListBYServices((ServiceType.BillPayment).ToString()).ConfigureAwait(false);
            }

            var rep = new BillFetchReportModel
            {
                RechargeAPI = aPIML.GetAPIDetail(),
                Operators = await opML.GetOPListBYServices((ServiceType.BillPayment).ToString()).ConfigureAwait(false),
                OpTypes = opTypes,
                IsAdmin = loginRes.RoleID == Role.Admin && loginRes.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowRechargeReport),
                IsAPIUser = loginRes.LoginTypeID == LoginType.ApplicationUser && loginRes.RoleID == Role.APIUser,
                IsEmployee = loginRes.LoginTypeID == LoginType.Employee ? true : false,
                IsEndUser = userML.IsEndUser(),
                IsWLAPIAllowed = ApplicationSetting.IsWLAPIAllowed && _lr.IsWLAPIAllowed,
                BillFetchSummary = await rml.GetBillFetchSummary(_lr.UserID)
            };
            return View(rep);
        }

        [HttpPost]
        [Route("Home/BillFetch-Report")]
        [Route("BillFetch-Report")]
        public async Task<IActionResult> _BillFetchReport([FromBody] RechargeReportFilter filter)
        {
            IUserML userML = new UserML(_lr);
            IBillFetchReportML ml = new ReportML(_accessor, _env);
            var model = new BillFetchReportModel();
            var loginResp = chkAlternateSession();
            model.IsWLAPIAllowed = ApplicationSetting.IsWLAPIAllowed && loginResp.IsWLAPIAllowed;
            if (loginResp != null)
            {
                model.IsAdmin = loginResp.RoleID == Role.Admin && loginResp.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowRechargeReport);
                model.CanMarkDispute = loginResp.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.MarkDispute);
                model.CanFail = (loginResp.RoleID == Role.Admin || model.IsWLAPIAllowed) && loginResp.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.PageFailed);
                model.CanSuccess = (loginResp.RoleID == Role.Admin || model.IsWLAPIAllowed) && loginResp.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.PageSuccess);
                model.IsAPIUser = loginResp.LoginTypeID == LoginType.ApplicationUser && loginResp.RoleID == Role.APIUser;
                model.IsEndUser = userML.IsEndUser();
                model.IsEmployee = loginResp.LoginTypeID == LoginType.Employee ? true : false;
                model.Report = await ml.GetBillFetchReport(filter).ConfigureAwait(false);
            }
            return PartialView("PartialView/_BillFetchReport", model);
        }

        [HttpGet]
        [Route("Home/BillFetch-Report")]
        [Route("BillFetch-Report")]
        public async Task<IActionResult> _BillFetchReportExport(RechargeReportFilter filter)
        {
            IUserML userML = new UserML(_accessor, _env);
            IBillFetchReportML ml = new ReportML(_accessor, _env);
            filter.IsExport = true;
            var loginResp = chkAlternateSession();
            var IsWLAPIAllowed = loginResp.IsWLAPIAllowed && ApplicationSetting.IsWLAPIAllowed;
            var IsAdmin = loginResp.RoleID == Role.Admin && loginResp.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowBillFetchReport);
            // List<BillFetchReportResponse> _reportRole;
            List<ProcBillFetchReportResponse> _report;

            _report = await ml.GetBillFetchReport(filter).ConfigureAwait(false);

            DataTable dataTable = ConverterHelper.O.ToDataTable(_report);

            dataTable.Columns.Remove("ResultCode");
            dataTable.Columns.Remove("Msg");
            if (!IsAdmin)
            {
                dataTable.Columns.Remove("API");
            }
            dataTable.Columns.Remove("Type_");
            dataTable.Columns.Remove("_Type");
            dataTable.Columns.Remove("RequestMode");
            dataTable.Columns.Remove("RequestModeID");
            dataTable.Columns.Remove("Balance");
            dataTable.Columns.Remove("LastBalance");
            dataTable.Columns.Remove("OpType");

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("BillFetchReport1");
                worksheet.Cells["A1"].LoadFromDataTable(dataTable, PrintHeaders: true);
                worksheet.Row(1).Height = 20;
                worksheet.Row(1).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Row(1).Style.Font.Bold = true;
                int rowindex = 2;
                foreach (DataRow row in dataTable.Rows)
                {
                    if (rowindex == 2)
                    {
                        worksheet.Cells[1, dataTable.Columns["EntryDate"].Ordinal + 1].Value = "BillFetchDate";
                    }

                    rowindex++;
                }
                for (var col = 1; col < dataTable.Columns.Count + 1; col++)
                {
                    worksheet.Column(col).AutoFit();
                }
                var exportToExcel = new ExportToExcel
                {
                    Contents = package.GetAsByteArray(),
                    FileName = "BillFetchReport.xlsx"
                };
                return File(exportToExcel.Contents, DOCType.XlsxContentType, exportToExcel.FileName);
            }
        }

        [HttpPost]
        [Route("Home/BillFetch-APiUrl")]
        [Route("BillFetch-APiUrl")]
        public async Task<IActionResult> _BillFetchApiResponse(string TID, string T)
        {
            IBillFetchReportML ml = new ReportML(_accessor, _env);
            var _report = await ml.GetBillFetchApiResponses(TID, T).ConfigureAwait(false);
            return PartialView("PartialView/_BillFetchApiResponse", _report);
        }
        #endregion

        [HttpPost]
        [Route("upload-DTHLeadOperator")]
        public IActionResult UploadDTHLeadOperator(IFormFile file, int Name)
        {
            IBannerML bannerML = new ResourceML(_accessor, _env);
            var _res = bannerML.UploadDTHLeadOperator(file, _lr.WID.ToString(), Name, _lr);
            return Json(_res);
        }

        [HttpPost]
        [Route("get-OperatorBanner")]
        public IActionResult _OperatorBannerMaster(int Name)
        {
            IBannerML bannerML = new ResourceML(_accessor, _env);
            var resp = bannerML.GetOperatorBanner(_lr.WID.ToString(), Name);
            return PartialView("PartialView/_OperatorBannerMaster", resp);
        }

        [HttpPost]
        [Route("rm-Operatorbanner")]
        public IActionResult RemoveOperatorBanner(string id, int Name)
        {
            IBannerML bannerML = new ResourceML(_accessor, _env);
            var _res = bannerML.RemoveOperatorBanners(id, Name, _lr.WID.ToString(), _lr);
            return Json(_res);
        }
        [HttpPost]
        [Route("get-OperatorBannerSeller")]
        public IActionResult OperatorBannerSeller(int Name)
        {
            IBannerML bannerML = new ResourceML(_accessor, _env);
            IOperatorML operatorOperation = new OperatorML(_accessor, _env);

            IEnumerable<int>[] nums = new IEnumerable<int>[10];
            IEnumerable<OperatorDetail> oplist = operatorOperation.GetOperators(OPTypes.DTHConnection);

            var opIDL = oplist.Select(p => p.OID).ToArray();
            var bImg = new List<BannerImage>();
            foreach (var item in opIDL)
            {
                var i = bannerML.GetOperatorBanner(_lr.WID.ToString(), item);
                foreach (var j in i)
                {
                    bImg.Add(j);
                }
            }
            return PartialView("PartialView/_OperatorBannerMasterSeller", bImg);
        }
        [HttpPost]
        [Route("Home/GetoperatorParam")]
        [Route("GetoperatorParam")]
        public IActionResult GetoperatorParam(int OID)
        {

            IReportML rmL = new ReportML(_accessor, _env);
            var res = rmL.GetOperatorParam(OID);
            return PartialView("PartialView/_GetOperatorParam", res);

        }
        #region RefferalImageUploadAndRemoveSection
        [HttpPost]
        [Route("RefferalImageUpload")]
        public IActionResult SavedRefferalImage(IFormFile file, int opType)
        {
            IBannerML bannerML = new ResourceML(_accessor, _env);
            var _res = bannerML.SavedRefferalImage(file, _lr.WID.ToString(), opType, _lr);

            return Json(_res);
        }
        [HttpPost]
        [Route("GetRefferalImage")]
        public IActionResult GetSavedRefferalImage()
        {
            IBannerML bannerML = new ResourceML(_accessor, _env);
            var resp = bannerML.GetSavedRefferalImage(_lr.WID.ToString());
            return PartialView("PartialView/_RefferalImage", resp);
        }
        [HttpPost]
        [Route("RemoveRefferalImage")]
        public IActionResult DeleteRefferalImage(string id)
        {
            IBannerML bannerML = new ResourceML(_accessor, _env);
            var _res = bannerML.RefferalImageRemove(id, _lr.WID.ToString(), _lr);
            return Json(_res);
        }
        #endregion

        [HttpPost]
        [Route("SendPendingRechargeNotification")]
        public async Task<IActionResult> SendPendingRechargeNotification()
        {
            var response = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            try
            {
                PendingNotifications p = new PendingNotifications();
                p.PendingRechargeNotificationAsync(_accessor, _env, _lr.UserID, userName: _lr.Name);
                await Task.Delay(0);
                response = new ResponseStatus
                {
                    Statuscode = ErrorCodes.One,
                    Msg = "Notification triggered successfully"
                };
            }
            catch (Exception ex)
            {
                response = new ResponseStatus
                {
                    Statuscode = ErrorCodes.Minus1,
                    Msg = ErrorCodes.AnError
                };
            }
            return Json(response);
        }

        [HttpPost]
        [Route("SendPendingRefundNotification")]
        public async Task<IActionResult> SendPendingRefundNotification()
        {
            var response = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            try
            {
                PendingNotifications p = new PendingNotifications();
                p.PendingRefundNotificationAsync(_accessor, _env, _lr.UserID, userName: _lr.Name);
                response = new ResponseStatus
                {
                    Statuscode = ErrorCodes.One,
                    Msg = "Notification triggered successfully"
                };
            }
            catch (Exception ex)
            {
                response = new ResponseStatus
                {
                    Statuscode = ErrorCodes.Minus1,
                    Msg = ErrorCodes.AnError
                };
            }
            return Json(response);
        }

        #region BulkQRGeneration
        [Route("BulkQRGeneration")]
        public IActionResult BulkQRGeneration()
        {
            return View();
        }

        [HttpPost]
        [Route("get-qr-data")]
        public async Task<IActionResult> _BulkQRGeneration(QRFilter qRFilter)
        {
            IReportML rmL = new ReportML(_accessor, _env);
            var res = await rmL.GetQRGenerationData(qRFilter);
            return PartialView("PartialView/_BulkQRGeneration", res);
        }

        [HttpPost]
        [Route("bulk-gen-qr")]
        public async Task<IActionResult> _GenerateQRBulk(int Qty)
        {

            IReportML rmL = new ReportML(_accessor, _env);
            var res = await rmL.BulkQRGeneration(Qty);
            return Json(res);

        }

        [HttpPost]
        [Route("down-qr")]
        public IActionResult _DownloadQRByRefID(int RefID)
        {

            IReportML rmL = new ReportML(_accessor, _env);
            var res = rmL.DownloadQR(RefID);
            return Json(res);
        }

        [HttpGet]
        public IActionResult QRCodeDownloadByAdmin(string bri, string aID)
        {

            IPaymentGatewayML paymentML = new PaymentGatewayML(_accessor, _env);
            string BankRefID = aID + "AUTOID" + bri;
            var res = paymentML.CreateQRIntent(BankRefID);
            if (res.Statuscode == ErrorCodes.One && !string.IsNullOrEmpty(res.CommonStr))
            {
                var qRCodeGenerator = new QRCodeGenerator();
                QRCodeData QCD = qRCodeGenerator.CreateQrCode(res.CommonStr, QRCodeGenerator.ECCLevel.Q);
                IUserWebsite _userWebsite = new UserML(_accessor, _env);
                var model = _userWebsite.GetCompanyProfileUser(_lr.UserID);
                QRCode qRCode = new QRCode(QCD);
                Bitmap b = new Bitmap(1050, 2100);
                Graphics g = Graphics.FromImage(b);
                g.DrawRectangle(new Pen(Color.Black, 3), 5, 5, b.Width - 12, b.Height - 12);
                Image imgLogo = (Image)(new Bitmap(Image.FromFile(DOCType.LogoSuffix.Replace("{0}", "1")), new Size(450, 175)));
                Image imgUPI = Image.FromFile(@"Image/QRImg/bhim_upi.jpg");
                Image imgAPP = Image.FromFile(@"Image/QRImg/app_logo.jpg");
                g.DrawImage(imgLogo, new Point(((b.Width - imgLogo.Width) / 2), 40));
                var str = "All in ONE BHIM UPI QR";
                var msW = g.MeasureString(str, new Font("Arial", 36));
                g.DrawString(str, new Font("Arial", 36), Brushes.Red, new Point(((b.Width - Convert.ToInt16(msW.Width)) / 2), (imgLogo.Height + 75)));
                //str = ("User QR Code");
                //msW = g.MeasureString(str, new Font("Arial", 36));
                //g.DrawString(str, new Font("Arial", 36), Brushes.Black, new Point(((b.Width - Convert.ToInt16(msW.Width)) / 2), 325));
                g.DrawImage(qRCode.GetGraphic(18), new Point(((b.Width - qRCode.GetGraphic(18).Width) / 2), 375));
                str = "Scan & Pay with any BHIM UPI app";
                msW = g.MeasureString(str, new Font("Arial", 36));
                g.DrawString(str, new Font("Arial", 36), Brushes.Black, new Point(((b.Width - Convert.ToInt16(msW.Width)) / 2), 1500));
                g.DrawImage(imgUPI, new Point(125, 1600));
                g.DrawImage(imgAPP, new Point(60, 1800));
                str = ("Customer Helpline : " + (model.CustomerCareMobileNos ?? string.Empty));
                msW = g.MeasureString(str, new Font("Arial", 32));
                g.DrawString(str, new Font("Arial", 32), Brushes.Black, new Point(((b.Width - Convert.ToInt16(msW.Width)) / 2), 2025));
                return File(ConverterHelper.BitmapToBytesCode(b), "image/png");
            }
            else
            {
                string msg = "";// res.Statuscode == ErrorCodes.One ? "QR Data Not Found" : res.Msg;
                Bitmap b = new Bitmap(500, 500);
                Graphics g = Graphics.FromImage(b);
                g.DrawString(msg, new Font("Arial", 36), Brushes.Red, new Point(10, 10));
                return File(ConverterHelper.BitmapToBytesCode(b), "image/png");
            }
        }

        #endregion





        [HttpPost]
        [Route("downloadpdfFileIRCTC")]
        public async Task<IActionResult> _Downloadpdfirctc(string fileName)
        {
            IResourceML ml = new ResourceML(_accessor, _env);
            var res = ml.downloadPdfirctc(_lr.LoginTypeID, _lr.UserID, _lr.WID);
            if (res.Statuscode == ErrorCodes.Minus1 || res.IrctcID == null || res.IrctcStatus == 0)
            {

                return Json(res);
            }
            if (string.IsNullOrEmpty(res.IrctcID))
            {
                res.Statuscode = ErrorCodes.Minus1;
                res.Msg = "Irctc ID not exist!";
                return Json(res);
            }

            var html = await this.RenderViewAsync("_IrctcCertficated", res, true);
            var response = ReportHelper.HtmlToPdfConverter(html, "Irctc Certificate.pdf");

            return Json(response);

        }


        [HttpPost]
        [Route("downloadpdfFile1")]
        public async Task<IActionResult> _downloadpdf1(string fileName)

        {
            var response = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError,
            };
            string html = null;
            IResourceML ml = new ResourceML(_accessor, _env);
            var res = ml.downloadPdf(_lr.LoginTypeID, _lr.UserID, _lr.WID);
            if (res.Statuscode == ErrorCodes.Minus1)
            {
                response = new ResponseStatus
                {
                    Statuscode = res.Statuscode,
                    Msg = res.Msg,
                };
                goto Finish;
            }
            res.OutletID = _lr.OutletID;
            res.OutletName = _lr.OutletName;
            res.WID = _lr.WID;
            html = await this.RenderViewAsync("MerchantCertificate", res, true);
            if (res.KYCStatus != KYCStatusType.COMPLETED)
            {
                response = new ResponseStatus
                {
                    Statuscode = ErrorCodes.Minus1,
                    Msg = "KYC not completed"
                };
                goto Finish;
            }
            response = ReportHelper.HtmlToPdfConverter(html, "Merchant Certificate.pdf", 1);
            Finish:
            return Json(response);
        }

        [HttpPost]

        [Route("_ResendSMS")]
        public IActionResult ResendSMS([FromBody] SMSSetting req)
        {


            IAlertML ml = new AlertML(_accessor, _env);
            var a = ml.ResendSMS(req.MobileNos, req.Msg);


            return Json(a);
        }

        [HttpPost]
        [Route("Get-realtime-Comm")]
        public IActionResult _RealTimeComm()
        {
            ISlabML sML = new SlabML(_accessor, _env);
            var sdml = sML.GetRealtimeComm();
            return PartialView("PartialView/_RealTimeComm", sdml);
        }


        [Route("Home/UserPackageLimitLedger")]
        [Route("UserPackageLimitLedger")]
        public IActionResult UserPackageLimitLedger()
        {
            IUserML userml = new UserML(_accessor, _env);
            var loginResp = chkAlternateSession();
            var ledgerReportModel = new LedgerReportModel();
            if (_lr != null)
            {
                ledgerReportModel.HaveChild = !userml.IsEndUser() || userml.IsCustomerCareAuthorised(ActionCodes.ShowUserLedger);
            }
            if (_lrEmp != null)
            {
                ledgerReportModel.HaveChild = _lrEmp.LoginTypeID == LoginType.Employee ? true : false;
            }

            ledgerReportModel.SelfMobile = loginResp.MobileNo ?? "";
            ledgerReportModel.IsEmployee = loginResp.LoginTypeID == LoginType.Employee ? true : false;
            return View(ledgerReportModel);
        }

        [HttpPost]
        [Route("Home/UserPackage-LimitLedger")]
        [Route("UserPackage-LimitLedger")]
        public async Task<IActionResult> _UserPackageLimitLedger([FromBody] ULedgerReportFilter filter)
        {
            var loginResp = chkAlternateSession();
            IUserML userml = new UserML(loginResp);
            if (_lr != null)
            {
                ViewBag.HaveChild = !userml.IsEndUser() || userml.IsCustomerCareAuthorised(ActionCodes.ShowUserLedger);
            }
            if (_lrEmp != null)
            {
                ViewBag.HaveChild = _lrEmp.LoginTypeID == LoginType.Employee ? true : false;
            }
            ViewBag.SelfMobile = loginResp.MobileNo ?? "";
            IReportML ml = new ReportML(_accessor, _env);
            IEnumerable<ProcUserLedgerResponse> _report = await ml.GetUserPackageLimitLedgerrList(filter);
            return PartialView("PartialView/_UserPackageLimitLedger", _report);
        }

        [HttpPost]
        [Route("Switch-IMPStoNEFT")]
        public IActionResult SwitchIMPStoNEFT(bool Status)
        {
            IUserML UserrML = new UserML(_accessor, _env);
            var res = UserrML.SwitchIMPStoNEFT(Status);
            if (res.Statuscode == ErrorCodes.One)
            {
                ILoginML lML = new LoginML(_accessor, _env);
                var PreLDetail = _lr;
                PreLDetail.IsRealAPI = Status;
                lML.ResetLoginSession(_lr);
            }
            return Json(res);
        }

        #region RailwayOutlet
        [HttpGet]
        [Route("RailwayOutletPending")]
        public IActionResult RailwayOutletPending()
        {
            return View();
        }

        [HttpPost]
        [Route("rail-outlet-pend")]
        public async Task<IActionResult> _RailwayOutletPending(UserRequest req)
        {
            var userML = new ReportML(_accessor, _env);
            var res = await userML.GetRailKYCUsers(req);
            return PartialView("PartialView/_RailwayOutletPending", res);
        }
        [HttpPost]
        [Route("act-rail-pend")]
        public IActionResult _ActivateRailPend(int outletID, string IRCTCID, int Status)
        {
            var userML = new ReportML(_accessor, _env);
            var res = userML.ActivateRailPending(outletID, IRCTCID, Status);
            return Json(res);
        }
        #endregion

        [Route("FetchBillSummary")]
        public IActionResult FetchBillSummary()
        {
            return View();
        }

        [HttpPost]
        [Route("FetchBillSummary")]
        public IActionResult FetchBillSummary(UserBillFetchReport Data)
        {
            IUserML userrML = new UserML(_accessor, _env);
            var res = userrML.GetUserBillFetch(Data);
            return PartialView("PartialView/_FetchBillSummary", res);

        }
    }
}

