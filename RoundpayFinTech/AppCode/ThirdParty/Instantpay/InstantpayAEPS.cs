using Fintech.AppCode.Configuration;
using Fintech.AppCode.HelperClass;
using Fintech.AppCode.StaticModel;
using Fintech.AppCode.WebRequest;
using Newtonsoft.Json;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.ThirdParty.Instantpay;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.MiddleLayer.Dmt_Api
{
    public partial class InstantPayUserOnboarding
    {
        public BalanceEquiryResp AEPSBalanceEnquiry(AEPSUniversalRequest modelRequest)
        {
            var res = new BalanceEquiryResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error)
            };
            string SrNo = string.Empty;
            try
            {
                if (modelRequest.PIDData.DeviceInfo.additionalInfo.Param.Count > 0)
                {
                    SrNo = modelRequest.PIDData.DeviceInfo.additionalInfo.Param.Where(x => x.Name == "srno").FirstOrDefault().Value;
                }
            }
            catch (Exception ex)
            {
            }
            var req = new
            {
                token = apiSetting.Token,
                request = new
                {
                    outlet_id = modelRequest.APIOutletID,
                    amount = 0,
                    aadhaar_uid = modelRequest.AdharNo,
                    bankiin = modelRequest.BankIIN,
                    latitude = modelRequest.Lattitude,
                    longitude = modelRequest.Longitude,
                    mobile = modelRequest.MobileNo,
                    agent_id = modelRequest.TID,
                    sp_key = "BAP",
                    pidDataType = modelRequest.PIDData.Data.Type,
                    pidData = modelRequest.PIDData.Data.Text,
                    ci = modelRequest.PIDData.Skey.Ci,
                    dc = modelRequest.PIDData.DeviceInfo.Dc,
                    dpId = modelRequest.PIDData.DeviceInfo.DpId,
                    errCode = modelRequest.PIDData.Resp.ErrCode,
                    errInfo = modelRequest.PIDData.Resp.ErrInfo,
                    fCount = modelRequest.PIDData.Resp.FCount,
                    hmac = modelRequest.PIDData.Hmac,
                    iCount = "0",
                    mc = modelRequest.PIDData.DeviceInfo.Mc,
                    mi = modelRequest.PIDData.DeviceInfo.Mi,
                    nmPoints = modelRequest.PIDData.Resp.NmPoints,
                    pCount = "0",
                    pType = "0",
                    qScore = modelRequest.PIDData.Resp.QScore,
                    rdsId = modelRequest.PIDData.DeviceInfo.RdsId,
                    rdsVer = modelRequest.PIDData.DeviceInfo.RdsVer,
                    sessionKey = modelRequest.PIDData.Skey.Text,
                    srno = SrNo
                },
                user_agent = modelRequest.UserAgent ?? string.Empty
            };
            var request = string.Empty;
            var response = string.Empty;
            string _URL = apiSetting.AEPSBaseURL + "ws/aepsweb/aeps/transaction";
            try
            {
                request = _URL + "?" + JsonConvert.SerializeObject(req);
                response = AppWebRequest.O.PostJsonDataUsingHWR(_URL, req);
                if (string.IsNullOrEmpty(response) == false)
                {
                    var apiResp = JsonConvert.DeserializeObject<IPAEPSResponse>(response);

                    if (apiResp.statuscode == "TXN")
                    {
                        if (apiResp.data != null)
                        {
                            if (apiResp.data.status == RechargeRespType._SUCCESS)
                            {
                                res.Statuscode = ErrorCodes.One;
                                res.Msg = ErrorCodes.SUCCESS;
                                if (!string.IsNullOrEmpty(apiResp.data.balance))
                                {
                                    if (apiResp.data.balance.Contains("-"))
                                    {
                                        if (apiResp.data.balance == "-")
                                        {

                                            apiResp.data.balance = "0";
                                        }
                                        else
                                        {
                                            apiResp.data.balance = apiResp.data.balance.Replace("-", "");
                                        }
                                    }
                                    res.Balance = Convert.ToDouble(apiResp.data.balance);
                                }
                                res.BankRRN = apiResp.data.opr_id;
                            }
                        }
                    }
                    else
                    {
                        res.Statuscode = ErrorCodes.Minus1;
                        res.Msg = apiResp.status;
                    }
                }
            }
            catch (Exception ex)
            {
                response = ex.Message + "_" + response;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "AEPSBalanceEnquiry",
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
            string SrNo = string.Empty;
            try
            {
                if (modelRequest.PIDData.DeviceInfo.additionalInfo.Param.Count > 0)
                {
                    SrNo = modelRequest.PIDData.DeviceInfo.additionalInfo.Param.Where(x => x.Name == "srno").FirstOrDefault().Value;
                }
            }
            catch (Exception ex)
            {
            }
            var req = new
            {
                token = apiSetting.Token,
                request = new
                {
                    outlet_id = modelRequest.APIOutletID,
                    amount = modelRequest.Amount,
                    aadhaar_uid = modelRequest.AdharNo,
                    bankiin = modelRequest.BankIIN,
                    latitude = modelRequest.Lattitude,
                    longitude = modelRequest.Longitude,
                    mobile = modelRequest.MobileNo,
                    agent_id = modelRequest.TID,
                    sp_key = "WAP",
                    pidDataType = modelRequest.PIDData.Data.Type,
                    pidData = modelRequest.PIDData.Data.Text,
                    ci = modelRequest.PIDData.Skey.Ci,
                    dc = modelRequest.PIDData.DeviceInfo.Dc,
                    dpId = modelRequest.PIDData.DeviceInfo.DpId,
                    errCode = modelRequest.PIDData.Resp.ErrCode,
                    errInfo = modelRequest.PIDData.Resp.ErrInfo,
                    fCount = modelRequest.PIDData.Resp.FCount,
                    hmac = modelRequest.PIDData.Hmac,
                    iCount = "0",
                    mc = modelRequest.PIDData.DeviceInfo.Mc,
                    mi = modelRequest.PIDData.DeviceInfo.Mi,
                    nmPoints = modelRequest.PIDData.Resp.NmPoints,
                    pCount = "0",
                    pType = "0",
                    qScore = modelRequest.PIDData.Resp.QScore,
                    rdsId = modelRequest.PIDData.DeviceInfo.RdsId,
                    rdsVer = modelRequest.PIDData.DeviceInfo.RdsVer,
                    sessionKey = modelRequest.PIDData.Skey.Text,
                    srno = SrNo
                },
                user_agent = modelRequest.UserAgent ?? string.Empty
            };
            var request = string.Empty;
            var response = string.Empty;
            string _URL = apiSetting.AEPSBaseURL + "ws/aepsweb/aeps/transaction";
            try
            {
                request = _URL + "?" + JsonConvert.SerializeObject(req);
                response = AppWebRequest.O.PostJsonDataUsingHWR(_URL, req);
                if (string.IsNullOrEmpty(response) == false)
                {
                    var apiResp = JsonConvert.DeserializeObject<IPAEPSResponse>(response);

                    if (apiResp.statuscode.In("TXN", "TUP"))
                    {
                        res.Statuscode = ErrorCodes.One;

                        if (apiResp.statuscode == "TUP")
                        {
                            res.Status = RechargeRespType.PENDING;
                            res.Msg = ErrorCodes.SUCCESS;
                        }

                        if (apiResp.data != null)
                        {
                            res.LiveID = apiResp.data.opr_id;
                            res.VendorID = apiResp.data.ipay_id;
                            if (!string.IsNullOrEmpty(apiResp.data.balance))
                            {
                                if (apiResp.data.balance.Contains("-"))
                                {
                                    if (apiResp.data.balance == "-")
                                    {

                                        apiResp.data.balance = "0";
                                    }
                                    else
                                    {
                                        apiResp.data.balance = apiResp.data.balance.Replace("-", "");
                                    }
                                }
                                res.Balance = Convert.ToDouble(apiResp.data.balance);
                            }
                            if (apiResp.data.status == RechargeRespType._SUCCESS)
                            {
                                res.Status = RechargeRespType.SUCCESS;
                                res.Msg = ErrorCodes.SUCCESS;
                            }
                        }
                    }
                    else
                    {
                        res.Statuscode = ErrorCodes.Minus1;
                        res.Status = RechargeRespType.FAILED;
                        res.Msg = apiResp.status;
                    }
                }
            }
            catch (Exception ex)
            {
                response = ex.Message + "_" + response;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Withdrawal",
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

            string SrNo = string.Empty;
            try
            {
                if (modelRequest.PIDData.DeviceInfo.additionalInfo.Param.Count > 0)
                {
                    SrNo = modelRequest.PIDData.DeviceInfo.additionalInfo.Param.Where(x => x.Name == "srno").FirstOrDefault().Value;
                }
            }
            catch (Exception ex)
            {
            }
            var req = new
            {
                token = apiSetting.Token,
                request = new
                {
                    outlet_id = modelRequest.APIOutletID,
                    amount = modelRequest.Amount,
                    aadhaar_uid = modelRequest.AdharNo,
                    bankiin = modelRequest.BankIIN,
                    latitude = modelRequest.Lattitude,
                    longitude = modelRequest.Longitude,
                    mobile = modelRequest.MobileNo,
                    agent_id = modelRequest.TID,
                    sp_key = "SAP",
                    pidDataType = modelRequest.PIDData.Data.Type,
                    pidData = modelRequest.PIDData.Data.Text,
                    ci = modelRequest.PIDData.Skey.Ci,
                    dc = modelRequest.PIDData.DeviceInfo.Dc,
                    dpId = modelRequest.PIDData.DeviceInfo.DpId,
                    errCode = modelRequest.PIDData.Resp.ErrCode,
                    errInfo = modelRequest.PIDData.Resp.ErrInfo,
                    fCount = modelRequest.PIDData.Resp.FCount,
                    hmac = modelRequest.PIDData.Hmac,
                    iCount = "0",
                    mc = modelRequest.PIDData.DeviceInfo.Mc,
                    mi = modelRequest.PIDData.DeviceInfo.Mi,
                    nmPoints = modelRequest.PIDData.Resp.NmPoints,
                    pCount = "0",
                    pType = "0",
                    qScore = modelRequest.PIDData.Resp.QScore,
                    rdsId = modelRequest.PIDData.DeviceInfo.RdsId,
                    rdsVer = modelRequest.PIDData.DeviceInfo.RdsVer,
                    sessionKey = modelRequest.PIDData.Skey.Text,
                    srno = SrNo
                },
                user_agent = modelRequest.UserAgent ?? string.Empty
            };
            var request = string.Empty;
            var response = string.Empty;
            string _URL = apiSetting.AEPSBaseURL + "ws/aepsweb/aeps/transaction";
            try
            {
                request = _URL + "?" + JsonConvert.SerializeObject(req);
                response = AppWebRequest.O.PostJsonDataUsingHWR(_URL, req);
                if (string.IsNullOrEmpty(response) == false)
                {
                    var apiResp = JsonConvert.DeserializeObject<IPAEPSResponse>(response);

                    if (apiResp.statuscode.In("TXN", "TUP"))
                    {
                        res.Statuscode = ErrorCodes.One;

                        if (apiResp.statuscode == "TUP")
                        {
                            res.Status = RechargeRespType.PENDING;
                            res.Msg = ErrorCodes.SUCCESS;
                        }

                        if (apiResp.data != null)
                        {
                            res.LiveID = apiResp.data.opr_id;
                            res.VendorID = apiResp.data.ipay_id;
                            if (!string.IsNullOrEmpty(apiResp.data.balance))
                            {
                                if (apiResp.data.balance.Contains("-"))
                                {
                                    if (apiResp.data.balance == "-")
                                    {

                                        apiResp.data.balance = "0";
                                    }
                                    else
                                    {
                                        apiResp.data.balance = apiResp.data.balance.Replace("-", "");
                                    }
                                }
                                res.Balance = Convert.ToDouble(apiResp.data.balance);
                            }
                            if (apiResp.data.status == RechargeRespType._SUCCESS)
                            {
                                res.Status = RechargeRespType.SUCCESS;
                                res.Msg = ErrorCodes.SUCCESS;
                                res.Statements = new List<MiniStatement>();
                                if (apiResp.data.mini_statement != null)
                                {
                                    if (apiResp.data.mini_statement.Count > 0)
                                    {
                                        foreach (var item in apiResp.data.mini_statement)
                                        {
                                            res.Statements.Add(new MiniStatement
                                            {
                                                TransactionDate = item.date,
                                                TransactionType = item.txnType,
                                                Narration = item.narration,
                                                Amount = item.amount
                                            });
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        res.Statuscode = ErrorCodes.Minus1;
                        res.Status = RechargeRespType.FAILED;
                        res.Msg = apiResp.status;
                    }
                }
            }
            catch (Exception ex)
            {
                response = ex.Message + "_" + response;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "MiniStatement",
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
                Statuscode = ErrorCodes.One,
                Msg = nameof(ErrorCodes.Request_Accpeted),
                Status = RechargeRespType.PENDING,
                Errorcode = ErrorCodes.Request_Accpeted
            };
            string SrNo = string.Empty;
            try
            {
                if (modelRequest.PIDData.DeviceInfo.additionalInfo.Param.Count > 0)
                {
                    SrNo = modelRequest.PIDData.DeviceInfo.additionalInfo.Param.Where(x => x.Name == "srno").FirstOrDefault().Value;
                }
            }
            catch (Exception ex)
            {
            }
            var req = new
            {
                token = apiSetting.Token,
                request = new
                {
                    outlet_id = modelRequest.APIOutletID,
                    amount = modelRequest.Amount,
                    aadhaar_uid = modelRequest.AdharNo,
                    bankiin = modelRequest.BankIIN,
                    latitude = modelRequest.Lattitude,
                    longitude = modelRequest.Longitude,
                    mobile = modelRequest.MobileNo,
                    agent_id = modelRequest.TID,
                    sp_key = "MZZ",
                    pidDataType = modelRequest.PIDData.Data.Type,
                    pidData = modelRequest.PIDData.Data.Text,
                    ci = modelRequest.PIDData.Skey.Ci,
                    dc = modelRequest.PIDData.DeviceInfo.Dc,
                    dpId = modelRequest.PIDData.DeviceInfo.DpId,
                    errCode = modelRequest.PIDData.Resp.ErrCode,
                    errInfo = modelRequest.PIDData.Resp.ErrInfo,
                    fCount = modelRequest.PIDData.Resp.FCount,
                    hmac = modelRequest.PIDData.Hmac,
                    iCount = "0",
                    mc = modelRequest.PIDData.DeviceInfo.Mc,
                    mi = modelRequest.PIDData.DeviceInfo.Mi,
                    nmPoints = modelRequest.PIDData.Resp.NmPoints,
                    pCount = "0",
                    pType = "0",
                    qScore = modelRequest.PIDData.Resp.QScore,
                    rdsId = modelRequest.PIDData.DeviceInfo.RdsId,
                    rdsVer = modelRequest.PIDData.DeviceInfo.RdsVer,
                    sessionKey = modelRequest.PIDData.Skey.Text,
                    srno = SrNo
                },
                user_agent = modelRequest.UserAgent ?? string.Empty
            };
            var request = string.Empty;
            var response = string.Empty;
            string _URL = apiSetting.AEPSBaseURL + "ws/aepsweb/aeps/transaction";
            try
            {
                request = _URL + "?" + JsonConvert.SerializeObject(req);
                response = AppWebRequest.O.PostJsonDataUsingHWR(_URL, req);
                if (string.IsNullOrEmpty(response) == false)
                {
                    var apiResp = JsonConvert.DeserializeObject<IPAEPSResponse>(response);

                    if (apiResp.statuscode.In("TXN", "TUP"))
                    {
                        res.Statuscode = ErrorCodes.One;

                        if (apiResp.statuscode == "TUP")
                        {
                            res.Status = RechargeRespType.PENDING;
                            res.Msg = ErrorCodes.SUCCESS;
                        }

                        if (apiResp.data != null)
                        {
                            res.LiveID = apiResp.data.opr_id;
                            res.VendorID = apiResp.data.ipay_id;
                            if (!string.IsNullOrEmpty(apiResp.data.balance))
                            {
                                if (apiResp.data.balance.Contains("-"))
                                {
                                    if (apiResp.data.balance == "-")
                                    {

                                        apiResp.data.balance = "0";
                                    }
                                    else
                                    {
                                        apiResp.data.balance = apiResp.data.balance.Replace("-", "");
                                    }
                                }
                                res.Balance = Convert.ToDouble(apiResp.data.balance);
                            }
                            if (apiResp.data.status == RechargeRespType._SUCCESS)
                            {
                                res.Status = RechargeRespType.SUCCESS;
                                res.Msg = ErrorCodes.SUCCESS;
                            }
                        }
                    }
                    else
                    {
                        res.Statuscode = ErrorCodes.Minus1;
                        res.Status = RechargeRespType.FAILED;
                        res.Msg = apiResp.status;
                    }
                }
            }
            catch (Exception ex)
            {
                response = ex.Message + "_" + response;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "AadharPay",
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
            var header = new Dictionary<string, string> {
                { "X-Ipay-Auth-Code","01"},
                { "X-Ipay-Client-Id",apiSetting.ClientID??string.Empty},
                { "X-Ipay-Client-Secret",apiSetting.ClientSecret??string.Empty},
                { "X-Ipay-Endpoint-Ip",apiSetting.IPAddress??string.Empty},
                //{ "X-Ipay-Request-Hash",apiSetting.IPAddress??string.Empty},
                //{ "X-Ipay-Request-Timestamp",DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")},
                //{ "User-Agent",modelRequest.UserAgent??string.Empty},
                { "Content-Type",ContentType.application_json},
                { "Accept","*/*"},
            };
            var fromD = ConnectionStringHelper.O.ConvertTransactionIDTo_dd_MMM_yyyy(modelRequest.TransactionID);

            var req = new
            {
                transactionDate = Convert.ToDateTime(fromD).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                externalRef = modelRequest.TID
            };
            string _URL = apiSetting.AEPSBaseURL + "reports/txnStatus";
            string request = _URL + "?" + JsonConvert.SerializeObject(req), response = string.Empty;
            try
            {
                response = AppWebRequest.O.PostJsonDataUsingHWRTLS(_URL, req, header).Result;
                if (!string.IsNullOrEmpty(response))
                {
                    var apiResp = JsonConvert.DeserializeObject<IPStsCheckResponse>(response);
                    if (apiResp != null)
                    {
                        if (apiResp.statuscode == "TXN")
                        {
                            if (apiResp.data != null)
                            {
                                res.Msg = apiResp.data.transactionStatus;
                                res.VendorID = (apiResp.data.order != null ? apiResp.data.order.refereceId : string.Empty);
                                res.Amount = Convert.ToInt32(apiResp.data.transactionAmount);
                                res.LiveID = apiResp.data.transactionReferenceId;
                                res.BankTransactionDate = string.Empty;
                                res.BankName =string.Empty;
                                if (apiResp.data.transactionStatusCode == "TXN")
                                {
                                    res.Statuscode = RechargeRespType.SUCCESS;
                                    res.Msg = RechargeRespType._SUCCESS;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                response = "EX:" + ex.Message + "|" + response;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "StatusCheck",
                    Error = ex.Message,
                });
            }
            res.Req = request;
            res.Resp = response;
            return res;
        }

    }
}
