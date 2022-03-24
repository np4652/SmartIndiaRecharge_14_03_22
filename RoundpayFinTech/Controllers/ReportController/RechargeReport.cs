using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Fintech.AppCode.Configuration;
using Fintech.AppCode.HelperClass;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using RoundpayFinTech.AppCode;
using RoundpayFinTech.AppCode.Configuration;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.MiddleLayer;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Report;
using RoundpayFinTech.AppCode.Model.Reports.Filter;
using RoundpayFinTech.Models;

namespace RoundpayFinTech.Controllers
{
    public partial class ReportController
    {
        #region RechargeReport
        [HttpGet]
        [Route("Home/RechargeReport")]
        [Route("RechargeReport")]
        public async Task<IActionResult> RechargeReport(int f = 0)
        {
            var loginRes = chkAlternateSession();
            IAPIML aPIML = new APIML(_accessor, _env);
            IOperatorML opML = new OperatorML(_accessor, _env);
            IUserML userML = new UserML(loginRes);
            var opTypes = opML.GetOptypes();
            if (opTypes.Any())
            {
                if ((ApplicationSetting.IsWLAPIAllowed && _lr.IsWLAPIAllowed) && (_lr.WID != 1))
                {
                    opTypes = opTypes.Where(x => x.ID.In(OPTypes.AllowToWhitelabel));
                }
                else
                {
                    opTypes = opTypes.Where(x => x.ServiceTypeID.In(ServiceType.Recharge, ServiceType.BillPayment, ServiceType.GenralInsurance, ServiceType.PSAService, ServiceType.PublicEService, ServiceType.Travel, ServiceType.CouponVouchers));
                }
            }
            IEnumerable<OperatorDetail> Ops = null;
            if (ApplicationSetting.IsWLAPIAllowed && _lr.IsWLAPIAllowed)
            {
                Ops = opML.GetOperators(string.Join(',', OPTypes.AllowToWhitelabel));
            }
            else
            {
                Ops = await opML.GetOPListBYServices(ServiceType.RechargeReportServices).ConfigureAwait(false);
            }

            var rep = new RechargeReportModel
            {
                Flag = f,
                RechargeAPI = aPIML.GetAPIDetail(),
                Operators = await opML.GetOPListBYServices(ServiceType.RechargeReportServices).ConfigureAwait(false),
                OpTypes = opTypes,
                IsAdmin = loginRes.RoleID == Role.Admin && loginRes.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowRechargeReport),
                IsAPIUser = loginRes.LoginTypeID == LoginType.ApplicationUser && loginRes.RoleID == Role.APIUser,
                IsEmployee = loginRes.LoginTypeID == LoginType.Employee ? true : false,
                IsEndUser = userML.IsEndUser(),
                IsWLAPIAllowed = ApplicationSetting.IsWLAPIAllowed && _lr.IsWLAPIAllowed
            };
            return View(rep);
        }

        [HttpPost]
        [Route("Home/Recharge-Report")]
        [Route("Recharge-Report")]
        public async Task<IActionResult> _RechargeReport([FromBody] RechargeReportFilter filter)
        {
            IUserML userML = new UserML(_lr);
            IRechargeReportML ml = new ReportML(_accessor, _env);
            var model = new RechargeReportModel();
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
                model.Report = await ml.GetRechargeReport(filter).ConfigureAwait(false);

            }
            return PartialView("PartialView/_RechargeReport", model);
        }
        [HttpGet]
        [Route("Home/Recharge-Report")]
        [Route("Recharge-Report")]
        public async Task<IActionResult> _RechargeReportExport(RechargeReportFilter filter)
        {
            IUserML userML = new UserML(_accessor, _env);
            IRechargeReportML ml = new ReportML(_accessor, _env);
            filter.IsExport = true;
            var loginResp = chkAlternateSession();
            var IsWLAPIAllowed = loginResp.IsWLAPIAllowed && ApplicationSetting.IsWLAPIAllowed;
            var IsAdmin = loginResp.RoleID == Role.Admin && loginResp.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowRechargeReport);
            List<RechargeReportResponse> _reportRole;
            List<ProcRechargeReportResponse> _report;
            DataTable dataTable = new DataTable();
            List<string> removableCol = new List<string> { "ResultCode", "Msg", "TID", "UserID", "OID", "_Type", "Optional1", "Optional2", "Optional3", "Optional4", "RefundStatus", "RefundStatus_", "IsWTR", "ModifyDate", "CommType" };

            if (ApplicationSetting.IsRoleFixed)
            {
                _reportRole = await ml.GetRechargeReportRole(filter).ConfigureAwait(false);
                dataTable = ConverterHelper.O.ToDataTable(_reportRole);
                if (!IsAdmin && !IsWLAPIAllowed)
                {
                    string[] additionalCol = { "API", "VendorID", "CCName", "CCMobile" };
                    foreach (string str in additionalCol)
                    {
                        removableCol.Add(str);
                    }
                }
                if (IsWLAPIAllowed)
                {
                    string[] additionalCol = { "CCName", "CCMobile" };
                    foreach (string str in additionalCol)
                    {
                        removableCol.Add(str);
                    }
                }
                bool IsRemoveSA = loginResp.RoleID.In(FixedRole.MasterDistributer, FixedRole.Distributor, FixedRole.Retailor, FixedRole.APIUser);
                bool IsRemoveMD = loginResp.RoleID.In(FixedRole.Distributor, FixedRole.Retailor, FixedRole.APIUser);
                bool IsRemoveDT = loginResp.RoleID.In(FixedRole.Retailor, FixedRole.APIUser);
                if (!loginResp.RoleID.In(FixedRole.APIUser, FixedRole.Admin))
                {
                    removableCol.Add("ApiRequestID");
                }
                if (IsRemoveSA)
                {
                    string[] additionalCol = { "SubAdmin", "SAMobile", "SACommType", "SASlabCommType", "SAComm", "SAGST", "SATDS", "ApiComm" };
                    foreach (string str in additionalCol)
                    {
                        removableCol.Add(str);
                    }
                }
                if (IsRemoveMD)
                {
                    string[] additionalCol = { "MasterDistributer", "MDMobile", "MDCommType", "MDSlabCommType", "MDComm", "MDGST", "MDTDS", "ApiComm" };
                    foreach (string str in additionalCol)
                    {
                        removableCol.Add(str);
                    }
                }
                if (IsRemoveDT)
                {
                    string[] additionalCol = { "Distributer", "DTMobile", "DTCommType", "DTSlabCommType", "DTComm", "DTGST", "DTTDS", "ApiComm" };
                    foreach (string str in additionalCol)
                    {
                        removableCol.Add(str);
                    }
                }
            }
            else
            {
                _report = await ml.GetRechargeReport(filter);
                dataTable = ConverterHelper.O.ToDataTable(_report);
                string[] col = { "Display1", "Display2", "Display3", "Display4" };
                foreach (string str in col)
                {
                    removableCol.Add(str);
                }

                if (!IsAdmin && !IsWLAPIAllowed)
                {
                    string[] additionalCol = { "API", "VendorID" };
                    foreach (string str in additionalCol)
                    {
                        removableCol.Add(str);
                    }
                }
                if (!loginResp.RoleID.In(Role.APIUser, Role.Admin))
                {
                    removableCol.Add("ApiRequestID");
                }
            }
            foreach (string str in removableCol)
            {
                if (dataTable.Columns.Contains(str))
                {
                    dataTable.Columns.Remove(str);
                }
            }
            if (dataTable.Columns.Contains("LastBalance"))
            {
                dataTable.Columns["LastBalance"].ColumnName = "OpeningBalance";
            }
            if (dataTable.Columns.Contains("Balance"))
            {
                dataTable.Columns["Balance"].ColumnName = "ClosingBalance";
            }
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
                var exportToExcel = new ExportToExcel
                {
                    Contents = package.GetAsByteArray(),
                    FileName = "RechargeReport.xlsx"
                };
                return File(exportToExcel.Contents, DOCType.XlsxContentType, exportToExcel.FileName);
            }
        }
        [HttpPost]
        [Route("Home/Recharge-APiUrl")]
        [Route("Recharge-APiUrl")]
        public async Task<IActionResult> _RechargeApiResponse(int TID, string T)
        {
            IRechargeReportML ml = new ReportML(_accessor, _env);
            var _report = await ml.GetRechargeApiResponses(TID, T).ConfigureAwait(false);
            return PartialView("PartialView/_RechargeApiResponse", _report);
        }
        [HttpPost]
        [Route("Home/t-summary")]
        [Route("t-summary")]
        public async Task<IActionResult> _RechargeReportSummary(int s)
        {
            IRechargeReportML ml = new ReportML(_accessor, _env);
            RechargeReportSummary _transactionSummary = await ml.GetTransactionSummary(s).ConfigureAwait(false);
            return Json(_transactionSummary);
        }

        [HttpPost]
        [Route("Home/u-comm")]
        [Route("u-comm")]
        public async Task<IActionResult> _UplineCommission(int t, string tr)
        {
            IRechargeReportML ml = new ReportML(_accessor, _env);
            var lst = await ml.GetUplineCommission(t, tr).ConfigureAwait(false);
            return PartialView("PartialView/_UplineCommission", lst);
        }

        [HttpPost]
        [Route("Home/w-2-r")]
        [Route("w-2-r")]
        public async Task<IActionResult> WTRRequest(int TID, string TransactionID, string RightAccount)
        {
            WTRRequest _req = new WTRRequest
            {
                TID = TID,
                RPID = TransactionID,
                RightAccount = RightAccount
            };
            IRechargeReportML ml = new ReportML(_accessor, _env);
            RefundRequestResponse _res = await ml.MarkWrong2Right(_req).ConfigureAwait(false);
            return Json(new { statuscode = _res.Statuscode, status = _res.Msg });
        }

        #endregion

        #region RechargeReportActions
        [HttpPost]
        [Route("Home/USF")]
        [Route("USF")]
        public IActionResult _UpdateStatusForm(char Status)
        {
            if (_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser || Status == 'W')
                return PartialView("PartialView/_UpdateTransactionStatus", Status);
            else
                return Ok("OK");
        }
        [HttpPost]
        [Route("Home/U-S-F")]
        [Route("U-S-F")]
        public async Task<IActionResult> _UpdateStatus(char Status, int TID, string TransactionID, string Remark)
        {
            var callbackData = new _CallbackData
            {
                TID = TID,
                TransactionID = TransactionID,
                Msg = Remark,
                RequestPage = "RechargeReport"
            };
            if (callbackData.TransactionID.Contains((char)160))
            {
                callbackData.RequestPage = "PendingPage";
                callbackData.TransactionID = callbackData.TransactionID.Replace(((char)160) + "", "");
            }
            if (Status.In('S', 's'))
                callbackData.TransactionStatus = RechargeRespType.SUCCESS;
            if (Status.In('F', 'f'))
                callbackData.TransactionStatus = RechargeRespType.FAILED;

            IRechargeReportML rml = new ReportML(_accessor, _env);
            return Json(await rml.UpdateTransactionStatus(callbackData).ConfigureAwait(false));
        }


        [HttpPost]
        [Route("Home/r-r")]
        [Route("r-r")]
        public async Task<IActionResult> RefundRequest(int TID, string TransactionID, bool Is, string o)
        {
            var _req = new RefundRequest
            {
                TID = TID,
                RPID = TransactionID,
                RequestMode = RequestMode.PANEL,
                IsResend = Is,
                OTP = o
            };

            IRechargeReportML rml = new ReportML(_accessor, _env);
            var _res = await rml.MarkDispute(_req).ConfigureAwait(false);
            if (_res.IsOTPRequired && !_req.IsResend)
            {
                return PartialView("PartialView/_RefundOTP", _res);
            }
            return Json(new { _res.Statuscode, _res.Msg });
        }
        #endregion

        #region PendingReport
        [HttpGet]
        [Route("Home/Pendings")]
        [Route("Pendings")]
        public async Task<IActionResult> PendingTransaction()
        {
            IAPIML aPIML = new APIML(_accessor, _env);
            IOperatorML opML = new OperatorML(_accessor, _env);
            var rep = new RechargeReportModel
            {
                RechargeAPI = aPIML.GetAPIDetail(),
                Operators = await opML.GetOPListBYServices(_lr.IsWLAPIAllowed && ApplicationSetting.IsWLAPIAllowed ? string.Join(",", OPTypes.AllowToWhitelabel) : ServiceType.RechargeReportServices).ConfigureAwait(false)
            };
            return View(rep);
        }

        [HttpPost]
        [Route("Home/pending")]
        [Route("pending")]
        public async Task<IActionResult> PendingTransaction(int APIID, int OID)
        {
            IReportML rml = new ReportML(_accessor, _env);
            IUserML userML = new UserML(_lr);
            var pendingTransactionModel = new PendingTransactionModel
            {
                CanSuccess = userML.IsCustomerCareAuthorised(ActionCodes.PageSuccess) || (_lr.LoginTypeID == LoginType.ApplicationUser && (_lr.RoleID == Role.Admin || (_lr.IsWLAPIAllowed && ApplicationSetting.IsWLAPIAllowed))),
                CanFail = userML.IsCustomerCareAuthorised(ActionCodes.PageFailed) || (_lr.LoginTypeID == LoginType.ApplicationUser && (_lr.RoleID == Role.Admin || (_lr.IsWLAPIAllowed && ApplicationSetting.IsWLAPIAllowed))),
                Report = await rml.PendingTransaction(APIID, OID, ReportType.Recharge).ConfigureAwait(false)
            };

            return PartialView("PartialView/_PendingTransaction", pendingTransactionModel);
        }
        [HttpPost]
        [Route("Home/rqsent")]
        [Route("rqsent")]
        public IActionResult RequestSent(int APIID)
        {
            IResponseStatus _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvalidParam
            };
            if (APIID > 0)
            {
                IReportML rml = new ReportML(_accessor, _env);
                _res = rml.UpdateRequestSent(APIID);
            }
            return Json(_res);
        }
        [HttpPost]
        [Route("Home/chk-sts")]
        [Route("chk-sts")]
        public async Task<IActionResult> Statuscheck(int TID, string TransactionID)
        {
            IResponseStatus _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = "Incomplete Request!"
            };
            if (TID > 0)
            {
                IRechargeReportML rml = new ReportML(_accessor, _env);
                _res = await rml.CheckStatusAsync(TID, TransactionID ?? string.Empty).ConfigureAwait(false);
            }
            ViewData["Heading"] = "Status Check Detail";
            return PartialView("PartialView/_StatusCheck", _res);
        }
        [HttpPost]
        [Route("Home/chk-sts-blk")]
        [Route("chk-sts-blk")]
        public async Task<IActionResult> StatuscheckBulk(int TID, string TransactionID)
        {
            IResponseStatus _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = "Incomplete Request!"
            };
            if (TID > 0)
            {
                IRechargeReportML rml = new ReportML(_accessor, _env);
                _res = await rml.CheckStatusAsync(TID, TransactionID ?? "").ConfigureAwait(false);
            }
            return Json(_res);
        }
        [HttpPost]
        [Route("Home/chk-sts-dmr")]
        [Route("chk-sts-dmr")]
        public async Task<IActionResult> StatuscheckDMR(int t, string tr)
        {
            IResponseStatus _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = "Incomplete Request!"
            };
            if (t > 0)
            {
                IDMRReportML rml = new ReportML(_accessor, _env);
                _res = await rml.CheckStatusDMRAsync(t, tr ?? string.Empty).ConfigureAwait(false);
            }
            ViewData["Heading"] = "Status Check Detail";
            return PartialView("PartialView/_StatusCheck", _res);
        }
        [HttpPost]
        [Route("Home/dmrpending-t")]
        [Route("dmrpending-tTr")]
        public async Task<IActionResult> _DMRPendingTransactionT()
        {
            IReportML rml = new ReportML(_accessor, _env);
            var Report = await rml.PendingTIDTransID(ReportType.DMR).ConfigureAwait(false);
            return Json(Report);
        }
        [HttpPost]
        [Route("Home/chk-sts-dmr-tTr")]
        [Route("chk-sts-dmr-tTr")]
        public async Task<IActionResult> StatuscheckDMRT(int t, string tr)
        {
            IDMRReportML rml = new ReportML(_accessor, _env);
            var _res = await rml.CheckStatusDMRAsync(t, tr ?? string.Empty, true).ConfigureAwait(false);
            return Json(_res);
        }
        [HttpPost]
        [Route("Home/ts-resend")]
        [Route("ts-resend")]
        public async Task<IActionResult> TResend(string TIDs, int APIID)
        {
            IResponseStatus _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = "Incomplete Request!"
            };
            if ((TIDs ?? "").Length > 0)
            {
                IRechargeReportML rml = new ReportML(_accessor, _env);
                _res = await rml.ResendTransactionsAsync(TIDs, APIID).ConfigureAwait(false);
            }
            ViewData["Heading"] = "Resend Transaction Detail";
            return PartialView("PartialView/_StatusCheck", _res);
        }
        [HttpPost]
        [Route("Home/ts-resend-t")]
        [Route("ts-resend-t")]
        public IActionResult TResendT(List<int> Id)
        {
            return PartialView("PartialView/_ResendConfirmation", Id);
        }
        [HttpPost]
        [Route("Home/ts-resend-tid")]
        [Route("ts-resend-tid")]
        public async Task<IActionResult> TResendTID(int TID, int APIID)
        {
            IRechargeReportML rml = new ReportML(_accessor, _env);
            var _res = await rml.ResendTransactionAsync(TID, APIID).ConfigureAwait(false);
            return Json(_res);
        }
        [HttpPost]
        [Route("Home/ts-resend-d")]
        [Route("ts-resend-d")]
        public async Task<IActionResult> TResend(string TIDs)
        {
            IResponseStatus _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = "Incomplete Request!"
            };
            if ((TIDs ?? "").Length > 0)
            {
                IRechargeReportML rml = new ReportML(_accessor, _env);
                _res = await rml.ResendTransactionsAsync(TIDs).ConfigureAwait(false);
            }
            ViewData["Heading"] = "Resend Transaction Detail";
            return PartialView("PartialView/_StatusCheck", _res);
        }
        #region EServicesPendings
        [Route("Home/EServicesPendings")]
        [Route("EServicesPendings.php")]
        public IActionResult EServicesPendings()
        {
            IOperatorML operatorML = new OperatorML(_accessor, _env);
            return View(operatorML.GetOperators(OPTypes.PublicEServices));
        }
        [HttpPost]
        [Route("Home/PESPendingTransaction")]
        [Route("PESPendingTransaction")]
        public async Task<IActionResult> PESPendingTransaction(int OID)
        {
            IReportML rml = new ReportML(_accessor, _env);
            IUserML userML = new UserML(_lr);
            var pendingTransactionModel = new PendingTransactionModel
            {
                CanSuccess = userML.IsCustomerCareAuthorised(ActionCodes.PageSuccess) || (_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.Admin),
                CanFail = userML.IsCustomerCareAuthorised(ActionCodes.PageFailed) || (_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.Admin),
                Report = await rml.PESTransaction(OID)
            };
            return PartialView("PartialView/_PESTransaction", pendingTransactionModel);
        }
        [HttpPost]
        [Route("Home/PESDetails")]
        [Route("PESDetails")]
        public async Task<IActionResult> PESDetails(int TID, string T)
        {
            IReportML ml = new ReportML(_accessor, _env);
            var _report = await ml.GetPESDetails(TID, T);
            return PartialView("PartialView/_PEStransactionDetail", _report);
        }
        #endregion
        #endregion

        #region FailToSuccess
        [HttpGet]
        [Route("Home/FailToSuccessR")]
        [Route("FailToSuccessR")]
        public async Task<IActionResult> FailToSuccessR()
        {
            IAPIML aPIML = new APIML(_accessor, _env);
            IOperatorML opML = new OperatorML(_accessor, _env);
            IUserML userML = new UserML(_lr);

            var rep = new RechargeReportModel
            {
                RechargeAPI = aPIML.GetAPIDetail(),
                Operators = await opML.GetOPListBYServices(ServiceType.RechargeReportServices),

                IsAdmin = _lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowRechargeReport),
                IsAPIUser = _lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.APIUser,
                IsEndUser = userML.IsEndUser()
            };
            return View(rep);
        }
        [HttpPost]
        [Route("Home/_FailToSuccess")]
        [Route("_FailToSuccess")]
        public async Task<IActionResult> _FailToSuccess([FromBody] RechargeReportFilter filter)
        {
            IUserML userML = new UserML(_lr);
            IRechargeReportML ml = new ReportML(_accessor, _env);
            var rechargeReportModel = new RechargeReportModel
            {
                IsAdmin = _lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowRechargeReport),
                IsAPIUser = _lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.APIUser,
                IsEndUser = userML.IsEndUser(),
                Report = await ml.GetRechargeFailToSuccess(filter)
            };
            return PartialView("PartialView/_FailToSuccessR", rechargeReportModel);
        }
        [HttpGet]
        [Route("Home/_FailToSuccess")]
        [Route("_FailToSuccess")]
        public async Task<IActionResult> _FailToSuccessExport(RechargeReportFilter filter)
        {
            IUserML userML = new UserML(_accessor, _env);
            IRechargeReportML ml = new ReportML(_accessor, _env);
            filter.IsExport = true;
            var IsAdmin = _lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowRechargeReport);

            List<ProcRechargeReportResponse> _report;

            _report = await ml.GetRechargeFailToSuccess(filter);
            DataTable dataTable = ConverterHelper.O.ToDataTable(_report);
            dataTable.Columns.Remove("ResultCode");
            dataTable.Columns.Remove("Msg");
            dataTable.Columns.Remove("TID");
            dataTable.Columns.Remove("UserID");
            dataTable.Columns.Remove("OID");
            dataTable.Columns.Remove("_Type");

            if (!IsAdmin)
            {
                dataTable.Columns.Remove("API");
                dataTable.Columns.Remove("VendorID");
            }
            dataTable.Columns.Remove("Optional1");
            dataTable.Columns.Remove("Optional2");
            dataTable.Columns.Remove("Optional3");
            dataTable.Columns.Remove("Optional4");
            dataTable.Columns.Remove("RefundStatus");
            dataTable.Columns.Remove("RefundStatus_");
            dataTable.Columns.Remove("IsWTR");
            dataTable.Columns.Remove("ModifyDate");
            dataTable.Columns.Remove("CommType");
            dataTable.Columns["LastBalance"].ColumnName = "OpeningBalance";
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
                var exportToExcel = new ExportToExcel
                {
                    Contents = package.GetAsByteArray(),
                    FileName = "FailToSuccess.xlsx"
                };
                return File(exportToExcel.Contents, DOCType.XlsxContentType, exportToExcel.FileName);
            }
        }
        #endregion
    }
}
