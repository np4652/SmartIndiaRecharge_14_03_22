using Fintech.AppCode.Configuration;
using Fintech.AppCode.DB;
using Fintech.AppCode.HelperClass;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using Fintech.AppCode.WebRequest;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.SDK;
using System;
using System.Threading.Tasks;
using Validators;

namespace RoundpayFinTech.AppCode.MiddleLayer
{
    public partial class AEPSML : IAEPSML, IAEPSControllerHelper
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly ISession _session;
        private IDAL _dal;
        private readonly IConnectionConfiguration _c;
        private readonly LoginResponse _lr;
        private readonly IUserML userML;
        private readonly IRequestInfo _rinfo;
        public AEPSML(IHttpContextAccessor accessor, IHostingEnvironment env, bool InSession = true)
        {
            _accessor = accessor;
            _env = env;
            _c = new ConnectionConfiguration(_accessor, _env);
            _dal = new DAL(_c.GetConnectionString());
            _rinfo = new RequestInfo(_accessor, _env);
            if (InSession)
            {
                _session = _accessor.HttpContext.Session;
                _lr = _session.GetObjectFromJson<LoginResponse>(SessionKeys.LoginResponse);
                userML = new UserML(_lr);
            }
        }

        private IDAL ChangeConString(string _date)
        {
            if (Validate.O.IsDateIn_dd_MMM_yyyy_Format(_date))
            {
                TypeMonthYear typeMonthYear = ConnectionStringHelper.O.GetTypeMonthYear(Convert.ToDateTime(_date.Replace(" ", "/")));
                if (typeMonthYear.ConType != ConnectionStringType.DBCon)
                {
                    _dal = new DAL(_c.GetConnectionString(typeMonthYear.ConType, (typeMonthYear.MM ?? "") + "_" + (typeMonthYear.YYYY ?? "")));
                }
            }
            return _dal;
        }

        //public async Task<AEPSTransactionServiceResp> MakeAEPSTransaction(AEPSTransactionServiceReq transactionServiceReq)
        //{
        //    transactionServiceReq.RequestIP = _rinfo.GetRemoteIP();
        //    IProcedureAsync _proc = new ProcAEPSTransactionService(_dal);
        //    return (AEPSTransactionServiceResp)await _proc.Call(transactionServiceReq);
        //}
        public AEPSTransactionServiceResp MakeAEPSMiniStmtTransaction(InititateMiniStatementTransactionRequest transactionServiceReq)
        {
            transactionServiceReq.IP = _rinfo.GetRemoteIP();
            transactionServiceReq.Browser = _rinfo.GetBrowser();
            IProcedure _proc = new ProcInititateMiniStatementTransaction(_dal);
            var procRes=(AEPSTransactionServiceResp) _proc.Call(transactionServiceReq);
            if (!string.IsNullOrEmpty(procRes.CallBackURL))
            {
                var respStatus = new RPFintechCallbackResponse
                {
                    Statuscode = ErrorCodes.Minus1,
                    Msg = ErrorCodes.NORESPONSE
                };

                var CBResp = AppWebRequest.O.CallUsingHttpWebRequest_GET(procRes.CallBackURL);
                if (!string.IsNullOrEmpty(CBResp))
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
                        var _ = _procHit.Call(_req).Result;
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
                    Amount = 1,
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

        public async Task<AEPSTransactionServiceResp> UpdateAEPSTransaction(AEPSTransactionServiceReq transactionServiceReq)
        {
            var res = new AEPSTransactionServiceResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = "Invalid status to update"
            };
            if (transactionServiceReq.Status.In(RechargeRespType.SUCCESS, RechargeRespType.FAILED))
            {
                transactionServiceReq.RequestIP = _rinfo.GetRemoteIP();
                var _callbackData = new _CallbackData
                {
                    TransactionStatus = transactionServiceReq.Status,
                    TID = transactionServiceReq.TID,
                    VendorID = transactionServiceReq.VendorID,
                    LiveID = transactionServiceReq.LiveID,
                    APICode = transactionServiceReq.APICode,
                    RequestPage = transactionServiceReq.UpdatePage,
                    RequestIP = transactionServiceReq.RequestIP,
                    Browser = _rinfo.GetBrowser(),
                    LoginID = 1,
                    LT = 1,
                    TransactionID = transactionServiceReq.TransactionID,
                    Request = transactionServiceReq.Req,
                    Response = transactionServiceReq.Resp
                };
                if (_callbackData.TransactionID != null && _callbackData.TransactionID != "")
                {
                    string TransDate = ConnectionStringHelper.O.ConvertTransactionIDTo_dd_MMM_yyyy(_callbackData.TransactionID);
                    _dal = ChangeConString(TransDate);
                }
                IProcedureAsync _proc = new ProcUpdateAEPSTransactionService(_dal);
                _callbackData = (_CallbackData)await _proc.Call(_callbackData).ConfigureAwait(false);
                res.Statuscode = _callbackData.Statuscode;
                res.Msg = _callbackData.Msg;
            }
            return res;
        }
        public async Task SaveAEPSLog(string APICode, string Method, string Requset, string Response)
        {
            var req = new CommonReq
            {
                str = APICode,
                CommonStr = Method,
                CommonStr2 = Requset,
                CommonStr3 = Response
            };
            IProcedureAsync _proc = new ProcLogAEPSCallbackReqRes(_dal);
            await _proc.Call(req).ConfigureAwait(false);
        }
    }
}
