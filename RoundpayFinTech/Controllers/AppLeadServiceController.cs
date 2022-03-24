using Fintech.AppCode;
using Fintech.AppCode.Configuration;
using Fintech.AppCode.HelperClass;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using QRCoder;
using RoundpayFinTech.AppCode;
using RoundpayFinTech.AppCode.Configuration;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.MiddleLayer;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.App;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Reports.Filter;
using RoundpayFinTech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.StaticModel.MoneyTransfer;
using RoundpayFinTech.AppCode.ThirdParty.CyberPlate;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Validators;

namespace RoundpayFinTech.Controllers
{
    public partial class AppController : Controller
    {
        #region LeadService
        [HttpPost]
        public IActionResult TypesForLeadService([FromBody] AppSessionReq Request)
        {
            var Loantyperesp = new LeadServiceTypeModel
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };

            var appResp = appML.CheckAppSession(Request);
            Loantyperesp.IsAppValid = appResp.IsAppValid;
            Loantyperesp.IsVersionValid = appResp.IsVersionValid;
            Loantyperesp.IsPasswordExpired = appResp.IsPasswordExpired;
            Loantyperesp.Statuscode = appResp.Statuscode;
            if (appResp.Statuscode == ErrorCodes.One)
            {
                if (!appResp.IsPasswordExpired)
                {
                    ILeadServiceML ml = new LeadServiceML(_accessor, _env, false);
                    //IBankML bankml = new BankML(_accessor, _env, false);
                    Loantyperesp.loanTypes = ml.GetLoanType();
                    Loantyperesp.customerTypes = ml.GetCustomerType();
                    Loantyperesp.insuranceTypes = ml.GetInsuranceTypes();
                    //Loantyperesp.CreditCardBanks = bankml.GetCrediCardBanks();
                }
            }
            else
            {
                Loantyperesp.Msg = appResp.Msg;
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "TypesForLeadService",
                CommonStr2 = JsonConvert.SerializeObject(Request),
                CommonStr3 = JsonConvert.SerializeObject(Loantyperesp)
            });
            return Json(Loantyperesp);

        }

        [HttpPost]
        public IActionResult SaveLeadService([FromBody] LeadAppServiceReq leadAppService)
        {
            var responseStatus = new AppResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };
            if (leadAppService.leadService != null)
            {
                var appRequest = new AppSessionReq
                {
                    APPID = leadAppService.APPID ?? "",
                    IMEI = leadAppService.IMEI ?? "",
                    LoginTypeID = leadAppService.LoginTypeID,
                    UserID = leadAppService.UserID,
                    SessionID = leadAppService.SessionID,
                    RegKey = leadAppService.RegKey ?? "",
                    SerialNo = leadAppService.SerialNo ?? "",
                    Version = leadAppService.Version ?? "",
                    Session = leadAppService.Session
                };
                var appResp = appML.CheckAppSession(appRequest);
                responseStatus.IsAppValid = appResp.IsAppValid;
                responseStatus.IsVersionValid = appResp.IsVersionValid;
                responseStatus.IsPasswordExpired = appResp.IsPasswordExpired;
                responseStatus.Statuscode = appResp.Statuscode;
                if (appResp.Statuscode == ErrorCodes.One)
                {
                    if (!appResp.IsPasswordExpired)
                    {
                        ILeadServiceML ml = new LeadServiceML(_accessor, _env, false);
                        if (!Validate.O.IsMobile(leadAppService.leadService.Mobile))
                        {
                            responseStatus.Statuscode = -1;
                            responseStatus.Msg = "Invalid Mobile No.";
                            return Json(responseStatus);
                        }

                        if (!String.IsNullOrEmpty(leadAppService.leadService.Email) && !Validate.O.IsEmail(leadAppService.leadService.Email))
                        {
                            responseStatus.Statuscode = -1;
                            responseStatus.Msg = "Invalid Email Id.";
                            return Json(responseStatus);
                        }
                        if (!String.IsNullOrEmpty(leadAppService.leadService.PAN) && !Validate.O.IsPAN(leadAppService.leadService.PAN))
                        {
                            responseStatus.Statuscode = -1;
                            responseStatus.Msg = "Invalid PAN";
                            return Json(responseStatus);
                        }
                        if (!String.IsNullOrEmpty(leadAppService.leadService.PinCode) && !Validate.O.IsPinCode(leadAppService.leadService.PinCode))
                        {
                            responseStatus.Statuscode = -1;
                            responseStatus.Msg = "Invalid Pin Code";
                           return Json(responseStatus); ;
                        }
                        var _req = new LeadServiceRequest
                        {
                            LT = 1,
                            Name = leadAppService.leadService.Name,
                            Email = leadAppService.leadService.Email,
                            Mobile = leadAppService.leadService.Mobile,
                            Age = leadAppService.leadService.Age,
                            PAN = leadAppService.leadService.PAN,
                            LoanTypeID = leadAppService.leadService.LoanTypeID,
                            InsuranceTypeID = leadAppService.leadService.InsuranceTypeID,
                            Amount = leadAppService.leadService.Amount,
                            CustomerTypeID = leadAppService.leadService.CustomerTypeID,
                            RequiredFor = leadAppService.leadService.RequiredFor,
                            Comments = leadAppService.leadService.Comments,
                            Remark = leadAppService.leadService.Remark,
                            EntryBy = leadAppService.UserID,
                            RequestModeID = RequestMode.APPS,
                            OID = leadAppService.leadService.OID,
                            BankID= leadAppService.leadService.BankID,
                            HaveLoan = leadAppService.leadService.HaveLoan,
                            OccupationType = leadAppService.leadService.OccupationType,
                            PinCode = leadAppService.leadService.PinCode
                            
                        
                        };
                        var mlRes = ml.SaveLeadServiceApp(_req);
                        responseStatus.Statuscode = mlRes.Statuscode;
                        responseStatus.Msg = mlRes.Msg;
                    }
                    else
                    {
                        responseStatus.Msg = appResp.Msg;
                    }
                }
            }
            else
            {
                responseStatus.Msg = "Incomplete parameter!";
            }
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "SaveLeadService",
                CommonStr2 = JsonConvert.SerializeObject(leadAppService),
                CommonStr3 = JsonConvert.SerializeObject(responseStatus)
            });
            return Json(responseStatus);
        }
        //[HttpPost]
        //public IActionResult GetLeadServiceRequest([FromBody] LeadAppServiceReq LeadRequest)
        //{
        //    ILeadServiceML ml = new LeadServiceML(_accessor, _env,false);
        //    var responseStatus = new AppResponse
        //    {
        //        Statuscode = ErrorCodes.Minus1,
        //        Msg = ErrorCodes.InvaildSession
        //    };
        //    if (LeadRequest.leadServiceRequest != null)
        //    {
        //        var appRequest = new AppSessionReq
        //        {
        //            APPID = LeadRequest.APPID ?? "",
        //            IMEI = LeadRequest.IMEI ?? "",
        //            LoginTypeID = LeadRequest.LoginTypeID,
        //            UserID = LeadRequest.UserID,
        //            SessionID = LeadRequest.SessionID,
        //            RegKey = LeadRequest.RegKey ?? "",
        //            SerialNo = LeadRequest.SerialNo ?? "",
        //            Version = LeadRequest.Version ?? "",
        //            Session = LeadRequest.Session
        //        };
        //        var appResp = appML.CheckAppSession(appRequest);
        //        responseStatus.IsAppValid = appResp.IsAppValid;
        //        responseStatus.IsVersionValid = appResp.IsVersionValid;
        //        responseStatus.IsPasswordExpired = appResp.IsPasswordExpired;
        //        responseStatus.Statuscode = appResp.Statuscode;
        //        if (appResp.Statuscode == ErrorCodes.One)
        //        {
        //            if (!appResp.IsPasswordExpired)
        //            {

        //                var _req = new LeadServiceRequest
        //                {
        //                    LT = 1,
        //                    OID = LeadRequest.leadServiceRequest.OID,
        //                    FromDate = LeadRequest.leadServiceRequest.FromDate,
        //                    ToDate = LeadRequest.leadServiceRequest.ToDate,
        //                    Mobile = LeadRequest.leadServiceRequest.Mobile,
        //                    ID = LeadRequest.leadServiceRequest.ID,
        //                    UserID = LeadRequest.UserID,
        //                    Criteria = LeadRequest.leadServiceRequest.Criteria,
        //                    CriteriaText = LeadRequest.leadServiceRequest.CriteriaText,
        //                    TopRows = LeadRequest.leadServiceRequest.TopRows,
        //                    RequestModeID= LeadRequest.leadServiceRequest.RequestModeID
        //                };
        //                return Json(ml.GetLeadServiceRequest(_req));
        //            }
        //        }
        //        else
        //        {
        //            responseStatus.Msg = appResp.Msg;
        //        }
        //    }
        //    else
        //    {
        //        responseStatus.Msg = "Invalid Parameter";
        //    }
        //    new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
        //    {
        //        CommonStr = "UpdateLeadServiceRequest",
        //        CommonStr2 = JsonConvert.SerializeObject(Request),
        //        CommonStr3 = JsonConvert.SerializeObject(responseStatus)
        //    });
        //    return Json(responseStatus);

        //}

        //[HttpPost]
        //public IActionResult UpdateLeadServiceRequest([FromBody] LeadAppServiceReq Request)
        //{
        //    ILeadServiceML ml = new LeadServiceML(_accessor, _env);
        //    var responseStatus = new AppResponse
        //    {
        //        Statuscode = ErrorCodes.Minus1,
        //        Msg = ErrorCodes.InvaildSession
        //    };
        //    if (Request.leadService != null)
        //    {
        //        var appResp = appML.CheckAppSession(Request);
        //        responseStatus.IsAppValid = appResp.IsAppValid;
        //        responseStatus.IsVersionValid = appResp.IsVersionValid;
        //        responseStatus.IsPasswordExpired = appResp.IsPasswordExpired;
        //        responseStatus.Statuscode = appResp.Statuscode;
        //        if (appResp.Statuscode == ErrorCodes.One)
        //        {
        //            if (!appResp.IsPasswordExpired)
        //            {
        //                if (Request.leadService.ID == 0)
        //                {
        //                    responseStatus.Statuscode = -1;
        //                    responseStatus.Msg = "Record cannot be updated";
        //                    return Json(responseStatus);
        //                }
        //                if (String.IsNullOrEmpty(Request.leadService.Remark) || Request.leadService.LeadStatus == null)
        //                {
        //                    responseStatus.Statuscode = -1;
        //                    responseStatus.Msg = "Please fill Remark and Lead Status Both.";
        //                    return Json(responseStatus);
        //                }
        //                var _req = new LeadServiceRequest
        //                {
        //                    LT = 1,
        //                    ID = Request.leadService.ID,
        //                    Remark = Request.leadService.Remark,
        //                    LeadStatus = Request.leadService.LeadStatus,
        //                    ModifyBy = Request.UserID
        //                };
        //                return Json(ml.UpdateLeadServiceRequestApp(_req));
        //            }
        //        }
        //        else
        //        {
        //            responseStatus.Msg = appResp.Msg;
        //        }
        //    }
        //    else
        //    {
        //        responseStatus.Msg = "Invalid Parameter";
        //    }
        //    new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
        //    {
        //        CommonStr = "UpdateLeadServiceRequest",
        //        CommonStr2 = JsonConvert.SerializeObject(Request),
        //        CommonStr3 = JsonConvert.SerializeObject(responseStatus)
        //    });
        //    return Json(responseStatus);

        //}
        [HttpPost]
        public IActionResult GetCrediCardBanks([FromBody] AppSessionReq appSessionReq)
        {
            var aepsBanks = new AEPSBanksResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };

            AppResponse appResp = appML.CheckAppSession(appSessionReq);
            aepsBanks.IsAppValid = appResp.IsAppValid;
            aepsBanks.IsVersionValid = appResp.IsVersionValid;
            aepsBanks.CheckID = appResp.CheckID;
            if (appResp.Statuscode == ErrorCodes.One)
            {
                IBankML bankML = new BankML(_accessor, _env, false);
                var banks = bankML.GetCrediCardBanks();

                aepsBanks.aepsBanks = banks;
                aepsBanks.Statuscode = ErrorCodes.One;
                aepsBanks.Msg = ErrorCodes.SUCCESS;
            }
            else
                aepsBanks.Msg = appResp.Msg;
            new PlansAPIML(_accessor, _env, false).LogAppReqResp(new CommonReq
            {
                CommonStr = "GetCrediCardBanks",
                CommonStr2 = JsonConvert.SerializeObject(appSessionReq),
                CommonStr3 = JsonConvert.SerializeObject(aepsBanks)
            });
            return Json(aepsBanks);
        }
        #endregion

    }

}