using Fintech.AppCode.Configuration;
using Fintech.AppCode.HelperClass;
using Fintech.AppCode.StaticModel;
using Fintech.AppCode.WebRequest;
using Newtonsoft.Json;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech
{
    public partial class SprintBBPSML
    {
        private string EncryptBody(string body)
        {
            return AppCode.ThirdParty.Sprint.AES.CryptAESIn(body, appSetting.Key, appSetting.IV);
        }

        public BalanceEquiryResp Enquiry(AEPSUniversalRequest modelRequest)
        {
            var res = new BalanceEquiryResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error)
            };
            TokenGeneration();
            var headers = new Dictionary<string, string>
            {
                { "Token", _JWTToken},
                { "Authorisedkey", appSetting.Authorisedkey}
            };
            var _URL = appSetting.AEPSBaseURL + "aeps/balanceenquiry/index";
            var req = new
            {
                latitude = modelRequest.Lattitude,
                longitude = modelRequest.Longitude,
                mobilenumber = modelRequest.MobileNo,
                referenceno = modelRequest.TransactionID,
                ipaddress = modelRequest.IPAddress,
                adhaarnumber = modelRequest.AdharNo,
                accessmodetype = modelRequest.RequestMode == Fintech.AppCode.StaticModel.RequestMode.APPS ? "APP" : "SITE",
                nationalbankidentification = modelRequest.BankIIN,
                requestremarks = "Enquiry request from " + modelRequest.MobileNo,
                data = modelRequest.PIDDATA,
                pipe = modelRequest.APIOpCode,
                timestamp = DateTime.Now.ToString("yyyy-MM-dd HH::mm:ss"),
                transactiontype = "BE",
                submerchantid = modelRequest.APIOutletID,
                is_iris = "No"
            };
            var EncBody = EncryptBody(JsonConvert.SerializeObject(req));
            var response = string.Empty;
            var encBodyJson = new
            {
                body = EncBody
            };
            var request = _URL + JsonConvert.SerializeObject(headers) + JsonConvert.SerializeObject(req) + JsonConvert.SerializeObject(encBodyJson);
            try
            {
                response = AppWebRequest.O.PostJsonDataUsingHWRTLS(_URL, encBodyJson, headers).Result;
                if (!string.IsNullOrEmpty(response))
                {
                    var apiresp = JsonConvert.DeserializeObject<SPRINTAEPSTransactionResponse>(response);
                    if (apiresp.status == true)
                    {
                        res.Statuscode = ErrorCodes.One;
                        res.Msg = ErrorCodes.SUCCESS;
                        res.Balance = string.IsNullOrEmpty(apiresp.balanceamount) ? 0 : Convert.ToDouble(apiresp.balanceamount);
                        res.BankRRN = apiresp.bankrrn;
                    }
                    else
                    {
                        res.Statuscode = ErrorCodes.Minus1;
                        res.Msg = apiresp.message;
                    }
                }
            }
            catch (Exception ex)
            {
                response = ex.Message + "_" + response;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "AEPS.Enquiry",
                    Error = ex.Message,
                });
            }
            res.Req = request;
            res.Resp = response;
            return res;
        }
        public WithdrawlResponse Withdrawal(AEPSUniversalRequest modelRequest)
        {
            var res = new WithdrawlResponse
            {
                Statuscode = ErrorCodes.One,
                Msg = nameof(ErrorCodes.Request_Accpeted),
                Status = RechargeRespType.PENDING,
                Errorcode = ErrorCodes.Request_Accpeted
            };
            TokenGeneration();
            var headers = new Dictionary<string, string>
            {
                { "Token", _JWTToken},
                { "Authorisedkey", appSetting.Authorisedkey}
            };
            var _URL = appSetting.AEPSBaseURL + "aeps/cashwithdraw/index";
            var req = new
            {
                latitude = modelRequest.Lattitude,
                longitude = modelRequest.Longitude,
                mobilenumber = modelRequest.MobileNo,
                referenceno = modelRequest.TID.ToString(),
                ipaddress = modelRequest.IPAddress,
                adhaarnumber = modelRequest.AdharNo,
                accessmodetype = modelRequest.RequestMode == Fintech.AppCode.StaticModel.RequestMode.APPS ? "APP" : "SITE",
                nationalbankidentification = modelRequest.BankIIN,
                requestremarks = "Enquiry request from " + modelRequest.MobileNo,
                data = modelRequest.PIDDATA,
                pipe = modelRequest.APIOpCode,
                timestamp = DateTime.Now.ToString("yyyy-MM-dd HH::mm:ss"),
                transactiontype = "CW",
                submerchantid = modelRequest.APIOutletID,
                amount = modelRequest.Amount,
                is_iris = "No"
            };
            var EncBody = EncryptBody(JsonConvert.SerializeObject(req));
            var response = string.Empty;
            var encBodyJson = new
            {
                body = EncBody
            };
            var request = _URL + JsonConvert.SerializeObject(headers) + JsonConvert.SerializeObject(req) + JsonConvert.SerializeObject(encBodyJson);
            try
            {
                response = AppWebRequest.O.PostJsonDataUsingHWRTLS(_URL, encBodyJson, headers).Result;
                if (!string.IsNullOrEmpty(response))
                {
                    var apiresp = JsonConvert.DeserializeObject<SPRINTAEPSTransactionResponse>(response);
                    res.Msg = apiresp.message;
                    res.VendorID = apiresp.ackno;
                    res.LiveID = apiresp.bankrrn;
                    if (apiresp.status == true)
                    {
                        if ((apiresp.response_code ?? -5) == 1)
                        {
                            res.Statuscode = ErrorCodes.One;
                            res.Msg = ErrorCodes.SUCCESS;
                            res.Balance = string.IsNullOrEmpty(apiresp.balanceamount) ? 0 : Convert.ToDouble(apiresp.balanceamount);
                            res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                            res.Status = RechargeRespType.SUCCESS;
                            res.Errorcode = ErrorCodes.Transaction_Successful;
                            modelRequest.ThreewayStatus = res.Status;
                            var threewayRes = StatusCheckThreeWay(modelRequest);
                            request = request + ":3WAY:" + (threewayRes.Req ?? string.Empty);
                            response = response + ":3WAY:" + (threewayRes.Resp ?? string.Empty);
                        }
                        else if ((apiresp.response_code ?? -5) == 0)
                        {
                            res.Statuscode = ErrorCodes.Minus1;
                            res.Status = RechargeRespType.FAILED;
                            res.Errorcode = 0;
                            res.LiveID = res.Msg;
                            //Call Threeway with failed
                            modelRequest.ThreewayStatus = res.Status;
                            
                            var threewayRes = StatusCheckThreeWay(modelRequest);
                            request = request + ":3WAY:" + (threewayRes.Req ?? string.Empty);
                            response = response + ":3WAY:" + (threewayRes.Resp ?? string.Empty);
                        }
                        else if ((apiresp.response_code ?? -5) == 2)
                        {
                            res.Statuscode = ErrorCodes.Minus1;
                            res.Status = RechargeRespType.FAILED;
                            res.Errorcode = 0;
                            res.LiveID = res.Msg;
                            //Call Status check
                            var StsChk=StatusCheck(modelRequest);
                            request = request + ":STATUSCHECK:" + (StsChk.Req ?? string.Empty);
                            response = response + ":STATUSCHECK:" + (StsChk.Resp ?? string.Empty);
                        }
                    }
                    else
                    {
                        if ((apiresp.response_code ?? -5) == 0)
                        {
                            res.Statuscode = ErrorCodes.Minus1;
                            res.Status = RechargeRespType.FAILED;
                            res.Errorcode = 0;
                            res.LiveID = res.Msg;
                            //Call Threeway with failed
                            modelRequest.ThreewayStatus = res.Status;
                            var threewayRes = StatusCheckThreeWay(modelRequest);
                            request = request + ":3WAY:" + (threewayRes.Req ?? string.Empty);
                            response = response + ":3WAY:" + (threewayRes.Resp ?? string.Empty);
                        }
                        else if ((apiresp.response_code ?? -5) == 2)
                        {
                            res.Statuscode = ErrorCodes.Minus1;
                            res.Status = RechargeRespType.FAILED;
                            res.Errorcode = 0;
                            res.LiveID = res.Msg;
                            //Call Status check
                            var StsChk = StatusCheck(modelRequest);
                            request = request + ":STATUSCHECK:" + (StsChk.Req ?? string.Empty);
                            response = response + ":STATUSCHECK:" + (StsChk.Resp ?? string.Empty);
                        }
                        else if (apiresp.response_code.In(4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 15, 18, 20, 24, 25, 26, 27))
                        {
                            res.Statuscode = RechargeRespType.FAILED;
                            res.Msg = RechargeRespType._FAILED;
                            res.LiveID = apiresp.message;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                response = ex.Message + "_" + response;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "AEPS.Withdrawal",
                    Error = ex.Message,
                });
            }
            res.Req = request;
            res.Resp = response;
            return res;
        }
        public MiniStatementResponse MiniStatement(AEPSUniversalRequest modelRequest)
        {
            var res = new MiniStatementResponse
            {
                Statuscode = ErrorCodes.One,
                Msg = nameof(ErrorCodes.Request_Accpeted),
                Status = RechargeRespType.PENDING
            };
            TokenGeneration();
            var headers = new Dictionary<string, string>
            {
                { "Token", _JWTToken},
                { "Authorisedkey", appSetting.Authorisedkey}
            };
            var _URL = appSetting.AEPSBaseURL + "aeps/ministatement/index";
            var req = new
            {
                latitude = modelRequest.Lattitude,
                longitude = modelRequest.Longitude,
                mobilenumber = modelRequest.MobileNo,
                referenceno = modelRequest.TransactionID,
                ipaddress = modelRequest.IPAddress,
                adhaarnumber = modelRequest.AdharNo,
                accessmodetype = modelRequest.RequestMode == Fintech.AppCode.StaticModel.RequestMode.APPS ? "APP" : "SITE",
                nationalbankidentification = modelRequest.BankIIN,
                requestremarks = "Enquiry request from " + modelRequest.MobileNo,
                data = modelRequest.PIDDATA,
                pipe = modelRequest.APIOpCode,
                timestamp = DateTime.Now.ToString("yyyy-MM-dd HH::mm:ss"),
                transactiontype = "MS",
                submerchantid = modelRequest.APIOutletID,
                is_iris = "No"
            };
            var EncBody = EncryptBody(JsonConvert.SerializeObject(req));
            var response = string.Empty;
            var encBodyJson = new
            {
                body = EncBody
            };
            var request = _URL + JsonConvert.SerializeObject(headers) + JsonConvert.SerializeObject(req) + JsonConvert.SerializeObject(encBodyJson);
            try
            {
                response = AppWebRequest.O.PostJsonDataUsingHWRTLS(_URL, encBodyJson, headers).Result;
                if (!string.IsNullOrEmpty(response))
                {
                    var apiresp = JsonConvert.DeserializeObject<SPRINTAEPSTransactionResponse>(response);
                    res.LiveID = apiresp.bankrrn;
                    res.VendorID = apiresp.ackno;
                    if (apiresp.status)
                    {
                        if (apiresp.response_code == 1)
                        {
                            res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                            res.Status = RechargeRespType.SUCCESS;
                            res.Balance = string.IsNullOrEmpty(apiresp.balanceamount) ? 0 : Convert.ToDouble(apiresp.balanceamount);
                            if (apiresp.ministatement != null)
                            {
                                if (apiresp.ministatement.Count > 0)
                                {
                                    foreach (var item in apiresp.ministatement)
                                    {
                                        res.Statements.Add(new MiniStatement
                                        {
                                            TransactionDate = item.date,
                                            TransactionType = item.txnType,
                                            Amount = item.amount,
                                            Narration = item.narration
                                        });
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        res.LiveID = res.Msg;
                        res.Status = RechargeRespType.FAILED;
                    }
                }
            }
            catch (Exception ex)
            {
                response = ex.Message + "_" + response;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "AEPS.MiniStatement",
                    Error = ex.Message,
                });
            }
            res.Req = request;
            res.Resp = response;
            return res;
        }
        public MiniBankTransactionServiceResp StatusCheck(AEPSUniversalRequest modelRequest)
        {
            var res = new MiniBankTransactionServiceResp
            {
                Statuscode = RechargeRespType.PENDING,
                Msg = ErrorCodes.NORESPONSE
            };

            TokenGeneration();
            var headers = new Dictionary<string, string>
            {
                { "Token", _JWTToken},
                { "Authorisedkey", appSetting.Authorisedkey}
            };
            var _URL = appSetting.AEPSBaseURL + "aeps/aepsquery/query";
            var req = new
            {
                reference = modelRequest.TID.ToString()
            };
            var EncBody = EncryptBody(JsonConvert.SerializeObject(req));
            var response = string.Empty;
            var encBodyJson = new
            {
                body = EncBody
            };
            var request = _URL + JsonConvert.SerializeObject(headers) + JsonConvert.SerializeObject(req) + JsonConvert.SerializeObject(encBodyJson);
            try
            {
                response = AppWebRequest.O.PostJsonDataUsingHWRTLS(_URL, encBodyJson, headers).Result;
                if (!string.IsNullOrEmpty(response))
                {
                    /**
                     Query successfully completed.
                        --- Status = true and txnstatus = 1 and response_code = 1 ( Hit threeway with success)
                        --- Status = true and txnstatus =3 and response_code = 0 ( Hit threeway with failed)
                        --- Status = true and txnstatus =2 and response_code = 2 ( Hit threeway with failed)
                    Query not completed.
                        -- status = false and response_code ( 3) (Transaction Not found in system)
                        -- status = false and response_code ( 15,8,20,9,10,11 ) (Bad request, Try again)
                     */
                    var apiresp = JsonConvert.DeserializeObject<SPRINTAEPSTransactionResponse>(response);
                    res.Msg = apiresp.message;
                    res.VendorID = apiresp.ackno;
                    res.Amount = Convert.ToInt32(apiresp.amount);
                    res.LiveID = apiresp.bankrrn;
                    res.CardNumber = "XXXXXXXXXXXX";
                    res.BankTransactionDate = string.Empty;
                    res.BankName = apiresp.bankiin;
                    if (apiresp.status == true)
                    {
                        if (apiresp.txnstatus == "1")
                        {
                            res.Statuscode = RechargeRespType.SUCCESS;
                            res.Msg = RechargeRespType._SUCCESS;
                            //CallThreeway with success
                            modelRequest.ThreewayStatus = res.Statuscode;
                            var threewayRes = StatusCheckThreeWay(modelRequest);
                            request = request + ":3WAY:" + (threewayRes.Req ?? string.Empty);
                            response = response + ":3WAY:" + (threewayRes.Resp ?? string.Empty);
                        }
                        else if (apiresp.txnstatus.In("2", "3"))
                        {
                            res.Statuscode = RechargeRespType.FAILED;
                            res.Msg = RechargeRespType._FAILED;
                            res.LiveID = apiresp.message;
                            //Call Threeway with failed
                            modelRequest.ThreewayStatus = res.Statuscode;
                            var threewayRes = StatusCheckThreeWay(modelRequest);
                            request = request + ":3WAY:" + (threewayRes.Req ?? string.Empty);
                            response = response + ":3WAY:" + (threewayRes.Resp ?? string.Empty);
                        }
                    }
                    else
                    {
                        if (apiresp.response_code.In(3, 15, 8, 20, 9, 10, 11))
                        {
                            res.Statuscode = RechargeRespType.FAILED;
                            res.Msg = RechargeRespType._FAILED;
                            res.LiveID = apiresp.message;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                response = ex.Message + "_" + response;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "AEPS.Statuscheck",
                    Error = ex.Message,
                });
            }
            res.Req = request;
            res.Resp = response;
            return res;
        }
        public MiniBankTransactionServiceResp StatusCheckThreeWay(AEPSUniversalRequest modelRequest)
        {
            var res = new MiniBankTransactionServiceResp
            {
                Statuscode = RechargeRespType.PENDING,
                Msg = ErrorCodes.NORESPONSE
            };

            TokenGeneration();
            var headers = new Dictionary<string, string>
            {
                { "Token", _JWTToken},
                { "Authorisedkey", appSetting.Authorisedkey}
            };
            var _URL = appSetting.AEPSBaseURL + "aeps/threeway/threeway";
            var threewayStatus = string.Empty;
            if (modelRequest.ThreewayStatus == RechargeRespType.FAILED)
            {
                threewayStatus = "failed";
            }
            else if (modelRequest.ThreewayStatus == RechargeRespType.SUCCESS)
            {
                threewayStatus = "success";
            }
            var req = new
            {
                reference = modelRequest.TID.ToString(),
                status = threewayStatus
            };
            var EncBody = EncryptBody(JsonConvert.SerializeObject(req));
            var response = string.Empty;
            var encBodyJson = new
            {
                body = EncBody
            };
            var request = _URL + JsonConvert.SerializeObject(headers) + JsonConvert.SerializeObject(req) + JsonConvert.SerializeObject(encBodyJson);
            try
            {
                response = AppWebRequest.O.PostJsonDataUsingHWRTLS(_URL, encBodyJson, headers).Result;
                if (!string.IsNullOrEmpty(response))
                {

                }
            }
            catch (Exception ex)
            {
                response = ex.Message + "_" + response;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "AEPS.StatusCheckThreeWay",
                    Error = ex.Message,
                });
            }
            res.Req = request;
            res.Resp = response;
            return res;
        }
        public WithdrawlResponse AadharPay(AEPSUniversalRequest modelRequest)
        {
            var res = new WithdrawlResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error)
            };
            TokenGeneration();
            var headers = new Dictionary<string, string>
            {
                { "Token", _JWTToken},
                { "Authorisedkey", appSetting.Authorisedkey}
            };
            var _URL = appSetting.AEPSBaseURL + "aadharpay/aadharpay/index";
            var req = new
            {
                latitude = modelRequest.Lattitude,
                longitude = modelRequest.Longitude,
                mobilenumber = modelRequest.MobileNo,
                referenceno = modelRequest.TID.ToString(),
                ipaddress = modelRequest.IPAddress,
                adhaarnumber = modelRequest.AdharNo,
                accessmodetype = modelRequest.RequestMode == Fintech.AppCode.StaticModel.RequestMode.APPS ? "APP" : "SITE",
                nationalbankidentification = modelRequest.BankIIN,
                requestremarks = "Enquiry request from " + modelRequest.MobileNo,
                data = modelRequest.PIDDATA,
                pipe = modelRequest.APIOpCode,
                timestamp = DateTime.Now.ToString("yyyy-MM-dd HH::mm:ss"),
                transactiontype = "FM",
                submerchantid = modelRequest.APIOutletID,
                amount = modelRequest.Amount,
                is_iris = false
            };
            var EncBody = EncryptBody(JsonConvert.SerializeObject(req));
            var response = string.Empty;
            var encBodyJson = new
            {
                body = EncBody
            };
            var request = _URL + JsonConvert.SerializeObject(headers) + JsonConvert.SerializeObject(req) + JsonConvert.SerializeObject(encBodyJson);
            try
            {
                response = AppWebRequest.O.PostJsonDataUsingHWRTLS(_URL, encBodyJson, headers).Result;
                if (!string.IsNullOrEmpty(response))
                {
                    var apiresp = JsonConvert.DeserializeObject<SPRINTAEPSTransactionResponse>(response);
                    res.Msg = apiresp.message;
                    res.VendorID = apiresp.ackno;
                    res.LiveID = apiresp.bankrrn;
                    if (apiresp.status == true)
                    {
                        if ((apiresp.response_code ?? -5) == 1)
                        {
                            res.Statuscode = ErrorCodes.One;
                            res.Msg = ErrorCodes.SUCCESS;
                            res.Balance = string.IsNullOrEmpty(apiresp.balanceamount) ? 0 : Convert.ToDouble(apiresp.balanceamount);
                            res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                            res.Status = RechargeRespType.SUCCESS;
                            res.Errorcode = ErrorCodes.Transaction_Successful;
                        }
                        else if ((apiresp.response_code ?? -5) == 0)
                        {
                            res.Statuscode = ErrorCodes.Minus1;
                            res.Status = RechargeRespType.FAILED;
                            res.Errorcode = 0;
                            res.LiveID = res.Msg;
                            
                        }
                        else if ((apiresp.response_code ?? -5) == 2)
                        {
                            res.Statuscode = ErrorCodes.Minus1;
                            res.Status = RechargeRespType.FAILED;
                            res.Errorcode = 0;
                            res.LiveID = res.Msg;
                            //Call Status check
                            var stsResp=StatusCheck(modelRequest);
                            request = request + ":STSCHK:" + (stsResp.Req ?? string.Empty);
                            response = response + ":STSCHK:" + (stsResp.Resp ?? string.Empty);
                        }
                    }
                    else
                    {
                        if ((apiresp.response_code ?? -5) == 0)
                        {
                            res.Statuscode = ErrorCodes.Minus1;
                            res.Status = RechargeRespType.FAILED;
                            res.Errorcode = 0;
                            res.LiveID = res.Msg;
                            //Call Threeway with failed
                            modelRequest.ThreewayStatus = res.Status;
                            
                        }
                        else if ((apiresp.response_code ?? -5) == 2)
                        {
                            res.Statuscode = ErrorCodes.Minus1;
                            res.Status = RechargeRespType.FAILED;
                            res.Errorcode = 0;
                            res.LiveID = res.Msg;
                            //Call Status check
                            StatusCheck(modelRequest);
                        }
                        else if (apiresp.response_code.In(4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 15, 18, 20, 24, 25, 26, 27))
                        {
                            res.Statuscode = RechargeRespType.FAILED;
                            res.Msg = RechargeRespType._FAILED;
                            res.LiveID = apiresp.message;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                response = ex.Message + "_" + response;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "AEPS.AadharPay",
                    Error = ex.Message,
                });
            }
            res.Req = request;
            res.Resp = response;
            return res;
        }
        public MiniBankTransactionServiceResp AadharPayStatusCheck(AEPSUniversalRequest modelRequest)
        {
            var res = new MiniBankTransactionServiceResp
            {
                Statuscode = RechargeRespType.PENDING,
                Msg = ErrorCodes.NORESPONSE
            };

            TokenGeneration();
            var headers = new Dictionary<string, string>
            {
                { "Token", _JWTToken},
                { "Authorisedkey", appSetting.Authorisedkey}
            };
            var _URL = appSetting.AEPSBaseURL + "aadharpay/aadharpayquery/query";
            var req = new
            {
                reference = modelRequest.TID.ToString()
            };
            var EncBody = EncryptBody(JsonConvert.SerializeObject(req));
            var response = string.Empty;
            var encBodyJson = new
            {
                Raw_Body = EncBody
            };
            var request = _URL + JsonConvert.SerializeObject(headers) + JsonConvert.SerializeObject(req) + JsonConvert.SerializeObject(encBodyJson);
            try
            {
                /**
                 * Query successfully completed.
                    --- Status = true and txnstatus = 1 and response_code = 1 Success
                    --- Status = true and txnstatus =3 and response_code = 0 failed
                    --- Status = true and txnstatus =2 and response_code = 2 Pending

                    Query not completed.
                    -- status = false and response_code ( 3) (Transaction Not found in system)
                    -- status = false and response_code ( 15,8,20,9,10,11 ) (Bad request, Try again)
                 * **/
                response = AppWebRequest.O.PostJsonDataUsingHWRTLS(_URL, encBodyJson, headers).Result;
                if (!string.IsNullOrEmpty(response))
                {
                    var apiresp = JsonConvert.DeserializeObject<SPRINTAEPSTransactionResponse>(response);
                    res.Msg = apiresp.message;
                    res.VendorID = apiresp.ackno;
                    res.Amount = Convert.ToInt32(apiresp.amount);
                    res.LiveID = apiresp.bankrrn;
                    res.CardNumber = "XXXXXXXXXXXX";
                    res.BankTransactionDate = string.Empty;
                    res.BankName = apiresp.bankiin;
                    if (apiresp.status == true)
                    {
                        if (apiresp.txnstatus == "1" && (apiresp.response_code ?? -5) == 1)
                        {
                            res.Statuscode = RechargeRespType.SUCCESS;
                            res.Msg = RechargeRespType._SUCCESS;
                        }
                        else if (apiresp.txnstatus.In("3") && (apiresp.response_code ?? -5) == 0)
                        {
                            res.Statuscode = RechargeRespType.FAILED;
                            res.Msg = RechargeRespType._FAILED;
                            res.LiveID = apiresp.message;
                        }
                    }
                    else
                    {
                        if ((apiresp.response_code ?? -5).In(3, 15, 8, 20, 9, 10, 11))
                        {
                            res.Statuscode = RechargeRespType.FAILED;
                            res.Msg = RechargeRespType._FAILED;
                            res.LiveID = apiresp.message;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                response = ex.Message + "_" + response;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "AadharPayStatusCheck",
                    Error = ex.Message,
                });
            }
            res.Req = request;
            res.Resp = response;
            return res;
        }
        public string BankList()
        {
            TokenGeneration();
            var headers = new Dictionary<string, string>
            {
                { "Token", _JWTToken},
                { "Authorisedkey", appSetting.Authorisedkey}
            };
            var _URL = appSetting.AEPSBaseURL + "aeps/banklist/index";
            var response = string.Empty;
            var request = appSetting.AEPSBaseURL + JsonConvert.SerializeObject(headers);
            try
            {
                response = AppWebRequest.O.PostJsonDataUsingHWRTLS(_URL, new { }, headers).Result;
                if (!string.IsNullOrEmpty(response))
                {

                }
            }
            catch (Exception ex)
            {
                response = ex.Message;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "AEPS.BankList",
                    Error = ex.Message,
                });
            }
            return response;
        }
    }
}
