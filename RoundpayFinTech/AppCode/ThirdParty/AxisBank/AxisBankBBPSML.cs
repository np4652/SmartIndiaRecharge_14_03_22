using Fintech.AppCode.Configuration;
using Fintech.AppCode.DB;
using Fintech.AppCode.HelperClass;
using Fintech.AppCode.StaticModel;
using Fintech.AppCode.WebRequest;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RoundpayFinTech.AppCode.Configuration;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.MiddleLayer;
using RoundpayFinTech.AppCode.Model.BBPS;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Recharge;
using RoundpayFinTech.AppCode.ThirdParty.CyberPlate;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.ThirdParty.AxisBank
{
    public class AxisBankBBPSML
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly IConfiguration Configuration;
        private readonly IConnectionConfiguration _c;
        private readonly AxisBankSetting appSetting;
        private readonly IDAL _dal;
        public AxisBankBBPSML(IHttpContextAccessor accessor, IHostingEnvironment env, IDAL dal)
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
        private AxisBankSetting AppSetting()
        {
            try
            {
                return new AxisBankSetting
                {
                    axischannelid = Configuration["SERVICESETTING:AXSBNK:axischannelid"],
                    axisclientid = Configuration["SERVICESETTING:AXSBNK:axisclientid"],
                    axisclientsecret = Configuration["SERVICESETTING:AXSBNK:axisclientsecret"],
                    axisencryptionkey = Configuration["SERVICESETTING:AXSBNK:axisencryptionkey"],
                    axissaltkey = Configuration["SERVICESETTING:AXSBNK:axissaltkey"],
                    axischannelpassword = Configuration["SERVICESETTING:AXSBNK:axischannelpassword"],
                    axisbodychannelid = Configuration["SERVICESETTING:AXSBNK:axisbodychannelid"],
                    BillerListURL = Configuration["SERVICESETTING:AXSBNK:BillerListURL"],
                    BillerFieldsURL = Configuration["SERVICESETTING:AXSBNK:BillerFieldsURL"],
                    BillerDetailsURL = Configuration["SERVICESETTING:AXSBNK:BillerDetailsURL"],
                    BillFetchURL = Configuration["SERVICESETTING:AXSBNK:BillFetchURL"],
                    FetchedBillURL = Configuration["SERVICESETTING:AXSBNK:FetchedBillURL"],
                    PaymentURL = Configuration["SERVICESETTING:AXSBNK:PaymentURL"],
                    PaymentStatusURL = Configuration["SERVICESETTING:AXSBNK:PaymentStatusURL"],
                    RaiseComplainURL = Configuration["SERVICESETTING:AXSBNK:RaiseComplainURL"],
                    accountNumber = Configuration["SERVICESETTING:AXSBNK:accountNumber"],
                    accountHolderName = Configuration["SERVICESETTING:AXSBNK:accountHolderName"],
                    ComplainStatusURL = Configuration["SERVICESETTING:AXSBNK:ComplainStatusURL"]
                };
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "AxisBankSetting",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser
                });
            }
            return new AxisBankSetting();
        }
        public AxisBankBillerListObject GetBillerList(string APIOpTypeID)
        {
            var bResp = new AxisBankBillerListObject
            {
            };
            var headers = new Dictionary<string, string>
            {
                { "axis-channel-id", appSetting.axischannelid },
                { "axis-client-id", appSetting.axisclientid },
                { "axis-client-secret", appSetting.axisclientsecret},
                { "axis-encryption-key", appSetting.axisencryptionkey },
                { "axis-salt-key", appSetting.axissaltkey },
                { "axis-channel-password", appSetting.axischannelpassword},
                { "axis-body-channel-id", appSetting.axisbodychannelid }
            };
            var reqObj = new { categoryCode = APIOpTypeID };
            var response = string.Empty;
            var request = appSetting.BillerListURL + "|" + JsonConvert.SerializeObject(headers) + "?" + JsonConvert.SerializeObject(reqObj);
            try
            {
                if (ApplicationSetting.IsHostedServerIsIndian)
                {
                    response = AppWebRequest.O.PostJsonDataUsingHWRTLS(appSetting.BillerListURL, reqObj, headers).Result;
                }
                else
                {
                    response = AppWebRequest.O.PostJsonDataUsingHWR("http://is.roundpay.net/API/LBPostJsonOnly", new { RequestURI = appSetting.BillerListURL, obj = reqObj, objHeader = headers, WithHeader = true });
                    try
                    {
                        response = JsonConvert.DeserializeObject<string>(response);
                    }
                    catch (Exception)
                    {
                    }
                }

                if (!string.IsNullOrEmpty(response))
                {
                    bResp = JsonConvert.DeserializeObject<AxisBankBillerListObject>(response);
                }
            }
            catch (Exception ex)
            {
                response = ex.Message + "|" + response;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "GetBillerList",
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
        public AxisBankBillerFieldsResponse AxisBankGetBillerFields(string billerID, string APIOpTypeID)
        {
            var bResp = new AxisBankBillerFieldsResponse { };
            var headers = new Dictionary<string, string>
            {
                { "axis-channel-id", appSetting.axischannelid },
                { "axis-client-id", appSetting.axisclientid },
                { "axis-client-secret", appSetting.axisclientsecret},
                { "axis-encryption-key", appSetting.axisencryptionkey },
                { "axis-salt-key", appSetting.axissaltkey },
                { "axis-channel-password", appSetting.axischannelpassword},
                { "axis-body-channel-id", appSetting.axisbodychannelid }
            };
            var reqObj = new { billerId = billerID, categoryCode = APIOpTypeID };
            var response = string.Empty;
            var request = appSetting.BillerFieldsURL + "|" + JsonConvert.SerializeObject(headers) + "?" + JsonConvert.SerializeObject(reqObj);
            try
            {

                if (ApplicationSetting.IsHostedServerIsIndian)
                {
                    response = AppWebRequest.O.PostJsonDataUsingHWRTLS(appSetting.BillerFieldsURL, reqObj, headers).Result;
                }
                else
                {
                    response = AppWebRequest.O.PostJsonDataUsingHWR("http://is.roundpay.net/API/LBPostJsonOnly", new { RequestURI = appSetting.BillerFieldsURL, obj = reqObj, objHeader = headers, WithHeader = true });
                    try
                    {
                        response = JsonConvert.DeserializeObject<string>(response);
                    }
                    catch (Exception)
                    {
                    }
                }

                if (!string.IsNullOrEmpty(response))
                {
                    bResp = JsonConvert.DeserializeObject<AxisBankBillerFieldsResponse>(response);
                }
            }
            catch (Exception ex)
            {
                response = ex.Message + "|" + response;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "AxisBankGetBillerFields",
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
        public AxisBankBillerDetailResponse AxisBankGetBillerDetails(string billerID, string APIOpTypeID)
        {
            var bResp = new AxisBankBillerDetailResponse { };
            var headers = new Dictionary<string, string>
            {
                { "axis-channel-id", appSetting.axischannelid },
                { "axis-client-id", appSetting.axisclientid },
                { "axis-client-secret", appSetting.axisclientsecret},
                { "axis-encryption-key", appSetting.axisencryptionkey },
                { "axis-salt-key", appSetting.axissaltkey },
                { "axis-channel-password", appSetting.axischannelpassword},
                { "axis-body-channel-id", appSetting.axisbodychannelid }
            };
            var reqObj = new { billerId = billerID, categoryCode = APIOpTypeID };
            var response = string.Empty;
            var request = appSetting.BillerDetailsURL + "|" + JsonConvert.SerializeObject(headers) + "?" + JsonConvert.SerializeObject(reqObj);
            try
            {

                if (ApplicationSetting.IsHostedServerIsIndian)
                {
                    response = AppWebRequest.O.PostJsonDataUsingHWRTLS(appSetting.BillerDetailsURL, reqObj, headers).Result;
                }
                else
                {
                    response = AppWebRequest.O.PostJsonDataUsingHWR("http://is.roundpay.net/API/LBPostJsonOnly", new { RequestURI = appSetting.BillerDetailsURL, obj = reqObj, objHeader = headers, WithHeader = true });
                    try
                    {
                        response = JsonConvert.DeserializeObject<string>(response);
                    }
                    catch (Exception)
                    {
                    }
                }
                if (!string.IsNullOrEmpty(response))
                {
                    bResp = JsonConvert.DeserializeObject<AxisBankBillerDetailResponse>(response);
                }
            }
            catch (Exception ex)
            {
                response = ex.Message + "|" + response;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "AxisBankGetBillerDetails",
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
        public async Task<BBPSResponse> AxisBankBillFetchRequest(BBPSLog bbpsLog)
        {
            var billResponse = new BBPSResponse
            {
                IsEditable = false,
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.URLNOTFOUND,
                ErrorCode = ErrorCodes.Unknown_Error.ToString(),
                ErrorMsg = ErrorCodes.URLNOTFOUND,
                IsEnablePayment = false
            };
            var headers = new Dictionary<string, string>
            {
                { "axis-channel-id", appSetting.axischannelid },
                { "axis-client-id", appSetting.axisclientid },
                { "axis-client-secret", appSetting.axisclientsecret},
                { "axis-encryption-key", appSetting.axisencryptionkey },
                { "axis-salt-key", appSetting.axissaltkey },
                { "axis-channel-password", appSetting.axischannelpassword},
                { "axis-body-channel-id", appSetting.axisbodychannelid }
            };
            string guid = Guid.NewGuid().ToString();
            var reqObj = new AxisBankBillFetchReqModel
            {
                billerId = bbpsLog.APIReqHelper.BillerID,
                categoryCode = bbpsLog.APIReqHelper.APIOpTypeID,
                mobileNumber = bbpsLog.CustomerMobile,
                agent = new AxisBankAgent
                {
                    app = (bbpsLog.APIReqHelper.InitChanel ?? string.Empty).Equals("MOB") ? appSetting.axischannelid : string.Empty,
                    channel = bbpsLog.APIReqHelper.InitChanel,
                    geocode = (bbpsLog.APIReqHelper.InitChanel ?? string.Empty).Equals("AGT") ? bbpsLog.GeoLocation : string.Empty,
                    id = bbpsLog.aPIDetail.APIOutletID,//(bbpsLog.APIReqHelper.InitChanel ?? string.Empty).StartsWith("INT") ? bbpsLog.aPIDetail.OnlineOutletID : 
                    ifsc = string.Empty,
                    imei = (bbpsLog.APIReqHelper.InitChanel ?? string.Empty).Equals("MOB") ? bbpsLog.IMEI : string.Empty,
                    ip = (bbpsLog.APIReqHelper.InitChanel ?? string.Empty).Equals("AGT") ? string.Empty : (bbpsLog.APIReqHelper.IPAddress == "::1" ? "122.176.71.56" : (bbpsLog.APIReqHelper.IPAddress ?? string.Empty)),
                    mac = !(bbpsLog.APIReqHelper.InitChanel ?? string.Empty).Equals("INT") ? string.Empty : "48-4D-7E-CB-DB-6F",
                    mobile = (bbpsLog.APIReqHelper.InitChanel ?? string.Empty).StartsWith("AGT") ? bbpsLog.OutletMobileNo : string.Empty,
                    os = (bbpsLog.APIReqHelper.InitChanel ?? string.Empty).Equals("MOB") ? "android" : string.Empty,
                    postalCode = (bbpsLog.APIReqHelper.InitChanel ?? string.Empty).Equals("AGT") ? bbpsLog.Pincode : string.Empty,
                    terminalId = (bbpsLog.APIReqHelper.InitChanel ?? string.Empty).Equals("AGT") ? bbpsLog.UserID.ToString() : string.Empty
                },
                customerParams = new List<AxisBankNameValue>()
            };
            if (bbpsLog.APIReqHelper.OpParams != null)
            {
                if (bbpsLog.APIReqHelper.OpParams.Count > 0)
                {
                    bbpsLog.APIReqHelper.OpParams.RemoveAll(x => x.IsAccountNo == true);
                    reqObj.customerParams.Add(new AxisBankNameValue
                    {
                        name = bbpsLog.APIReqHelper.AccountNoKey,
                        value = bbpsLog.AccountNumber ?? string.Empty
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
                        reqObj.customerParams.Add(new AxisBankNameValue
                        {
                            name = bbpsLog.APIReqHelper.OpParams[i].Param,
                            value = bbpsLog.Optional1 ?? string.Empty
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
                        reqObj.customerParams.Add(new AxisBankNameValue
                        {
                            name = bbpsLog.APIReqHelper.OpParams[i].Param,
                            value = bbpsLog.Optional2 ?? string.Empty
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
                        reqObj.customerParams.Add(new AxisBankNameValue
                        {
                            name = bbpsLog.APIReqHelper.OpParams[i].Param,
                            value = bbpsLog.Optional3 ?? string.Empty
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
                        reqObj.customerParams.Add(new AxisBankNameValue
                        {
                            name = bbpsLog.APIReqHelper.OpParams[i].Param,
                            value = bbpsLog.Optional4 ?? string.Empty
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
            string APIContext = string.Empty;
            var _URL = appSetting.BillFetchURL;
            try
            {
                bbpsLog.Request = string.Format("{0}?{1}", _URL, JsonConvert.SerializeObject(reqObj) + "]]" + JsonConvert.SerializeObject(headers));


                if (ApplicationSetting.IsHostedServerIsIndian)
                {
                    response = AppWebRequest.O.PostJsonDataUsingHWRTLS(_URL, reqObj, headers).Result;
                }
                else
                {
                    response = AppWebRequest.O.PostJsonDataUsingHWR("http://is.roundpay.net/API/LBPostJsonOnly", new { RequestURI = _URL, obj = reqObj, objHeader = headers, WithHeader = true });
                    try
                    {
                        response = JsonConvert.DeserializeObject<string>(response);
                    }
                    catch (Exception)
                    {
                    }
                }
                bbpsLog.Response = response;
                if (!string.IsNullOrEmpty(response))
                {
                    var bResp = JsonConvert.DeserializeObject<AxisBankBillFetchPreResponse>(response);
                    if (bResp.statusCode.Equals("success"))
                    {
                        int i = 0;
                        int LoopTill = 12;
                    RETRY:
                        bbpsLog.APIContext = bResp.data.context;
                        var hitReqResp = AxisBankGetFetchedBill(bResp.data.context);
                        bbpsLog.Request = bbpsLog.Request + "____________" + hitReqResp.Request;
                        bbpsLog.Response = bbpsLog.Response + "____________" + hitReqResp.Response;
                        if (!string.IsNullOrEmpty(hitReqResp.Response))
                        {
                            if (!hitReqResp.IsException)
                            {
                                var bRespFetch = JsonConvert.DeserializeObject<AxisBankBillFetchResponse>(hitReqResp.Response);
                                if (bRespFetch.data != null)
                                {
                                    if (bRespFetch.data.fetchAPIStatus.Equals("Active") && bRespFetch.statusCode == "success")
                                    {
                                        bbpsLog.helper.Status = ErrorCodes.One;
                                        billResponse.Statuscode = ErrorCodes.One;
                                        billResponse.ErrorCode = ErrorCodes.Transaction_Successful.ToString();
                                        billResponse.ErrorMsg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                                        billResponse.Msg = billResponse.Msg;
                                        billResponse.IsEnablePayment = true;
                                        billResponse.BillNumber = bRespFetch.data.bill.billNumber;
                                        billResponse.BillDate = string.IsNullOrEmpty(bRespFetch.data.bill.billDate) ? string.Empty : Convert.ToDateTime(bRespFetch.data.bill.billDate).ToString("dd MMM yyyy");
                                        billResponse.CustomerName = bRespFetch.data.bill.customerName;
                                        billResponse.BillPeriod = bRespFetch.data.bill.billPeriod;
                                        billResponse.DueDate = string.IsNullOrEmpty(bRespFetch.data.bill.dueDate) ? string.Empty : Convert.ToDateTime(bRespFetch.data.bill.dueDate).ToString("dd MMM yyyy");
                                        billResponse.Amount = (Convert.ToDecimal(string.IsNullOrEmpty(bRespFetch.data.bill.amount) ? "0" : bRespFetch.data.bill.amount)).ToString();
                                        bbpsLog.Amount = Convert.ToDecimal(billResponse.Amount);
                                        bbpsLog.BillNumber = billResponse.BillNumber;
                                        bbpsLog.CustomerName = billResponse.CustomerName;
                                        bbpsLog.BillDate = billResponse.BillDate;
                                        bbpsLog.DueDate = billResponse.DueDate;
                                        bbpsLog.BillPeriod = billResponse.BillPeriod;
                                        APIContext = bRespFetch.data.context;
                                        if (bRespFetch.data.bill.additionalInfo != null)
                                        {
                                            if (bRespFetch.data.bill.additionalInfo.Count > 0)
                                            {
                                                billResponse.billAdditionalInfo = new List<BillAdditionalInfo>();
                                                for (int j = 0; j < bRespFetch.data.bill.additionalInfo.Count; j++)
                                                {
                                                    billResponse.billAdditionalInfo.Add(new BillAdditionalInfo
                                                    {
                                                        InfoName = bRespFetch.data.bill.additionalInfo[j].name,
                                                        InfoValue = bRespFetch.data.bill.additionalInfo[j].value
                                                    });
                                                    bbpsLog.helper.tpBFAInfo.Rows.Add(new object[] { bRespFetch.data.bill.additionalInfo[j].name, bRespFetch.data.bill.additionalInfo[j].value, i + 1 });
                                                    if (string.IsNullOrEmpty(bbpsLog.helper.EarlyPaymentDate) && !string.IsNullOrEmpty(bbpsLog.APIReqHelper.EarlyPaymentDateKey))
                                                    {
                                                        if (bRespFetch.data.bill.additionalInfo[j].name.Equals(bbpsLog.APIReqHelper.EarlyPaymentDateKey))
                                                        {
                                                            bbpsLog.helper.EarlyPaymentDate = Convert.ToDateTime(bRespFetch.data.bill.additionalInfo[j].value).ToString("dd MMM yyyy");
                                                        }
                                                    }
                                                    if (!string.IsNullOrEmpty(bbpsLog.APIReqHelper.BillMonthKey) && string.IsNullOrEmpty(bbpsLog.BillMonth))
                                                    {
                                                        if (!string.IsNullOrEmpty(bRespFetch.data.bill.additionalInfo[j].name).Equals(bbpsLog.APIReqHelper.BillMonthKey))
                                                        {
                                                            bbpsLog.BillMonth = bRespFetch.data.bill.additionalInfo[j].value;
                                                            billResponse.BillMonth = string.IsNullOrEmpty(bRespFetch.data.bill.additionalInfo[j].value) ? string.Empty : Convert.ToDateTime(bRespFetch.data.bill.additionalInfo[j].value).ToString("dd MMM yyyy");
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (bRespFetch.data.fetchAPIStatus.In("Acknowledged", "Retrying", "Active"))
                                        {
                                            while (i < LoopTill)
                                            {
                                                i++;
                                                await Task.Delay(5 * 1000).ConfigureAwait(false);
                                                goto RETRY;
                                            }
                                            if (i >= LoopTill)
                                            {
                                                billResponse.IsShowMsgOnly = true;

                                                IErrorCodeML errorCodeML = new ErrorCodeML(_accessor, _env, false);
                                                var eFromDB = errorCodeML.GetAPIErrorCodeDescription(bbpsLog.aPIDetail.GroupCode, bRespFetch.statusCode);
                                                billResponse.ErrorMsg = (eFromDB.Error ?? string.Empty).Replace("{AccountKey}", bbpsLog.APIReqHelper.AccountNoKey);
                                                billResponse.ErrorMsg = billResponse.Msg =  billResponse.ErrorMsg.Replace("{REPLACE}", bRespFetch.statusMessage);
                                                billResponse.ErrorMsg = billResponse.Msg = bbpsLog.helper.Reason = string.IsNullOrEmpty(billResponse.Msg) ? nameof(ErrorCodes.Unable_to_get_bill_details_Please_try_again_after_few_minute).Replace("_", " ") : billResponse.Msg;
                                                billResponse.ErrorCode = eFromDB.Code ?? ErrorCodes.Unknown_Error.ToString();
                                            }
                                            else
                                            {
                                                billResponse.IsShowMsgOnly = true;
                                                IErrorCodeML errorCodeML = new ErrorCodeML(_accessor, _env, false);
                                                var eFromDB = errorCodeML.GetAPIErrorCodeDescription(bbpsLog.aPIDetail.GroupCode, bRespFetch.statusCode);
                                                billResponse.ErrorMsg = (eFromDB.Error ?? string.Empty).Replace("{AccountKey}", bbpsLog.APIReqHelper.AccountNoKey);
                                                billResponse.ErrorMsg = billResponse.Msg  = billResponse.ErrorMsg.Replace("{REPLACE}", bRespFetch.statusMessage);
                                                billResponse.ErrorMsg = billResponse.Msg = bbpsLog.helper.Reason = string.IsNullOrEmpty(billResponse.Msg) ? nameof(ErrorCodes.Unable_to_get_bill_details_Please_try_again_after_few_minute).Replace("_", " ") : billResponse.Msg;
                                                billResponse.ErrorCode = eFromDB.Code ?? ErrorCodes.Unable_to_get_bill_details_Please_try_again_after_few_minute.ToString();
                                            }
                                        }
                                        else
                                        {
                                            billResponse.IsShowMsgOnly = true;
                                            IErrorCodeML errorCodeML = new ErrorCodeML(_accessor, _env, false);
                                            var eFromDB = errorCodeML.GetAPIErrorCodeDescription(bbpsLog.aPIDetail.GroupCode, bRespFetch.statusCode);
                                            billResponse.ErrorMsg = (eFromDB.Error ?? string.Empty).Replace("{AccountKey}", bbpsLog.APIReqHelper.AccountNoKey);
                                            billResponse.ErrorMsg = billResponse.Msg = billResponse.ErrorMsg.Replace("{REPLACE}", bRespFetch.statusMessage);
                                            billResponse.ErrorMsg = billResponse.Msg = bbpsLog.helper.Reason = string.IsNullOrEmpty(billResponse.Msg) ? nameof(ErrorCodes.Unable_to_get_bill_details_Please_try_again_after_few_minute).Replace("_", " ") : billResponse.Msg;
                                            billResponse.ErrorCode = eFromDB.Code ?? ErrorCodes.Unable_to_get_bill_details_Please_try_again_after_few_minute.ToString();
                                        }
                                    }
                                }
                                else
                                {
                                    billResponse.IsShowMsgOnly = true;
                                    IErrorCodeML errorCodeML = new ErrorCodeML(_accessor, _env, false);
                                    var eFromDB = errorCodeML.GetAPIErrorCodeDescription(bbpsLog.aPIDetail.GroupCode, bRespFetch.statusCode);
                                    billResponse.ErrorMsg = (eFromDB.Error ?? string.Empty).Replace("{AccountKey}", bbpsLog.APIReqHelper.AccountNoKey);
                                    billResponse.ErrorMsg = billResponse.Msg = billResponse.ErrorMsg.Replace("{REPLACE}", bRespFetch.statusMessage);
                                    billResponse.ErrorMsg = billResponse.Msg = bbpsLog.helper.Reason = string.IsNullOrEmpty(billResponse.Msg) ? nameof(ErrorCodes.Unable_to_get_bill_details_Please_try_again_after_few_minute).Replace("_", " ") : billResponse.Msg;
                                    billResponse.ErrorCode = eFromDB.Code ?? ErrorCodes.Unable_to_get_bill_details_Please_try_again_after_few_minute.ToString();
                                }
                            }
                        }
                    }
                    else
                    {
                        billResponse.IsShowMsgOnly = true;
                    }
                }
            }
            catch (Exception ex)
            {
                billResponse.IsShowMsgOnly = true;
                response = ex.Message + "|" + response;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "AxisBankBillFetchRequest",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = 1
                });
            }
            //bbpsLog.Response = response;
            if (!string.IsNullOrEmpty(APIContext))
            {
                bbpsLog.APIContext = APIContext;
            }
            return billResponse;
        }
        public HitRequestResponseModel AxisBankGetFetchedBill(string context)
        {
            var returnResp = new HitRequestResponseModel();
            var headers = new Dictionary<string, string>
            {
                { "axis-channel-id", appSetting.axischannelid },
                { "axis-client-id", appSetting.axisclientid },
                { "axis-client-secret", appSetting.axisclientsecret},
                { "axis-encryption-key", appSetting.axisencryptionkey },
                { "axis-salt-key", appSetting.axissaltkey },
                { "axis-channel-password", appSetting.axischannelpassword},
                { "axis-body-channel-id", appSetting.axisbodychannelid }
            };
            var reqObj = new { context };

            string response = string.Empty;
            returnResp.Request = (appSetting.FetchedBillURL ?? string.Empty) + "?" + JsonConvert.SerializeObject(reqObj) + "|" + JsonConvert.SerializeObject(headers);
            try
            {
                if (ApplicationSetting.IsHostedServerIsIndian)
                {
                    returnResp.Response = AppWebRequest.O.PostJsonDataUsingHWRTLS(appSetting.FetchedBillURL, reqObj, headers).Result;
                }
                else
                {
                    returnResp.Response = AppWebRequest.O.PostJsonDataUsingHWR("http://is.roundpay.net/API/LBPostJsonOnly", new { RequestURI = appSetting.FetchedBillURL, obj = reqObj, objHeader = headers, WithHeader = true });
                    try
                    {
                        returnResp.Response = JsonConvert.DeserializeObject<string>(returnResp.Response);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            catch (Exception ex)
            {
                returnResp.IsException = true;
                returnResp.Response = ex.Message + "|" + response;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "AxisBankGetFetchedBill",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = 1
                });
            }
            return returnResp;
        }
        public async Task<HitRequestResponseModel> AxisBankMakePayment(BBPSPaymentRequest bBPSRequest)
        {
            var returnResp = new HitRequestResponseModel
            {
                IsBillValidated = true
            };
            var headers = new Dictionary<string, string>
            {
                { "axis-channel-id", appSetting.axischannelid },
                { "axis-client-id", appSetting.axisclientid },
                { "axis-client-secret", appSetting.axisclientsecret},
                { "axis-encryption-key", appSetting.axisencryptionkey },
                { "axis-salt-key", appSetting.axissaltkey },
                { "axis-channel-password", appSetting.axischannelpassword},
                { "axis-body-channel-id", appSetting.axisbodychannelid }
            };
            string response = string.Empty, context = string.Empty;

            if (bBPSRequest.IsValidation && string.IsNullOrEmpty(bBPSRequest.APIContext))
            {
                returnResp.IsValidaionResponseAlso = true;
                var bilValResp = AxisBankBillValidation(bBPSRequest, out context);
                returnResp.RequestV = bilValResp.Request;
                returnResp.ResponseV = bilValResp.Response;
                if (!string.IsNullOrEmpty(context))
                {
                    int i = 0;
                    int LoopTill = 12;
                RETRY:
                    var hitReqResp = AxisBankGetFetchedBill(context);
                    returnResp.RequestV = returnResp.RequestV + "____________" + hitReqResp.Request;
                    returnResp.ResponseV = returnResp.ResponseV + "____________" + hitReqResp.Response;
                    if (!string.IsNullOrEmpty(hitReqResp.Response))
                    {
                        if (!hitReqResp.IsException)
                        {
                            var bRespFetch = JsonConvert.DeserializeObject<AxisBankBillFetchResponse>(hitReqResp.Response);
                            if (bRespFetch.data != null)
                            {
                                if (bRespFetch.data.fetchAPIStatus.Equals("Active") && bRespFetch.statusCode == "success")
                                {
                                    context = bRespFetch.data.context;
                                }
                                else
                                {
                                    if (bRespFetch.data.fetchAPIStatus.In("Acknowledged", "Retrying", "Active"))
                                    {
                                        while (i < LoopTill)
                                        {
                                            i++;
                                            await Task.Delay(5 * 1000).ConfigureAwait(false);
                                            goto RETRY;
                                        }
                                    }
                                    else
                                    {
                                        i = 12;
                                        returnResp.Request = hitReqResp.Request;
                                        returnResp.Response = hitReqResp.Response;
                                        return returnResp;
                                    }
                                }
                            }
                        }
                    }
                }
                bBPSRequest.APIContext = context;
                if (string.IsNullOrEmpty(context))
                {
                    returnResp.IsBillValidated = false;
                    return returnResp;
                }
            }
            var reqObj = new AxisBankMakePaymentRequest
            {
                amount = bBPSRequest.RequestedAmount.ToString(),
                referenceId = "RP" + bBPSRequest.TID,
                context = bBPSRequest.APIContext,
                txnMode = bBPSRequest.PaymentMode.ToUpper(),
                remittanceDetails = new AxisBankRemitter
                {
                    accountNumber = appSetting.accountNumber,
                    accountHolderName = appSetting.accountHolderName
                }
            };
            returnResp.Request = (appSetting.PaymentURL ?? string.Empty) + "?" + JsonConvert.SerializeObject(reqObj) + "|" + JsonConvert.SerializeObject(headers);
            try
            {
                if (ApplicationSetting.IsHostedServerIsIndian)
                {
                    returnResp.Response = AppWebRequest.O.PostJsonDataUsingHWRTLS(appSetting.PaymentURL, reqObj, headers).Result;
                }
                else
                {
                    returnResp.Response = AppWebRequest.O.PostJsonDataUsingHWR("http://is.roundpay.net/API/LBPostJsonOnly", new { RequestURI = appSetting.PaymentURL, obj = reqObj, objHeader = headers, WithHeader = true });
                    try
                    {
                        returnResp.Response = JsonConvert.DeserializeObject<string>(returnResp.Response);
                    }
                    catch (Exception)
                    {
                    }
                }
                ProcAPIResponse procAPIResponse = new ProcAPIResponse(_dal);
                procAPIResponse.UpdateAPIContext(reqObj.context, bBPSRequest.TID);
                if (!string.IsNullOrEmpty(returnResp.Response))
                {
                    var apiResp = JsonConvert.DeserializeObject<AxisBankPaymentResponse>(returnResp.Response);

                    if (apiResp.statusCode.Equals("success"))
                    {
                        await Task.Delay(30000).ConfigureAwait(false);
                        var stsResp = AxisBankGetPaymentStatus(apiResp.data.context);
                        returnResp.Request = returnResp.Request + "________________" + stsResp.Request;
                        returnResp.Response = stsResp.Response;
                    }
                }
            }
            catch (Exception ex)
            {
                returnResp.IsException = true;
                returnResp.ResponsePre = ex.Message + "|" + response;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "AxisBankMakePayment",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = 1
                });
            }
            return returnResp;
        }
        public HitRequestResponseModel AxisBankBillValidation(BBPSPaymentRequest bBPSRequest, out string context)
        {
            context = string.Empty;
            var returnResp = new HitRequestResponseModel { };
            var headers = new Dictionary<string, string>
            {
                { "axis-channel-id", appSetting.axischannelid },
                { "axis-client-id", appSetting.axisclientid },
                { "axis-client-secret", appSetting.axisclientsecret},
                { "axis-encryption-key", appSetting.axisencryptionkey },
                { "axis-salt-key", appSetting.axissaltkey },
                { "axis-channel-password", appSetting.axischannelpassword},
                { "axis-body-channel-id", appSetting.axisbodychannelid }
            };
            string guid = Guid.NewGuid().ToString();
            var reqObj = new AxisBankBillFetchReqModel
            {
                billerId = bBPSRequest.BillerID,
                categoryCode = bBPSRequest.APIOpType,
                mobileNumber = bBPSRequest.CustomerMobile,
                agent = new AxisBankAgent
                {
                    app = (bBPSRequest.InitChanel ?? string.Empty).Equals("MOB") ? appSetting.axischannelid : string.Empty,
                    channel = bBPSRequest.InitChanel,
                    geocode = (bBPSRequest.InitChanel ?? string.Empty).Equals("AGT") ? bBPSRequest.GeoLocation : string.Empty,
                    id = bBPSRequest.APIOutletID,
                    ifsc = string.Empty,
                    imei = (bBPSRequest.InitChanel ?? string.Empty).Equals("MOB") ? bBPSRequest.IMEI : string.Empty,
                    ip = (bBPSRequest.InitChanel ?? string.Empty).Equals("AGT") ? string.Empty : (bBPSRequest.IPAddress == "::1" ? "122.176.71.56" : bBPSRequest.IPAddress),
                    mac = !(bBPSRequest.InitChanel ?? string.Empty).Equals("INT") ? string.Empty : "48-4D-7E-CB-DB-6F",
                    mobile = (bBPSRequest.InitChanel ?? string.Empty).StartsWith("AGT") ? bBPSRequest.UserMobileNo : string.Empty,
                    os = (bBPSRequest.InitChanel ?? string.Empty).Equals("MOB") ? "android" : string.Empty,
                    postalCode = (bBPSRequest.InitChanel ?? string.Empty).Equals("AGT") ? bBPSRequest.Pincode : string.Empty,
                    terminalId = (bBPSRequest.InitChanel ?? string.Empty).Equals("AGT") ? bBPSRequest.TID.ToString() : string.Empty
                },
                customerParams = new List<AxisBankNameValue>()
            };
            if (bBPSRequest.OpParams != null)
            {
                if (bBPSRequest.OpParams.Count > 0)
                {
                    bBPSRequest.OpParams.RemoveAll(x => x.IsAccountNo == true);
                    reqObj.customerParams.Add(new AxisBankNameValue
                    {
                        name = bBPSRequest.AccountNoKey ?? string.Empty,
                        value = bBPSRequest.AccountNo ?? string.Empty
                    });
                }
                for (int i = 0; i < bBPSRequest.OpParams.Count; i++)
                {
                    if (i == 0)
                    {
                        reqObj.customerParams.Add(new AxisBankNameValue
                        {
                            name = bBPSRequest.OpParams[i].Param,
                            value = bBPSRequest.Optional1 ?? string.Empty
                        });
                    }
                    if (i == 1)
                    {
                        reqObj.customerParams.Add(new AxisBankNameValue
                        {
                            name = bBPSRequest.OpParams[i].Param,
                            value = bBPSRequest.Optional2 ?? string.Empty
                        });
                    }
                    if (i == 2)
                    {
                        reqObj.customerParams.Add(new AxisBankNameValue
                        {
                            name = bBPSRequest.OpParams[i].Param,
                            value = bBPSRequest.Optional3 ?? string.Empty
                        });
                    }
                    if (i == 3)
                    {
                        reqObj.customerParams.Add(new AxisBankNameValue
                        {
                            name = bBPSRequest.OpParams[i].Param,
                            value = bBPSRequest.Optional4 ?? string.Empty
                        });
                    }
                }
            }
            string response = string.Empty;
            var _URL = appSetting.BillFetchURL;
            try
            {
                returnResp.Request = string.Format("{0}?{1}", _URL, JsonConvert.SerializeObject(reqObj) + "]]" + JsonConvert.SerializeObject(headers));

                if (ApplicationSetting.IsHostedServerIsIndian)
                {
                    returnResp.Response = response = AppWebRequest.O.PostJsonDataUsingHWRTLS(_URL, reqObj, headers).Result;
                }
                else
                {
                    returnResp.Response = response = AppWebRequest.O.PostJsonDataUsingHWR("http://is.roundpay.net/API/LBPostJsonOnly", new { RequestURI = _URL, obj = reqObj, objHeader = headers, WithHeader = true });
                    try
                    {
                        returnResp.Response = JsonConvert.DeserializeObject<string>(response);
                    }
                    catch (Exception)
                    {
                    }
                }
                if (!string.IsNullOrEmpty(returnResp.Response))
                {
                    var bResp = JsonConvert.DeserializeObject<AxisBankBillFetchPreResponse>(returnResp.Response);
                    if (bResp.statusCode.Equals("success"))
                    {
                        context = bResp.data.context;
                    }
                }
            }
            catch (Exception ex)
            {
                response = ex.Message + "|" + response;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "AxisBankBillValidation",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = 1
                });
            }
            returnResp.Response = string.IsNullOrEmpty(returnResp.Response) ? response : returnResp.Response;
            return returnResp;
        }
        public HitRequestResponseModel AxisBankGetPaymentStatus(string context)
        {
            var returnResp = new HitRequestResponseModel { };
            var headers = new Dictionary<string, string>
            {
                { "axis-channel-id", appSetting.axischannelid },
                { "axis-client-id", appSetting.axisclientid },
                { "axis-client-secret", appSetting.axisclientsecret},
                { "axis-encryption-key", appSetting.axisencryptionkey },
                { "axis-salt-key", appSetting.axissaltkey },
                { "axis-channel-password", appSetting.axischannelpassword},
                { "axis-body-channel-id", appSetting.axisbodychannelid }
            };
            string response = string.Empty;
            var reqObj = new
            {
                context
            };
            returnResp.Request = (appSetting.PaymentStatusURL ?? string.Empty) + "?" + JsonConvert.SerializeObject(reqObj) + "|" + JsonConvert.SerializeObject(headers);
            try
            {
                if (ApplicationSetting.IsHostedServerIsIndian)
                {
                    returnResp.Response = response = AppWebRequest.O.PostJsonDataUsingHWRTLS(appSetting.PaymentStatusURL, reqObj, headers).Result;
                }
                else
                {
                    returnResp.Response = response = AppWebRequest.O.PostJsonDataUsingHWR("http://is.roundpay.net/API/LBPostJsonOnly", new { RequestURI = appSetting.PaymentStatusURL, obj = reqObj, objHeader = headers, WithHeader = true });
                    try
                    {
                        returnResp.Response = JsonConvert.DeserializeObject<string>(response);
                    }
                    catch (Exception ex)
                    {
                        returnResp.IsException = true;
                        returnResp.Response = ex.Message + "|" + response;
                        var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                        {
                            ClassName = GetType().Name,
                            FuncName = "AxisBankGetPaymentStatus",
                            Error = ex.Message,
                            LoginTypeID = LoginType.ApplicationUser,
                            UserId = 1
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                returnResp.IsException = true;
                returnResp.Response = ex.Message + "|" + response;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "AxisBankGetPaymentStatus",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = 1
                });
            }
            return returnResp;
        }
        public BBPSComplainAPIResponse AxisBankRaiseComplain(BBPSComplainRequest bBPSComplainRequest)
        {
            var returnResp = new BBPSComplainAPIResponse
            {

            };
            var headers = new Dictionary<string, string>
            {
                { "axis-channel-id", appSetting.axischannelid },
                { "axis-client-id", appSetting.axisclientid },
                { "axis-client-secret", appSetting.axisclientsecret},
                { "axis-encryption-key", appSetting.axisencryptionkey },
                { "axis-salt-key", appSetting.axissaltkey },
                { "axis-channel-password", appSetting.axischannelpassword},
                { "axis-body-channel-id", appSetting.axisbodychannelid }
            };
            string response = string.Empty;
            string TransDate = !string.IsNullOrEmpty(bBPSComplainRequest.TransactionID) ? ConnectionStringHelper.O.ConvertTransactionIDTo_dd_MMM_yyyy(bBPSComplainRequest.TransactionID ?? string.Empty) : string.Empty;
            if (!string.IsNullOrEmpty(TransDate))
            {
                TransDate = Convert.ToDateTime(TransDate.Replace(" ", "/")).ToString("yyyy-MM-dd");
            }
            var reqObj = new
            {
                txnId = bBPSComplainRequest.VendorID,
                txnDate = TransDate,
                issueType = "1",
                description = bBPSComplainRequest.Reason
            };
            returnResp.Request = (appSetting.RaiseComplainURL ?? string.Empty) + "?" + JsonConvert.SerializeObject(reqObj) + "|" + JsonConvert.SerializeObject(headers);
            try
            {
                if (ApplicationSetting.IsHostedServerIsIndian)
                {
                    returnResp.Response = AppWebRequest.O.PostJsonDataUsingHWRTLS(appSetting.RaiseComplainURL, reqObj, headers).Result;
                }
                else
                {
                    returnResp.Response = AppWebRequest.O.PostJsonDataUsingHWR("http://is.roundpay.net/API/LBPostJsonOnly", new { RequestURI = appSetting.RaiseComplainURL, obj = reqObj, objHeader = headers, WithHeader = true });
                    try
                    {
                        returnResp.Response = JsonConvert.DeserializeObject<string>(returnResp.Response);
                    }
                    catch (Exception)
                    {
                    }
                }
                if (!string.IsNullOrEmpty(returnResp.Response))
                {
                    var _apiRes = JsonConvert.DeserializeObject<AxisBankPaymentResponse>(returnResp.Response);
                    if (_apiRes != null)
                    {
                        if (_apiRes.statusCode.Equals("success"))
                        {
                            returnResp.Statuscode = 1;
                            returnResp.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                            returnResp.ErrorCode = ErrorCodes.Transaction_Successful;
                            returnResp.LiveID = _apiRes.data.referenceId;
                        }
                        else
                        {
                            returnResp.Statuscode = 3;
                            returnResp.Msg = _apiRes.statusMessage;
                            returnResp.ErrorCode = ErrorCodes.Unknown_Error;
                            returnResp.LiveID = _apiRes.statusMessage;
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
                    FuncName = "AxisBankRaiseComplain",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = 1
                });
            }
            return returnResp;
        }
        public BBPSComplainAPIResponse AxisBankComplainStatus(BBPSComplainRequest bBPSComplainRequest)
        {
            var returnResp = new BBPSComplainAPIResponse
            {
                Statuscode = RechargeRespType.FAILED,
                Msg = "Complain could not be register",
                ErrorCode = ErrorCodes.Unknown_Error,
                Remark = "Complain could not be register"
            };
            var headers = new Dictionary<string, string>
            {
                { "axis-channel-id", appSetting.axischannelid },
                { "axis-client-id", appSetting.axisclientid },
                { "axis-client-secret", appSetting.axisclientsecret},
                { "axis-encryption-key", appSetting.axisencryptionkey },
                { "axis-salt-key", appSetting.axissaltkey },
                { "axis-channel-password", appSetting.axischannelpassword},
                { "axis-body-channel-id", appSetting.axisbodychannelid }
            };
            string response = string.Empty;
            string TransDate = !string.IsNullOrEmpty(bBPSComplainRequest.TransactionID) ? ConnectionStringHelper.O.ConvertTransactionIDTo_dd_MMM_yyyy(bBPSComplainRequest.TransactionID ?? string.Empty) : string.Empty;
            if (!string.IsNullOrEmpty(TransDate))
            {
                TransDate = Convert.ToDateTime(TransDate.Replace(" ", "/")).ToString("yyyy-MM-dd");
            }
            var reqObj = new
            {
                referenceId = bBPSComplainRequest.ComplaintID
            };
            returnResp.Request = (appSetting.ComplainStatusURL ?? string.Empty) + "?" + JsonConvert.SerializeObject(reqObj) + "|" + JsonConvert.SerializeObject(headers);
            try
            {
                if (ApplicationSetting.IsHostedServerIsIndian)
                {
                    returnResp.Response = AppWebRequest.O.PostJsonDataUsingHWRTLS(appSetting.ComplainStatusURL, reqObj, headers).Result;
                }
                else
                {
                    returnResp.Response = AppWebRequest.O.PostJsonDataUsingHWR("http://is.roundpay.net/API/LBPostJsonOnly", new { RequestURI = appSetting.ComplainStatusURL, obj = reqObj, objHeader = headers, WithHeader = true });
                    try
                    {
                        returnResp.Response = JsonConvert.DeserializeObject<string>(returnResp.Response);
                    }
                    catch (Exception)
                    {
                    }
                }
                if (!string.IsNullOrEmpty(returnResp.Response))
                {
                    var _apiRes = JsonConvert.DeserializeObject<AxisBankComplainStatusModel>(returnResp.Response);
                    if (_apiRes != null)
                    {
                        if (_apiRes.statusCode.Equals("success"))
                        {

                            returnResp.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                            returnResp.ErrorCode = ErrorCodes.Transaction_Successful;
                            returnResp.ComplainAssignedTo = _apiRes.data.assignedTo;
                            returnResp.ComplainStatus = _apiRes.data.isComplaintOpen == "Y" ? "Open" : "Close";
                            returnResp.Statuscode = _apiRes.data.isComplaintOpen == "Y" ? RechargeRespType.PENDING : RechargeRespType.SUCCESS;
                            returnResp.ComplainReason = _apiRes.data.transactionResponseReason;
                            returnResp.Remark = _apiRes.data.transactionResponseReason;
                            returnResp.NPCIRefID = _apiRes.data.npciRefId;
                        }
                        else
                        {
                            returnResp.Statuscode = 3;
                            returnResp.Msg = _apiRes.statusMessage;
                            returnResp.ErrorCode = ErrorCodes.Unknown_Error;
                            returnResp.Remark = _apiRes.statusMessage;
                        }
                        returnResp.StatusAsOn = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    }
                }
            }
            catch (Exception ex)
            {
                returnResp.Response = ex.Message + "|" + response;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "AxisBankComplainStatus",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = 1
                });
            }
            return returnResp;
        }
    }
}
