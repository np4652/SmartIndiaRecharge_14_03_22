using System;
using System.Threading.Tasks;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.MiddleLayer;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.App;
using RoundpayFinTech.AppCode.Model.Employee;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Reports.Filter;
using RoundpayFinTech.AppCode.StaticModel;

namespace RoundpayFinTech.Controllers
{
    public partial class AppController : IAppControllerEmployee
    {
        [HttpPost]
        public async Task<IActionResult> GetComparisionChart([FromBody] AppSessionReq appRequest)
        {
            var Response = new ComparisionResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };
            try
            {
                var appResp = appML.CheckEmpSession(appRequest);
                if (appResp.Statuscode == ErrorCodes.One)
                {
                    if (!appResp.IsPasswordExpired && appRequest.LoginTypeID == LoginType.Employee)
                    {
                        IEmpML ml = new EmpML(_accessor, _env, false);
                        Response.data = await Task.FromResult(ml.GetComparisionChart(appRequest.UserID)).ConfigureAwait(false);
                        Response.Statuscode = ErrorCodes.One;
                        Response.Msg = ErrorCodes.SUCCESS;
                    }
                    else
                    {
                        Response.Msg = appResp.IsPasswordExpired ? "Password Expired" : "Permission denied";
                    }
                }
            }
            catch (Exception ex)
            {
                Response.Msg = ErrorCodes.AnError;
            }
            return Json(Response);
        }

        [HttpPost]
        public async Task<IActionResult> GetEmpDownlineUser([FromBody] AppSessionReq appRequest)
        {
            var Response = new DownlineUserResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };
            try
            {
                var appResp = appML.CheckEmpSession(appRequest);
                if (appResp.Statuscode == ErrorCodes.One)
                {
                    if (!appResp.IsPasswordExpired && appRequest.LoginTypeID == LoginType.Employee)
                    {
                        IEmpML ml = new EmpML(_accessor, _env, false);
                        Response.data = await Task.FromResult(ml.GetEmpDownlineUser(appRequest.UserID)).ConfigureAwait(false);
                        Response.Statuscode = ErrorCodes.One;
                        Response.Msg = ErrorCodes.SUCCESS;
                    }
                    else
                    {
                        Response.Msg = appResp.IsPasswordExpired ? "Password Expired" : "Permission denied";
                    }
                }
            }
            catch (Exception ex)
            {
                Response.Msg = ErrorCodes.AnError;
            }
            return Json(Response);
        }

        [HttpPost]

        public async Task<IActionResult> GetLastdayVsTodayChart([FromBody] AppSessionReq appRequest)
        {
            var Response = new LastDayVsTodayResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };
            try
            {
                var appResp = appML.CheckEmpSession(appRequest);
                if (appResp.Statuscode == ErrorCodes.One)
                {
                    if (!appResp.IsPasswordExpired && appRequest.LoginTypeID == LoginType.Employee)
                    {
                        IEmpML ml = new EmpML(_accessor, _env, false);
                        Response.data = await Task.FromResult(ml.GetLastdayVsTodayChart(appRequest.UserID)).ConfigureAwait(false);
                        Response.Statuscode = ErrorCodes.One;
                        Response.Msg = ErrorCodes.SUCCESS;
                    }
                    else
                    {
                        Response.Msg = appResp.IsPasswordExpired ? "Password Expired" : "Permission denied";
                    }
                }
            }
            catch (Exception ex)
            {
                Response.Msg = ErrorCodes.AnError;
            }
            return Json(Response);
        }

        [HttpPost]
        public async Task<IActionResult> GetLastSevenDayPSTData([FromBody] AppSessionReq appRequest)
        {
            var Response = new PSTDataListResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };
            try
            {
                var appResp = appML.CheckEmpSession(appRequest);
                if (appResp.Statuscode == ErrorCodes.One)
                {
                    if (!appResp.IsPasswordExpired && appRequest.LoginTypeID == LoginType.Employee)
                    {
                        IEmpML ml = new EmpML(_accessor, _env, false);
                        Response.Data = ml.GetLastSevenDayPSTDataForEmp(appRequest.UserID);
                        Response.Statuscode = ErrorCodes.One;
                        Response.Msg = ErrorCodes.SUCCESS;
                    }
                    else
                    {
                        Response.Msg = appResp.IsPasswordExpired ? "Password Expired" : "Permission denied";
                    }
                }
            }
            catch (Exception ex)
            {
                Response.Msg = ErrorCodes.AnError;
            }
            return Json(Response);
        }
        [HttpPost]

        public async Task<IActionResult> GetTargetSegment([FromBody] AppSessionReq appRequest)
        {
            var Response = new TargetSegmentResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };
            try
            {
                var appResp = appML.CheckEmpSession(appRequest);
                if (appResp.Statuscode == ErrorCodes.One)
                {
                    if (!appResp.IsPasswordExpired && appRequest.LoginTypeID == LoginType.Employee)
                    {
                        IEmpML ml = new EmpML(_accessor, _env, false);
                        Response.data = await Task.FromResult(ml.GetTargetSegment(appRequest.UserID)).ConfigureAwait(false);
                        Response.Statuscode = ErrorCodes.One;
                        Response.Msg = ErrorCodes.SUCCESS;
                    }
                    else
                    {
                        Response.Msg = appResp.IsPasswordExpired ? "Password Expired" : "Permission denied";
                    }
                }
            }
            catch (Exception ex)
            {
                Response.Msg = ErrorCodes.AnError;
            }
            return Json(Response);
        }

        [HttpPost]
        public async Task<IActionResult> GetUserCommitment([FromBody] AppSessionReq appRequest)
        {
            var Response = new UserCommitmentResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };
            try
            {
                var appResp = appML.CheckEmpSession(appRequest);
                if (appResp.Statuscode == ErrorCodes.One)
                {
                    if (!appResp.IsPasswordExpired && appRequest.LoginTypeID == LoginType.Employee)
                    {
                        IEmpML ml = new EmpML(_accessor, _env, false);
                        Response.data = await Task.FromResult(ml.GetUserCommitment(appRequest.UserID)).ConfigureAwait(false);
                        Response.Statuscode = ErrorCodes.One;
                        Response.Msg = ErrorCodes.SUCCESS;
                    }
                    else
                    {
                        Response.Msg = appResp.IsPasswordExpired ? "Password Expired" : "Permission denied";
                    }
                }
            }
            catch (Exception ex)
            {
                Response.Msg = ErrorCodes.AnError;
            }
            return Json(Response);
        }

        [HttpPost]
        public async Task<IActionResult> GetEmployeeeUser([FromBody] EmployeeUserFilterRequest appRequest)
        {
            var Response = new EmployeeUserResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };
            try
            {
                var appResp = appML.CheckEmpSession(appRequest);
                if (appResp.Statuscode == ErrorCodes.One)
                {
                    if (!appResp.IsPasswordExpired && appRequest.LoginTypeID == LoginType.Employee)
                    {
                        IEmpML ml = new EmpML(_accessor, _env, false);
                        var req = new CommonFilter
                        {
                            LoginID = appRequest.UserID,
                            Criteria = appRequest.CriteriaID,
                            CriteriaText = appRequest.CriteriaText,
                            SortByID = appRequest.SortById,
                            IsDesc = appRequest.IsDesc,
                            UserID = appRequest.UID,
                            TopRows = appRequest.TopRows,
                            RoleID = appRequest.RoleID
                        };
                        var res = await Task.FromResult(ml.GetEmployeeeUser(req)).ConfigureAwait(false);
                        Response.data = res.EmpReports;
                        Response.Statuscode = ErrorCodes.One;
                        Response.Msg = ErrorCodes.SUCCESS;
                    }
                    else
                    {
                        Response.Msg = appResp.IsPasswordExpired ? "Password Expired" : "Permission denied";
                    }
                }
            }
            catch (Exception ex)
            {
                Response.Msg = ErrorCodes.AnError;
            }
            return Json(Response);
        }

        [HttpPost]

        public async Task<IActionResult> GetEmployees([FromBody] EmployeeFilterRequest appRequest)
        {
            var Response = new EmployeeListResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };
            try
            {
                var appResp = appML.CheckEmpSession(appRequest);
                if (appResp.Statuscode == ErrorCodes.One)
                {
                    if (!appResp.IsPasswordExpired && appRequest.LoginTypeID == LoginType.Employee)
                    {
                        IEmpML ml = new EmpML(_accessor, _env, false);
                        var req = new CommonFilter
                        {
                            LoginID = appRequest.UserID,
                            Criteria = appRequest.CriteriaID,
                            CriteriaText = appRequest.CriteriaText,
                            SortByID = appRequest.SortById,
                            IsDesc = appRequest.IsDesc,
                            UserID = appRequest.UID,
                            TopRows = appRequest.TopRows,
                            EmployeeRole = appRequest.EmployeeRole
                        };
                        var res = await Task.FromResult(ml.GetEmployee(req)).ConfigureAwait(false);
                        Response.data = res.Employees;
                        Response.Statuscode = ErrorCodes.One;
                        Response.Msg = ErrorCodes.SUCCESS;
                    }
                    else
                    {
                        Response.Msg = appResp.IsPasswordExpired ? "Password Expired" : "Permission denied";
                    }
                }
            }
            catch (Exception ex)
            {
                Response.Msg = ErrorCodes.AnError;
            }
            return Json(Response);
        }

        [HttpPost]
        public async Task<IActionResult> GetPSTReportEmp([FromBody] PSTRequest appRequest)
        {
            var Response = new PSTResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };
            try
            {
                var appResp = appML.CheckEmpSession(appRequest);
                if (appResp.Statuscode == ErrorCodes.One)
                {
                    if (!appResp.IsPasswordExpired && appRequest.LoginTypeID == LoginType.Employee)
                    {
                        IEmpML ml = new EmpML(_accessor, _env, false);
                        var req = new CommonFilter
                        {
                            LoginID = appRequest.UserID,
                            FromDate = appRequest.RequestedDate,
                            RoleID = appRequest.RoleID
                        };
                        Response.data = await Task.FromResult(ml.GetPSTReportEmp(req).Result).ConfigureAwait(false);
                        Response.Statuscode = ErrorCodes.One;
                        Response.Msg = ErrorCodes.SUCCESS;
                    }
                    else
                    {
                        Response.Msg = appResp.IsPasswordExpired ? "Password Expired" : "Permission denied";
                    }
                }
            }
            catch (Exception ex)
            {
                Response.Msg = ErrorCodes.AnError;
            }
            return Json(Response);
        }

        [HttpPost]
        public async Task<IActionResult> GetTertiaryReportEmp([FromBody] TertiaryRequest appRequest)
        {
            var Response = new TertiaryResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };
            try
            {
                var appResp = appML.CheckEmpSession(appRequest);
                if (appResp.Statuscode == ErrorCodes.One)
                {
                    if (!appResp.IsPasswordExpired && appRequest.LoginTypeID == LoginType.Employee)
                    {
                        IEmpML ml = new EmpML(_accessor, _env, false);
                        var req = new CommonReq
                        {
                            LoginID = appRequest.UserID,
                            LoginTypeID = appRequest.LoginTypeID,
                            CommonInt = appRequest.EmpID,
                            CommonStr = appRequest.RequestedDate
                        };
                        Response.data = await Task.FromResult(ml.GetTertiaryReportEmp(req).Result).ConfigureAwait(false);
                        Response.Statuscode = ErrorCodes.One;
                        Response.Msg = ErrorCodes.SUCCESS;
                    }
                    else
                    {
                        Response.Msg = appResp.IsPasswordExpired ? "Password Expired" : "Permission denied";
                    }
                }
            }
            catch (Exception ex)
            {
                Response.Msg = ErrorCodes.AnError;
            }
            return Json(Response);
        }

        [HttpPost]
        public async Task<IActionResult> GetEmployeeTargetReport([FromBody] EmpTargetRequest appRequest)
        {
            var Response = new EmpTargetResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };
            try
            {
                var appResp = appML.CheckEmpSession(appRequest);
                if (appResp.Statuscode == ErrorCodes.One)
                {
                    if (!appResp.IsPasswordExpired && appRequest.LoginTypeID == LoginType.Employee)
                    {
                        IEmpML ml = new EmpML(_accessor, _env, false);
                        var req = new CommonReq
                        {
                            LoginID = appRequest.UserID,
                            LoginTypeID = appRequest.LoginTypeID,
                            CommonInt = appRequest.EmpID,
                            CommonStr = appRequest.RequestedDate
                        };
                        Response.data = await Task.FromResult(ml.GetEmpTargetReport(req).Result).ConfigureAwait(false);
                        Response.Statuscode = ErrorCodes.One;
                        Response.Msg = ErrorCodes.SUCCESS;
                    }
                    else
                    {
                        Response.Msg = appResp.IsPasswordExpired ? "Password Expired" : "Permission denied";
                    }
                }
            }
            catch (Exception ex)
            {
                Response.Msg = ErrorCodes.AnError;
            }
            return Json(Response);
        }

        [HttpPost]
        public async Task<IActionResult> SetUserCommitment([FromBody] UserCommitmentRequest appRequest)
        {
            var Response = new AppResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };
            try
            {
                var appResp = appML.CheckEmpSession(appRequest);
                if (appResp.Statuscode == ErrorCodes.One)
                {
                    if (!appResp.IsPasswordExpired && appRequest.LoginTypeID == LoginType.Employee)
                    {
                        IEmpML ml = new EmpML(_accessor, _env, false);
                        var req = new UserCommitment
                        {
                            EmpID = appRequest.UserID,
                            UserID = appRequest.UID,
                            Commitment = appRequest.Commitment,
                            Latitude = appRequest.Latitude,
                            Longitute = appRequest.Longitute,
                            LoginTypeID = appRequest.LoginTypeID
                        };
                        var res = await Task.FromResult(ml.SetUserCommitment(req)).ConfigureAwait(false);
                        Response.Statuscode = res.Statuscode;
                        Response.Msg = res.Msg;
                    }
                    else
                    {
                        Response.Msg = appResp.IsPasswordExpired ? "Password Expired" : "Permission denied";
                    }
                }
            }
            catch (Exception ex)
            {
                Response.Msg = ErrorCodes.AnError;
            }
            return Json(Response);
        }

        [HttpPost]
        public async Task<IActionResult> GetUserCommitmentChart([FromBody] AppSessionReq appRequest)
        {
            var Response = new UserCommitmentChartResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };
            try
            {
                var appResp = appML.CheckEmpSession(appRequest);
                if (appResp.Statuscode == ErrorCodes.One)
                {
                    if (!appResp.IsPasswordExpired && appRequest.LoginTypeID == LoginType.Employee)
                    {
                        IEmpML ml = new EmpML(_accessor, _env, false);
                        var _resp = await Task.FromResult(ml.GetUserCommitment(appRequest.UserID)).ConfigureAwait(false);
                        int TotalCommitment = 0, TotalAchieved = 0;
                        foreach (var item in _resp)
                        {
                            TotalCommitment += item.Commitment;
                            TotalAchieved += item.Achieved;
                        }
                        Response.TotalCommitment = TotalCommitment;
                        Response.TotalAchieved = TotalAchieved;
                        Response.Statuscode = ErrorCodes.One;
                        Response.Msg = ErrorCodes.SUCCESS;
                    }
                    else
                    {
                        Response.Msg = appResp.IsPasswordExpired ? "Password Expired" : "Permission denied";
                    }
                }
            }
            catch (Exception ex)
            {
                Response.Msg = ErrorCodes.AnError;
            }
            return Json(Response);
        }

        [HttpPost]
        public async Task<IActionResult> GetEmpTodayLivePST([FromBody] AppSessionReq appRequest)
        {
            var Response = new TodayLivePSTResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };
            try
            {
                var appResp = appML.CheckEmpSession(appRequest);
                if (appResp.Statuscode == ErrorCodes.One)
                {
                    if (!appResp.IsPasswordExpired && appRequest.LoginTypeID == LoginType.Employee)
                    {
                        IEmpML ml = new EmpML(_accessor, _env, false);
                        Response.data = await Task.FromResult(ml.GetEmpTodayLivePST(appRequest.UserID)).ConfigureAwait(false);
                        Response.Statuscode = ErrorCodes.One;
                        Response.Msg = ErrorCodes.SUCCESS;
                    }
                    else
                    {
                        Response.Msg = appResp.IsPasswordExpired ? "Password Expired" : "Permission denied";
                    }
                }
            }
            catch (Exception ex)
            {
                Response.Msg = ErrorCodes.AnError;
            }
            return Json(Response);
        }

        [HttpPost]
        public async Task<IActionResult> RechargeReportForEmployee([FromBody] AppRechargeReportReq appRechargeReportReq)
        {
            var appRechargeRespose = new AppReportResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };

            var appResp = appML.CheckEmpSession(appRechargeReportReq);
            appRechargeRespose.IsAppValid = appResp.IsAppValid;
            appRechargeRespose.IsVersionValid = appResp.IsVersionValid;
            appRechargeRespose.IsPasswordExpired = appResp.IsPasswordExpired;
            appRechargeRespose.Statuscode = appResp.Statuscode;
            if (appResp.Statuscode == ErrorCodes.One)
            {
                if (!appResp.IsPasswordExpired)
                {
                    IAppReportML appReportML = new ReportML(_accessor, _env, false);
                    var _filter = new _RechargeReportFilter
                    {
                        AccountNo = appRechargeReportReq.AccountNo,
                        TransactionID = appRechargeReportReq.TransactionID,
                        OID = appRechargeReportReq.OID,
                        Status = appRechargeReportReq.Status,
                        TopRows = appRechargeReportReq.TopRows,
                        FromDate = appRechargeReportReq.FromDate,
                        ToDate = appRechargeReportReq.ToDate,
                        IsExport = appRechargeReportReq.IsExport,
                        LoginID = appRechargeReportReq.UserID,
                        LT = appRechargeReportReq.LoginTypeID,
                        OPTypeID = appRechargeReportReq.OpTypeID,
                        IsRecent = appRechargeReportReq.IsRecent,
                        OutletNo = appRechargeReportReq.ChildMobile
                    };

                    appRechargeRespose.Statuscode = ErrorCodes.One;
                    appRechargeRespose.Msg = ErrorCodes.SUCCESS;
                    appRechargeRespose.RechargeReport = await appReportML.GetAppRechargeReport(_filter).ConfigureAwait(false);
                }
            }
            else
            {
                appRechargeRespose.Msg = appResp.Msg;
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "RechargeReport",
                CommonStr2 = JsonConvert.SerializeObject(appRechargeReportReq),
                CommonStr3 = JsonConvert.SerializeObject(appRechargeRespose)
            });
            return Json(appRechargeRespose);
        }


        [HttpPost]
        public async Task<IActionResult> LedgerReportForEmployee([FromBody] AppLedgerReq appLedgerReq)
        {
            var appReportResponse = new AppReportResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            var appRequest = new AppSessionReq
            {
                APPID = appLedgerReq.APPID,
                IMEI = appLedgerReq.IMEI,
                LoginTypeID = appLedgerReq.LoginTypeID,
                UserID = appLedgerReq.UserID,
                SessionID = appLedgerReq.SessionID,
                RegKey = appLedgerReq.RegKey,
                SerialNo = appLedgerReq.SerialNo,
                Version = appLedgerReq.Version,
                Session = appLedgerReq.Session
            };
            var appResp = appML.CheckEmpSession(appRequest);
            appReportResponse.IsAppValid = appResp.IsAppValid;
            appReportResponse.IsVersionValid = appResp.IsVersionValid;
            appReportResponse.IsPasswordExpired = appResp.IsPasswordExpired;
            appReportResponse.Statuscode = appResp.Statuscode;
            if (appResp.Statuscode == ErrorCodes.One)
            {
                if (!appResp.IsPasswordExpired)
                {
                    IAppReportML appReportML = new ReportML(_accessor, _env, false);
                    var _filter = new ProcUserLedgerRequest
                    {
                        Mobile_F = appLedgerReq.AccountNo,
                        LT = appLedgerReq.LoginTypeID,
                        LoginID = appLedgerReq.UserID,
                        DebitCredit_F = appLedgerReq.Status,
                        TopRows = appLedgerReq.TopRows,
                        FromDate_F = appLedgerReq.FromDate,
                        ToDate_F = appLedgerReq.ToDate,
                        TransactionId_F = appLedgerReq.TransactionID,
                        WalletTypeID = appLedgerReq.WalletTypeID
                    };
                    appReportResponse.Statuscode = ErrorCodes.One;
                    appReportResponse.Msg = ErrorCodes.SUCCESS;
                    appReportResponse.LedgerReport = await appReportML.GetAppUserLedgerList(_filter);
                }
            }
            else
            {
                appReportResponse.Msg = appResp.Msg;
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "LedgerReport",
                CommonStr2 = JsonConvert.SerializeObject(appLedgerReq),
                CommonStr3 = JsonConvert.SerializeObject(appReportResponse)
            });
            return Json(appReportResponse);
        }

        [HttpPost]
        public async Task<IActionResult> UserDaybookForEmployee([FromBody] AppReportCommon appReportCommon)
        {
            var appReportResponse = new AppReportResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            var appRequest = new AppSessionReq
            {
                APPID = appReportCommon.APPID,
                IMEI = appReportCommon.IMEI,
                LoginTypeID = appReportCommon.LoginTypeID,
                UserID = appReportCommon.UserID,
                SessionID = appReportCommon.SessionID,
                RegKey = appReportCommon.RegKey,
                SerialNo = appReportCommon.SerialNo,
                Version = appReportCommon.Version,
                Session = appReportCommon.Session
            };
            var appResp = appML.CheckEmpSession(appRequest);
            appReportResponse.IsAppValid = appResp.IsAppValid;
            appReportResponse.IsVersionValid = appResp.IsVersionValid;
            appReportResponse.IsPasswordExpired = appResp.IsPasswordExpired;
            appReportResponse.Statuscode = appResp.Statuscode;
            if (appResp.Statuscode == ErrorCodes.One)
            {
                if (!appResp.IsPasswordExpired)
                {
                    IAppReportML appReportML = new ReportML(_accessor, _env, false);
                    var commonReq = new CommonReq
                    {
                        CommonStr = appReportCommon.FromDate ?? DateTime.Now.ToString("dd MMM yyyy"),

                        LoginTypeID = appReportCommon.LoginTypeID,
                        LoginID = appReportCommon.UserID,
                        CommonStr2 = appReportCommon.AccountNo ?? "" //Child UserMobileNo/ID
                    };
                    commonReq.CommonStr3 = appReportCommon.ToDate ?? commonReq.CommonStr;
                    appReportResponse.Statuscode = ErrorCodes.One;
                    appReportResponse.Msg = ErrorCodes.SUCCESS;
                    await Task.Delay(0);
                    appReportResponse.UserDaybookReport = appReportML.UserDaybookApp(commonReq);
                }
            }
            else
            {
                appReportResponse.Msg = appResp.Msg;
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "UserDaybook",
                CommonStr2 = JsonConvert.SerializeObject(appReportCommon),
                CommonStr3 = JsonConvert.SerializeObject(appReportResponse)
            });
            return Json(appReportResponse);
        }

        [HttpPost]
        public async Task<IActionResult> DMTReportForEmployee([FromBody] AppDMTReportReq appDMTReportReq)
        {
            var appRechargeRespose = new AppReportResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };

            var appResp = appML.CheckEmpSession(appDMTReportReq);
            appRechargeRespose.IsAppValid = appResp.IsAppValid;
            appRechargeRespose.IsVersionValid = appResp.IsVersionValid;
            appRechargeRespose.IsPasswordExpired = appResp.IsPasswordExpired;
            appRechargeRespose.Statuscode = appResp.Statuscode;
            if (appResp.Statuscode == ErrorCodes.One)
            {
                if (!appResp.IsPasswordExpired)
                {
                    IAppReportML appReportML = new ReportML(_accessor, _env, false);
                    var _filter = new _RechargeReportFilter
                    {
                        AccountNo = appDMTReportReq.AccountNo,
                        TransactionID = appDMTReportReq.TransactionID,
                        Status = appDMTReportReq.Status,
                        TopRows = appDMTReportReq.TopRows,
                        FromDate = appDMTReportReq.FromDate,
                        ToDate = appDMTReportReq.ToDate,
                        IsExport = appDMTReportReq.IsExport,
                        LoginID = appDMTReportReq.UserID,
                        LT = appDMTReportReq.LoginTypeID,
                        OutletNo = appDMTReportReq.ChildMobile
                    };

                    appRechargeRespose.Statuscode = ErrorCodes.One;
                    appRechargeRespose.Msg = ErrorCodes.SUCCESS;
                    appRechargeRespose.DMTReport = await appReportML.GetAppDMRReport(_filter);
                }
            }
            else
            {
                appRechargeRespose.Msg = appResp.Msg;
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "DMTReport",
                CommonStr2 = JsonConvert.SerializeObject(appDMTReportReq),
                CommonStr3 = JsonConvert.SerializeObject(appRechargeRespose)
            });
            return Json(appRechargeRespose);
        }

        [HttpPost]
        public async Task<IActionResult> FundDCReportForEmployee([FromBody] AppFundDCReq appFundDCReq)
        {
            var appReportResponse = new AppReportResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            var appRequest = new AppSessionReq
            {
                APPID = appFundDCReq.APPID,
                IMEI = appFundDCReq.IMEI,
                LoginTypeID = appFundDCReq.LoginTypeID,
                UserID = appFundDCReq.UserID,
                SessionID = appFundDCReq.SessionID,
                RegKey = appFundDCReq.RegKey,
                SerialNo = appFundDCReq.SerialNo,
                Version = appFundDCReq.Version,
                Session = appFundDCReq.Session
            };
            var appResp = appML.CheckEmpSession(appRequest);
            appReportResponse.IsAppValid = appResp.IsAppValid;
            appReportResponse.IsVersionValid = appResp.IsVersionValid;
            appReportResponse.IsPasswordExpired = appResp.IsPasswordExpired;
            appReportResponse.Statuscode = appResp.Statuscode;
            if (appResp.Statuscode == ErrorCodes.One)
            {
                if (!appResp.IsPasswordExpired)
                {
                    IAppReportML appReportML = new ReportML(_accessor, _env, false);
                    var _filter = new ULFundReceiveReportFilter
                    {
                        ServiceID = appFundDCReq.ServiceID,
                        LoginId = appFundDCReq.UserID,
                        MobileNo = appFundDCReq.AccountNo,
                        TID = appFundDCReq.TransactionID,
                        FDate = appFundDCReq.FromDate,
                        TDate = appFundDCReq.ToDate,
                        IsSelf = appFundDCReq.IsSelf,
                        WalletTypeID = appFundDCReq.WalletTypeID,
                        OtherUserMob = appFundDCReq.OtherUserMob,
                        LT = appFundDCReq.LoginTypeID,
                        UserID = appFundDCReq.UserID
                    };
                    appReportResponse.Statuscode = ErrorCodes.One;
                    appReportResponse.Msg = ErrorCodes.SUCCESS;
                    appReportResponse.FundDCReport = await appReportML.GetAppUserFundReceive(_filter);
                }
            }
            else
            {
                appReportResponse.Msg = appResp.Msg;
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "FundDCReport",
                CommonStr2 = JsonConvert.SerializeObject(appFundDCReq),
                CommonStr3 = JsonConvert.SerializeObject(appReportResponse)
            });
            return Json(appReportResponse);
        }
        //--------------------------Ahmed Gulzar--------------------------//
        //--------------------------Sep 26, 2020--------------------------//
        [HttpPost]
        public IActionResult CreateMeeting([FromBody] AppSessionReq appRequest)
        {
            var Response = new GetCreateMeetingResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };
            try
            {
                var appResp = appML.CheckEmpSession(appRequest);
                if (appResp.Statuscode == ErrorCodes.One)
                {
                    Response.IsVersionValid = true;
                    if (!appResp.IsPasswordExpired && appRequest.LoginTypeID == LoginType.Employee)
                    {
                        Response.IsPasswordExpired = false;
                        IEmpML ml = new EmpML(_accessor, _env, false);
                        if (!ml.IsClosingDone(appRequest.UserID, appRequest.LoginTypeID))
                        {
                            Response.data = ml.ReasonandPurpuse();
                            Response.Statuscode = ErrorCodes.One;
                            Response.Msg = ErrorCodes.SUCCESS;
                        }
                        else
                        {
                            Response.data = null;
                            Response.Statuscode = ErrorCodes.Minus1;
                            Response.Msg = "Closing done for the day";
                        }
                    }
                    else
                    {
                        Response.Msg = appResp.IsPasswordExpired ? "Password Expired" : "Permission denied";
                    }
                }
            }
            catch (Exception ex)
            {
                Response.Msg = ErrorCodes.AnError;
            }
            return Json(Response);
        }

        [HttpPost]
        public IActionResult PostCreateMeeting(string CreateMeetingRequest, IFormFile file)
        {
            var Response = new AppResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvalidLogin
            };
            if (string.IsNullOrEmpty(CreateMeetingRequest)) { return Json(Response); }
            var newInput = CreateMeetingRequest.Trim();
            newInput = CreateMeetingRequest.TrimStart('"');
            newInput = newInput.TrimEnd('"');

            PostCreateMeetingRequest appRequest;
            try
            {
                appRequest = JsonConvert.DeserializeObject<PostCreateMeetingRequest>(newInput);
            }
            catch (Exception ex)
            {
                Response.Msg = ErrorCodes.InvalidParam;
                return Json(Response);
            }
            try
            {
                var appResp = appML.CheckEmpSession(appRequest);
                if (appResp.Statuscode == ErrorCodes.One)
                {
                    Response.IsVersionValid = true;
                    if (!appResp.IsPasswordExpired && appRequest.LoginTypeID == LoginType.Employee)
                    {
                        Response.IsPasswordExpired = false;
                        IEmpML ml = new EmpML(_accessor, _env, false);
                        if (!ml.IsClosingDone(appRequest.UserID, appRequest.LoginTypeID))
                        {
                            IBannerML _bannerML = new ResourceML(_accessor, _env);
                            LoginResponse _lr = new LoginResponse()
                            {
                                LoginTypeID = appRequest.LoginTypeID,
                                UserID = appRequest.UserID
                            };
                            Meetingdetails model = new Meetingdetails
                            {
                                LoginID = appRequest.UserID,
                                LoginTypeID = appRequest.LoginTypeID,
                                Name = appRequest.Name,
                                OutletName = appRequest.OutletName,
                                Area = appRequest.Area,
                                Pincode = appRequest.Pincode,
                                PurposeId = appRequest.PurposeId,
                                Purpose = appRequest.Purpose,
                                Consumption = appRequest.Consumption,
                                Isusingotherbrands = appRequest.Isusingotherbrands,
                                Otherbrandconsumption = appRequest.Otherbrandconsumption,
                                ReasonId = appRequest.ReasonId,
                                Reason = appRequest.Reason,
                                Remark = appRequest.Remark,
                                MobileNo = appRequest.MobileNo,
                                Latitute = appRequest.Latitute,
                                Longitute = appRequest.Longitute,
                                RechargeConsumption = appRequest.RechargeConsumption,
                                BillPaymentConsumption = appRequest.BillPaymentConsumption,
                                MoneyTransferConsumption = appRequest.MoneyTransferConsumption,
                                AEPSConsumption = appRequest.AEPSConsumption,
                                MiniATMConsumption = appRequest.MiniATMConsumption,
                                InsuranceConsumption = appRequest.InsuranceConsumption,
                                HotelConsumption = appRequest.HotelConsumption,
                                PanConsumption = appRequest.PanConsumption,
                                VehicleConsumption = appRequest.VehicleConsumption
                            };
                            if (appRequest.IsImage)
                            {
                                var _res = _bannerML.UploadEmployeeImage(file, _lr, appRequest.MobileNo, FolderType.ShopImage);
                                if (_res.Statuscode == ErrorCodes.One)
                                {
                                    var _resp = ml.CreateMeeting(model);
                                    Response.Statuscode = _resp.Statuscode;
                                    Response.Msg = _resp.Msg;
                                }
                                else
                                {
                                    Response.Statuscode = _res.Statuscode;
                                    Response.Msg = _res.Msg;
                                }
                            }
                            else
                            {
                                var _resp = ml.CreateMeeting(model);
                                Response.Statuscode = _resp.Statuscode;
                                Response.Msg = _resp.Msg;
                            }
                        }
                        else
                        {
                            Response.Statuscode = ErrorCodes.Minus1;
                            Response.Msg = "Closing done for the day";
                        }
                    }
                    else
                    {
                        Response.Msg = appResp.IsPasswordExpired ? "Password Expired" : "Permission denied";
                    }
                }
            }
            catch (Exception ex)
            {
                Response.Msg = ErrorCodes.AnError;
            }
            return Json(Response);
        }

        [HttpPost]
        public IActionResult GetUserByMobile([FromBody] GetUserByMobileRequest appRequest)
        {
            var Response = new GetUserByMobileResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };
            try
            {
                var appResp = appML.CheckEmpSession(appRequest);
                if (appResp.Statuscode == ErrorCodes.One)
                {
                    Response.IsVersionValid = true;
                    if (!appResp.IsPasswordExpired && appRequest.LoginTypeID == LoginType.Employee)
                    {
                        Response.IsPasswordExpired = false;
                        IEmpML ml = new EmpML(_accessor, _env, false);
                        Response.Data = ml.GetUserdatabyMobileNo(appRequest.Mobile, appRequest.UserID, appRequest.LoginTypeID);
                        Response.Statuscode = ErrorCodes.One;
                        Response.Msg = ErrorCodes.SUCCESS;
                    }
                    else
                    {
                        Response.Msg = appResp.IsPasswordExpired ? "Password Expired" : "Permission denied";
                    }
                }
            }
            catch (Exception ex)
            {
                Response.Msg = ErrorCodes.AnError;
            }
            return Json(Response);
        }
        [HttpPost]
        public IActionResult GetAreabyPincode([FromBody] GetAreabyPincodeRequest appRequest)
        {
            var Response = new GetAreabyPincodeResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };
            try
            {
                var appResp = appML.CheckEmpSession(appRequest);
                if (appResp.Statuscode == ErrorCodes.One)
                {
                    Response.IsVersionValid = true;
                    if (!appResp.IsPasswordExpired && appRequest.LoginTypeID == LoginType.Employee)
                    {
                        Response.IsPasswordExpired = false;
                        IEmpML ml = new EmpML(_accessor, _env, false);
                        Response.Data = ml.GetAreabypincode(appRequest.Pincode, appRequest.UserID, appRequest.LoginTypeID);
                        Response.Statuscode = ErrorCodes.One;
                        Response.Msg = ErrorCodes.SUCCESS;
                    }
                    else
                    {
                        Response.Msg = appResp.IsPasswordExpired ? "Password Expired" : "Permission denied";
                    }
                }
            }
            catch (Exception ex)
            {
                Response.Msg = ErrorCodes.AnError;
            }
            return Json(Response);
        }

        [HttpPost]
        public IActionResult GetMeetingDetail([FromBody] CommonFilterRequest appRequest)
        {
            var Response = new GetMeetingDetailResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };
            try
            {
                var appResp = appML.CheckEmpSession(appRequest);
                if (appResp.Statuscode == ErrorCodes.One)
                {
                    Response.IsVersionValid = true;
                    if (!appResp.IsPasswordExpired && appRequest.LoginTypeID == LoginType.Employee)
                    {
                        Response.IsPasswordExpired = false;
                        CommonReq commonReq = new CommonReq
                        {
                            LoginID = appRequest.UserID,
                            LoginTypeID = appRequest.LoginTypeID,
                            CommonInt = appRequest.Top,
                            CommonStr = appRequest.DtFrom,
                            CommonStr2 = appRequest.DtTill,
                            CommonInt2 = appRequest.Criteria,
                            CommonStr3 = appRequest.CValue
                        };
                        IEmpML ml = new EmpML(_accessor, _env, false);
                        Response.Data = ml.GetMeetingdetails(commonReq);
                        Response.Statuscode = ErrorCodes.One;
                        Response.Msg = ErrorCodes.SUCCESS;
                    }
                    else
                    {
                        Response.Msg = appResp.IsPasswordExpired ? "Password Expired" : "Permission denied";
                    }
                }
            }
            catch (Exception ex)
            {
                Response.Msg = ErrorCodes.AnError;
            }
            return Json(Response);
        }

        [HttpPost]
        public IActionResult PostDailyClosing([FromBody] DailyClosingRequest appRequest)
        {
            var Response = new PostDailyClosingResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };
            try
            {
                var appResp = appML.CheckEmpSession(appRequest);
                if (appResp.Statuscode == ErrorCodes.One)
                {
                    Response.IsVersionValid = true;
                    if (!appResp.IsPasswordExpired && appRequest.LoginTypeID == LoginType.Employee)
                    {
                        Response.IsPasswordExpired = false;
                        DailyClosingModel req = new DailyClosingModel
                        {
                            LoginID = appRequest.UserID,
                            LoginTypeID = appRequest.LoginTypeID,
                            Travel = appRequest.Travel,
                            Expense = appRequest.Expense
                        };
                        IEmpML ml = new EmpML(_accessor, _env, false);
                        Response.Data = ml.DailyClosing(req);
                        Response.Statuscode = ErrorCodes.One;
                        Response.Msg = ErrorCodes.SUCCESS;
                    }
                    else
                    {
                        Response.Msg = appResp.IsPasswordExpired ? "Password Expired" : "Permission denied";
                    }
                }
            }
            catch (Exception ex)
            {
                Response.Msg = ErrorCodes.AnError;
            }
            return Json(Response);
        }

        [HttpPost]
        public async Task<IActionResult> GetMeetingReport([FromBody] CommonFilterRequest appRequest)
        {
            var Response = new GetMeetingReportResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };
            try
            {
                var appResp = appML.CheckEmpSession(appRequest);
                if (appResp.Statuscode == ErrorCodes.One)
                {
                    Response.IsVersionValid = true;
                    if (!appResp.IsPasswordExpired && appRequest.LoginTypeID == LoginType.Employee)
                    {
                        Response.IsPasswordExpired = false;
                        CommonReq commonReq = new CommonReq
                        {
                            LoginID = appRequest.UserID,
                            LoginTypeID = appRequest.LoginTypeID,
                            CommonInt = appRequest.Top,
                            CommonStr = appRequest.DtFrom,
                            CommonStr2 = appRequest.DtTill,
                            CommonInt2 = appRequest.Criteria,
                            CommonStr3 = appRequest.CValue
                        };
                        IEmpML ml = new EmpML(_accessor, _env, false);
                        Response.Data = await ml.GetMeetingReport(commonReq).ConfigureAwait(false);
                        Response.Statuscode = ErrorCodes.One;
                        Response.Msg = ErrorCodes.SUCCESS;
                    }
                    else
                    {
                        Response.Msg = appResp.IsPasswordExpired ? "Password Expired" : "Permission denied";
                    }
                }
            }
            catch (Exception ex)
            {
                Response.Msg = ErrorCodes.AnError;
            }
            return Json(Response);
        }

        [HttpPost]
        public async Task<IActionResult> GetMeetingSubReport([FromBody] CommonFilterRequest appRequest)
        {
            var Response = new GetMeetingSubReportResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };
            try
            {
                var appResp = appML.CheckEmpSession(appRequest);
                if (appResp.Statuscode == ErrorCodes.One)
                {
                    Response.IsVersionValid = true;
                    if (!appResp.IsPasswordExpired && appRequest.LoginTypeID == LoginType.Employee)
                    {
                        Response.IsPasswordExpired = false;
                        CommonReq commonReq = new CommonReq
                        {
                            LoginID = appRequest.UserID,
                            LoginTypeID = appRequest.LoginTypeID,
                            CommonInt = appRequest.SearchId
                        };
                        IEmpML ml = new EmpML(_accessor, _env, false);
                        Response.Data = await ml.GetMeetingDetailReport(commonReq).ConfigureAwait(false);
                        Response.Statuscode = ErrorCodes.One;
                        Response.Msg = ErrorCodes.SUCCESS;
                    }
                    else
                    {
                        Response.Msg = appResp.IsPasswordExpired ? "Password Expired" : "Permission denied";
                    }
                }
            }
            catch (Exception ex)
            {
                Response.Msg = ErrorCodes.AnError;
            }
            return Json(Response);
        }
        //--------------------------Sep 26, 2020--------------------------//
        //--------------------------Sep 28, 2020--------------------------//
        [HttpPost]
        public async Task<IActionResult> GetMapPoints([FromBody] CommonFilterRequest appRequest)
        {
            var Response = new GetMapPointsResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };
            try
            {
                var appResp = appML.CheckEmpSession(appRequest);
                if (appResp.Statuscode == ErrorCodes.One)
                {
                    Response.IsVersionValid = true;
                    if (!appResp.IsPasswordExpired && appRequest.LoginTypeID == LoginType.Employee)
                    {
                        Response.IsPasswordExpired = false;
                        CommonReq commonReq = new CommonReq
                        {
                            LoginID = appRequest.UserID,
                            LoginTypeID = appRequest.LoginTypeID,
                            CommonInt = appRequest.SearchId
                        };
                        IEmpML ml = new EmpML(_accessor, _env, false);
                        Response.Data = await ml.GetMapPoints(commonReq).ConfigureAwait(false); ;
                        Response.Statuscode = ErrorCodes.One;
                        Response.Msg = ErrorCodes.SUCCESS;
                    }
                    else
                    {
                        Response.Msg = appResp.IsPasswordExpired ? "Password Expired" : "Permission denied";
                    }
                }
            }
            catch (Exception ex)
            {
                Response.Msg = ErrorCodes.AnError;
            }
            return Json(Response);
        }
        //--------------------------Sep 28, 2020--------------------------//
        //--------------------------Oct 31, 2020--------------------------//
        [HttpPost]
        public IActionResult GetTodayTransactors([FromBody] CommonFilterRequest appRequest)
        {
            var Response = new TodayTransactorModelResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };
            try
            {
                var appResp = appML.CheckEmpSession(appRequest);
                if (appResp.Statuscode == ErrorCodes.One)
                {
                    Response.IsVersionValid = true;
                    if (!appResp.IsPasswordExpired && appRequest.LoginTypeID == LoginType.Employee)
                    {
                        Response.IsPasswordExpired = false;
                        IEmpML ml = new EmpML(_accessor, _env, false);
                        Response.Data = ml.TodayTransactors(appRequest.Criteria, appRequest.UserID);
                        Response.Statuscode = ErrorCodes.One;
                        Response.Msg = ErrorCodes.SUCCESS;
                    }
                    else
                    {
                        Response.Msg = appResp.IsPasswordExpired ? "Password Expired" : "Permission denied";
                    }
                }
            }
            catch (Exception ex)
            {
                Response.Msg = ErrorCodes.AnError;
            }
            return Json(Response);
        }

        [HttpPost]
        public IActionResult GetTodayOutletsListForEmp([FromBody] AppSessionReq appRequest)
        {
            var Response = new TodayOutletListResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };
            try
            {
                var appResp = appML.CheckEmpSession(appRequest);
                if (appResp.Statuscode == ErrorCodes.One)
                {
                    Response.IsVersionValid = true;
                    if (!appResp.IsPasswordExpired && appRequest.LoginTypeID == LoginType.Employee)
                    {
                        Response.IsPasswordExpired = false;
                        IEmpML ml = new EmpML(_accessor, _env, false);
                        Response.Data = ml.TodayOutletsListForEmp(appRequest.UserID);
                        Response.Statuscode = ErrorCodes.One;
                        Response.Msg = ErrorCodes.SUCCESS;
                    }
                    else
                    {
                        Response.Msg = appResp.IsPasswordExpired ? "Password Expired" : "Permission denied";
                    }
                }
            }
            catch (Exception ex)
            {
                Response.Msg = ErrorCodes.AnError;
            }
            return Json(Response);
        }
        //--------------------------Oct 31, 2020--------------------------//
    }
}
