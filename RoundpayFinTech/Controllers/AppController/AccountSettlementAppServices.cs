using Fintech.AppCode.HelperClass;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using Fintech.AppCode.WebRequest;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using QRCoder;
using RoundpayFinTech.AppCode.Configuration;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.MiddleLayer;
using RoundpayFinTech.AppCode.Model.App;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.Controllers
{
    public partial class AppController
    {
        [HttpPost]
        public async Task<IActionResult> GetSettlementAccount([FromBody] AppSessionReq appSessionReq)
        {
            var appResponseData = new AppResponseData
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (appSessionReq.LoginTypeID == LoginType.ApplicationUser)
            {
                var appResp = appML.CheckAppSession(appSessionReq);
                appResponseData.IsAppValid = appResp.IsAppValid;
                appResponseData.IsVersionValid = appResp.IsVersionValid;
                appResponseData.IsPasswordExpired = appResp.IsPasswordExpired;
                appResponseData.Statuscode = appResp.Statuscode;
                appResponseData.IsSattlemntAccountVerify = ApplicationSetting.IsSattlemntAccountVerify;
                if (appResp.Statuscode == ErrorCodes.One)
                {
                    if (!appResp.IsPasswordExpired)
                    {
                        ISettlementaccountML ml = new SettlementaccountML(_accessor, _env, false);
                        appResponseData.data = ml.GetSettlementaccountList(appSessionReq.UserID, appSessionReq.UserID);
                    }
                }

            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "GetSettlementAccount",
                CommonStr2 = JsonConvert.SerializeObject(appSessionReq),
                CommonStr3 = JsonConvert.SerializeObject(appResponseData)
            });
            return Json(appResponseData);
        }

        [HttpPost]
        public IActionResult UpdateSettlementAccount([FromBody] AppSettlementAccountUpdateReq appSessionReq)
        {
            var appResponseData = new AppResponseData
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            var appResp = appML.CheckAppSession(appSessionReq);
            appResponseData.IsAppValid = appResp.IsAppValid;
            appResponseData.IsVersionValid = appResp.IsVersionValid;
            appResponseData.IsPasswordExpired = appResp.IsPasswordExpired;
            appResponseData.Statuscode = appResp.Statuscode;

            if (appResp.Statuscode == ErrorCodes.One)
            {
                if (!appResp.IsPasswordExpired)
                {
                    ISettlementaccountML ml = new SettlementaccountML(_accessor, _env, false);
                    appResponseData.data= ml.UpdateSettlementaccount(new Models.SattlementAccountModels
                    {
                        AccountHolder= appSessionReq.AccountHolder,
                        AccountNumber= appSessionReq.AccountNumber,
                        BankID= appSessionReq.BankID,
                        BankName= appSessionReq.BankName,
                        ID= appSessionReq.UpdateID,
                        EntryBy= appSessionReq.UserID,
                        UserID=appSessionReq.UserID,
                        IFSC=appSessionReq.IFSC
                    });
                }
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "UpdateSettlementAccount",
                CommonStr2 = JsonConvert.SerializeObject(appSessionReq),
                CommonStr3 = JsonConvert.SerializeObject(appResponseData)
            });
            return Json(appResponseData);
        }

        [HttpPost]
        public IActionResult ToggleDefaulSettlementAcount([FromBody] AppSettlementAccountUpdateReq appSessionReq)
        {
            var appResponseData = new AppResponseData
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            var appResp = appML.CheckAppSession(appSessionReq);
            appResponseData.IsAppValid = appResp.IsAppValid;
            appResponseData.IsVersionValid = appResp.IsVersionValid;
            appResponseData.IsPasswordExpired = appResp.IsPasswordExpired;
            appResponseData.Statuscode = appResp.Statuscode;

            if (appResp.Statuscode == ErrorCodes.One)
            {
                if (!appResp.IsPasswordExpired)
                {
                    ISettlementaccountML ml = new SettlementaccountML(_accessor, _env, false);
                    appResponseData.data = ml.SetDefaultSettlementaccount(new Models.SattlementAccountModels
                    {
                        LoginID = appSessionReq.UserID,
                        UserID = appSessionReq.UserID,
                        ID = appSessionReq.UpdateID
                    });
                }
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "ToggleDefaulSettlementAcount",
                CommonStr2 = JsonConvert.SerializeObject(appSessionReq),
                CommonStr3 = JsonConvert.SerializeObject(appResponseData)
            });
            return Json(appResponseData);
        }

        [HttpPost]
        public IActionResult DeleteSettlementAcount([FromBody] AppSettlementAccountUpdateReq appSessionReq)
        {
            var appResponseData = new AppResponseData
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            var appResp = appML.CheckAppSession(appSessionReq);
            appResponseData.IsAppValid = appResp.IsAppValid;
            appResponseData.IsVersionValid = appResp.IsVersionValid;
            appResponseData.IsPasswordExpired = appResp.IsPasswordExpired;
            appResponseData.Statuscode = appResp.Statuscode;

            if (appResp.Statuscode == ErrorCodes.One)
            {
                if (!appResp.IsPasswordExpired)
                {
                    ISettlementaccountML ml = new SettlementaccountML(_accessor, _env, false);
                    appResponseData.data = ml.SetDeleteSettlementaccount(new CommonReq
                    {
                        LoginTypeID = LoginType.ApplicationUser,
                        LoginID = appSessionReq.UserID,
                        UserID = appSessionReq.UserID,
                        CommonInt = appSessionReq.UpdateID
                    });
                }
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "DeleteSettlementAcount",
                CommonStr2 = JsonConvert.SerializeObject(appSessionReq),
                CommonStr3 = JsonConvert.SerializeObject(appResponseData)
            });
            return Json(appResponseData);
        }

        [HttpPost]
        public IActionResult VerifySettlementAccountOfUser([FromBody] AppSettlementAccountUpdateReq appSessionReq)
        {
            var appResponseData = new AppResponseData
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            var appResp = appML.CheckAppSession(appSessionReq);
            appResponseData.IsAppValid = appResp.IsAppValid;
            appResponseData.IsVersionValid = appResp.IsVersionValid;
            appResponseData.IsPasswordExpired = appResp.IsPasswordExpired;
            appResponseData.Statuscode = appResp.Statuscode;

            if (appResp.Statuscode == ErrorCodes.One)
            {
                if (!appResp.IsPasswordExpired)
                {
                    ISettlementaccountML ml = new SettlementaccountML(_accessor, _env, false);
                    var accRes = ml.GeSettlementAccountByID(new CommonReq
                    {
                        CommonInt = appSessionReq.UpdateID,
                        LoginID= appSessionReq.UserID
                    });
                    if (accRes.ID > 0)
                    {
                        IMoneyTransferML moneyTransferML = new MoneyTransferML(_accessor, _env);
                        var vRes=  moneyTransferML.VerifyAccount(new AppCode.Model.MoneyTransfer.MBeneVerifyRequest
                        {
                            UserID = appSessionReq.UserID,
                            OutletID = appResp.GetID,
                            AccountNo = accRes.AccountNumber,
                            BankID = accRes.BankID,
                            IFSC = accRes.IFSC,
                            RequestMode = RequestMode.APPS,
                            SenderMobile = appResp.MobileNo,
                            AccountTableID = accRes.ID ?? 0
                        });
                        appResponseData.Msg = vRes.Msg;
                        appResponseData.data = vRes;
                    }
                }
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "VerifySettlementAccountOfUser",
                CommonStr2 = JsonConvert.SerializeObject(appSessionReq),
                CommonStr3 = JsonConvert.SerializeObject(appResponseData)
            });
            return Json(appResponseData);
        }

        [HttpPost]
        public IActionResult UpdateUTRByUser([FromBody] AppSettlementAccountUpdateReq appSessionReq)
        {
            var appResponseData = new AppResponseData
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            var appResp = appML.CheckAppSession(appSessionReq);
            appResponseData.IsAppValid = appResp.IsAppValid;
            appResponseData.IsVersionValid = appResp.IsVersionValid;
            appResponseData.IsPasswordExpired = appResp.IsPasswordExpired;
            appResponseData.Statuscode = appResp.Statuscode;

            if (appResp.Statuscode == ErrorCodes.One)
            {
                if (!appResp.IsPasswordExpired)
                {
                    ISettlementaccountML ml = new SettlementaccountML(_accessor, _env, false);
                    appResponseData.data = ml.UpdateUTRByUser(appSessionReq.UpdateID, appSessionReq.UTR, appSessionReq.UserID);
                }
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "UpdateUTRByUser",
                CommonStr2 = JsonConvert.SerializeObject(appSessionReq),
                CommonStr3 = JsonConvert.SerializeObject(appResponseData)
            });
            return Json(appResponseData);
        }
    }
}
