using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.MiddleLayer;
using RoundpayFinTech.AppCode.Model.App;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.Controllers
{
    public partial class AppController
    {
        [HttpPost]
        public async Task<IActionResult> MyCommission([FromBody] AppMyCommReq appMyCommReq)
        {
            var appReportResponse = new AppReportResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            var appRequest = new AppSessionReq
            {
                APPID = appMyCommReq.APPID,
                IMEI = appMyCommReq.IMEI,
                LoginTypeID = appMyCommReq.LoginTypeID,
                UserID = appMyCommReq.UserID,
                SessionID = appMyCommReq.SessionID,
                RegKey = appMyCommReq.RegKey,
                SerialNo = appMyCommReq.SerialNo,
                Version = appMyCommReq.Version,
                Session = appMyCommReq.Session
            };
            var appResp = appML.CheckAppSession(appRequest);
            appReportResponse.IsAppValid = appResp.IsAppValid;
            appReportResponse.IsVersionValid = appResp.IsVersionValid;
            appReportResponse.IsPasswordExpired = appResp.IsPasswordExpired;
            appReportResponse.Statuscode = appResp.Statuscode;
            if (appResp.Statuscode == ErrorCodes.One)
            {
                if (!appResp.IsPasswordExpired)
                {
                    var _filter = new CommonReq
                    {
                        LoginTypeID = appMyCommReq.LoginTypeID,
                        LoginID = appMyCommReq.UserID,
                        CommonInt = appMyCommReq.SlabID,
                    };
                    appReportResponse.Statuscode = ErrorCodes.One;
                    appReportResponse.Msg = ErrorCodes.SUCCESS;
                    await Task.Delay(0).ConfigureAwait(false);
                    ISlabML slabML = new SlabML(_accessor, _env, false);
                    SlabDetailModel slabDetailModel = slabML.GetSlabCommissionApp(_filter);
                    appReportResponse.SlabCommissions = slabDetailModel.SlabDetails;
                }
            }
            else
            {
                appReportResponse.Msg = appResp.Msg;
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "MyCommission",
                CommonStr2 = JsonConvert.SerializeObject(appMyCommReq),
                CommonStr3 = JsonConvert.SerializeObject(appReportResponse)
            });
            return Json(appReportResponse);
        }
        [HttpPost]
        public async Task<IActionResult> GetRealLapuCommission([FromBody] AppMyCommReq appMyCommReq)
        {
            var appResponse = new AppObjectResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            var appRequest = new AppSessionReq
            {
                APPID = appMyCommReq.APPID,
                IMEI = appMyCommReq.IMEI,
                LoginTypeID = appMyCommReq.LoginTypeID,
                UserID = appMyCommReq.UserID,
                SessionID = appMyCommReq.SessionID,
                RegKey = appMyCommReq.RegKey,
                SerialNo = appMyCommReq.SerialNo,
                Version = appMyCommReq.Version,
                Session = appMyCommReq.Session
            };
            var appResp = appML.CheckAppSession(appRequest);
            appResponse.IsAppValid = appResp.IsAppValid;
            appResponse.IsVersionValid = appResp.IsVersionValid;
            appResponse.IsPasswordExpired = appResp.IsPasswordExpired;
            appResponse.Statuscode = appResp.Statuscode;
            if (appResp.Statuscode == ErrorCodes.One)
            {
                if (!appResp.IsPasswordExpired)
                {
                    var _filter = new CommonReq
                    {
                        LoginID = appMyCommReq.UserID,
                        CommonInt = appMyCommReq.OID,
                    };
                    appResponse.Statuscode = ErrorCodes.One;
                    appResponse.Msg = ErrorCodes.SUCCESS;
                    ISlabML slabML = new SlabML(_accessor, _env, false);
                    appResponse.slabDetail = await slabML.GetLapuRealComm(_filter).ConfigureAwait(false);
                }
            }
            else
            {
                appResponse.Msg = appResp.Msg;
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "GetRealLapuCommission",
                CommonStr2 = JsonConvert.SerializeObject(appMyCommReq),
                CommonStr3 = JsonConvert.SerializeObject(appResponse)
            });
            return Json(appResponse);
        }
        [HttpPost]
        public async Task<IActionResult> GetCalculatedCommission([FromBody] AppMyCommReq appMyCommReq)
        {
            var appResponse = new AppObjectResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            var appRequest = new AppSessionReq
            {
                APPID = appMyCommReq.APPID,
                IMEI = appMyCommReq.IMEI,
                LoginTypeID = appMyCommReq.LoginTypeID,
                UserID = appMyCommReq.UserID,
                SessionID = appMyCommReq.SessionID,
                RegKey = appMyCommReq.RegKey,
                SerialNo = appMyCommReq.SerialNo,
                Version = appMyCommReq.Version,
                Session = appMyCommReq.Session
            };
            var appResp = appML.CheckAppSession(appRequest);
            appResponse.IsAppValid = appResp.IsAppValid;
            appResponse.IsVersionValid = appResp.IsVersionValid;
            appResponse.IsPasswordExpired = appResp.IsPasswordExpired;
            appResponse.Statuscode = appResp.Statuscode;
            if (appResp.Statuscode == ErrorCodes.One)
            {
                if (!appResp.IsPasswordExpired)
                {
                    var _filter = new CommonReq
                    {
                        LoginID = appMyCommReq.UserID,
                        CommonInt = appMyCommReq.OID,
                        CommonDecimal = appMyCommReq.Amount
                    };
                    appResponse.Statuscode = ErrorCodes.One;
                    appResponse.Msg = ErrorCodes.SUCCESS;
                    ISlabML slabML = new SlabML(_accessor, _env, false);
                    appResponse.commissionDisplay = await slabML.GetDisplayCommission(_filter).ConfigureAwait(false);
                }
            }
            else
            {
                appResponse.Msg = appResp.Msg;
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "GetRealLapuCommission",
                CommonStr2 = JsonConvert.SerializeObject(appMyCommReq),
                CommonStr3 = JsonConvert.SerializeObject(appResponse)
            });
            return Json(appResponse);
        }
        [HttpPost]
        public async Task<IActionResult> DisplayCommission([FromBody] AppSessionReq appSessionReq)
        {
            var appReportResponse = new AppReportResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };

            var appResp = appML.CheckAppSession(appSessionReq);
            appReportResponse.IsAppValid = appResp.IsAppValid;
            appReportResponse.IsVersionValid = appResp.IsVersionValid;
            appReportResponse.IsPasswordExpired = appResp.IsPasswordExpired;
            appReportResponse.Statuscode = appResp.Statuscode;
            if (appResp.Statuscode == ErrorCodes.One)
            {
                if (!appResp.IsPasswordExpired)
                {
                    var _filter = new CommonReq
                    {
                        LoginTypeID = appSessionReq.LoginTypeID,
                        LoginID = appSessionReq.UserID
                    };
                    appReportResponse.Statuscode = ErrorCodes.One;
                    appReportResponse.Msg = ErrorCodes.SUCCESS;
                    await Task.Delay(0).ConfigureAwait(false);
                    ISlabML slabML = new SlabML(_accessor, _env, false);
                    appReportResponse.SlabDetailDisplayLvl = slabML.GetSlabDetailForDisplayForApp(_filter);
                }
            } 
            else
            {
                appReportResponse.Msg = appResp.Msg;
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "DisplayCommissionLvl",
                CommonStr2 = JsonConvert.SerializeObject(appSessionReq),
                CommonStr3 = JsonConvert.SerializeObject(appReportResponse)
            });
            return Json(appReportResponse);
        }
        [HttpPost]
        public async Task<IActionResult> IncentiveDetail([FromBody] AppMyCommReq appMyCommReq)
        {
            var appReportResponse = new AppReportResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            var appResp = appML.CheckAppSession(appMyCommReq);
            appReportResponse.IsAppValid = appResp.IsAppValid;
            appReportResponse.IsVersionValid = appResp.IsVersionValid;
            appReportResponse.IsPasswordExpired = appResp.IsPasswordExpired;
            appReportResponse.Statuscode = appResp.Statuscode;
            if (appResp.Statuscode == ErrorCodes.One)
            {
                if (!appResp.IsPasswordExpired)
                {
                    var _filter = new CommonReq
                    {
                        LoginTypeID = appMyCommReq.LoginTypeID,
                        LoginID = appMyCommReq.UserID,
                        CommonInt = appMyCommReq.OID
                    };
                    appReportResponse.Statuscode = ErrorCodes.One;
                    appReportResponse.Msg = ErrorCodes.SUCCESS;
                    await Task.Delay(0).ConfigureAwait(false);
                    ISlabML slabML = new SlabML(_accessor, _env, false);
                    appReportResponse.IncentiveDetails = slabML.GetIncentive(_filter);
                }
            }
            else
            {
                appReportResponse.Msg = appResp.Msg;
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "IncentiveDetail",
                CommonStr2 = JsonConvert.SerializeObject(appMyCommReq),
                CommonStr3 = JsonConvert.SerializeObject(appReportResponse)
            });
            return Json(appReportResponse);
        }
        [HttpPost]
        public IActionResult SpecialOpsCommissionDetail([FromBody] AppSessionReq appSessionReq)
        {
            var appReportResponse = new AppResponseData
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };

            var appResp = appML.CheckAppSession(appSessionReq);
            appReportResponse.IsAppValid = appResp.IsAppValid;
            appReportResponse.IsVersionValid = appResp.IsVersionValid;
            appReportResponse.IsPasswordExpired = appResp.IsPasswordExpired;
            appReportResponse.Statuscode = appResp.Statuscode;
            if (appResp.Statuscode == ErrorCodes.One)
            {
                if (!appResp.IsPasswordExpired)
                {
                    var _filter = new CommonReq
                    {
                        LoginTypeID = appSessionReq.LoginTypeID,
                        LoginID = appSessionReq.UserID
                    };
                    appReportResponse.Statuscode = ErrorCodes.One;
                    appReportResponse.Msg = ErrorCodes.SUCCESS;
                    ISlabML sML = new SlabML(_accessor, _env, false);
                    appReportResponse.data = sML.GetSpecialSlabDetailApp(appSessionReq.OID, appSessionReq.UserID);
                }
            }
            else
            {
                appReportResponse.Msg = appResp.Msg;
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "SpecialOpsCommissionDetail",
                CommonStr2 = JsonConvert.SerializeObject(appSessionReq),
                CommonStr3 = JsonConvert.SerializeObject(appReportResponse)
            });
            return Json(appReportResponse);
        }
        [HttpPost]
        public IActionResult RealTimeCommission([FromBody] AppSessionReq appSessionReq)
        {
            var appReportResponse = new AppResponseData
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };

            var appResp = appML.CheckAppSession(appSessionReq);
            appReportResponse.IsAppValid = appResp.IsAppValid;
            appReportResponse.IsVersionValid = appResp.IsVersionValid;
            appReportResponse.IsPasswordExpired = appResp.IsPasswordExpired;
            appReportResponse.Statuscode = appResp.Statuscode;
            if (appResp.Statuscode == ErrorCodes.One)
            {
                if (!appResp.IsPasswordExpired)
                {
                    appReportResponse.Statuscode = ErrorCodes.One;
                    appReportResponse.Msg = ErrorCodes.SUCCESS;
                    ISlabML sML = new SlabML(_accessor, _env,false);
                    appReportResponse.data = sML.GetRealtimeCommApp(appSessionReq.UserID);
                }
            }
            else
            {
                appReportResponse.Msg = appResp.Msg;
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "RealTimeCommission",
                CommonStr2 = JsonConvert.SerializeObject(appSessionReq),
                CommonStr3 = JsonConvert.SerializeObject(appReportResponse)
            });
            return Json(appReportResponse);
        }
    }
}
