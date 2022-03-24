using Fintech.AppCode.DB;
using Fintech.AppCode.HelperClass;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using Fintech.AppCode.WebRequest;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.MiddleLayer;
using RoundpayFinTech.AppCode.Model.BBPS;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Recharge;
using RoundpayFinTech.AppCode.StaticModel.MoneyTransfer;
using RoundpayFinTech.AppCode.ThirdParty.CyberPlate;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace RoundpayFinTech.AppCode.ThirdParty.PayU
{
    public class PayuBBPSML
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly IConfiguration Configuration;
        private readonly IConnectionConfiguration _c;
        private readonly PayuAppBBPSSetting appSetting;
        private readonly IDAL _dal;

        private static long TOKEN_CREATED = 0;
        private static int EXPIRES_IN = 0;
        private static string TOKEN = string.Empty;
        private static string TOKEN_REFRESH = string.Empty;
        public PayuBBPSML(IHttpContextAccessor accessor, IHostingEnvironment env, IDAL dal)
        {
            _accessor = accessor;
            _env = env;
            var builder = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory());
            builder.AddJsonFile((_env.IsProduction() ? "appsettings.json" : "appsettings.Development.json"));
            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
            _dal = dal;
            appSetting = AppSetting();
        }
        private PayuAppBBPSSetting AppSetting()
        {
            try
            {
                return new PayuAppBBPSSetting
                {
                    client_id = Configuration["SERVICESETTING:PAYUBBPS:client_id"],
                    client_secret = Configuration["SERVICESETTING:PAYUBBPS:client_secret"],
                    grant_type = Configuration["SERVICESETTING:PAYUBBPS:grant_type"],
                    scope = Configuration["SERVICESETTING:PAYUBBPS:scope"],
                    TokenAPIURL = Configuration["SERVICESETTING:PAYUBBPS:TokenAPIURL"],
                    BillerDetailByCatURL = Configuration["SERVICESETTING:PAYUBBPS:BillerDetailByCatURL"],
                    FetchBillURL = Configuration["SERVICESETTING:PAYUBBPS:FetchBillURL"],
                    BillPaymentURL = Configuration["SERVICESETTING:PAYUBBPS:BillPaymentURL"],
                    ComplainURL = Configuration["SERVICESETTING:PAYUBBPS:ComplainURL"],
                    ComplainStatusURL = Configuration["SERVICESETTING:PAYUBBPS:ComplainStatusURL"],
                    StatusCheckURL = Configuration["SERVICESETTING:PAYUBBPS:StatusCheckURL"]
                };
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "PayuAppBBPSSetting",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser
                });
            }
            return new PayuAppBBPSSetting();
        }

        private void TokenGeneration()
        {
            string _request = string.Empty, _response = string.Empty;
            if ((EXPIRES_IN - (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - TOKEN_CREATED) / 1000) < 5 || string.IsNullOrEmpty(TOKEN))
            {
                var req = new PayUTokenRequest
                {
                    client_id = appSetting.client_id,
                    client_secret = appSetting.client_secret,
                    grant_type = appSetting.grant_type,
                    scope = appSetting.scope
                };
                StringBuilder sb = new StringBuilder();
                sb.Append(nameof(req.client_id));
                sb.Append("=");
                sb.Append(req.client_id);
                sb.Append("&");
                sb.Append(nameof(req.client_secret));
                sb.Append("=");
                sb.Append(req.client_secret);
                sb.Append("&");
                sb.Append(nameof(req.grant_type));
                sb.Append("=");
                sb.Append(req.grant_type);
                sb.Append("&");
                sb.Append(nameof(req.scope));
                sb.Append("=");
                sb.Append(req.scope);

                _request = appSetting.TokenAPIURL + "?" + sb.ToString();
                var apiResp = AppWebRequest.O.CallUsingHttpWebRequest_POST(appSetting.TokenAPIURL, sb.ToString());
                _response = apiResp;
                if (!string.IsNullOrEmpty(apiResp))
                {
                    try
                    {
                        var res = JsonConvert.DeserializeObject<PayUTokenResponse>(apiResp);
                        if (res.expires_in > 0)
                        {
                            EXPIRES_IN = res.expires_in;
                            TOKEN_CREATED = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                            TOKEN = res.access_token;
                            TOKEN_REFRESH = res.refresh_token;
                        }
                    }
                    catch (Exception ex)
                    {
                        _response = "Exception:" + ex.Message + "||" + _response;
                    }
                }
            }
            if (!string.IsNullOrEmpty(_request) || !string.IsNullOrEmpty(_response))
            {
                IProcedure proc = new ProcLogAPITokenGeneration(_dal);
                proc.Call(new CommonReq
                {
                    str = APICode.PAYUBBPS,
                    CommonStr = _request,
                    CommonStr2 = _response
                });
            }
        }
        public PayUBillerResponse GetBillerByCategory(string billerCategoryName)
        {
            TokenGeneration();
            var bResp = new PayUBillerResponse { };
            var headers = new Dictionary<string, string>
            {
                { "Authorization", "bearer "+TOKEN }
            };
            var response = string.Empty;
            var request = appSetting.BillerDetailByCatURL + billerCategoryName + "|" + JsonConvert.SerializeObject(headers);
            try
            {

                //response = AppWebRequest.O.PostJsonDataUsingHWRTLS(appSetting.BillerDetailByCatURL, new { }, headers).Result;
                response= AppWebRequest.O.CallUsingHttpWebRequest_GETAsync(appSetting.BillerDetailByCatURL + billerCategoryName, headers).Result;

                if (!string.IsNullOrEmpty(response))
                {
                    bResp = JsonConvert.DeserializeObject<PayUBillerResponse>(response);
                }
            }
            catch (Exception ex)
            {
                response = ex.Message + "|" + response;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "GetBillerByCategory",
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
                CommonStr = request,
                CommonStr2 = response
            });
            return bResp;
        }

        public BBPSResponse FetchBill(BBPSLog bbpsLog)
        {
            TokenGeneration();
            var billResponse = new BBPSResponse
            {
                IsEditable = false,
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.URLNOTFOUND,
                ErrorCode = ErrorCodes.Unknown_Error.ToString(),
                ErrorMsg = ErrorCodes.URLNOTFOUND,
                IsEnablePayment=false,
                IsShowMsgOnly=true
            };
            var headers = new Dictionary<string, string>
            {
                { "Authorization", "bearer "+TOKEN }
            };
            var customerParams = new Dictionary<string, string>
            {
                { bbpsLog.APIReqHelper.AccountNoKey,bbpsLog.AccountNumber}
            };
            if (bbpsLog.APIReqHelper.OpParams != null)
            {
                if (bbpsLog.APIReqHelper.OpParams.Count > 0)
                {
                    bbpsLog.APIReqHelper.OpParams.RemoveAll(x => x.IsAccountNo == true);
                    for (int i = 0; i < bbpsLog.APIReqHelper.OpParams.Count; i++)
                    {
                        if (i == 0)
                        {
                            customerParams.Add(bbpsLog.APIReqHelper.OpParams[i].Param, bbpsLog.Optional1 ?? string.Empty);
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
                            customerParams.Add(bbpsLog.APIReqHelper.OpParams[i].Param, bbpsLog.Optional2 ?? string.Empty);
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
                            customerParams.Add(bbpsLog.APIReqHelper.OpParams[i].Param, bbpsLog.Optional3 ?? string.Empty);
                            if (!string.IsNullOrEmpty(bbpsLog.APIReqHelper.OpParams[i].RegEx))
                            {
                                if (!Regex.IsMatch((bbpsLog.Optional3 ?? string.Empty), bbpsLog.APIReqHelper.OpParams[i].RegEx))
                                {
                                    billResponse.Msg = string.Format("{0} {1}", nameof(ErrorCodes.Invalid_Parameter).Replace("_", " "), "Optional3");
                                    billResponse.ErrorCode = ErrorCodes.Invalid_Parameter.ToString();
                                    billResponse.ErrorMsg = billResponse.Msg;
                                    billResponse.IsShowMsgOnly = true;
                                    return billResponse;
                                }
                            }
                        }
                        if (i == 3)
                        {
                            customerParams.Add(bbpsLog.APIReqHelper.OpParams[i].Param, bbpsLog.Optional4 ?? string.Empty);
                            if (!string.IsNullOrEmpty(bbpsLog.APIReqHelper.OpParams[i].RegEx))
                            {
                                if (!Regex.IsMatch((bbpsLog.Optional4 ?? string.Empty), bbpsLog.APIReqHelper.OpParams[i].RegEx))
                                {
                                    billResponse.Msg = string.Format("{0} {1}", nameof(ErrorCodes.Invalid_Parameter).Replace("_", " "), "Optional4");
                                    billResponse.ErrorCode = ErrorCodes.Invalid_Parameter.ToString();
                                    billResponse.ErrorMsg = billResponse.Msg;
                                    billResponse.IsShowMsgOnly = true;
                                    return billResponse;
                                }
                            }
                        }
                    }
                }
            }
            var deviceDetailsINT = new
            {
                INITIATING_CHANNEL = "INT",
                MAC = string.IsNullOrEmpty(bbpsLog.APIReqHelper.MAC) ? "54-E1-AD-27-FE-E3" : bbpsLog.APIReqHelper.MAC,
                IP = bbpsLog.APIReqHelper.IPAddress
            };
            var deviceDetailsAGT = new
            {
                INITIATING_CHANNEL = "AGT",
                TERMINAL_ID = bbpsLog.OutletMobileNo,
                MOBILE = bbpsLog.OutletMobileNo,
                GEOCODE = bbpsLog.GeoLocation,
                POSTAL_CODE = bbpsLog.Pincode
            };
            var reqObj = new
            {
                agentId = (bbpsLog.APIReqHelper.InitChanel ?? string.Empty).StartsWith("INT") ? bbpsLog.aPIDetail.OnlineOutletID : bbpsLog.aPIDetail.APIOutletID,
                billerId = bbpsLog.APIReqHelper.BillerID ?? string.Empty,
                customerName = bbpsLog.CustomerName ?? string.Empty,
                customerPhoneNumber = bbpsLog.CustomerMobile ?? string.Empty,
                customerParams,
                deviceDetails = (bbpsLog.APIReqHelper.InitChanel ?? string.Empty).StartsWith("INT") ? (object)deviceDetailsINT : deviceDetailsAGT,
                timeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                refId = bbpsLog.SessionNo.PadRight(35, 'P')
            };
            var response = string.Empty;
            var request = appSetting.FetchBillURL + "?" + JsonConvert.SerializeObject(reqObj) + "|" + JsonConvert.SerializeObject(headers);
            bbpsLog.Request = request;
            try
            {
                response = AppWebRequest.O.PostJsonDataUsingHWRTLS(appSetting.FetchBillURL, reqObj, headers).Result;

                if (!string.IsNullOrEmpty(response))
                {
                    var bResp = JsonConvert.DeserializeObject<PayUBillFetchResponse>(response);
                    if (bResp.code == 200)
                    {
                        bbpsLog.helper.Status =ErrorCodes.One;
                        billResponse.Statuscode = ErrorCodes.One;
                        billResponse.ErrorCode = ErrorCodes.Transaction_Successful.ToString();
                        billResponse.ErrorMsg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                        billResponse.Msg = billResponse.Msg;
                        billResponse.IsEnablePayment = true;
                        billResponse.IsShowMsgOnly = false;
                        billResponse.BillNumber = bbpsLog.AccountNumber;
                        billResponse.BillDate = string.IsNullOrEmpty(bResp.payload.billDate) ? string.Empty : Convert.ToDateTime(bResp.payload.billDate).ToString("dd MMM yyyy");
                        billResponse.CustomerName = bResp.payload.accountHolderName;
                        billResponse.BillPeriod = string.Empty;
                        billResponse.DueDate = string.IsNullOrEmpty(bResp.payload.dueDate) ? string.Empty : Convert.ToDateTime(bResp.payload.dueDate).ToString("dd MMM yyyy");
                        billResponse.Amount = bResp.payload.amount.ToString();
                        bbpsLog.Amount = Convert.ToDecimal(billResponse.Amount);
                        bbpsLog.BillNumber = billResponse.BillNumber;
                        bbpsLog.CustomerName = billResponse.CustomerName;
                        bbpsLog.BillDate = billResponse.BillDate;
                        bbpsLog.DueDate = billResponse.DueDate;
                        bbpsLog.BillPeriod = billResponse.BillPeriod;
                        if (bResp.payload.additionalParams != null)
                        {
                            int i = 0;
                            billResponse.billAdditionalInfo = new List<BillAdditionalInfo>();
                            foreach (dynamic item in bResp.payload.additionalParams)
                            {
                                billResponse.billAdditionalInfo.Add(new BillAdditionalInfo
                                {
                                    InfoName = item.Name,
                                    InfoValue =item.Value
                                });
                                bbpsLog.helper.tpBFAInfo.Rows.Add(new object[] { item.Name, item.Value, i + 1 });
                                if (string.IsNullOrEmpty(bbpsLog.helper.EarlyPaymentDate) && !string.IsNullOrEmpty(bbpsLog.APIReqHelper.EarlyPaymentDateKey))
                                {
                                    if (item.Name.Equals(bbpsLog.APIReqHelper.EarlyPaymentDateKey))
                                    {
                                        bbpsLog.helper.EarlyPaymentDate = Convert.ToDateTime(item.Value).ToString("dd MMM yyyy");
                                    }
                                }
                                if (!string.IsNullOrEmpty(bbpsLog.APIReqHelper.BillMonthKey) && string.IsNullOrEmpty(bbpsLog.BillMonth))
                                {
                                    if (!string.IsNullOrEmpty(item.Name).Equals(bbpsLog.APIReqHelper.BillMonthKey))
                                    {
                                        bbpsLog.BillMonth = item.Value;
                                        billResponse.BillMonth = string.IsNullOrEmpty(item.Value) ? string.Empty : Convert.ToDateTime(item.Value).ToString("dd MMM yyyy");
                                    }

                                }
                                i++;
                            }
                        }
                    }
                    else
                    {
                        billResponse.IsShowMsgOnly = true;
                        IErrorCodeML errorCodeML = new ErrorCodeML(_accessor, _env, false);
                        var eFromDB = errorCodeML.GetAPIErrorCodeDescription(bbpsLog.aPIDetail.GroupCode, bResp.payload.errors[0].errorCode);
                        billResponse.ErrorMsg = billResponse.Msg = (eFromDB.Error ?? string.Empty).Replace("{AccountKey}", bbpsLog.APIReqHelper.AccountNoKey);
                        billResponse.ErrorMsg = billResponse.Msg = billResponse.Msg.Replace("{REPLACE}", bResp.payload.errors[0].reason);
                        billResponse.ErrorMsg = billResponse.Msg = bbpsLog.helper.Reason = string.IsNullOrEmpty(billResponse.Msg) ? nameof(ErrorCodes.Unable_to_get_bill_details_Please_try_again_after_few_minute).Replace("_", " ") : billResponse.Msg;
                        billResponse.ErrorCode = eFromDB.Code;
                    }
                }
            }
            catch (Exception ex)
            {
                response = ex.Message + "|" + response;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "FetchBill",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = 1
                });
            }
            bbpsLog.Response = response;
            return billResponse;
        }
        public HitRequestResponseModel BillPayment(BBPSPaymentRequest bBPSRequest)
        {
            TokenGeneration();
            var returnResp = new HitRequestResponseModel { };
            var headers = new Dictionary<string, string>
            {
                { "Authorization", "bearer "+TOKEN }
            };
            var customerParams = new Dictionary<string, string>
            {
                { bBPSRequest.AccountNoKey,bBPSRequest.AccountNo}
            };
            if (bBPSRequest.OpParams != null)
            {
                if (bBPSRequest.OpParams.Count > 0)
                {
                    bBPSRequest.OpParams.RemoveAll(x => x.IsAccountNo == true);
                    for (int i = 0; i < bBPSRequest.OpParams.Count; i++)
                    {
                        if (i == 0)
                        {
                            customerParams.Add(bBPSRequest.OpParams[i].Param, bBPSRequest.Optional1 ?? string.Empty);
                        }
                        if (i == 1)
                        {
                            customerParams.Add(bBPSRequest.OpParams[i].Param, bBPSRequest.Optional2 ?? string.Empty);
                        }
                        if (i == 2)
                        {
                            customerParams.Add(bBPSRequest.OpParams[i].Param, bBPSRequest.Optional3 ?? string.Empty);
                        }
                        if (i == 3)
                        {
                            customerParams.Add(bBPSRequest.OpParams[i].Param, bBPSRequest.Optional4 ?? string.Empty);
                        }
                    }
                }
            }
            var deviceDetailsINT = new
            {
                INITIATING_CHANNEL = "INT",
                MAC = string.IsNullOrEmpty(bBPSRequest.MAC) ? "54-E1-AD-27-FE-E3" : bBPSRequest.MAC,
                IP = bBPSRequest.IPAddress.Equals("::1") ? "122.176.71.56" : bBPSRequest.IPAddress
            };
            var deviceDetailsAGT = new
            {
                INITIATING_CHANNEL = "AGT",
                TERMINAL_ID = bBPSRequest.UserMobileNo,
                MOBILE = bBPSRequest.UserMobileNo,
                GEOCODE = bBPSRequest.GeoLocation,
                POSTAL_CODE = bBPSRequest.Pincode
            };
            var paramsPayMode = new object();
            if (bBPSRequest.PaymentMode.ToUpper().Equals("WALLET"))
            {
                paramsPayMode = new { walletName = "PayTM" };
            }
            else if (bBPSRequest.PaymentMode.ToUpper().Equals("UPI"))
            {
                paramsPayMode = new { walletName = bBPSRequest.PaymentModeInAPI };
            }
            var reqObj = new
            {
                agentId = bBPSRequest.APIOutletID,
                billerId = bBPSRequest.BillerID ?? string.Empty,
                customerParams,
                deviceDetails = (bBPSRequest.InitChanel ?? string.Empty).StartsWith("INT") ? (object)deviceDetailsINT : deviceDetailsAGT,
                timeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                refId = ("A" + bBPSRequest.TID).PadRight(35, 'P'),
                isQuickPay = bBPSRequest.IsQuickPay,
                additionalParams = new { agentTxnID = bBPSRequest.TransactionID },
                planId = "NA",
                paymentDetails = new PayUPaymentDetail
                {
                    paymentMode = bBPSRequest.PaymentMode.ToUpper(),
                    ParamsPayMode = paramsPayMode
                },
                paidAmount = bBPSRequest.RequestedAmount,
                paymentName = "BillPay",
                userDetails = new
                {
                    firstName = (bBPSRequest.UserName ?? string.Empty).Split(' ')[0],
                    phone = bBPSRequest.UserMobileNo ?? string.Empty,
                    email = bBPSRequest.UserEmailID ?? string.Empty
                }
            };
            var response = string.Empty;
            var request = appSetting.BillPaymentURL + "?" + JsonConvert.SerializeObject(reqObj) + "|" + JsonConvert.SerializeObject(headers);
            returnResp.Request = request;
            try
            {
                response = AppWebRequest.O.PostJsonDataUsingHWRTLS(appSetting.BillPaymentURL, reqObj, headers).Result;
            }
            catch (Exception ex)
            {
                response = ex.Message + "|" + response;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "FetchBill",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = 1
                });
            }
            returnResp.Response = response;
            return returnResp;
        }
        public BBPSComplainAPIResponse RaiseComplain(BBPSComplainRequest bBPSComplainRequest)
        {
            TokenGeneration();
            var returnResp = new BBPSComplainAPIResponse
            {

            };

            var headers = new Dictionary<string, string>
            {
                { "Authorization", "bearer "+TOKEN }
            };
            string response = string.Empty;
            string TransDate = !string.IsNullOrEmpty(bBPSComplainRequest.TransactionID) ? ConnectionStringHelper.O.ConvertTransactionIDTo_dd_MMM_yyyy(bBPSComplainRequest.TransactionID ?? string.Empty) : string.Empty;
            if (!string.IsNullOrEmpty(TransDate))
            {
                TransDate = Convert.ToDateTime(TransDate.Replace(" ", "/")).ToString("yyyy-MM-dd");
            }
            var reqObj = new
            {
                fetchParameters = new
                {
                    complaintType = "Transaction",
                    description = bBPSComplainRequest.Description,
                    disposition = bBPSComplainRequest.Reason,
                    txnReferenceId = bBPSComplainRequest.TransactionID,
                },
                xchangeId = "501"
            };
            returnResp.Request = (appSetting.ComplainURL ?? string.Empty) + "?" + JsonConvert.SerializeObject(reqObj) + "|" + JsonConvert.SerializeObject(headers);
            try
            {
                returnResp.Response = AppWebRequest.O.PostJsonDataUsingHWRTLS(appSetting.ComplainStatusURL, reqObj, headers).Result;

                if (!string.IsNullOrEmpty(returnResp.Response))
                {
                    var _apiRes = JsonConvert.DeserializeObject<PayUComplainResp>(returnResp.Response);
                    if (_apiRes != null)
                    {
                        if (_apiRes.code == 200)
                        {
                            returnResp.Statuscode = 1;
                            returnResp.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                            returnResp.ErrorCode = ErrorCodes.Transaction_Successful;
                            returnResp.LiveID = _apiRes.payload.complaintId;
                        }
                        else
                        {
                            returnResp.Statuscode = 3;
                            if (_apiRes.payload.errors != null)
                            {
                                if (_apiRes.payload.errors.Count > 0)
                                {
                                    returnResp.Msg = _apiRes.payload.errors[0].reason;
                                    returnResp.ErrorCode = ErrorCodes.Unknown_Error;
                                    returnResp.LiveID = _apiRes.payload.errors[0].reason;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                returnResp.Response = ex.Message + "|" + response;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "RaiseComplain",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = 1
                });
            }
            return returnResp;
        }
        public BBPSComplainAPIResponse CheckComplainStatus(BBPSComplainRequest bBPSComplainRequest)
        {
            TokenGeneration();
            var returnResp = new BBPSComplainAPIResponse
            {

            };
            var headers = new Dictionary<string, string>
            {
                { "Authorization", "bearer "+TOKEN }
            };
            string response = string.Empty;
            string TransDate = !string.IsNullOrEmpty(bBPSComplainRequest.TransactionID) ? ConnectionStringHelper.O.ConvertTransactionIDTo_dd_MMM_yyyy(bBPSComplainRequest.TransactionID ?? string.Empty) : string.Empty;
            if (!string.IsNullOrEmpty(TransDate))
            {
                TransDate = Convert.ToDateTime(TransDate.Replace(" ", "/")).ToString("yyyy-MM-dd");
            }
            var reqObj = new
            {
                fetchParameters = new
                {
                    complaintType = "Transaction",
                    complaintId = bBPSComplainRequest.ComplaintID
                },
                xchangeId = "506"
            };
            returnResp.Request = (appSetting.ComplainStatusURL ?? string.Empty) + "?" + JsonConvert.SerializeObject(reqObj) + "|" + JsonConvert.SerializeObject(headers);
            try
            {
                returnResp.Response = AppWebRequest.O.PostJsonDataUsingHWRTLS(appSetting.ComplainStatusURL, reqObj, headers).Result;

                if (!string.IsNullOrEmpty(returnResp.Response))
                {
                    var _apiRes = JsonConvert.DeserializeObject<PayUComplainResp>(returnResp.Response);
                    if (_apiRes != null)
                    {
                        if (_apiRes.code == 200)
                        {
                            returnResp.Statuscode = 1;
                            returnResp.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                            returnResp.ErrorCode = ErrorCodes.Transaction_Successful;
                            returnResp.LiveID = _apiRes.payload.complaintId;
                        }
                        else
                        {
                            returnResp.Statuscode = 3;
                            if (_apiRes.payload.errors != null)
                            {
                                if (_apiRes.payload.errors.Count > 0)
                                {
                                    returnResp.Msg = _apiRes.payload.errors[0].reason;
                                    returnResp.ErrorCode = ErrorCodes.Unknown_Error;
                                    returnResp.Remark = _apiRes.payload.errors[0].reason;
                                }
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                returnResp.Response = ex.Message + "|" + response;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "CheckComplainStatus",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = 1
                });
            }
            return returnResp;
        }
        public HitRequestResponseModel BillPaymentStatus(int TID)
        {
            TokenGeneration();
            var returnResp = new HitRequestResponseModel();

            var headers = new Dictionary<string, string>
            {
                { "Authorization", "bearer "+TOKEN }
            };
            string _URL = appSetting.StatusCheckURL + ("A" + TID).PadRight(35, 'P');
            returnResp.Request = _URL + "|" + JsonConvert.SerializeObject(headers);
            try
            {
                returnResp.Response = AppWebRequest.O.CallUsingHttpWebRequest_GETAsync(_URL, headers).Result;
            }
            catch (Exception x)
            {
                returnResp.Response = x.Message + "|" + returnResp.Response;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "BillPaymentStatus",
                    Error = x.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = 1
                });
            }
            return returnResp;
        }
    }
}
