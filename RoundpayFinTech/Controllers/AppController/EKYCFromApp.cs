using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using RoundpayFinTech.AppCode;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.MiddleLayer;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.App;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.Controllers
{
    public partial class AppController
    {
        [HttpPost]
        public IActionResult GetEKYCDetail([FromBody] AppSessionReq appSessionReq)
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
                appResponseData.Msg = appResp.Msg;
                if (appResp.Statuscode == ErrorCodes.One)
                {
                    if (!appResp.IsPasswordExpired)
                    {
                        IEKYCML userML = new EKYCML(_accessor, _env);
                        var details = userML.GetEKYCDetailOfUser(new CommonReq
                        {
                            LoginID = appSessionReq.UserID
                        });
                        if (!string.IsNullOrEmpty(details.GSTAuthorizedSignatory))
                        {
                            if (details.GSTAuthorizedSignatory.Split(',').Length > 0)
                            {
                                details.Directors = new SelectList(details.GSTAuthorizedSignatory.Split(',').Select(x => new { value = x.Trim() }), "value", "value");
                            }
                        }
                        UserML userML1 = new UserML(_accessor, _env, false);
                        var CompanyType = userML1.GetCompanyTypeMaster(new CommonReq
                        {
                            LoginTypeID = LoginType.ApplicationUser,
                            LoginID = appSessionReq.UserID
                        });
                        if (CompanyType.Any())
                        {
                            details.CompanyTypeSelect = new SelectList(CompanyType, "ID", "CompanyName");
                        }
                        appResponseData.data = details;
                    }
                }
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "GetEKYCDetail",
                CommonStr2 = JsonConvert.SerializeObject(appSessionReq),
                CommonStr3 = JsonConvert.SerializeObject(appResponseData)
            });
            return Json(appResponseData);
        }
        [HttpPost]
        public IActionResult ValidateGSTINForEKYC([FromBody] AppEKYCEditRequest appSessionReq)
        {
            var appResponseData = new AppResponse
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
                appResponseData.Msg = appResp.Msg;
                if (appResp.Statuscode == ErrorCodes.One)
                {
                    if (!appResp.IsPasswordExpired)
                    {
                        if (appSessionReq.IsConcent == false && appSessionReq.IsSkip == false)
                        {
                            appResponseData.Statuscode = ErrorCodes.Minus1;
                            appResponseData.Msg = "Please select consent";
                        }
                        if (appResponseData.Statuscode == ErrorCodes.One)
                        {
                            IEKYCML ml = new EKYCML(_accessor, _env);
                            var res = ml.ValidateGST(new EKYCRequestModel
                            {
                                UserID = appSessionReq.UserID,
                                VerificationAccount = appSessionReq.VerificationAccount,
                                IsSkip = appSessionReq.IsSkip,
                                CompanyTypeID=appSessionReq.CompanyTypeID
                            });
                            appResponseData.Statuscode = res.Statuscode;
                            appResponseData.Msg = res.Msg;
                        }
                    }
                }
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "ValidateGSTINForEKYC",
                CommonStr2 = JsonConvert.SerializeObject(appSessionReq),
                CommonStr3 = JsonConvert.SerializeObject(appResponseData)
            });
            return Json(appResponseData);
        }

        [HttpPost]
        public IActionResult ValidateAadharForEKYC([FromBody] AppEKYCEditRequest appSessionReq)
        {
            var appResponseData = new AppResponse
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
                appResponseData.Msg = appResp.Msg;
                if (appResp.Statuscode == ErrorCodes.One)
                {
                    if (!appResp.IsPasswordExpired)
                    {
                        if (appSessionReq.IsConcent == false && appSessionReq.IsSkip == false)
                        {
                            appResponseData.Statuscode = ErrorCodes.Minus1;
                            appResponseData.Msg = "Please select consent";
                        }
                        if (appResponseData.Statuscode == ErrorCodes.One)
                        {
                            IEKYCML ml = new EKYCML(_accessor, _env);
                            var res = ml.GenerateAadharOTP(new EKYCRequestModel
                            {
                                UserID = appSessionReq.UserID,
                                VerificationAccount = appSessionReq.VerificationAccount,
                                DirectorName = appSessionReq.Director
                            });
                            appResponseData.Statuscode = res.Statuscode;
                            appResponseData.Msg = res.Msg;
                            appResponseData.RID = res.ReferenceID;
                        }
                    }
                }
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "ValidateAadharForEKYC",
                CommonStr2 = JsonConvert.SerializeObject(appSessionReq),
                CommonStr3 = JsonConvert.SerializeObject(appResponseData)
            });
            return Json(appResponseData);
        }

        [HttpPost]
        public IActionResult ValidateAadharOTPForEKYC([FromBody] AppEKYCEditRequest appSessionReq)
        {
            var appResponseData = new AppResponse
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
                appResponseData.Msg = appResp.Msg;
                if (appResp.Statuscode == ErrorCodes.One)
                {
                    if (!appResp.IsPasswordExpired)
                    {
                        IEKYCML ml = new EKYCML(_accessor, _env);
                        var res = ml.ValidateAadharOTP(new EKYCRequestModel
                        {
                            UserID = appSessionReq.UserID,
                            OTP = appSessionReq.OTP,
                            ReferenceID = appSessionReq.ReffID
                        });
                        appResponseData.Statuscode = res.Statuscode;
                        appResponseData.Msg = res.Msg;
                    }
                }
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "ValidateAadharOTPForEKYC",
                CommonStr2 = JsonConvert.SerializeObject(appSessionReq),
                CommonStr3 = JsonConvert.SerializeObject(appResponseData)
            });
            return Json(appResponseData);
        }

        [HttpPost]
        public IActionResult ValidatePAN([FromBody] AppEKYCEditRequest appSessionReq)
        {
            var appResponseData = new AppResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            var appResp = appML.CheckAppSession(appSessionReq);
            appResponseData.IsAppValid = appResp.IsAppValid;
            appResponseData.IsVersionValid = appResp.IsVersionValid;
            appResponseData.IsPasswordExpired = appResp.IsPasswordExpired;
            appResponseData.Statuscode = appResp.Statuscode;
            appResponseData.Msg = appResp.Msg;
            if (appResp.Statuscode == ErrorCodes.One)
            {
                if (!appResp.IsPasswordExpired)
                {
                    if (appSessionReq.IsConcent == false && appSessionReq.IsSkip == false)
                    {
                        appResponseData.Statuscode = ErrorCodes.Minus1;
                        appResponseData.Msg = "Please select consent";
                    }
                    if (appResponseData.Statuscode == ErrorCodes.One)
                    {
                        IEKYCML ml = new EKYCML(_accessor, _env);
                        var res = ml.GetPanDetail(new EKYCRequestModel
                        {
                            UserID = appSessionReq.UserID,
                            VerificationAccount = appSessionReq.VerificationAccount,
                            DirectorName = appSessionReq.Director
                        });
                        appResponseData.Statuscode = res.Statuscode;
                        appResponseData.Msg = res.Msg;
                    }
                }
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "ValidatePAN",
                CommonStr2 = JsonConvert.SerializeObject(appSessionReq),
                CommonStr3 = JsonConvert.SerializeObject(appResponseData)
            });
            return Json(appResponseData);
        }
        [HttpPost]
        public IActionResult ValidateBankAccount([FromBody] AppEKYCEditRequest appSessionReq)
        {
            var appResponseData = new AppResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            var appResp = appML.CheckAppSession(appSessionReq);
            appResponseData.IsAppValid = appResp.IsAppValid;
            appResponseData.IsVersionValid = appResp.IsVersionValid;
            appResponseData.IsPasswordExpired = appResp.IsPasswordExpired;
            appResponseData.Statuscode = appResp.Statuscode;
            appResponseData.Msg = appResp.Msg;
            if (appResp.Statuscode == ErrorCodes.One)
            {
                if (!appResp.IsPasswordExpired)
                {
                    if (appSessionReq.IsConcent == false && appSessionReq.IsSkip == false)
                    {
                        appResponseData.Statuscode = ErrorCodes.Minus1;
                        appResponseData.Msg = "Please select consent";
                    }
                    if (appResponseData.Statuscode == ErrorCodes.One)
                    {
                        IEKYCML ml = new EKYCML(_accessor, _env);
                        var res = ml.GetBankAccountDetail(new EKYCRequestModel
                        {
                            UserID = appSessionReq.UserID,
                            VerificationAccount = appSessionReq.VerificationAccount,
                            IFSC = appSessionReq.IFSC,
                            BankID = appSessionReq.BankID,
                            RequestMode = RequestMode.APPS
                        });
                        appResponseData.Statuscode = res.Statuscode;
                        appResponseData.Msg = res.Msg;
                    }
                }
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "ValidateBankAccount",
                CommonStr2 = JsonConvert.SerializeObject(appSessionReq),
                CommonStr3 = JsonConvert.SerializeObject(appResponseData)
            });
            return Json(appResponseData);
        }
        [HttpPost]
        public IActionResult EditEKYCStep([FromBody] AppEKYCEditRequest appSessionReq)
        {
            var appResponseData = new AppResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            var appResp = appML.CheckAppSession(appSessionReq);
            appResponseData.IsAppValid = appResp.IsAppValid;
            appResponseData.IsVersionValid = appResp.IsVersionValid;
            appResponseData.IsPasswordExpired = appResp.IsPasswordExpired;
            appResponseData.Statuscode = appResp.Statuscode;
            appResponseData.Msg = appResp.Msg;
            if (appResp.Statuscode == ErrorCodes.One)
            {
                if (!appResp.IsPasswordExpired)
                {
                    IEKYCML ml = new EKYCML(_accessor, _env);
                    var res = ml.EditEKYCStep(new EKYCRequestModel
                    {
                        UserID = appSessionReq.UserID,
                        EditStepID = appSessionReq.StepID
                    });
                    appResponseData.Statuscode = res.Statuscode;
                    appResponseData.Msg = res.Msg;
                }
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "EditEKYCStep",
                CommonStr2 = JsonConvert.SerializeObject(appSessionReq),
                CommonStr3 = JsonConvert.SerializeObject(appResponseData)
            });
            return Json(appResponseData);
        }
    }
}
