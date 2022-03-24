using Fintech.AppCode.DB;
using Fintech.AppCode.HelperClass;
using Fintech.AppCode.StaticModel;
using Fintech.AppCode.WebRequest;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using RoundpayFinTech.AppCode.Configuration;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.MiddleLayer;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.BBPS;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Recharge;
using RoundpayFinTech.AppCode.ThirdParty.CyberPlate;
using RoundpayFinTech_bA.AppCode.ThirdParty.BillAvenue;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace RoundpayFinTech.AppCode.ThirdParty.BillAvenue
{
    public class BillAvenueML
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly IConfiguration Configuration;
        private readonly IDAL _dal;
        private readonly BAAPISetting apiSetting;
        private const string BACertificate = "Image/BillAvenue/billavenue_UAT_Certificate.crt";
        public BillAvenueML(IHttpContextAccessor accessor, IHostingEnvironment env, IDAL dal)
        {
            _accessor = accessor;
            _env = env;
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory());
            builder.AddJsonFile((_env.IsProduction() ? "appsettings.json" : "appsettings.Development.json"));
            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
            _dal = dal;
            apiSetting = AppSetting();
        }
        private BAAPISetting AppSetting()
        {
            var setting = new BAAPISetting();
            try
            {
                setting = new BAAPISetting
                {
                    accessCode = Configuration["SERVICESETTING:BLAVNUE:accessCode"],
                    agentId = Configuration["SERVICESETTING:BLAVNUE:agentId"],
                    BillerInfoURL = Configuration["SERVICESETTING:BLAVNUE:BillerInfoURL"],
                    BillFetchURL = Configuration["SERVICESETTING:BLAVNUE:BillFetchURL"],
                    BillPaymentURL = Configuration["SERVICESETTING:BLAVNUE:BillPaymentURL"],
                    StatusCheckURL = Configuration["SERVICESETTING:BLAVNUE:StatusCheckURL"],
                    ComplaintRegURL = Configuration["SERVICESETTING:BLAVNUE:ComplaintRegURL"],
                    ComplaintTrackURL = Configuration["SERVICESETTING:BLAVNUE:ComplaintTrackURL"],
                    DepositEnquiryURL = Configuration["SERVICESETTING:BLAVNUE:DepositEnquiryURL"],
                    BillValidationURL = Configuration["SERVICESETTING:BLAVNUE:BillValidationURL"],
                    instituteId = Configuration["SERVICESETTING:BLAVNUE:instituteId"],
                    Key = Configuration["SERVICESETTING:BLAVNUE:Key"],
                    ver = Configuration["SERVICESETTING:BLAVNUE:ver"]
                };
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "BAAPISetting",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser
                });
            }
            return setting;
        }
        public BABillerInfoResponse GetBillerInfo(string BillerID, string TransactionID, int OID, int APIID)
        {
            var req = new BABillerInfoRequest
            {
                billerId = BillerID
            };

            var objBADMT = new BABillerInfoResponse
            {
                errorInfo = new BAErrorInfo
                {
                    error = new BAError { }
                },
                biller = new BABiller
                {
                    billerInputParams = new List<paramInfo>
                    {

                    },
                    billerPaymentChannels = new List<paymentChannelInfo>
                    {

                    }
                }
            };
            string response = string.Empty;
            var _URL = apiSetting.BillerInfoURL;
            BA_AesCryptUtil bA_AesCryptUtil = new BA_AesCryptUtil(apiSetting.Key);
            var xmlReq = XMLHelper.O.SerializeToXml(req, "billerInfoRequest", false);
            xmlReq = @"<?xml version=""1.0"" encoding=""UTF-8""?>" + Environment.NewLine + xmlReq;
            var encRequest = bA_AesCryptUtil.encrypt(xmlReq.ToString());

            StringBuilder sb = new StringBuilder();
            sb.Append("accessCode=");
            sb.Append(apiSetting.accessCode);
            sb.Append("&requestId=");
            sb.Append(TransactionID.PadRight(35, 'M'));
            sb.Append("&ver=");
            sb.Append(apiSetting.ver);
            sb.Append("&instituteId=");
            sb.Append(apiSetting.instituteId);
            //string postData = string.Format("encRequest={0}", encRequest);

            _URL = string.Format("{0}?{1}", _URL, sb.ToString());

            try
            {
                //if (ApplicationSetting.IsBBPSInStaging)
                //{
                //    sb.Append("&encRequest=");
                //    sb.Append(encRequest);
                //    response = AppWebRequest.O.CallUsingHttpWebRequest_POSTWithCER(_URL, sb.ToString(), BACertificate);
                //}
                //else
                //{

                //}
                response = AppWebRequest.O.CallUsingHttpWebRequest_POST(_URL, encRequest, ContentType.text_xml);
                var decResp = response;
                if (!response.Contains("errorInfo"))
                {
                    decResp = bA_AesCryptUtil.decrypt(response);
                    response += "/AfterDecrypt: " + decResp;
                }
                if (!string.IsNullOrEmpty(decResp))
                {
                    objBADMT = XMLHelper.O.DesrializeToObject(objBADMT, decResp, "billerInfoResponse", true);
                }
            }
            catch (Exception ex)
            {
                response = ex.Message + "|" + response;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "GetBillerInfo",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = 1
                });
            }
            IOperatorML opML = new OperatorML(_accessor, _env, false);
            opML.UpdateBillerLog(new Fintech.AppCode.Model.CommonReq
            {
                CommonInt = APIID,
                CommonInt2 = OID,
                CommonStr = string.Format("{0}&BillerID={1}", _URL, BillerID),
                CommonStr2 = response
            });
            return objBADMT;
        }
        public BABillerInfoResponseList GetBillerInfo(List<ResponseStatus> billerIDs, string TransactionID)
        {
            var req = new List<BABillerInfoRequest>();
            if (billerIDs.Count > 0)
            {
                req = billerIDs.Select(x => new BABillerInfoRequest { billerId = x.CommonStr }).ToList();
            }
            var sbReq = new StringBuilder();
            sbReq.Append(@"<?xml version=""1.0"" encoding=""UTF-8""?><billerInfoRequest>");
            foreach (var item in billerIDs)
            {
                sbReq.Append("<billerId>");
                sbReq.Append(item.CommonStr);
                sbReq.Append("</billerId>");
            }
            sbReq.Append("</billerInfoRequest>");

            var objBADMT = new BABillerInfoResponseList
            {
                errorInfo = new BAErrorInfo
                {
                    error = new BAError { }
                },
                billerList = new List<biller>
                {

                }
            };
            string response = string.Empty;
            var _URL = apiSetting.BillerInfoURL;
            BA_AesCryptUtil bA_AesCryptUtil = new BA_AesCryptUtil(apiSetting.Key);

            var encRequest = bA_AesCryptUtil.encrypt(sbReq.ToString());

            StringBuilder sb = new StringBuilder();
            sb.Append("accessCode=");
            sb.Append(apiSetting.accessCode);
            sb.Append("&requestId=");
            sb.Append(TransactionID.PadRight(35, 'M'));
            sb.Append("&ver=");
            sb.Append(apiSetting.ver);
            sb.Append("&instituteId=");
            sb.Append(apiSetting.instituteId);
            string postData = string.Format("encRequest={0}", encRequest);
            _URL = string.Format("{0}?{1}", _URL, sb.ToString());
            try
            {
                //response = AppWebRequest.O.CallUsingHttpWebRequest_POSTWithCER(_URL, encRequest, BACertificate);
                response = AppWebRequest.O.CallUsingHttpWebRequest_POST(_URL, encRequest, ContentType.text_xml); //ErrorCodes.BillAvenueRes;
                var decResp = response;
                //will remove

                if (!response.Contains("errorInfo"))
                {
                    decResp = bA_AesCryptUtil.decrypt(response);
                    decResp = decResp.Replace("</responseCode>", "</responseCode><billerList>").Replace("</billerInfoResponse>", "</billerList></billerInfoResponse>");
                    response += "/AfterDecrypt: " + decResp;
                }
                if (!string.IsNullOrEmpty(decResp))
                {
                    objBADMT = XMLHelper.O.DesrializeToObject(objBADMT, decResp, "billerInfoResponse", true);
                }
            }
            catch (Exception ex)
            {
                response = ex.Message + "|" + response;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "GetBillerInfo",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = 1
                });
            }
            IOperatorML opML = new OperatorML(_accessor, _env, false);
            opML.UpdateBillerLog(new Fintech.AppCode.Model.CommonReq
            {
                CommonInt = 0,
                CommonInt2 = 0,
                CommonStr = string.Format("{0}&BillerID={1}", _URL, sbReq.ToString()),
                CommonStr2 = response
            });
            return objBADMT;
        }
        public BBPSResponse BillFetchAPI(BBPSLog bbpsLog)
        {
            var billResponse = new BBPSResponse
            {
                IsEditable = false,
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.URLNOTFOUND,
                ErrorCode = ErrorCodes.Unknown_Error.ToString(),
                ErrorMsg = ErrorCodes.URLNOTFOUND
            };

            var req = new BABillFetchRequest
            {
                billerId = bbpsLog.APIReqHelper.BillerID,
                customerInfo = new BACustomerInfo
                {
                    customerMobile = bbpsLog.CustomerMobile
                },
                agentDeviceInfo = new BAAgentDeviceInfo
                {
                    ip = bbpsLog.APIReqHelper.IPAddress == "::1" ? "122.176.71.56" : bbpsLog.APIReqHelper.IPAddress,
                    mac = bbpsLog.APIReqHelper.MAC,
                    initChannel = bbpsLog.APIReqHelper.InitChanel
                },
                agentId = bbpsLog.aPIDetail.APIOutletID, //(bbpsLog.APIReqHelper.InitChanel ?? string.Empty).StartsWith("INT") ? bbpsLog.aPIDetail.OnlineOutletID : apiSetting.agentId,
                inputParams = new List<input>()
            };
            if (ApplicationSetting.IsBBPSInStaging)
            {
                req.agentDeviceInfo.ip = "192.168.2.73";
                req.agentDeviceInfo.mac = "01-23-45-67-89-ab";
                req.customerInfo.customerEmail = " ";
                req.customerInfo.customerAdhaar = " ";
                req.customerInfo.customerPan = " ";
            }
            if (bbpsLog.APIReqHelper.InitChanel == "MOB")
            {
                req.agentDeviceInfo.mac = " ";
                req.agentDeviceInfo.app = "AIAPP";
                req.agentDeviceInfo.os = "Android";
                req.agentDeviceInfo.imei = bbpsLog.IMEI ?? string.Empty;
            }
            if (bbpsLog.APIReqHelper.OpParams != null)
            {
                if (bbpsLog.APIReqHelper.OpParams.Count > 0)
                {
                    bbpsLog.APIReqHelper.OpParams.RemoveAll(x => x.IsAccountNo == true);
                    req.inputParams.Add(new input
                    {
                        paramName = bbpsLog.APIReqHelper.AccountNoKey,
                        paramValue = bbpsLog.AccountNumber ?? string.Empty
                    });
                    if (!string.IsNullOrEmpty(bbpsLog.APIReqHelper.RegxAccount))
                    {
                        if (!Regex.IsMatch((bbpsLog.AccountNumber ?? string.Empty), bbpsLog.APIReqHelper.RegxAccount))
                        {
                            billResponse.Msg = string.Format("{0} {1}", nameof(ErrorCodes.Invalid_Parameter).Replace("_", " "), "Account");
                            billResponse.ErrorCode = ErrorCodes.Invalid_Parameter.ToString();
                            billResponse.ErrorMsg = billResponse.Msg;
                            return billResponse;
                        }
                    }
                }
                for (int i = 0; i < bbpsLog.APIReqHelper.OpParams.Count; i++)
                {
                    if (i == 0)
                    {
                        req.inputParams.Add(new input
                        {
                            paramName = bbpsLog.APIReqHelper.OpParams[i].Param,
                            paramValue = bbpsLog.Optional1 ?? string.Empty
                        });
                        if (!string.IsNullOrEmpty(bbpsLog.APIReqHelper.OpParams[i].RegEx))
                        {
                            if (!Regex.IsMatch((bbpsLog.Optional1 ?? string.Empty), bbpsLog.APIReqHelper.OpParams[i].RegEx))
                            {
                                billResponse.Msg = string.Format("{0} {1}", nameof(ErrorCodes.Invalid_Parameter).Replace("_", " "), "Optional1");
                                billResponse.ErrorCode = ErrorCodes.Invalid_Parameter.ToString();
                                billResponse.ErrorMsg = billResponse.Msg;
                                return billResponse;
                            }
                        }
                    }
                    if (i == 1)
                    {
                        req.inputParams.Add(new input
                        {
                            paramName = bbpsLog.APIReqHelper.OpParams[i].Param,
                            paramValue = bbpsLog.Optional2 ?? string.Empty
                        });
                        if (!string.IsNullOrEmpty(bbpsLog.APIReqHelper.OpParams[i].RegEx))
                        {
                            if (!Regex.IsMatch((bbpsLog.Optional2 ?? string.Empty), bbpsLog.APIReqHelper.OpParams[i].RegEx))
                            {
                                billResponse.Msg = string.Format("{0} {1}", nameof(ErrorCodes.Invalid_Parameter).Replace("_", " "), "Optional2");
                                billResponse.ErrorCode = ErrorCodes.Invalid_Parameter.ToString();
                                billResponse.ErrorMsg = billResponse.Msg;
                                return billResponse;
                            }
                        }
                    }
                    if (i == 2)
                    {
                        req.inputParams.Add(new input
                        {
                            paramName = bbpsLog.APIReqHelper.OpParams[i].Param,
                            paramValue = bbpsLog.Optional3 ?? string.Empty
                        });
                        if (!string.IsNullOrEmpty(bbpsLog.APIReqHelper.OpParams[i].RegEx))
                        {
                            if (!Regex.IsMatch((bbpsLog.Optional3 ?? string.Empty), bbpsLog.APIReqHelper.OpParams[i].RegEx))
                            {
                                billResponse.Msg = string.Format("{0} {1}", nameof(ErrorCodes.Invalid_Parameter).Replace("_", " "), "Optional3");
                                billResponse.ErrorCode = ErrorCodes.Invalid_Parameter.ToString();
                                billResponse.ErrorMsg = billResponse.Msg;
                                return billResponse;
                            }
                        }
                    }
                    if (i == 3)
                    {
                        req.inputParams.Add(new input
                        {
                            paramName = bbpsLog.APIReqHelper.OpParams[i].Param,
                            paramValue = bbpsLog.Optional4 ?? string.Empty
                        });
                        if (!string.IsNullOrEmpty(bbpsLog.APIReqHelper.OpParams[i].RegEx))
                        {
                            if (!Regex.IsMatch((bbpsLog.Optional4 ?? string.Empty), bbpsLog.APIReqHelper.OpParams[i].RegEx))
                            {
                                billResponse.Msg = string.Format("{0} {1}", nameof(ErrorCodes.Invalid_Parameter).Replace("_", " "), "Optional4");
                                billResponse.ErrorCode = ErrorCodes.Invalid_Parameter.ToString();
                                billResponse.ErrorMsg = billResponse.Msg;
                                return billResponse;
                            }
                        }
                    }
                }
            }

            string response = string.Empty;
            var _URL = apiSetting.BillFetchURL;
            BA_AesCryptUtil bA_AesCryptUtil = new BA_AesCryptUtil(apiSetting.Key);
            var xmlReq = XMLHelper.O.SerializeToXml(req, "billFetchRequest", false);
            xmlReq = xmlReq.Replace("> <", "><");
            var encRequest = bA_AesCryptUtil.encrypt(xmlReq.ToString());

            StringBuilder sb = new StringBuilder();
            sb.Append("accessCode=");
            sb.Append(apiSetting.accessCode);
            sb.Append("&encRequest=");
            sb.Append(encRequest);
            sb.Append("&requestId=");
            sb.Append(bbpsLog.SessionNo.PadRight(35, 'M'));
            sb.Append("&ver=");
            sb.Append(apiSetting.ver);
            sb.Append("&instituteId=");
            sb.Append(apiSetting.instituteId);
            bbpsLog.Request = string.Format("{0}?{1}", _URL, xmlReq + "]]" + sb.ToString());
            try
            {
                response = AppWebRequest.O.CallUsingHttpWebRequest_POSTWithCER(_URL, sb.ToString(), BACertificate);
                var decResp = response;
                if (!response.Contains("errorInfo") && !response.Contains("billFetchResponse"))
                {
                    decResp = bA_AesCryptUtil.decrypt(response);
                    if (decResp != response)
                    {
                        response = decResp;
                    }
                }
                if (!string.IsNullOrEmpty(decResp))
                {
                    var objBADMT = new BABillFetchResponse
                    {
                        inputParams = new List<input>
                        {

                        },
                        additionalInfo = new List<info>(),
                        billerResponse = new BABillerResponse
                        {
                            amountOptions = new List<BABillerResponse.option> { }
                        },
                        errorInfo = new BAErrorInfo { }
                    };
                    var _apiRes = XMLHelper.O.DesrializeToObject(objBADMT, decResp, "billFetchResponse", true);
                    if (_apiRes != null)
                    {
                        if (_apiRes.responseCode == "000")
                        {
                            bbpsLog.helper.Status = ErrorCodes.One;
                            billResponse.Statuscode = ErrorCodes.One;
                            billResponse.ErrorCode = ErrorCodes.Transaction_Successful.ToString();
                            billResponse.ErrorMsg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                            billResponse.Msg = billResponse.Msg;
                            billResponse.IsEnablePayment = true;
                            billResponse.BillNumber = _apiRes.billerResponse.billNumber;
                            billResponse.BillDate = string.IsNullOrEmpty(_apiRes.billerResponse.billDate) ? string.Empty : Convert.ToDateTime(_apiRes.billerResponse.billDate).ToString("dd MMM yyyy");
                            billResponse.CustomerName = _apiRes.billerResponse.customerName;
                            billResponse.BillPeriod = _apiRes.billerResponse.billPeriod;
                            billResponse.DueDate = string.IsNullOrEmpty(_apiRes.billerResponse.dueDate) ? string.Empty : Convert.ToDateTime(_apiRes.billerResponse.dueDate).ToString("dd MMM yyyy");
                            billResponse.Amount = (Convert.ToDecimal(_apiRes.billerResponse.billAmount ?? "0") / 100).ToString();
                            bbpsLog.Amount = Convert.ToDecimal(billResponse.Amount);
                            bbpsLog.BillNumber = billResponse.BillNumber;
                            bbpsLog.CustomerName = billResponse.CustomerName;
                            bbpsLog.BillDate = billResponse.BillDate;
                            bbpsLog.DueDate = billResponse.DueDate;
                            bbpsLog.BillPeriod = billResponse.BillPeriod;

                            if (_apiRes.additionalInfo.Count > 0)
                            {
                                billResponse.billAdditionalInfo = new List<BillAdditionalInfo>();
                                for (int i = 0; i < _apiRes.additionalInfo.Count; i++)
                                {
                                    billResponse.billAdditionalInfo.Add(new BillAdditionalInfo
                                    {
                                        InfoName = _apiRes.additionalInfo[i].infoName,
                                        InfoValue = _apiRes.additionalInfo[i].infoValue
                                    });
                                    bbpsLog.helper.tpBFAInfo.Rows.Add(new object[] { _apiRes.additionalInfo[i].infoName, _apiRes.additionalInfo[i].infoValue, i + 1 });
                                    if (string.IsNullOrEmpty(bbpsLog.helper.EarlyPaymentDate) && !string.IsNullOrEmpty(bbpsLog.APIReqHelper.EarlyPaymentDateKey))
                                    {
                                        if (_apiRes.additionalInfo[i].infoName.Equals(bbpsLog.APIReqHelper.EarlyPaymentDateKey))
                                        {
                                            bbpsLog.helper.EarlyPaymentDate = Convert.ToDateTime(_apiRes.additionalInfo[i].infoValue).ToString("dd MMM yyyy");
                                        }
                                    }
                                    if (!string.IsNullOrEmpty(bbpsLog.APIReqHelper.BillMonthKey) && string.IsNullOrEmpty(bbpsLog.BillMonth))
                                    {
                                        if (!string.IsNullOrEmpty(_apiRes.additionalInfo[i].infoName).Equals(bbpsLog.APIReqHelper.BillMonthKey))
                                        {
                                            bbpsLog.BillMonth = _apiRes.additionalInfo[i].infoValue;
                                            billResponse.BillMonth = string.IsNullOrEmpty(_apiRes.additionalInfo[i].infoValue) ? string.Empty : Convert.ToDateTime(_apiRes.additionalInfo[i].infoValue).ToString("dd MMM yyyy");
                                        }

                                    }
                                }
                            }
                            if (_apiRes.inputParams.Count > 0)
                            {
                                for (int i = 0; i < _apiRes.inputParams.Count; i++)
                                {
                                    bbpsLog.helper.tpBFInputParam.Rows.Add(new object[] { _apiRes.inputParams[i].paramName, _apiRes.inputParams[i].paramValue, i + 1 });
                                }
                            }
                            if (_apiRes.billerResponse.amountOptions.Count > 0)
                            {
                                billResponse.billAmountOptions = new List<BillAmountOption>();
                                for (int i = 0; i < _apiRes.billerResponse.amountOptions.Count; i++)
                                {
                                    billResponse.billAmountOptions.Add(new BillAmountOption
                                    {
                                        AmountName = _apiRes.billerResponse.amountOptions[i].amountName,
                                        AmountValue = Convert.ToDecimal(_apiRes.billerResponse.amountOptions[i].amountValue) / 100
                                    });
                                    bbpsLog.helper.tpBFAmountOps.Rows.Add(new object[] { _apiRes.billerResponse.amountOptions[i].amountName, _apiRes.billerResponse.amountOptions[i].amountValue, i + 1 });

                                    if (bbpsLog.helper.EarlyPaymentAmount == 0 && !string.IsNullOrEmpty(bbpsLog.APIReqHelper.EarlyPaymentAmountKey))
                                    {
                                        if (_apiRes.billerResponse.amountOptions[i].amountName.Equals(bbpsLog.APIReqHelper.EarlyPaymentAmountKey))
                                        {
                                            bbpsLog.helper.EarlyPaymentAmount = Convert.ToDecimal(_apiRes.billerResponse.amountOptions[i].amountValue) / 100;
                                        }
                                    }
                                    else if (bbpsLog.helper.LatePaymentAmount == 0 && !string.IsNullOrEmpty(bbpsLog.APIReqHelper.LatePaymentAmountKey))
                                    {
                                        if (_apiRes.billerResponse.amountOptions[i].amountName.Equals(bbpsLog.APIReqHelper.LatePaymentAmountKey))
                                        {
                                            bbpsLog.helper.LatePaymentAmount = Convert.ToDecimal(_apiRes.billerResponse.amountOptions[i].amountValue) / 100;
                                        }
                                    }
                                }
                            }
                            if (!string.IsNullOrEmpty(bbpsLog.helper.EarlyPaymentDate) && bbpsLog.helper.EarlyPaymentAmount > 0)
                            {
                                if (Convert.ToDateTime(bbpsLog.helper.EarlyPaymentDate.Replace(" ", "/")) >= Convert.ToDateTime(DateTime.Now.ToString("dd/MMM/yyyy")))
                                {
                                    billResponse.Amount = bbpsLog.helper.EarlyPaymentAmount.ToString();
                                    billResponse.BillAmountKey = bbpsLog.APIReqHelper.EarlyPaymentAmountKey;
                                }
                            }
                            else if (!string.IsNullOrEmpty(bbpsLog.DueDate) && bbpsLog.helper.LatePaymentAmount > 0)
                            {
                                if (Convert.ToDateTime(bbpsLog.DueDate.Replace(" ", "/")) < Convert.ToDateTime(DateTime.Now.ToString("dd/MMM/yyyy")))
                                {
                                    billResponse.Amount = bbpsLog.helper.LatePaymentAmount.ToString();
                                    billResponse.BillAmountKey = bbpsLog.APIReqHelper.LatePaymentAmountKey;
                                }
                            }
                        }
                        else
                        {
                            if (_apiRes.errorInfo != null && (_apiRes.errorInfo ?? new BAErrorInfo { }).error != null)
                            {
                                billResponse.IsShowMsgOnly = true;
                                IErrorCodeML errorCodeML = new ErrorCodeML(_accessor, _env, false);
                                var eFromDB = errorCodeML.GetAPIErrorCodeDescription(bbpsLog.aPIDetail.GroupCode, _apiRes.errorInfo.error.errorCode);
                                billResponse.ErrorMsg = billResponse.Msg = (eFromDB.Error ?? string.Empty).Replace("{AccountKey}", bbpsLog.APIReqHelper.AccountNoKey);
                                billResponse.ErrorMsg = billResponse.Msg = billResponse.Msg.Replace("{REPLACE}", _apiRes.errorInfo.error.errorMessage);
                                billResponse.ErrorMsg = billResponse.Msg = bbpsLog.helper.Reason = string.IsNullOrEmpty(billResponse.Msg) ? nameof(ErrorCodes.Unable_to_get_bill_details_Please_try_again_after_few_minute).Replace("_", " ") : billResponse.Msg;
                                billResponse.ErrorCode = eFromDB.Code ?? ErrorCodes.Unable_to_get_bill_details_Please_try_again_after_few_minute.ToString();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                response = ex.Message + "|" + response;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "BillFetchAPI",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = 1
                });
            }
            bbpsLog.Response = response;
            return billResponse;
        }
        public HitRequestResponseModel BillValidation(BBPSPaymentRequest bBPSRequest)
        {
            var returnResp = new HitRequestResponseModel
            {
                IsValidaionResponseAlso = true
            };
            var req = new BABillValidationRequest
            {
                billerId = bBPSRequest.BillerID,
                agentId = bBPSRequest.APIOutletID,
                inputParams = new List<input>()
            };
            var billerResponse = new BABillerResponse
            {
                amountOptions = new List<BABillerResponse.option>()
            };
            var objBADMT = new BABillFetchResponse
            {
                inputParams = new List<input> { },
                additionalInfo = new List<info>(),
                billerResponse = new BABillerResponse
                {
                    amountOptions = new List<BABillerResponse.option> { }
                },
                errorInfo = new BAErrorInfo { }
            };
            var AccountParam = string.Empty;
            if (bBPSRequest.OpParams != null)
            {
                if (bBPSRequest.OpParams.Count > 0)
                {
                    AccountParam = bBPSRequest.OpParams.Where(w => w.IsAccountNo == true).Select(x => x.Param).ToList()[0];
                    req.inputParams.Add(new input
                    {
                        paramName = AccountParam,
                        paramValue = bBPSRequest.AccountNo
                    });
                    bBPSRequest.OpParams.RemoveAll(x => x.IsAccountNo == true);
                }

                for (int i = 0; i < bBPSRequest.OpParams.Count; i++)
                {
                    if (i == 0)
                    {
                        req.inputParams.Add(new input
                        {
                            paramName = bBPSRequest.OpParams[i].Param,
                            paramValue = bBPSRequest.Optional1
                        });
                    }
                    if (i == 1)
                    {
                        req.inputParams.Add(new input
                        {
                            paramName = bBPSRequest.OpParams[i].Param,
                            paramValue = bBPSRequest.Optional2
                        });
                    }
                    if (i == 2)
                    {
                        req.inputParams.Add(new input
                        {
                            paramName = bBPSRequest.OpParams[i].Param,
                            paramValue = bBPSRequest.Optional3
                        });
                    }
                    if (i == 3)
                    {
                        req.inputParams.Add(new input
                        {
                            paramName = bBPSRequest.OpParams[i].Param,
                            paramValue = bBPSRequest.Optional4
                        });
                    }
                }
            }
            bBPSRequest.PaymentMode = (bBPSRequest.PaymentMode ?? string.Empty).Trim();
            string response = string.Empty;
            var _URL = apiSetting.BillValidationURL;
            BA_AesCryptUtil bA_AesCryptUtil = new BA_AesCryptUtil(apiSetting.Key);
            var xmlReq = XMLHelper.O.SerializeToXml(req, "billValidationRequest", false);
            var encRequest = bA_AesCryptUtil.encrypt(xmlReq.ToString());

            StringBuilder sb = new StringBuilder();
            sb.Append("accessCode=");
            sb.Append(apiSetting.accessCode);
            sb.Append("&encRequest=");
            sb.Append(encRequest);
            sb.Append("&requestId=");
            sb.Append(("A" + bBPSRequest.TID).PadRight(35, 'M'));
            sb.Append("&ver=");
            sb.Append(apiSetting.ver);
            sb.Append("&instituteId=");
            sb.Append(apiSetting.instituteId);
            returnResp.RequestV = _URL + "?" + xmlReq + "[[" + sb.ToString();
            try
            {
                response = AppWebRequest.O.CallUsingHttpWebRequest_POSTWithCER(_URL, sb.ToString(), BACertificate);
                var decResp = returnResp.Response = response;
                if (!response.Contains("errorInfo") && !response.Contains("billFetchResponse"))
                {
                    decResp = bA_AesCryptUtil.decrypt(response);
                    if (decResp != response)
                    {
                        response = decResp;

                    }
                }
                if (!string.IsNullOrEmpty(decResp))
                {
                    returnResp.ResponseV = decResp;
                }
            }
            catch (Exception ex)
            {
                response = ex.Message + "|" + response;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "BillValidation",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = 1
                });
            }
            return returnResp;
        }
        public HitRequestResponseModel BillPayement(BBPSPaymentRequest bBPSRequest)
        {
            var returnResp = new HitRequestResponseModel { };
            string response = string.Empty;
            try
            {
                returnResp.IsBillValidated = true;
                var AccountParam = string.Empty;
                if (bBPSRequest.OpParams.Count > 0)
                {
                    AccountParam = bBPSRequest.OpParams.Where(w => w.IsAccountNo == true).Select(x => x.Param).ToList()[0];
                }
                if (bBPSRequest.IsValidation)
                {
                    returnResp = BillValidation(bBPSRequest);
                    returnResp.IsBillValidated = true;
                    if (!string.IsNullOrEmpty(returnResp.ResponseV))
                    {
                        var apiModel = new BABillValidationResponse
                        {
                            additionalInfo = new List<info>()
                        };
                        var _apiRes = XMLHelper.O.DesrializeToObject(apiModel, returnResp.ResponseV, "billValidationResponse", true);
                        if (_apiRes != null)
                        {
                            if (!_apiRes.responseCode.Equals("000"))
                            {
                                returnResp.IsBillValidated = false;
                                returnResp.ResponseV = returnResp.ResponseV.Replace("complianceCode", "errorCode");
                                return returnResp;
                            }
                        }
                    }
                }
                var req = new BABillPaymentRequest
                {
                    billerId = bBPSRequest.BillerID,
                    billerAdhoc = bBPSRequest.billerAdhoc,
                    customerInfo = new BACustomerInfo
                    {
                        customerMobile = bBPSRequest.CustomerMobile,
                        customerAdhaar = " ",
                        customerEmail = " ",
                        customerPan = " "
                    },
                    agentDeviceInfo = new BAAgentDeviceInfo
                    {
                        ip = bBPSRequest.IPAddress,
                        mac = bBPSRequest.MAC,
                        initChannel = bBPSRequest.InitChanel
                    },
                    agentId = bBPSRequest.APIOutletID,
                    inputParams = new List<input>()
                };
                if (ApplicationSetting.IsBBPSInStaging)
                {
                    req.agentDeviceInfo.ip = "192.168.2.73";
                }
                if (bBPSRequest.InitChanel == "MOB")
                {
                    req.agentDeviceInfo.mac = " ";
                    req.agentDeviceInfo.app = "AIAPP";
                    req.agentDeviceInfo.os = "Android";
                    req.agentDeviceInfo.imei = bBPSRequest.IMEI ?? string.Empty;
                }
                var billerResponse = new BABillerResponse
                {
                    amountOptions = new List<BABillerResponse.option>()
                };

                var objBADMT = new BABillFetchResponse
                {
                    inputParams = new List<input>
                    {

                    },
                    additionalInfo = new List<info>(),
                    billerResponse = new BABillerResponse
                    {
                        amountOptions = new List<BABillerResponse.option> { }
                    },
                    errorInfo = new BAErrorInfo { }
                };
                if (!string.IsNullOrEmpty(bBPSRequest.FetchBillResponse))
                {
                    objBADMT = XMLHelper.O.DesrializeToObject(objBADMT, bBPSRequest.FetchBillResponse, "billFetchResponse", true);
                }
                if (!bBPSRequest.IsQuickPay)
                {
                    req.additionalInfo = new List<info>();
                    req.billerResponse = objBADMT.billerResponse;
                }
                if (!string.IsNullOrEmpty(AccountParam))
                {
                    req.inputParams.Add(new input
                    {
                        paramName = AccountParam,
                        paramValue = bBPSRequest.AccountNo
                    });
                }
                if (bBPSRequest.OpParams != null)
                {
                    if (bBPSRequest.OpParams.Count > 0)
                    {
                        if (string.IsNullOrEmpty(AccountParam))
                        {
                            AccountParam = bBPSRequest.OpParams.Where(w => w.IsAccountNo == true).Select(x => x.Param).ToList()[0];
                            req.inputParams.Add(new input
                            {
                                paramName = AccountParam,
                                paramValue = bBPSRequest.AccountNo
                            });
                        }
                        bBPSRequest.OpParams.RemoveAll(x => x.IsAccountNo == true);
                    }

                    for (int i = 0; i < bBPSRequest.OpParams.Count; i++)
                    {
                        if (i == 0)
                        {
                            req.inputParams.Add(new input
                            {
                                paramName = bBPSRequest.OpParams[i].Param,
                                paramValue = bBPSRequest.Optional1
                            });
                        }
                        if (i == 1)
                        {
                            req.inputParams.Add(new input
                            {
                                paramName = bBPSRequest.OpParams[i].Param,
                                paramValue = bBPSRequest.Optional2
                            });
                        }
                        if (i == 2)
                        {
                            req.inputParams.Add(new input
                            {
                                paramName = bBPSRequest.OpParams[i].Param,
                                paramValue = bBPSRequest.Optional3
                            });
                        }
                        if (i == 3)
                        {
                            req.inputParams.Add(new input
                            {
                                paramName = bBPSRequest.OpParams[i].Param,
                                paramValue = bBPSRequest.Optional4
                            });
                        }
                    }
                }
                if (bBPSRequest.AInfos != null)
                {
                    foreach (var item in bBPSRequest.AInfos)
                    {
                        req.additionalInfo.Add(new info
                        {
                            infoName = item.InfoName,
                            infoValue = item.InfoValue
                        });
                    }
                }
                req.paymentInfo = new List<info>
                {
                    new info
                    {
                        infoName = "Remarks",//Wallet name
                        infoValue = "Received"
                    }
                };
                bBPSRequest.PaymentMode = (bBPSRequest.PaymentMode ?? string.Empty).Trim();
                if (bBPSRequest.PaymentMode.ToUpper().Equals("WALLET"))
                {
                    req.paymentInfo[0].infoName = "WalletName";
                    req.paymentInfo[0].infoValue = "PAYTM";
                    req.paymentInfo.Add(new info
                    {
                        infoName = "MobileNo",
                        infoValue = bBPSRequest.UserMobileNo
                    });
                }
                if (bBPSRequest.PaymentMode.ToUpper().Equals("UPI"))
                {
                    req.paymentInfo[0].infoName = bBPSRequest.PaymentModeInAPI;
                    req.paymentInfo[0].infoValue = bBPSRequest.UserMobileNo + "@upi";
                }
                req.paymentMethod = new BAPaymentMethod
                {
                    paymentMode = bBPSRequest.PaymentMode.Equals("CASH") ? "Cash" : bBPSRequest.PaymentMode.Trim(),
                    quickPay = bBPSRequest.IsQuickPay ? "Y" : "N",
                    splitPay = "N"//bBPSRequest.ExactNess == EXACTNESS.Exact || bBPSRequest.IsQuickPay ? "N" : "Y"
                };

                req.amountInfo = new BAAmountInfo
                {
                    amount = (bBPSRequest.RequestedAmount * 100).ToString().Split('.')[0],
                    currency = "356",
                    custConvFee = bBPSRequest.CCFAmount.ToString(),
                    amountTags = ""
                };
                //if (!ApplicationSetting.IsBBPSInStaging)
                //{
                //    req.amountInfo.amountTags.amountTag = "Bill Amount";
                //    req.amountInfo.amountTags.value = (bBPSRequest.RequestedAmount * 100).ToString();
                //}
                //if (!string.IsNullOrEmpty(bBPSRequest.EarlyPaymentDate) && !string.IsNullOrEmpty(bBPSRequest.EarlyPaymentAmountKey))
                //{
                //    if (Convert.ToDateTime(bBPSRequest.EarlyPaymentDate.Replace(" ", "/")) >= Convert.ToDateTime(DateTime.Now.ToString("dd/MMM/yyyy")))
                //    {
                //        req.amountInfo.amountTags.amountTag = bBPSRequest.EarlyPaymentAmountKey;
                //    }
                //}
                //else if (!string.IsNullOrEmpty(bBPSRequest.DueDate) && !string.IsNullOrEmpty(bBPSRequest.LatePaymentAmountKey))
                //{
                //    if (Convert.ToDateTime(bBPSRequest.DueDate.Replace(" ", "/")) < Convert.ToDateTime(DateTime.Now.ToString("dd/MMM/yyyy")))
                //    {
                //        req.amountInfo.amountTags.amountTag = bBPSRequest.LatePaymentAmountKey;
                //    }
                //}

                var _URL = apiSetting.BillPaymentURL;
                BA_AesCryptUtil bA_AesCryptUtil = new BA_AesCryptUtil(apiSetting.Key);
                var xmlReq = XMLHelper.O.SerializeToXml(req, "billPaymentRequest", false);
                xmlReq = xmlReq.Replace("> <", "><").Replace("<amountTags />", "<amountTags></amountTags>");
                var encRequest = bA_AesCryptUtil.encrypt(xmlReq.ToString());

                StringBuilder sb = new StringBuilder();
                sb.Append("accessCode=");
                sb.Append(apiSetting.accessCode);
                sb.Append("&encRequest=");
                sb.Append(encRequest);
                sb.Append("&requestId=");
                sb.Append(("A" + bBPSRequest.TID).PadRight(35, 'M'));
                sb.Append("&ver=");
                sb.Append(apiSetting.ver);
                sb.Append("&instituteId=");
                sb.Append(apiSetting.instituteId);
                returnResp.Request = _URL + "?" + xmlReq + "[[" + sb.ToString();

                response = AppWebRequest.O.CallUsingHttpWebRequest_POSTWithCER(_URL, sb.ToString(), BACertificate);
                var decResp = returnResp.Response = response;
                if (!response.Contains("errorInfo") && !response.Contains("billFetchResponse"))
                {
                    decResp = bA_AesCryptUtil.decrypt(response);
                    if (decResp != response)
                    {
                        response = decResp;
                    }
                }
                if (!string.IsNullOrEmpty(decResp))
                {
                    returnResp.Response = decResp;
                }
            }
            catch (Exception ex)
            {
                response = ex.Message + "|" + response;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "BillPayement",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = 1
                });
            }
            return returnResp;
        }
        public BBPSComplainAPIResponse ComplainBillAvenue(BBPSComplainRequest bBPSComplainRequest)
        {
            var returnResp = new BBPSComplainAPIResponse
            {

            };
            var req = new BAComplaintRegistrationReq
            {
                billerId = bBPSComplainRequest.BillerID,
                complaintType = bBPSComplainRequest.ComplainType == BBPSComplainType.Service ? nameof(BBPSComplainType.Service) : nameof(BBPSComplainType.Transaction),
                agentId = bBPSComplainRequest.APIOutletID,
                complaintDesc = bBPSComplainRequest.Description,
                txnRefId = bBPSComplainRequest.VendorID
            };
            if (bBPSComplainRequest.ComplainType == BBPSComplainType.Service)
            {
                req.participationType = bBPSComplainRequest.ParticipationType;
                req.servReason = bBPSComplainRequest.Reason;
            }
            else
            {
                req.complaintDisposition = bBPSComplainRequest.Reason;
            }

            string response = string.Empty;
            var _URL = apiSetting.ComplaintRegURL;
            BA_AesCryptUtil bA_AesCryptUtil = new BA_AesCryptUtil(apiSetting.Key);
            var xmlReq = XMLHelper.O.SerializeToXml(req, "complaintRegistrationReq", false);
            var encRequest = bA_AesCryptUtil.encrypt(xmlReq.ToString());

            StringBuilder sb = new StringBuilder();
            sb.Append("accessCode=");
            sb.Append(apiSetting.accessCode);
            sb.Append("&encRequest=");
            sb.Append(encRequest);
            sb.Append("&requestId=");
            sb.Append(bBPSComplainRequest.TransactionID.PadRight(35, 'M'));
            sb.Append("&ver=");
            sb.Append(apiSetting.ver);
            sb.Append("&instituteId=");
            sb.Append(apiSetting.instituteId);
            returnResp.Request = _URL + "?" + xmlReq + "[[" + sb.ToString();
            try
            {
                response = AppWebRequest.O.CallUsingHttpWebRequest_POSTWithCER(_URL, sb.ToString(), BACertificate);
                var decResp = returnResp.Response = response;
                if (!response.Contains("errorInfo") && !response.Contains("billFetchResponse"))
                {
                    decResp = bA_AesCryptUtil.decrypt(response);
                    if (decResp != response)
                    {
                        response = decResp;
                    }
                }
                if (!string.IsNullOrEmpty(decResp))
                {
                    var objBA = new BAComplaintRegistrationResp
                    {
                    };
                    var _apiRes = XMLHelper.O.DesrializeToObject(objBA, response, "complaintRegistrationResp", true);
                    if (_apiRes != null)
                    {
                        if (_apiRes.responseCode.Equals("000"))
                        {
                            returnResp.Statuscode = 1;
                            returnResp.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                            returnResp.ErrorCode = ErrorCodes.Transaction_Successful;
                            returnResp.LiveID = _apiRes.complaintId;
                        }
                        else
                        {
                            returnResp.Statuscode = 3;
                            returnResp.Msg = _apiRes.errorInfo.error.errorMessage;
                            returnResp.ErrorCode = ErrorCodes.Unknown_Error;
                            returnResp.LiveID = _apiRes.errorInfo.error.errorMessage;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                response = ex.Message + "|" + response;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "ComplainBillAvenue",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = 1
                });
            }
            returnResp.Response = response;
            return returnResp;
        }
        public BBPSComplainAPIResponse TrackComplainBillAvenue(BBPSComplainRequest bBPSComplainRequest)
        {
            var returnResp = new BBPSComplainAPIResponse
            {
                Statuscode = RechargeRespType.FAILED,
                Msg = "Complain could not be register",
                ErrorCode = ErrorCodes.Unknown_Error,
                Remark = "Complain could not be register"
            };
            var req = new BAComplaintTrackingReq
            {
                complaintType = bBPSComplainRequest.ComplainType == BBPSComplainType.Service ? nameof(BBPSComplainType.Service) : nameof(BBPSComplainType.Transaction),
                complaintId = bBPSComplainRequest.ComplaintID
            };

            string response = string.Empty;
            var _URL = apiSetting.ComplaintTrackURL;
            BA_AesCryptUtil bA_AesCryptUtil = new BA_AesCryptUtil(apiSetting.Key);
            var xmlReq = XMLHelper.O.SerializeToXml(req, "complaintTrackingReq", false);
            var encRequest = bA_AesCryptUtil.encrypt(xmlReq.ToString());

            StringBuilder sb = new StringBuilder();
            sb.Append("accessCode=");
            sb.Append(apiSetting.accessCode);
            sb.Append("&encRequest=");
            sb.Append(encRequest);
            sb.Append("&requestId=");
            sb.Append(bBPSComplainRequest.TransactionID.PadRight(35, 'M'));
            sb.Append("&ver=");
            sb.Append(apiSetting.ver);
            sb.Append("&instituteId=");
            sb.Append(apiSetting.instituteId);
            returnResp.Request = _URL + "?" + xmlReq + "[[" + sb.ToString();
            try
            {
                response = AppWebRequest.O.CallUsingHttpWebRequest_POSTWithCER(_URL, sb.ToString(), BACertificate);
                var decResp = returnResp.Response = response;
                if (!response.Contains("errorInfo") && !response.Contains("billFetchResponse"))
                {
                    decResp = bA_AesCryptUtil.decrypt(response);
                    if (decResp != response)
                    {
                        returnResp.Response = response = decResp;
                    }
                }
                if (!string.IsNullOrEmpty(decResp))
                {
                    var objBA = new BAComplaintTrackingResp
                    {
                        errorInfo = new BAErrorInfo
                        {
                            error = new BAError { }
                        }
                    };
                    var _apiRes = XMLHelper.O.DesrializeToObject(objBA, response, "complaintTrackingResp", true);
                    if (_apiRes != null)
                    {
                        returnResp.ComplainAssignedTo = _apiRes.complaintAssigned;
                        returnResp.ComplainStatus = _apiRes.complaintStatus;
                        if (string.IsNullOrEmpty(_apiRes.respReason) && _apiRes.errorInfo != null)
                        {
                            returnResp.ComplainReason = _apiRes.errorInfo.error != null ? _apiRes.errorInfo.error.errorMessage : string.Empty;
                        }
                        else
                        {
                            returnResp.ComplainReason = _apiRes.respReason;
                        }
                        returnResp.Remark = returnResp.ComplainReason;
                        if (_apiRes.respCode.Equals("000"))
                        {
                            returnResp.Statuscode = RechargeRespType.SUCCESS;
                            returnResp.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                            returnResp.ErrorCode = ErrorCodes.Transaction_Successful;
                            returnResp.LiveID = _apiRes.complaintId;
                        }
                        else
                        {
                            returnResp.Statuscode = RechargeRespType.PENDING;
                            returnResp.Msg = nameof(RechargeRespType.PENDING);
                            returnResp.ErrorCode = ErrorCodes.Request_Accpeted;
                            returnResp.LiveID = bBPSComplainRequest.ComplaintID;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                response = ex.Message + "|" + response;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "TrackComplainBillAvenue",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = 1
                });
            }
            returnResp.Response = response;
            return returnResp;
        }
        public HitRequestResponseModel BillPaymentStatus(string VendorID, string TransactionReqID)
        {
            var returnResp = new HitRequestResponseModel();
            var req = new BATransactionStatusReq
            {
                trackType = "TRANS_REF_ID",
                trackValue = VendorID
            };

            string response = string.Empty;
            var _URL = apiSetting.StatusCheckURL;
            BA_AesCryptUtil bA_AesCryptUtil = new BA_AesCryptUtil(apiSetting.Key);
            var xmlReq = XMLHelper.O.SerializeToXml(req, "transactionStatusReq", false);
            var encRequest = bA_AesCryptUtil.encrypt(xmlReq.ToString());

            StringBuilder sb = new StringBuilder();
            sb.Append("accessCode=");
            sb.Append(apiSetting.accessCode);
            sb.Append("&encRequest=");
            sb.Append(encRequest);
            sb.Append("&requestId=");
            sb.Append(TransactionReqID.PadRight(35, 'M'));
            sb.Append("&ver=");
            sb.Append(apiSetting.ver);
            sb.Append("&instituteId=");
            sb.Append(apiSetting.instituteId);
            returnResp.Request = _URL + "?" + xmlReq + "[[" + sb.ToString();
            try
            {
                response = AppWebRequest.O.CallUsingHttpWebRequest_POSTWithCER(_URL, sb.ToString(), BACertificate);
                var decResp = returnResp.Response = response;
                if (!response.Contains("errorInfo") && !response.Contains("billFetchResponse"))
                {
                    decResp = bA_AesCryptUtil.decrypt(response);
                    if (decResp != response)
                    {
                        returnResp.Response = response = decResp;
                    }
                }
            }
            catch (Exception ex)
            {
                response = ex.Message + "|" + response;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "BillPaymentStatus",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = 1
                });
            }
            returnResp.Response = response;
            return returnResp;
        }
        public HitRequestResponseModel BillPaymentStatusFromTID(int TID, string TransactionReqID)
        {
            var returnResp = new HitRequestResponseModel();
            var req = new BATransactionStatusReq
            {
                trackType = "REQUEST_ID",
                trackValue = ("A" + TID).PadRight(35, 'M')
            };

            string response = string.Empty;
            var _URL = apiSetting.StatusCheckURL;
            BA_AesCryptUtil bA_AesCryptUtil = new BA_AesCryptUtil(apiSetting.Key);
            var xmlReq = XMLHelper.O.SerializeToXml(req, "transactionStatusReq", false);
            var encRequest = bA_AesCryptUtil.encrypt(xmlReq.ToString());

            StringBuilder sb = new StringBuilder();
            sb.Append("accessCode=");
            sb.Append(apiSetting.accessCode);
            sb.Append("&encRequest=");
            sb.Append(encRequest);
            sb.Append("&requestId=");
            sb.Append(TransactionReqID.PadRight(35, 'M'));
            sb.Append("&ver=");
            sb.Append(apiSetting.ver);
            sb.Append("&instituteId=");
            sb.Append(apiSetting.instituteId);
            returnResp.Request = _URL + "?" + xmlReq + "[[" + sb.ToString();
            try
            {
                response = AppWebRequest.O.CallUsingHttpWebRequest_POSTWithCER(_URL, sb.ToString(), BACertificate);
                var decResp = returnResp.Response = response;
                if (!response.Contains("errorInfo") && !response.Contains("billFetchResponse"))
                {
                    decResp = bA_AesCryptUtil.decrypt(response);
                    if (decResp != response)
                    {
                        returnResp.Response = response = decResp;
                    }
                }
            }
            catch (Exception ex)
            {
                response = ex.Message + "|" + response;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "BillPaymentStatus",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = 1
                });
            }
            returnResp.Response = response;
            return returnResp;
        }
    }
}
