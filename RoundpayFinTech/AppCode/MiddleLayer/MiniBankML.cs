using Fintech.AppCode.Configuration;
using Fintech.AppCode.HelperClass;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using Fintech.AppCode.WebRequest;
using Newtonsoft.Json;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.SDK;
using RoundpayFinTech.AppCode.StaticModel.MoneyTransfer;
using RoundpayFinTech.AppCode.ThirdParty.Fingpay;
using RoundpayFinTech.AppCode.ThirdParty.Mahagram;
using System;
using System.Threading.Tasks;
using Validators;

namespace RoundpayFinTech.AppCode.MiddleLayer
{
    public partial class AEPSML : IMiniBankML
    {
        public async Task<MiniBankTransactionServiceResp> MakeMiniBankTransaction(MiniBankTransactionServiceReq transactionServiceReq)
        {
            transactionServiceReq.RequestIP = _rinfo.GetRemoteIP();
            IProcedureAsync _proc = new ProcMiniBankTransactionService(_dal);
            var procRes = (MiniBankTransactionServiceResp)await _proc.Call(transactionServiceReq).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(procRes.CallBackURL) && transactionServiceReq.TXNType.In("CW", "DP"))
            {
                var respStatus = new RPFintechCallbackResponse
                {
                    Statuscode = ErrorCodes.Minus1,
                    Msg = ErrorCodes.NORESPONSE
                };
                var CBResp = string.Empty;
                try
                {
                    CBResp = AppWebRequest.O.CallUsingHttpWebRequest_GET(procRes.CallBackURL);
                }
                catch (Exception ex)
                {
                    CBResp = "ExceptionRP:" + ex.Message;
                }

                if (!string.IsNullOrEmpty(CBResp) && !(CBResp ?? string.Empty).StartsWith("ExceptionRP:"))
                {
                    try
                    {
                        respStatus = JsonConvert.DeserializeObject<RPFintechCallbackResponse>(CBResp);
                        if ((respStatus ?? new RPFintechCallbackResponse()).Statuscode != 1)
                        {
                            respStatus.Statuscode = RechargeRespType.FAILED;
                        }
                        IProcedureAsync _procHit = new ProcUpdateAPIURLHitting(_dal);
                        var _req = new APIURLHitting
                        {
                            UserID = procRes.UserID,
                            TransactionID = procRes.TransactionID,
                            URL = procRes.CallBackURL,
                            Response = CBResp
                        };
                        var _ = await _procHit.Call(_req).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                        {
                            ClassName = GetType().Name,
                            FuncName = "MBCallback",
                            Error = ex.Message + "||" + CBResp,
                            LoginTypeID = 1,
                            UserId = 1
                        });
                    }
                }
                IProcedure proc = new ProcUpdateMiniBank(_dal);
                var procres = (ResponseStatus)proc.Call(new MiniBankTransactionServiceResp
                {
                    LT = 1,
                    LoginID = 1,
                    Statuscode = respStatus.Statuscode,
                    VendorID = transactionServiceReq.VendorID,
                    Amount = Convert.ToInt32(transactionServiceReq.AmountR),
                    LiveID = respStatus.Msg ?? string.Empty,
                    Req = procRes.CallBackURL,
                    Resp = CBResp,
                    RequestPage = "CallbackUpdate",
                    Remark = respStatus.Msg ?? string.Empty,
                    CardNumber = string.Empty,
                    TID = procRes.TID,
                    IPAddress = _rinfo.GetRemoteIP(),
                    Browser = _rinfo.GetBrowser(),
                    BankBalance = "0.0",
                    APIRequestID = respStatus.APIRequestID
                });
                procRes.Statuscode = respStatus.Statuscode != ErrorCodes.One ? ErrorCodes.Minus1 : respStatus.Statuscode;
                procRes.Msg = "(PT)" + respStatus.Msg ?? "No response found";
            }

            return procRes;
        }

        public MBStatuscheckResponseApp MBStatusCheck(MBStatusCheckRequest transactionServiceReq)
        {
            var res = new MBStatuscheckResponseApp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = "Invalid Request"
            };
            var _APICode = string.Empty;
            MiniBankTransactionServiceResp apiReqForProc = null;


            if (transactionServiceReq.APIStatus != 3 || transactionServiceReq.TID > 0)
            {
                if (transactionServiceReq.TID > 0)
                {
                    IProcedure _proc = new ProcMiniBankGetDBStatus(_dal);
                    _APICode = (string)_proc.Call(transactionServiceReq.TID);
                }
                if (_APICode.Equals(APICode.MAHAGRAM) && (transactionServiceReq.APIStatus != 3 || !string.IsNullOrEmpty(transactionServiceReq.VendorID)))
                {
                    IMahagramAPIML mahagramAPIML = new MahagramAPIML(_accessor, _env, string.Empty);
                    transactionServiceReq.SCode = transactionServiceReq.SCode == ServiceCode.AEPS ? transactionServiceReq.SCode : string.Empty;
                    var apiResp = mahagramAPIML.MiniBankStatusCheck(transactionServiceReq);
                    apiReqForProc = new MiniBankTransactionServiceResp
                    {
                        LT = 1,
                        LoginID = 1,
                        Statuscode = apiResp.Statuscode,
                        VendorID = transactionServiceReq.VendorID,
                        Amount = apiResp.Amount,
                        LiveID = apiResp.LiveID,
                        Req = apiResp.Req,
                        Resp = apiResp.Resp,
                        RequestPage = transactionServiceReq.RequestPage,
                        Remark = apiResp.Msg,
                        CardNumber = apiResp.CardNumber,
                        TID = transactionServiceReq.TID,
                        IPAddress = _rinfo.GetRemoteIP(),
                        Browser = _rinfo.GetBrowser(),
                        BankBalance = apiResp.BankBalance,
                        BankName = apiResp.BankName
                    };

                    res.Statuscode = apiResp.Statuscode;
                    res.Msg = "(AP)" + apiResp.Msg;
                    res.LiveID = apiResp.LiveID;
                    res.BankName = apiResp.BankName;
                    res.CardNumber = apiResp.CardNumber;
                    res.Balance = apiResp.BankBalance;
                    res.TransactionTime = apiResp.BankTransactionDate;
                    res.Amount = apiResp.Amount;
                    res.Request = apiResp.Req;
                    res.Response = apiResp.Resp;
                }
                if (_APICode.Equals(APICode.FINGPAY))
                {
                    FingpayML fingpayML = new FingpayML(_dal);
                    IAPISetting aPISetting = new OnboardingML(_accessor, _env);
                    var fingPaySetting = aPISetting.GetFingpay();
                    var apiResp = fingpayML.MiniBankStatusCheck(fingPaySetting, transactionServiceReq);
                    apiReqForProc = new MiniBankTransactionServiceResp
                    {
                        LT = 1,
                        LoginID = 1,
                        Statuscode = apiResp.Statuscode,
                        VendorID = transactionServiceReq.VendorID,
                        Amount = apiResp.Amount,
                        LiveID = apiResp.LiveID,
                        Req = apiResp.Req,
                        Resp = apiResp.Resp,
                        RequestPage = transactionServiceReq.RequestPage,
                        Remark = apiResp.Msg,
                        CardNumber = apiResp.CardNumber,
                        TID = transactionServiceReq.TID,
                        IPAddress = _rinfo.GetRemoteIP(),
                        Browser = _rinfo.GetBrowser(),
                        BankBalance = apiResp.BankBalance,
                        BankName = apiResp.BankName
                    };
                    res.Statuscode = apiResp.Statuscode;
                    res.Msg = "(AP)" + apiResp.Msg;
                    res.LiveID = apiResp.LiveID;
                    res.BankName = apiResp.BankName;
                    res.CardNumber = apiResp.CardNumber;
                    res.Balance = apiResp.BankBalance;
                    res.TransactionTime = apiResp.BankTransactionDate;
                    res.Amount = apiResp.Amount;

                }
                else if (_APICode.Equals(APICode.MOSAMBEE))
                {
                    //apiReqForProc = new MiniBankTransactionServiceResp
                    //{
                    //    LT = 1,
                    //    LoginID = 1,
                    //    Statuscode = apiResp.Statuscode,
                    //    VendorID = transactionServiceReq.VendorID,
                    //    Amount = apiResp.Amount,
                    //    LiveID = apiResp.LiveID,
                    //    Req = apiResp.Req,
                    //    Resp = apiResp.Resp,
                    //    RequestPage = transactionServiceReq.RequestPage,
                    //    Remark = apiResp.Msg,
                    //    CardNumber = apiResp.CardNumber,
                    //    TID = transactionServiceReq.TID,
                    //    IPAddress = _rinfo.GetRemoteIP(),
                    //    Browser = _rinfo.GetBrowser(),
                    //    BankBalance = apiResp.BankBalance,
                    //    BankName = apiResp.BankName
                    //};
                    //res.Statuscode = apiResp.Statuscode;
                    //res.Msg = "(AP)" + apiResp.Msg;
                    //res.LiveID = apiResp.LiveID;
                    //res.BankName = apiResp.BankName;
                    //res.CardNumber = apiResp.CardNumber;
                    //res.Balance = apiResp.BankBalance;
                    //res.TransactionTime = apiResp.BankTransactionDate;
                    //res.Amount = apiResp.Amount;

                }
                else if (_APICode.Equals(APICode.SPRINT))
                {
                    SprintBBPSML aepsML = new SprintBBPSML(_accessor, _env, _dal);
                    var apiResp=aepsML.StatusCheck(new AEPSUniversalRequest
                    {
                        TID= transactionServiceReq.TID,
                        TransactionID= transactionServiceReq.TransactionID
                    });
                    apiReqForProc = new MiniBankTransactionServiceResp
                    {
                        LT = 1,
                        LoginID = 1,
                        Statuscode = apiResp.Statuscode,
                        VendorID = transactionServiceReq.VendorID,
                        Amount = apiResp.Amount,
                        LiveID = apiResp.LiveID,
                        Req = apiResp.Req,
                        Resp = apiResp.Resp,
                        RequestPage = transactionServiceReq.RequestPage,
                        Remark = apiResp.Msg,
                        CardNumber = apiResp.CardNumber,
                        TID = transactionServiceReq.TID,
                        IPAddress = _rinfo.GetRemoteIP(),
                        Browser = _rinfo.GetBrowser(),
                        BankBalance = apiResp.BankBalance,
                        BankName = apiResp.BankName
                    };
                    res.Statuscode = apiResp.Statuscode;
                    res.Msg = "(AP)" + apiResp.Msg;
                    res.LiveID = apiResp.LiveID;
                    res.BankName = apiResp.BankName;
                    res.CardNumber = apiResp.CardNumber;
                    res.Balance = apiResp.BankBalance;
                    res.TransactionTime = apiResp.BankTransactionDate;
                    res.Amount = apiResp.Amount;
                }
            }
            else
            {
                apiReqForProc = new MiniBankTransactionServiceResp
                {
                    LT = 1,
                    LoginID = 1,
                    Statuscode = RechargeRespType.FAILED,
                    VendorID = transactionServiceReq.VendorID,
                    Amount = transactionServiceReq.Amount,
                    LiveID = transactionServiceReq.SDKMsg,
                    CardNumber = transactionServiceReq.AccountNo,
                    BankName = transactionServiceReq.BankName,
                    RequestPage = transactionServiceReq.RequestPage,
                    Remark = transactionServiceReq.SDKMsg,
                    TID = transactionServiceReq.TID,
                    IPAddress = _rinfo.GetRemoteIP(),
                    Browser = _rinfo.GetBrowser()
                };
            }
            var rsStatus = UpdateMiniBankResponse(apiReqForProc);
            res.TransactionID = rsStatus.CommonStr2;
            return res;
        }
        public ResponseStatus UpdateMiniBankResponse(MiniBankTransactionServiceResp apiReqForProc)
        {
            if (apiReqForProc != null)
            {
                string TransDate = string.Empty;
                if (Validate.O.IsTransactionIDValid(apiReqForProc.TransactionID))
                {
                    TransDate = ConnectionStringHelper.O.ConvertTransactionIDTo_dd_MMM_yyyy(apiReqForProc.TransactionID);
                    _dal = ChangeConString(TransDate);
                }
                IProcedure proc = new ProcUpdateMiniBank(_dal);
                var procRes = (ResponseStatus)proc.Call(apiReqForProc);
                if (procRes.Statuscode == ErrorCodes.One)
                {
                    if (!string.IsNullOrEmpty(procRes.CommonStr))
                    {
                        var CBResp = string.Empty;
                        try
                        {
                            CBResp = AppWebRequest.O.CallUsingHttpWebRequest_GET(procRes.CommonStr);
                            if (!string.IsNullOrEmpty(CBResp))
                            {

                                IProcedureAsync _procHit = new ProcUpdateAPIURLHitting(_dal);
                                var _req = new APIURLHitting
                                {
                                    UserID = procRes.CommonInt,
                                    TransactionID = apiReqForProc.TID.ToString(),
                                    URL = procRes.CommonStr,
                                    Response = CBResp
                                };
                                var _ = _procHit.Call(_req).Result;
                            }
                        }
                        catch (Exception ex)
                        {
                            var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                            {
                                ClassName = GetType().Name,
                                FuncName = "MBCallbackUpdate",
                                Error = ex.Message + "||" + CBResp,
                                LoginTypeID = 1,
                                UserId = 1
                            });
                        }
                        apiReqForProc.Req = procRes.CommonStr;
                        apiReqForProc.Resp = CBResp;
                        proc.Call(apiReqForProc);
                    }
                }
                return procRes;
            }
            return new ResponseStatus { Statuscode = ErrorCodes.Minus1, Msg = "No updation found" };
        }
    }
}
