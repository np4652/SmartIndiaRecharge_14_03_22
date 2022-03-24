using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Org.BouncyCastle.Bcpg.OpenPgp;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.MiddleLayer;
using RoundpayFinTech.AppCode.Model.App;
using RoundpayFinTech.AppCode.Model.MoneyTransfer;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Validators;

namespace RoundpayFinTech.Controllers
{
    public partial class AppController
    {
        #region DMTRelated
        [HttpPost]
        public async Task<IActionResult> CreateSender([FromBody] SenderDetailReq senderDetailReq)
        {
            var appResponse = new AppResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };
            if (senderDetailReq.senderRequest != null)
            {
                appResponse = appML.CheckAppSession(senderDetailReq);
                if (appResponse.Statuscode == ErrorCodes.One)
                {
                    if (!appResponse.IsPasswordExpired)
                    {
                        var dMTReq = new DMTReq
                        {
                            UserID = senderDetailReq.UserID,
                            LT = 1,
                            RequestMode = RequestMode.APPS,
                            OutletID = appResponse.GetID,
                            IsValidate = false,
                            SenderNO = senderDetailReq.senderRequest.MobileNo
                        };
                        var senderReq = new SenderRequest
                        {
                            UserID = senderDetailReq.UserID,
                            Name = senderDetailReq.senderRequest.Name,
                            LastName = senderDetailReq.senderRequest.LastName,
                            MobileNo = senderDetailReq.senderRequest.MobileNo,
                            Pincode = senderDetailReq.senderRequest.Pincode,
                            Address = senderDetailReq.senderRequest.Address,
                            OTP = senderDetailReq.senderRequest.OTP ?? "",
                            Dob = senderDetailReq.senderRequest.Dob
                        };
                        var createSen = new CreateSen
                        {
                            dMTReq = dMTReq,
                            senderRequest = senderReq
                        };
                        IDmtML dmtML = new DmtML(_accessor, _env);
                        var responseStatus = await dmtML.CreateSender(createSen);
                        appResponse.Statuscode = responseStatus.Statuscode;
                        appResponse.Msg = responseStatus.Msg;
                        appResponse.SID = responseStatus.ReffID;

                    }
                }
            }
            else
            {
                appResponse.Msg = "Incomplete parameter!";
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "CreateSender",
                CommonStr2 = JsonConvert.SerializeObject(senderDetailReq),
                CommonStr3 = JsonConvert.SerializeObject(appResponse)
            });
            return Json(appResponse);
        }
        [HttpPost]
        public async Task<IActionResult> GetSender([FromBody] SenderDetailReq senderDetailReq)
        {
            var getSenderResp = new AppGetSenderResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };
            if (senderDetailReq.senderRequest != null)
            {
                var appResp = appML.CheckAppSession(senderDetailReq);
                getSenderResp.IsAppValid = appResp.IsAppValid;
                getSenderResp.IsVersionValid = appResp.IsVersionValid;
                getSenderResp.IsPasswordExpired = appResp.IsPasswordExpired;
                getSenderResp.Statuscode = appResp.Statuscode;
                if (appResp.Statuscode == ErrorCodes.One)
                {
                    if (!appResp.IsPasswordExpired)
                    {
                        var dMTReq = new DMTReq
                        {
                            UserID = senderDetailReq.UserID,
                            LT = 1,
                            RequestMode = RequestMode.APPS,
                            OutletID = appResp.GetID,
                            IsValidate = false,
                            SenderNO = senderDetailReq.senderRequest.MobileNo
                        };
                        IDmtML dmtML = new DmtML(_accessor, _env);

                        var responseStatus = await dmtML.CheckSender(dMTReq).ConfigureAwait(false);
                        getSenderResp.Statuscode = responseStatus.Statuscode;
                        getSenderResp.Msg = responseStatus.Msg;
                        getSenderResp.IsSenderNotExists = responseStatus.CommonInt == 1;
                        getSenderResp.SenderName = responseStatus.CommonStr2 ?? "";
                        getSenderResp.SenderBalance = responseStatus.CommonStr ?? "";
                        getSenderResp.SID = responseStatus.ReffID ?? string.Empty;
                    }
                }
                else
                {
                    getSenderResp.Msg = appResp.Msg;
                }
            }
            else
            {
                getSenderResp.Msg = "Incomplete parameter!";
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "GetSender",
                CommonStr2 = JsonConvert.SerializeObject(senderDetailReq),
                CommonStr3 = JsonConvert.SerializeObject(getSenderResp)
            });
            return Json(getSenderResp);
        }
        [HttpPost]
        public async Task<IActionResult> VerifySender([FromBody] SenderDetailReq senderDetailReq)
        {
            var appResp = new AppResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };

            if (senderDetailReq.senderRequest != null)
            {
                appResp = appML.CheckAppSession(senderDetailReq);
                if (appResp.Statuscode == ErrorCodes.One)
                {
                    if (!appResp.IsPasswordExpired)
                    {
                        var dMTReq = new DMTReq
                        {
                            UserID = senderDetailReq.UserID,
                            LT = 1,
                            RequestMode = RequestMode.APPS,
                            OutletID = appResp.GetID,
                            IsValidate = false,
                            SenderNO = senderDetailReq.senderRequest.MobileNo,
                            ReffID = senderDetailReq.SID
                        };
                        IDmtML dmtML = new DmtML(_accessor, _env);
                        var responseStatus = await dmtML.VerifySender(dMTReq, senderDetailReq.senderRequest.OTP ?? "");
                        appResp.Statuscode = responseStatus.Statuscode;
                        appResp.Msg = responseStatus.Msg;
                    }
                }
            }
            else
            {
                appResp.Msg = "Incomplete parameter!";
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "VerifySender",
                CommonStr2 = JsonConvert.SerializeObject(senderDetailReq),
                CommonStr3 = JsonConvert.SerializeObject(appResp)
            });
            return Json(appResp);
        }
        [HttpPost]
        public async Task<IActionResult> AddBeneficiary([FromBody] AddBeneRequest addBeneRequest)
        {
            var appResponse = new AppResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };

            if (addBeneRequest.senderRequest != null)
            {
                appResponse = appML.CheckAppSession(addBeneRequest);
                if (appResponse.Statuscode == ErrorCodes.One)
                {
                    if (!appResponse.IsPasswordExpired)
                    {
                        var dMTReq = new DMTReq
                        {
                            UserID = addBeneRequest.UserID,
                            LT = 1,
                            RequestMode = RequestMode.APPS,
                            OutletID = appResponse.GetID,
                            IsValidate = false,
                            SenderNO = addBeneRequest.senderRequest.MobileNo,
                            ReffID = addBeneRequest.SID
                        };
                        var addBeni = new AddBeni
                        {
                            BeneName = addBeneRequest.beneDetail.BeneName ?? "",
                            MobileNo = addBeneRequest.beneDetail.MobileNo ?? "",
                            AccountNo = addBeneRequest.beneDetail.AccountNo ?? "",
                            BankName = addBeneRequest.beneDetail.BankName ?? "",
                            IFSC = addBeneRequest.beneDetail.IFSC ?? "",
                            SenderMobileNo = addBeneRequest.senderRequest.MobileNo,
                            BankID = addBeneRequest.beneDetail.BankID
                        };
                        IDmtML dmtML = new DmtML(_accessor, _env);
                        var responseStatus = await dmtML.CreateBeneficiary(addBeni, dMTReq);
                        appResponse.Statuscode = responseStatus.Statuscode;
                        appResponse.Msg = responseStatus.Msg;
                        appResponse.SID = responseStatus.ReffID;
                        appResponse.IsOTPRequired = responseStatus.Statuscode == ErrorCodes.One && responseStatus.CommonInt == ErrorCodes.One && (responseStatus.ReffID ?? string.Empty) != string.Empty;
                    }
                }
            }
            else
            {
                appResponse.Msg = "Incomplete parameter!";
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "AddBeneficiary",
                CommonStr2 = JsonConvert.SerializeObject(addBeneRequest),
                CommonStr3 = JsonConvert.SerializeObject(appResponse)
            });
            return Json(appResponse);
        }
        [HttpPost]
        public async Task<IActionResult> DeleteBeneficiary([FromBody] AddBeneRequest addBeneRequest)
        {
            var appResponse = new AppResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };

            if (addBeneRequest.senderRequest != null)
            {
                appResponse = appML.CheckAppSession(addBeneRequest);
                if (appResponse.Statuscode == ErrorCodes.One)
                {
                    if (!appResponse.IsPasswordExpired)
                    {
                        var dMTReq = new DMTReq
                        {
                            UserID = addBeneRequest.UserID,
                            LT = 1,
                            RequestMode = RequestMode.APPS,
                            OutletID = appResponse.GetID,
                            IsValidate = false,
                            SenderNO = addBeneRequest.senderRequest.MobileNo,
                            ReffID = addBeneRequest.SID
                        };

                        IDmtML dmtML = new DmtML(_accessor, _env);
                        var responseStatus = await dmtML.DeleteBeneficiary(dMTReq, addBeneRequest.beneDetail.BeneID, addBeneRequest.OTP);
                        appResponse.Statuscode = responseStatus.Statuscode;
                        appResponse.Msg = responseStatus.Msg;
                        appResponse.IsOTPRequired = responseStatus.CommonBool;
                    }
                }
            }
            else
            {
                appResponse.Msg = "Incomplete parameter!";
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "DeleteBeneficiary",
                CommonStr2 = JsonConvert.SerializeObject(addBeneRequest),
                CommonStr3 = JsonConvert.SerializeObject(appResponse)
            });
            return Json(appResponse);
        }
        [HttpPost]
        public async Task<IActionResult> GetBankList([FromBody] AppSessionReq appSessionReq)
        {
            var appBankResponse = new AppBankResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            var appResp = appML.CheckAppSession(appSessionReq);
            appBankResponse.IsAppValid = appResp.IsAppValid;
            appBankResponse.IsVersionValid = appResp.IsVersionValid;
            appBankResponse.IsPasswordExpired = appResp.IsPasswordExpired;
            appBankResponse.Statuscode = appResp.Statuscode;
            if (appResp.Statuscode == ErrorCodes.One)
            {
                if (!appResp.IsPasswordExpired)
                {
                    var _filter = new CommonReq
                    {

                        LoginTypeID = appSessionReq.LoginTypeID,
                        LoginID = appSessionReq.UserID,
                        CommonInt = 0
                    };
                    appBankResponse.Statuscode = ErrorCodes.One;
                    appBankResponse.Msg = ErrorCodes.SUCCESS;
                    await Task.Delay(0);
                    IBankML bankML = new BankML(_accessor, _env, false);
                    appBankResponse.BankMasters = bankML.BankMastersApp(_filter);
                }
            }
            else
            {
                appBankResponse.Msg = appResp.Msg;
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "GetBankList",
                CommonStr2 = JsonConvert.SerializeObject(appSessionReq),
                CommonStr3 = JsonConvert.SerializeObject(appBankResponse)
            });
            return Json(appBankResponse);
        }
        [HttpPost]
        public async Task<IActionResult> GetBeneficiary([FromBody] SenderDetailReq senderDetailReq)
        {
            var appBeneficiaryResp = new AppBeneficiaryResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };
            if (senderDetailReq.senderRequest != null)
            {
                var appResp = appML.CheckAppSession(senderDetailReq);
                appBeneficiaryResp.IsAppValid = appResp.IsAppValid;
                appBeneficiaryResp.IsVersionValid = appResp.IsVersionValid;
                appBeneficiaryResp.IsPasswordExpired = appResp.IsPasswordExpired;
                appBeneficiaryResp.Statuscode = appResp.Statuscode;
                if (appResp.Statuscode == ErrorCodes.One)
                {
                    if (!appResp.IsPasswordExpired)
                    {
                        var dMTReq = new DMTReq
                        {
                            UserID = senderDetailReq.UserID,
                            LT = 1,
                            RequestMode = RequestMode.APPS,
                            OutletID = appResp.GetID,
                            IsValidate = false,
                            SenderNO = senderDetailReq.senderRequest.MobileNo,
                            ReffID = senderDetailReq.SID
                        };

                        IDmtML dmtML = new DmtML(_accessor, _env);
                        var responseStatus = await dmtML.GetBeneficiary(dMTReq);
                        appBeneficiaryResp.Statuscode = responseStatus.Statuscode;
                        appBeneficiaryResp.Msg = responseStatus.Msg;
                        appBeneficiaryResp.Benis = responseStatus.addBeni;
                    }
                }
                else
                {
                    appBeneficiaryResp.Msg = appResp.Msg;
                }
            }
            else
            {
                appBeneficiaryResp.Msg = "Incomplete parameter!";
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "GetBeneficiary",
                CommonStr2 = JsonConvert.SerializeObject(senderDetailReq),
                CommonStr3 = JsonConvert.SerializeObject(appBeneficiaryResp)
            });
            return Json(appBeneficiaryResp);
        }
        [HttpPost]
        public async Task<IActionResult> GenerateBenficiaryOTP([FromBody] SenderDetailReq senderDetailReq)
        {
            var getSenderResp = new AppGetSenderResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };
            if (senderDetailReq.senderRequest != null)
            {
                var appResp = appML.CheckAppSession(senderDetailReq);
                getSenderResp.IsAppValid = appResp.IsAppValid;
                getSenderResp.IsVersionValid = appResp.IsVersionValid;
                getSenderResp.IsPasswordExpired = appResp.IsPasswordExpired;
                getSenderResp.Statuscode = appResp.Statuscode;
                if (appResp.Statuscode == ErrorCodes.One)
                {
                    if (!appResp.IsPasswordExpired)
                    {
                        var dMTReq = new DMTReq
                        {
                            UserID = senderDetailReq.UserID,
                            LT = 1,
                            RequestMode = RequestMode.APPS,
                            OutletID = appResp.GetID,
                            IsValidate = false,
                            SenderNO = senderDetailReq.senderRequest.MobileNo
                        };
                        IDmtML dmtML = new DmtML(_accessor, _env);
                        var responseStatus = await dmtML.GenerateOTP(dMTReq);
                        getSenderResp.Statuscode = responseStatus.Statuscode;
                        getSenderResp.Msg = responseStatus.Msg;
                        getSenderResp.SID = responseStatus.ReffID ?? string.Empty;
                    }
                }
                else
                {
                    getSenderResp.Msg = appResp.Msg;
                }
            }
            else
            {
                getSenderResp.Msg = "Incomplete parameter!";
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "GenerateBenficiaryOTP",
                CommonStr2 = JsonConvert.SerializeObject(senderDetailReq),
                CommonStr3 = JsonConvert.SerializeObject(getSenderResp)
            });
            return Json(getSenderResp);
        }
        [HttpPost]
        public async Task<IActionResult> ValidateBeneficiary([FromBody] SenderDetailReq senderDetailReq)
        {
            var appResp = new AppResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };
            if (senderDetailReq.senderRequest != null)
            {
                appResp = appML.CheckAppSession(senderDetailReq);
                if (appResp.Statuscode == ErrorCodes.One)
                {
                    if (!appResp.IsPasswordExpired)
                    {
                        var dMTReq = new DMTReq
                        {
                            UserID = senderDetailReq.UserID,
                            LT = 1,
                            RequestMode = RequestMode.APPS,
                            OutletID = appResp.GetID,
                            IsValidate = false,
                            SenderNO = senderDetailReq.senderRequest.MobileNo,
                            ReffID = senderDetailReq.SID
                        };
                        IDmtML dmtML = new DmtML(_accessor, _env);
                        var responseStatus = await dmtML.ValidateBeneficiary(dMTReq, senderDetailReq.BeneMobile, senderDetailReq.AccountNo, senderDetailReq.OTP);
                        appResp.Statuscode = responseStatus.Statuscode;
                        appResp.Msg = responseStatus.Msg;
                    }
                }
            }
            else
            {
                appResp.Msg = "Incomplete parameter!";
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "ValidateBeneficiary",
                CommonStr2 = JsonConvert.SerializeObject(senderDetailReq),
                CommonStr3 = JsonConvert.SerializeObject(appResp)
            });
            return Json(appResp);
        }
        [HttpPost]
        public async Task<IActionResult> VerifyAccount([FromBody] AddBeneRequest addBeneRequest)
        {
            var dMRTransactionResponse = new _DMRTransactionResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };
            var appRequest = new AppSessionReq
            {
                APPID = addBeneRequest.APPID,
                IMEI = addBeneRequest.IMEI,
                LoginTypeID = addBeneRequest.LoginTypeID,
                UserID = addBeneRequest.UserID,
                SessionID = addBeneRequest.SessionID,
                RegKey = addBeneRequest.RegKey,
                SerialNo = addBeneRequest.SerialNo,
                Version = addBeneRequest.Version,
                Session = addBeneRequest.Session
            };
            if (addBeneRequest.senderRequest != null)
            {
                var appResp = appML.CheckAppSession(appRequest);
                dMRTransactionResponse.IsAppValid = appResp.IsAppValid;
                dMRTransactionResponse.IsVersionValid = appResp.IsVersionValid;
                dMRTransactionResponse.IsPasswordExpired = appResp.IsPasswordExpired;
                dMRTransactionResponse.Statuscode = appResp.Statuscode;
                if (appResp.Statuscode == ErrorCodes.One)
                {
                    if (!appResp.IsPasswordExpired)
                    {
                        var dMTReq = new DMTReq
                        {
                            UserID = addBeneRequest.UserID,
                            LT = 1,
                            RequestMode = RequestMode.APPS,
                            OutletID = appResp.GetID,
                            IsValidate = false,
                            SenderNO = addBeneRequest.senderRequest.MobileNo
                        };

                        var reqSendMoney = new ReqSendMoney
                        {
                            AccountNo = addBeneRequest.beneDetail.AccountNo,
                            Amount = 0,
                            BeneID = "",
                            Channel = false,
                            IFSC = addBeneRequest.beneDetail.IFSC ?? string.Empty,
                            MobileNo = addBeneRequest.beneDetail.MobileNo ?? string.Empty,
                            Bank = addBeneRequest.beneDetail.BankName ?? string.Empty,
                            BankID = addBeneRequest.beneDetail.BankID,
                            BeneName = addBeneRequest.beneDetail.BeneName ?? string.Empty
                        };
                        IDmtML dmtML = new DmtML(_accessor, _env);
                        DMRTransactionResponse resp = await dmtML.Verification(dMTReq, reqSendMoney);
                        dMRTransactionResponse.Status = RechargeRespType.GetRechargeStatusText(resp.Statuscode);
                        dMRTransactionResponse.Msg = resp.Msg;
                        dMRTransactionResponse.Statuscode = resp.Statuscode;
                        dMRTransactionResponse.TransactionID = resp.TransactionID;
                        dMRTransactionResponse.BeneName = resp.BeneName;
                    }
                }
                else
                {
                    dMRTransactionResponse.Msg = appResp.Msg;
                }
            }
            else
            {
                dMRTransactionResponse.Msg = "Incomplete parameter!";
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "VerifyAccount",
                CommonStr2 = JsonConvert.SerializeObject(addBeneRequest),
                CommonStr3 = JsonConvert.SerializeObject(dMRTransactionResponse)
            });
            return Json(dMRTransactionResponse);
        }
        [HttpPost]
        public async Task<IActionResult> SendMoney([FromBody] AppSendMoneyReq appSendMoneyReq)
        {
            var dMRTransactionResponse = new DMRTransactionResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };
            var appRequest = new AppSessionReq
            {
                APPID = appSendMoneyReq.APPID,
                IMEI = appSendMoneyReq.IMEI,
                LoginTypeID = appSendMoneyReq.LoginTypeID,
                UserID = appSendMoneyReq.UserID,
                SessionID = appSendMoneyReq.SessionID,
                RegKey = appSendMoneyReq.RegKey,
                SerialNo = appSendMoneyReq.SerialNo,
                Version = appSendMoneyReq.Version,
                Session = appSendMoneyReq.Session
            };
            if (appSendMoneyReq != null && appSendMoneyReq.reqSendMoney != null)
            {
                var appResp = appML.CheckAppSession(appRequest);
                if (appResp.Statuscode == ErrorCodes.One)
                {
                    var dMTReq = new DMTReq
                    {
                        UserID = appSendMoneyReq.UserID,
                        LT = 1,
                        RequestMode = RequestMode.APPS,
                        OutletID = appResp.GetID,
                        IsValidate = false,
                        SenderNO = appSendMoneyReq.reqSendMoney.MobileNo
                    };
                    appSendMoneyReq.reqSendMoney.SecKey = appSendMoneyReq.SecurityKey ?? "";
                    IDmtML dmtML = new DmtML(_accessor, _env);
                    var _cResp = dmtML.CheckRepeativeTransaction(appSendMoneyReq.SessionID, appSendMoneyReq.UserID, appSendMoneyReq.reqSendMoney.Amount, appSendMoneyReq.reqSendMoney.AccountNo, 0);
                    if (_cResp.Statuscode > 0)
                    {
                        var resp = await dmtML.SendMoney(dMTReq, appSendMoneyReq.reqSendMoney);
                        dMRTransactionResponse.Statuscode = resp.Statuscode;
                        dMRTransactionResponse.Status = RechargeRespType.GetRechargeStatusText(resp.Statuscode);
                        dMRTransactionResponse.Msg = resp.Msg;
                        dMRTransactionResponse.TID = resp.TID;
                        dMRTransactionResponse.TransactionID = resp.TransactionID;
                        dMRTransactionResponse.VendorID = resp.VendorID;
                        dMRTransactionResponse.Response = resp.Response;
                        dMRTransactionResponse.Request = resp.Request;
                        dMRTransactionResponse.GroupID = resp.GroupID;
                        if (resp.Statuscode == RechargeRespType.FAILED && _cResp.Statuscode > ErrorCodes.One)
                        {
                            dmtML.CheckRepeativeTransaction(appSendMoneyReq.SessionID, appSendMoneyReq.UserID, appSendMoneyReq.reqSendMoney.Amount, appSendMoneyReq.reqSendMoney.AccountNo, _cResp.Statuscode);
                        }
                    }
                    else
                    {
                        dMRTransactionResponse.Statuscode = _cResp.Statuscode;
                        dMRTransactionResponse.Msg = _cResp.Msg;
                    }
                }
                else
                {
                    dMRTransactionResponse.Msg = appResp.Msg;
                }
            }
            else
            {
                dMRTransactionResponse.Msg = "Incomplete parameter!";
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "SendMoney",
                CommonStr2 = JsonConvert.SerializeObject(appSendMoneyReq),
                CommonStr3 = JsonConvert.SerializeObject(dMRTransactionResponse)
            });
            return Json(dMRTransactionResponse);
        }
        [HttpPost]
        public async Task<IActionResult> GetChargedAmount([FromBody] AppAmountRequest appAmountRequest)
        {
            var appVerificationCharge = new AppVerificationChargeResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };
            var appRequest = new AppSessionReq
            {
                APPID = appAmountRequest.APPID,
                IMEI = appAmountRequest.IMEI,
                LoginTypeID = appAmountRequest.LoginTypeID,
                UserID = appAmountRequest.UserID,
                SessionID = appAmountRequest.SessionID,
                RegKey = appAmountRequest.RegKey,
                SerialNo = appAmountRequest.SerialNo,
                Version = appAmountRequest.Version,
                Session = appAmountRequest.Session
            };
            if (appAmountRequest != null)
            {
                var appResp = appML.CheckAppSession(appRequest);
                appVerificationCharge.IsAppValid = appResp.IsAppValid;
                appVerificationCharge.IsVersionValid = appResp.IsVersionValid;
                if (appResp.Statuscode == ErrorCodes.One)
                {
                    var _req = new CommonReq
                    {
                        LoginID = appAmountRequest.UserID,
                        LoginTypeID = appAmountRequest.LoginTypeID,
                        CommonDecimal = appAmountRequest.Amount
                    };
                    ISellerML sellerML = new SellerML(_accessor, _env);
                    await Task.Delay(0);
                    var resp = sellerML.GetChargeForApp(_req);
                    appVerificationCharge.Statuscode = resp.Statuscode;
                    appVerificationCharge.Msg = resp.Msg;
                    appVerificationCharge.ChargedAmount = resp.Charged;
                }
                else
                {
                    appVerificationCharge.Msg = appResp.Msg;
                }
            }
            else
            {
                appVerificationCharge.Msg = "Incomplete parameter!";
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "GetChargedAmount",
                CommonStr2 = JsonConvert.SerializeObject(appAmountRequest),
                CommonStr3 = JsonConvert.SerializeObject(appVerificationCharge)
            });
            return Json(appVerificationCharge);
        }
        [HttpPost]
        public async Task<IActionResult> GetDMTReceipt([FromBody] AppDMTRecieptReq appDMTRecieptReq)
        {
            var appDMTRecieptResp = new AppDMTRecieptResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };
            var appRequest = new AppSessionReq
            {
                APPID = appDMTRecieptReq.APPID,
                IMEI = appDMTRecieptReq.IMEI,
                LoginTypeID = appDMTRecieptReq.LoginTypeID,
                UserID = appDMTRecieptReq.UserID,
                SessionID = appDMTRecieptReq.SessionID,
                RegKey = appDMTRecieptReq.RegKey,
                SerialNo = appDMTRecieptReq.SerialNo,
                Version = appDMTRecieptReq.Version,
                Session = appDMTRecieptReq.Session
            };
            if (appDMTRecieptReq != null)
            {
                var appResp = appML.CheckAppSession(appRequest);
                appDMTRecieptResp.IsAppValid = appResp.IsAppValid;
                appDMTRecieptResp.IsVersionValid = appResp.IsVersionValid;
                appDMTRecieptResp.IsPasswordExpired = appResp.IsPasswordExpired;
                appDMTRecieptResp.Statuscode = appResp.Statuscode;

                if (appResp.Statuscode == ErrorCodes.One)
                {
                    if (!appResp.IsPasswordExpired)
                    {
                        var _req = new CommonReq
                        {
                            LoginID = appDMTRecieptReq.UserID,
                            LoginTypeID = appDMTRecieptReq.LoginTypeID,
                            CommonStr = appDMTRecieptReq.GroupID
                        };
                        ISellerML sellerML = new SellerML(_accessor, _env);
                        await Task.Delay(0).ConfigureAwait(false);
                        appDMTRecieptResp.Statuscode = ErrorCodes.One;
                        appDMTRecieptResp.Msg = ErrorCodes.REQSUC;
                        appDMTRecieptResp.transactionDetail = sellerML.DMRReceiptApp(_req);
                    }
                }
                else
                {
                    appDMTRecieptResp.Msg = appResp.Msg;
                }
            }
            else
            {
                appDMTRecieptResp.Msg = "Incomplete parameter!";
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "GetDMTReceipt",
                CommonStr2 = JsonConvert.SerializeObject(appDMTRecieptReq),
                CommonStr3 = JsonConvert.SerializeObject(appDMTRecieptResp)
            });
            return Json(appDMTRecieptResp);
        }
        #endregion
        #region DMTPipeRelated
        [HttpPost]
        public IActionResult CreateSenderP([FromBody] SenderDetailReq senderDetailReq)
        {
            var appResponse = new AppResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };
            if (senderDetailReq.senderRequest != null)
            {
                appResponse = appML.CheckAppSession(senderDetailReq);
                if (appResponse.Statuscode == ErrorCodes.One)
                {
                    if (!appResponse.IsPasswordExpired)
                    {
                        IMoneyTransferML mtml = new MoneyTransferML(_accessor, _env);
                        var responseStatus = mtml.CreateSender(new MTSenderDetail
                        {
                            RequestMode = RequestMode.APPS,
                            SenderMobile = senderDetailReq.senderRequest.MobileNo,
                            UserID = senderDetailReq.UserID,
                            OutletID = appResponse.GetID,
                            OID = senderDetailReq.OID,
                            FName = senderDetailReq.senderRequest.Name,
                            LName = senderDetailReq.senderRequest.LastName,
                            Address = senderDetailReq.senderRequest.Address,
                            DOB = senderDetailReq.senderRequest.Dob,
                            Pincode = Validate.O.IsNumeric(senderDetailReq.senderRequest.Pincode ?? "") ? Convert.ToInt32(senderDetailReq.senderRequest.Pincode) : 0,
                            OTP = senderDetailReq.senderRequest.OTP
                        });
                        appResponse.Statuscode = responseStatus.Statuscode;
                        appResponse.Msg = responseStatus.Msg;
                        appResponse.SID = responseStatus.ReferenceID;
                        appResponse.IsOTPRequired = responseStatus.IsOTPGenerated;
                        appResponse.IsResendAvailable = responseStatus.IsOTPResendAvailble;
                    }
                }
            }
            else
            {
                appResponse.Msg = "Incomplete parameter!";
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "CreateSenderP",
                CommonStr2 = JsonConvert.SerializeObject(senderDetailReq),
                CommonStr3 = JsonConvert.SerializeObject(appResponse)
            });
            return Json(appResponse);
        }
        [HttpPost]
        public IActionResult GetSenderP([FromBody] SenderDetailReq senderDetailReq)
        {
            var getSenderResp = new AppGetSenderResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };
            if (senderDetailReq.senderRequest != null)
            {
                var appResp = appML.CheckAppSession(senderDetailReq);
                getSenderResp.IsAppValid = appResp.IsAppValid;
                getSenderResp.IsVersionValid = appResp.IsVersionValid;
                getSenderResp.IsPasswordExpired = appResp.IsPasswordExpired;
                getSenderResp.Statuscode = appResp.Statuscode;
                if (appResp.Statuscode == ErrorCodes.One)
                {
                    if (!appResp.IsPasswordExpired)
                    {
                        IMoneyTransferML mtml = new MoneyTransferML(_accessor, _env);
                        var mTResp = mtml.GetSender(new MTCommonRequest
                        {
                            RequestMode = RequestMode.APPS,
                            SenderMobile = senderDetailReq.senderRequest.MobileNo ?? "",
                            UserID = senderDetailReq.UserID,
                            OutletID = appResp.GetID,
                            OID = senderDetailReq.OID
                        });
                        if (mTResp.IsNotActive)
                        {
                            var senderOTP = mtml.SenderResendOTP(new MTOTPRequest
                            {

                                RequestMode = RequestMode.APPS,
                                SenderMobile = senderDetailReq.senderRequest.MobileNo ?? "",
                                UserID = senderDetailReq.UserID,
                                OutletID = appResp.GetID,
                                OID = senderDetailReq.OID,
                                ReferenceID = mTResp.ReferenceID ?? string.Empty
                            });
                            getSenderResp.Statuscode = senderOTP.Statuscode;
                            getSenderResp.Msg = senderOTP.Msg;
                            getSenderResp.IsOTPGenerated = senderOTP.IsOTPGenerated;                            
                            getSenderResp.SID = senderOTP.ReferenceID ?? string.Empty;
                        }
                        else
                        {
                            getSenderResp.Statuscode = mTResp.Statuscode;
                            getSenderResp.Msg = mTResp.Msg;
                            getSenderResp.IsSenderNotExists = mTResp.IsSenderNotExists;
                            getSenderResp.IsEKYCAvailable = mTResp.IsEKYCAvailable;
                            getSenderResp.IsOTPGenerated = mTResp.IsOTPGenerated;                         
                            getSenderResp.SID = mTResp.ReferenceID ?? string.Empty;
                        }
                        getSenderResp.SenderName = mTResp.SenderName;
                        getSenderResp.RemainingLimit = mTResp.RemainingLimit;
                        getSenderResp.AvailbleLimit = mTResp.AvailbleLimit;
                        getSenderResp.IsSenderNotExists = mTResp.IsSenderNotExists;
                        getSenderResp.IsEKYCAvailable = mTResp.IsEKYCAvailable;

                    }
                }
                else
                {
                    getSenderResp.Msg = appResp.Msg;
                }
            }
            else
            {
                getSenderResp.Msg = "Incomplete parameter!";
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "GetSenderP",
                CommonStr2 = JsonConvert.SerializeObject(senderDetailReq),
                CommonStr3 = JsonConvert.SerializeObject(getSenderResp)
            });
            return Json(getSenderResp);
        }
        [HttpPost]
        public IActionResult VerifySenderP([FromBody] SenderDetailReq senderDetailReq)
        {
            var appResp = new AppResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };

            if (senderDetailReq.senderRequest != null)
            {
                appResp = appML.CheckAppSession(senderDetailReq);
                if (appResp.Statuscode == ErrorCodes.One)
                {
                    if (!appResp.IsPasswordExpired)
                    {
                        IMoneyTransferML mtml = new MoneyTransferML(_accessor, _env);
                        var responseStatus = mtml.VerifySender(new MTOTPRequest
                        {
                            RequestMode = RequestMode.APPS,
                            SenderMobile = senderDetailReq.senderRequest.MobileNo ?? "",
                            UserID = senderDetailReq.UserID,
                            OutletID = appResp.GetID,
                            OID = senderDetailReq.OID,
                            ReferenceID = senderDetailReq.SID,
                            OTP = senderDetailReq.senderRequest.OTP
                        });
                        appResp.Statuscode = responseStatus.Statuscode;
                        appResp.Msg = responseStatus.Msg;
                    }
                }
            }
            else
            {
                appResp.Msg = "Incomplete parameter!";
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "VerifySenderP",
                CommonStr2 = JsonConvert.SerializeObject(senderDetailReq),
                CommonStr3 = JsonConvert.SerializeObject(appResp)
            });
            return Json(appResp);
        }
        [HttpPost]
        public IActionResult AddBeneficiaryP([FromBody] AddBeneRequest addBeneRequest)
        {
            var appResponse = new AppResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };

            if (addBeneRequest.senderRequest != null)
            {
                appResponse = appML.CheckAppSession(addBeneRequest);
                if (appResponse.Statuscode == ErrorCodes.One)
                {
                    if (!appResponse.IsPasswordExpired)
                    {

                        IMoneyTransferML mtml = new MoneyTransferML(_accessor, _env);
                        var responseStatus = mtml.CreateBeneficiary(new MTBeneficiaryAddRequest
                        {
                            RequestMode = RequestMode.APPS,
                            SenderMobile = addBeneRequest.senderRequest.MobileNo,
                            UserID = addBeneRequest.UserID,
                            OutletID = appResponse.GetID,
                            OID = addBeneRequest.OID,
                            ReferenceID = addBeneRequest.SID,
                            BeneDetail = new MBeneDetail
                            {
                                AccountNo = addBeneRequest.beneDetail.AccountNo,
                                BankID = addBeneRequest.beneDetail.BankID,
                                BankName = addBeneRequest.beneDetail.BankName,
                                BeneName = addBeneRequest.beneDetail.BeneName,
                                IFSC = addBeneRequest.beneDetail.IFSC,
                                MobileNo = addBeneRequest.beneDetail.MobileNo,
                                TransMode = addBeneRequest.beneDetail.TransMode//1 IMPS,2 NEFT
                            },
                        });
                        appResponse.Statuscode = responseStatus.Statuscode;
                        appResponse.Msg = responseStatus.Msg;
                        appResponse.SID = responseStatus.ReferenceID;
                        appResponse.IsOTPRequired = responseStatus.IsOTPGenerated;
                    }
                }
            }
            else
            {
                appResponse.Msg = "Incomplete parameter!";
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "AddBeneficiaryP",
                CommonStr2 = JsonConvert.SerializeObject(addBeneRequest),
                CommonStr3 = JsonConvert.SerializeObject(appResponse)
            });
            return Json(appResponse);
        }
        [HttpPost]
        public IActionResult DeleteBeneficiaryP([FromBody] AddBeneRequest addBeneRequest)
        {
            var appResponse = new AppResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };

            if (addBeneRequest.senderRequest != null)
            {
                appResponse = appML.CheckAppSession(addBeneRequest);
                if (appResponse.Statuscode == ErrorCodes.One)
                {
                    if (!appResponse.IsPasswordExpired)
                    {
                        IMoneyTransferML mtml = new MoneyTransferML(_accessor, _env);
                        var responseStatus = mtml.RemoveBeneficiary(new MBeneVerifyRequest
                        {
                            RequestMode = RequestMode.APPS,
                            SenderMobile = addBeneRequest.senderRequest.MobileNo ?? string.Empty,
                            UserID = addBeneRequest.UserID,
                            OutletID = appResponse.GetID,
                            OID = addBeneRequest.OID,
                            ReferenceID = addBeneRequest.SID,
                            OTP = addBeneRequest.OTP,
                            BeneficiaryID = addBeneRequest.beneDetail.BeneID
                        });
                        appResponse.Statuscode = responseStatus.Statuscode;
                        appResponse.Msg = responseStatus.Msg;
                        appResponse.IsOTPRequired = responseStatus.IsOTPGenerated;
                    }
                }
            }
            else
            {
                appResponse.Msg = "Incomplete parameter!";
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "DeleteBeneficiary",
                CommonStr2 = JsonConvert.SerializeObject(addBeneRequest),
                CommonStr3 = JsonConvert.SerializeObject(appResponse)
            });
            return Json(appResponse);
        }
        [HttpPost]
        public IActionResult GetBeneficiaryP([FromBody] SenderDetailReq senderDetailReq)
        {
            var appBeneficiaryResp = new AppBeneficiaryResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };
            if (senderDetailReq.senderRequest != null)
            {
                var appResp = appML.CheckAppSession(senderDetailReq);
                appBeneficiaryResp.IsAppValid = appResp.IsAppValid;
                appBeneficiaryResp.IsVersionValid = appResp.IsVersionValid;
                appBeneficiaryResp.IsPasswordExpired = appResp.IsPasswordExpired;
                appBeneficiaryResp.Statuscode = appResp.Statuscode;
                if (appResp.Statuscode == ErrorCodes.One)
                {
                    if (!appResp.IsPasswordExpired)
                    {
                        IMoneyTransferML mtml = new MoneyTransferML(_accessor, _env);
                        var responseStatus = mtml.GetBeneficiary(new MTCommonRequest
                        {
                            RequestMode = RequestMode.PANEL,
                            SenderMobile = senderDetailReq.senderRequest.MobileNo,
                            UserID = senderDetailReq.UserID,
                            OutletID = appResp.GetID,
                            OID = senderDetailReq.OID,
                            ReferenceID = senderDetailReq.SID
                        });
                        appBeneficiaryResp.Statuscode = responseStatus.Statuscode;
                        appBeneficiaryResp.Msg = responseStatus.Msg;
                        appBeneficiaryResp.Beneficiaries = responseStatus.Beneficiaries;
                    }
                }
                else
                {
                    appBeneficiaryResp.Msg = appResp.Msg;
                }
            }
            else
            {
                appBeneficiaryResp.Msg = "Incomplete parameter!";
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "GetBeneficiaryP",
                CommonStr2 = JsonConvert.SerializeObject(senderDetailReq),
                CommonStr3 = JsonConvert.SerializeObject(appBeneficiaryResp)
            });
            return Json(appBeneficiaryResp);
        }
        [HttpPost]
        public IActionResult GenerateBenficiaryOTPP([FromBody] SenderDetailReq senderDetailReq)
        {
            var getSenderResp = new AppGetSenderResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };
            if (senderDetailReq.senderRequest != null)
            {
                var appResp = appML.CheckAppSession(senderDetailReq);
                getSenderResp.IsAppValid = appResp.IsAppValid;
                getSenderResp.IsVersionValid = appResp.IsVersionValid;
                getSenderResp.IsPasswordExpired = appResp.IsPasswordExpired;
                getSenderResp.Statuscode = appResp.Statuscode;
                if (appResp.Statuscode == ErrorCodes.One)
                {
                    if (!appResp.IsPasswordExpired)
                    {

                        IMoneyTransferML mtml = new MoneyTransferML(_accessor, _env);
                        var responseStatus = mtml.GenerateOTP(new MTBeneficiaryAddRequest
                        {
                            RequestMode = RequestMode.APPS,
                            SenderMobile = senderDetailReq.senderRequest.MobileNo ?? "",
                            UserID = senderDetailReq.UserID,
                            OutletID = appResp.GetID,
                            OID = senderDetailReq.OID,
                            ReferenceID = senderDetailReq.SID,
                            BeneDetail = new MBeneDetail
                            {
                                BeneID = senderDetailReq.BeneID
                            }
                        });
                        getSenderResp.Statuscode = responseStatus.Statuscode;
                        getSenderResp.Msg = responseStatus.Msg;
                        getSenderResp.SID = responseStatus.ReferenceID ?? string.Empty;
                    }
                }
                else
                {
                    getSenderResp.Msg = appResp.Msg;
                }
            }
            else
            {
                getSenderResp.Msg = "Incomplete parameter!";
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "GenerateBenficiaryOTPP",
                CommonStr2 = JsonConvert.SerializeObject(senderDetailReq),
                CommonStr3 = JsonConvert.SerializeObject(getSenderResp)
            });
            return Json(getSenderResp);
        }
        [HttpPost]
        public IActionResult ValidateBeneficiaryP([FromBody] SenderDetailReq senderDetailReq)
        {
            var appResp = new AppResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };
            if (senderDetailReq.senderRequest != null)
            {
                appResp = appML.CheckAppSession(senderDetailReq);
                if (appResp.Statuscode == ErrorCodes.One)
                {
                    if (!appResp.IsPasswordExpired)
                    {
                        IMoneyTransferML mtml = new MoneyTransferML(_accessor, _env);
                        var responseStatus = mtml.ValidateBeneficiary(new MBeneVerifyRequest
                        {
                            RequestMode = RequestMode.PANEL,
                            SenderMobile = senderDetailReq.senderRequest.MobileNo,
                            UserID = senderDetailReq.UserID,
                            OutletID = appResp.GetID,
                            OID = senderDetailReq.OID,
                            MobileNo = senderDetailReq.BeneMobile,
                            AccountNo = senderDetailReq.AccountNo,
                            OTP = senderDetailReq.OTP,
                            ReferenceID = senderDetailReq.SID,
                            BeneficiaryID = senderDetailReq.BeneID
                        });
                        appResp.Statuscode = responseStatus.Statuscode;
                        appResp.Msg = responseStatus.Msg;
                    }
                }
            }
            else
            {
                appResp.Msg = "Incomplete parameter!";
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "ValidateBeneficiaryP",
                CommonStr2 = JsonConvert.SerializeObject(senderDetailReq),
                CommonStr3 = JsonConvert.SerializeObject(appResp)
            });
            return Json(appResp);
        }
        [HttpPost]
        public IActionResult VerifyAccountP([FromBody] AddBeneRequest addBeneRequest)
        {
            var dMRTransactionResponse = new _DMRTransactionResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };
            var appRequest = new AppSessionReq
            {
                APPID = addBeneRequest.APPID,
                IMEI = addBeneRequest.IMEI,
                LoginTypeID = addBeneRequest.LoginTypeID,
                UserID = addBeneRequest.UserID,
                SessionID = addBeneRequest.SessionID,
                RegKey = addBeneRequest.RegKey,
                SerialNo = addBeneRequest.SerialNo,
                Version = addBeneRequest.Version,
                Session = addBeneRequest.Session
            };
            if (addBeneRequest.senderRequest != null)
            {
                var appResp = appML.CheckAppSession(appRequest);
                dMRTransactionResponse.IsAppValid = appResp.IsAppValid;
                dMRTransactionResponse.IsVersionValid = appResp.IsVersionValid;
                dMRTransactionResponse.IsPasswordExpired = appResp.IsPasswordExpired;
                dMRTransactionResponse.Statuscode = appResp.Statuscode;
                if (appResp.Statuscode == ErrorCodes.One)
                {
                    if (!appResp.IsPasswordExpired)
                    {
                        IMoneyTransferML mtml = new MoneyTransferML(_accessor, _env);
                        var resp = mtml.VerifyAccount(new MBeneVerifyRequest
                        {
                            RequestMode = RequestMode.APPS,
                            SenderMobile = addBeneRequest.senderRequest.MobileNo,
                            UserID = addBeneRequest.UserID,
                            OutletID = appResp.GetID,
                            OID = addBeneRequest.OID,
                            AccountNo = addBeneRequest.beneDetail.AccountNo,
                            IFSC = addBeneRequest.beneDetail.IFSC,
                            BankID = addBeneRequest.beneDetail.BankID,
                            Bank = addBeneRequest.beneDetail.BankName,
                            BeneficiaryName = addBeneRequest.beneDetail.BeneName,
                            TransMode = addBeneRequest.TransMode ?? "IMPS",
                            ReferenceID = addBeneRequest.SID
                        });

                        dMRTransactionResponse.Status = RechargeRespType.GetRechargeStatusText(resp.Statuscode);
                        dMRTransactionResponse.Msg = resp.Msg;
                        dMRTransactionResponse.Statuscode = resp.Statuscode;
                        dMRTransactionResponse.TransactionID = resp.TransactionID;
                        dMRTransactionResponse.BeneName = resp.BeneName;
                    }
                }
                else
                {
                    dMRTransactionResponse.Msg = appResp.Msg;
                }
            }
            else
            {
                dMRTransactionResponse.Msg = "Incomplete parameter!";
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "VerifyAccountP",
                CommonStr2 = JsonConvert.SerializeObject(addBeneRequest),
                CommonStr3 = JsonConvert.SerializeObject(dMRTransactionResponse)
            });
            return Json(dMRTransactionResponse);
        }
        [HttpPost]
        public IActionResult SendMoneyP([FromBody] AppSendMoneyReq appSendMoneyReq)
        {
            var dMRTransactionResponse = new DMRTransactionResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };
            var appRequest = new AppSessionReq
            {
                APPID = appSendMoneyReq.APPID,
                IMEI = appSendMoneyReq.IMEI,
                LoginTypeID = appSendMoneyReq.LoginTypeID,
                UserID = appSendMoneyReq.UserID,
                SessionID = appSendMoneyReq.SessionID,
                RegKey = appSendMoneyReq.RegKey,
                SerialNo = appSendMoneyReq.SerialNo,
                Version = appSendMoneyReq.Version,
                Session = appSendMoneyReq.Session
            };
            if (appSendMoneyReq != null && appSendMoneyReq.reqSendMoney != null)
            {
                var appResp = appML.CheckAppSession(appRequest);
                if (appResp.Statuscode == ErrorCodes.One)
                {

                    appSendMoneyReq.reqSendMoney.SecKey = appSendMoneyReq.SecurityKey ?? "";
                    IDmtML dmtML = new DmtML(_accessor, _env);
                    var _cResp = dmtML.CheckRepeativeTransaction(appSendMoneyReq.SessionID, appSendMoneyReq.UserID, appSendMoneyReq.reqSendMoney.Amount, appSendMoneyReq.reqSendMoney.AccountNo, 0);
                    if (_cResp.Statuscode > 0)
                    {
                        IMoneyTransferML mtml = new MoneyTransferML(_accessor, _env);
                        var resp = mtml.AccountTransfer(new MBeneVerifyRequest
                        {
                            RequestMode = RequestMode.APPS,
                            SenderMobile = appSendMoneyReq.reqSendMoney.MobileNo,
                            UserID = appSendMoneyReq.UserID,
                            OutletID = appResp.GetID,
                            OID = appSendMoneyReq.reqSendMoney.o,
                            AccountNo = appSendMoneyReq.reqSendMoney.AccountNo,
                            IFSC = appSendMoneyReq.reqSendMoney.IFSC,
                            BankID = appSendMoneyReq.reqSendMoney.BankID,
                            Bank = appSendMoneyReq.reqSendMoney.Bank,
                            Amount = appSendMoneyReq.reqSendMoney.Amount,
                            BeneficiaryName = appSendMoneyReq.reqSendMoney.BeneName,
                            TransMode = appSendMoneyReq.reqSendMoney.Channel ? "IMPS" : "NEFT",
                            SecureKey = appSendMoneyReq.reqSendMoney.SecKey,
                            ReferenceID = appSendMoneyReq.reqSendMoney.RefferenceID,
                            BeneficiaryID = appSendMoneyReq.reqSendMoney.BeneID
                        });
                        dMRTransactionResponse.Statuscode = resp.Statuscode;
                        dMRTransactionResponse.Status = RechargeRespType.GetRechargeStatusText(resp.Statuscode);
                        dMRTransactionResponse.Msg = resp.Msg;
                        dMRTransactionResponse.TID = resp.TID;
                        dMRTransactionResponse.TransactionID = resp.TransactionID;
                        dMRTransactionResponse.GroupID = resp.GroupID;
                        if (resp.Statuscode == RechargeRespType.FAILED && _cResp.Statuscode > ErrorCodes.One)
                        {
                            dmtML.CheckRepeativeTransaction(appSendMoneyReq.SessionID, appSendMoneyReq.UserID, appSendMoneyReq.reqSendMoney.Amount, appSendMoneyReq.reqSendMoney.AccountNo, _cResp.Statuscode);
                        }
                    }
                    else
                    {
                        dMRTransactionResponse.Statuscode = _cResp.Statuscode;
                        dMRTransactionResponse.Msg = _cResp.Msg;
                    }
                }
                else
                {
                    dMRTransactionResponse.Msg = appResp.Msg;
                }
            }
            else
            {
                dMRTransactionResponse.Msg = "Incomplete parameter!";
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "SendMoneyP",
                CommonStr2 = JsonConvert.SerializeObject(appSendMoneyReq),
                CommonStr3 = JsonConvert.SerializeObject(dMRTransactionResponse)
            });
            return Json(dMRTransactionResponse);
        }
        [HttpPost]
        public IActionResult GetChargedAmountP([FromBody] AppAmountRequest appAmountRequest)
        {
            var appVerificationCharge = new AppVerificationChargeResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };
            var appRequest = new AppSessionReq
            {
                APPID = appAmountRequest.APPID,
                IMEI = appAmountRequest.IMEI,
                LoginTypeID = appAmountRequest.LoginTypeID,
                UserID = appAmountRequest.UserID,
                SessionID = appAmountRequest.SessionID,
                RegKey = appAmountRequest.RegKey,
                SerialNo = appAmountRequest.SerialNo,
                Version = appAmountRequest.Version,
                Session = appAmountRequest.Session
            };
            if (appAmountRequest != null)
            {
                var appResp = appML.CheckAppSession(appRequest);
                appVerificationCharge.IsAppValid = appResp.IsAppValid;
                appVerificationCharge.IsVersionValid = appResp.IsVersionValid;
                if (appResp.Statuscode == ErrorCodes.One)
                {

                    IMoneyTransferML mtml = new MoneyTransferML(_accessor, _env);
                    var resp = mtml.GetCharge(new MTGetChargeRequest
                    {
                        RequestMode = RequestMode.PANEL,
                        UserID = appAmountRequest.UserID,
                        OutletID = appResp.GetID,
                        OID = appAmountRequest.OID,
                        Amount = appAmountRequest.Amount
                    });
                    appVerificationCharge.Statuscode = resp.Statuscode;
                    appVerificationCharge.Msg = resp.Msg;
                    appVerificationCharge.ChargedAmount = resp.Charged;
                }
                else
                {
                    appVerificationCharge.Msg = appResp.Msg;
                }
            }
            else
            {
                appVerificationCharge.Msg = "Incomplete parameter!";
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "GetChargedAmountP",
                CommonStr2 = JsonConvert.SerializeObject(appAmountRequest),
                CommonStr3 = JsonConvert.SerializeObject(appVerificationCharge)
            });
            return Json(appVerificationCharge);
        }

        #endregion

        #region UPIPayment
        [HttpPost]
        public IActionResult DoUPIPayment([FromBody] AppSendMoneyReq appSendMoneyReq)
        {
            var dMRTransactionResponse = new DMRTransactionResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };
            var appRequest = new AppSessionReq
            {
                APPID = appSendMoneyReq.APPID,
                IMEI = appSendMoneyReq.IMEI,
                LoginTypeID = appSendMoneyReq.LoginTypeID,
                UserID = appSendMoneyReq.UserID,
                SessionID = appSendMoneyReq.SessionID,
                RegKey = appSendMoneyReq.RegKey,
                SerialNo = appSendMoneyReq.SerialNo,
                Version = appSendMoneyReq.Version,
                Session = appSendMoneyReq.Session
            };
            if (appSendMoneyReq != null && appSendMoneyReq.reqSendMoney != null)
            {
                var appResp = appML.CheckAppSession(appRequest);
                if (appResp.Statuscode == ErrorCodes.One)
                {
                    IDmtML dmtML = new DmtML(_accessor, _env);
                    var _cResp = dmtML.CheckRepeativeTransaction(appSendMoneyReq.SessionID, appSendMoneyReq.UserID, appSendMoneyReq.reqSendMoney.Amount, appSendMoneyReq.reqSendMoney.AccountNo, 0);
                    if (_cResp.Statuscode > 0)
                    {
                        IMoneyTransferML mtml = new MoneyTransferML(_accessor, _env);
                        var resp = mtml.DoUPIPaymentService(new MBeneVerifyRequest
                        {
                            RequestMode = RequestMode.APPS,
                            UserID = appSendMoneyReq.UserID,
                            AccountNo = appSendMoneyReq.reqSendMoney.AccountNo,
                            Amount = appSendMoneyReq.reqSendMoney.Amount,
                            BeneficiaryName = appSendMoneyReq.reqSendMoney.BeneName
                        });
                        dMRTransactionResponse.Statuscode = resp.Statuscode;
                        dMRTransactionResponse.Status = RechargeRespType.GetRechargeStatusText(resp.Statuscode);
                        dMRTransactionResponse.Msg = resp.Msg;
                        dMRTransactionResponse.TID = resp.TID;
                        dMRTransactionResponse.TransactionID = resp.TransactionID;
                        dMRTransactionResponse.GroupID = resp.GroupID;
                        if (resp.Statuscode == RechargeRespType.FAILED && _cResp.Statuscode > ErrorCodes.One)
                        {
                            dmtML.CheckRepeativeTransaction(appSendMoneyReq.SessionID, appSendMoneyReq.UserID, appSendMoneyReq.reqSendMoney.Amount, appSendMoneyReq.reqSendMoney.AccountNo, _cResp.Statuscode);
                        }
                    }
                    else
                    {
                        dMRTransactionResponse.Statuscode = _cResp.Statuscode;
                        dMRTransactionResponse.Msg = _cResp.Msg;
                    }
                }
                else
                {
                    dMRTransactionResponse.Msg = appResp.Msg;
                }
            }
            else
            {
                dMRTransactionResponse.Msg = "Incomplete parameter!";
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "DoUPIPayment",
                CommonStr2 = JsonConvert.SerializeObject(appSendMoneyReq),
                CommonStr3 = JsonConvert.SerializeObject(dMRTransactionResponse)
            });
            return Json(dMRTransactionResponse);
        }
        #endregion
    }
}
