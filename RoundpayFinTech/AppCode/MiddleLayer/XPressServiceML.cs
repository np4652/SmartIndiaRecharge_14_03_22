using Fintech.AppCode.Configuration;
using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.MiddleLayer.Dmt_Api;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.MoneyTransfer;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.StaticModel.MoneyTransfer;
using RoundpayFinTech.AppCode.ThirdParty.Paytm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Validators;

namespace RoundpayFinTech.AppCode.MiddleLayer
{
    public class XPressServiceML
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly IDAL _dal;
        private readonly IConnectionConfiguration _c;
        private readonly IRequestInfo _info;

        public XPressServiceML(IHttpContextAccessor accessor, IHostingEnvironment env)
        {
            _accessor = accessor;
            _env = env;
            _c = new ConnectionConfiguration(_accessor, _env);
            _dal = new DAL(_c.GetConnectionString());
            _info = new RequestInfo(_accessor, _env);
        }

        public async Task<DMTTransactionResponse> DoPayout(PayoutTransactionRequest payoutRequest)
        {
            var res = new DMTTransactionResponse
            {
                ErrorCode = ErrorCodes.Invalid_Access,
                Message = nameof(ErrorCodes.Invalid_Access).Replace("_", " "),
                Statuscode = ErrorCodes.Minus1
            };
            #region RequestValidationFromCode  
            /**
             * Request validation from Code started
             * **/
            if (payoutRequest.UserID < 2)
            {
                res.ErrorCode = ErrorCodes.Invalid_Parameter;
                res.Message = "Unauthorised access!";
                return res;
            }
            //if (payoutRequest.OutletID < 10000)
            //{
            //    res.ErrorCode = ErrorCodes.Invalid_Parameter;
            //    res.Message = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " " + nameof(payoutRequest.OutletID);
            //    return res;
            //}
            if ((payoutRequest.Token ?? string.Empty).Length != 32)
            {
                res.ErrorCode = ErrorCodes.Invalid_Parameter;
                res.Message = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " " + nameof(payoutRequest.Token);
                return res;
            }

            if (payoutRequest.PayoutRequest == null)
            {
                res.ErrorCode = ErrorCodes.Invalid_Parameter;
                res.Message = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " " + nameof(payoutRequest.PayoutRequest);
                return res;
            }
            if (!(payoutRequest.PayoutRequest.SPKey ?? string.Empty).In(SPKeys.NEFT, SPKeys.IMPS, SPKeys.RTGS, SPKeys.UPI))
            {
                res.ErrorCode = ErrorCodes.Invalid_Parameter;
                res.Message = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " " + nameof(payoutRequest.PayoutRequest.SPKey);
                return res;
            }
            if ((payoutRequest.PayoutRequest.APIRequestID ?? string.Empty).Trim() == string.Empty)
            {
                res.ErrorCode = ErrorCodes.Invalid_Parameter;
                res.Message = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " " + nameof(payoutRequest.PayoutRequest.APIRequestID);
                return res;
            }
            if ((payoutRequest.PayoutRequest.AccountNo ?? string.Empty).Trim() == string.Empty)
            {
                res.ErrorCode = ErrorCodes.Invalid_Parameter;
                res.Message = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " " + nameof(payoutRequest.PayoutRequest.AccountNo);
                return res;
            }
            if (payoutRequest.PayoutRequest.AmountR < 1)
            {
                res.ErrorCode = ErrorCodes.Invalid_Parameter;
                res.Message = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " " + nameof(payoutRequest.PayoutRequest.AmountR);
                return res;
            }
            if (payoutRequest.PayoutRequest.BankID < 1 && payoutRequest.PayoutRequest.SPKey != SPKeys.UPI)
            {
                res.ErrorCode = ErrorCodes.Invalid_Parameter;
                res.Message = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " " + nameof(payoutRequest.PayoutRequest.BankID);
                return res;
            }
            if (string.IsNullOrEmpty(payoutRequest.PayoutRequest.BeneName))
            {
                res.ErrorCode = ErrorCodes.Invalid_Parameter;
                res.Message = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " " + nameof(payoutRequest.PayoutRequest.BeneName);
                return res;
            }
            if (!Validate.O.IsMobile(payoutRequest.PayoutRequest.BeneMobile ?? string.Empty))
            {
                res.ErrorCode = ErrorCodes.Invalid_Parameter;
                res.Message = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " " + nameof(payoutRequest.PayoutRequest.BeneMobile);
                return res;
            }
            if ((payoutRequest.PayoutRequest.IFSC ?? string.Empty).Length != 11 && payoutRequest.PayoutRequest.SPKey != SPKeys.UPI)
            {
                res.ErrorCode = ErrorCodes.Invalid_Parameter;
                res.Message = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " " + nameof(payoutRequest.PayoutRequest.IFSC);
                return res;
            }

            if (!Validate.O.IsMobile(payoutRequest.PayoutRequest.SenderMobile ?? string.Empty))
            {
                res.ErrorCode = ErrorCodes.Invalid_Parameter;
                res.Message = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " " + nameof(payoutRequest.PayoutRequest.SenderMobile);
                return res;
            }
            if (string.IsNullOrEmpty(payoutRequest.PayoutRequest.SenderName))
            {
                res.ErrorCode = ErrorCodes.Invalid_Parameter;
                res.Message = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " " + nameof(payoutRequest.PayoutRequest.SenderName);
                return res;
            }
            if (string.IsNullOrEmpty(payoutRequest.PayoutRequest.SenderEmail))
            {
                res.ErrorCode = ErrorCodes.Invalid_Parameter;
                res.Message = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " " + nameof(payoutRequest.PayoutRequest.SenderEmail);
                return res;
            }
            
            #endregion

            #region RequestValidationFromDB
            /**
             * Request validation from DB started
             * **/
            var validateReq = new ValidataRechargeApirequest
            {
                LoginID = payoutRequest.UserID,
                IPAddress = _info.GetRemoteIP(),
                Token = payoutRequest.Token,
                SPKey = payoutRequest.PayoutRequest.SPKey
            };
            IProcedureAsync _procValidate = new ProcValidataRechargeApirequest(_dal);
            var validateRes = (ValidataRechargeApiResp)await _procValidate.Call(validateReq).ConfigureAwait(false);
            if (validateRes.Statuscode == ErrorCodes.Minus1)
            {
                res.Message = validateRes.Msg;
                res.ErrorCode = Convert.ToInt32(validateRes.ErrorCode ?? "0");
                return res;
            }
            #endregion
            payoutRequest.PayoutRequest.UserID = payoutRequest.UserID;
            payoutRequest.PayoutRequest.OID = validateRes.OID;
            payoutRequest.PayoutRequest.OutletID = payoutRequest.OutletID;
            payoutRequest.PayoutRequest.RequestModeID = RequestMode.API;

            res = await CallProcAndService(payoutRequest.PayoutRequest, res).ConfigureAwait(false);
            return res;
        }
        public async Task<DMTTransactionResponse> DoPayoutVerify(PayoutTransactionRequest payoutRequest)
        {
            var res = new DMTTransactionResponse
            {
                ErrorCode = ErrorCodes.Invalid_Access,
                Message = nameof(ErrorCodes.Invalid_Access).Replace("_", " "),
                Statuscode = ErrorCodes.Minus1
            };
            #region RequestValidationFromCode  
            /**
             * Request validation from Code started
             * **/
            if (payoutRequest.UserID < 2)
            {
                res.ErrorCode = ErrorCodes.Invalid_Parameter;
                res.Message = "Unauthorised access!";
                return res;
            }
            //if (payoutRequest.OutletID < 10000)
            //{
            //    res.ErrorCode = ErrorCodes.Invalid_Parameter;
            //    res.Message = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " " + nameof(payoutRequest.OutletID);
            //    return res;
            //}
            if ((payoutRequest.Token ?? string.Empty).Length != 32)
            {
                res.ErrorCode = ErrorCodes.Invalid_Parameter;
                res.Message = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " " + nameof(payoutRequest.Token);
                return res;
            }

            if (payoutRequest.PayoutRequest == null)
            {
                res.ErrorCode = ErrorCodes.Invalid_Parameter;
                res.Message = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " " + nameof(payoutRequest.PayoutRequest);
                return res;
            }

            if (string.IsNullOrEmpty(payoutRequest.PayoutRequest.APIRequestID))
            {
                res.ErrorCode = ErrorCodes.Invalid_Parameter;
                res.Message = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " " + nameof(payoutRequest.PayoutRequest.APIRequestID);
                return res;
            }
            if (string.IsNullOrEmpty(payoutRequest.PayoutRequest.AccountNo))
            {
                res.ErrorCode = ErrorCodes.Invalid_Parameter;
                res.Message = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " " + nameof(payoutRequest.PayoutRequest.AccountNo);
                return res;
            }
            if (string.IsNullOrEmpty(payoutRequest.PayoutRequest.Bank))
            {
                res.ErrorCode = ErrorCodes.Invalid_Parameter;
                res.Message = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " " + nameof(payoutRequest.PayoutRequest.Bank);
                return res;
            }
            if (payoutRequest.PayoutRequest.BankID < 1)
            {
                res.ErrorCode = ErrorCodes.Invalid_Parameter;
                res.Message = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " " + nameof(payoutRequest.PayoutRequest.BankID);
                return res;
            }

            if (!Validate.O.IsMobile(payoutRequest.PayoutRequest.BeneMobile ?? string.Empty))
            {
                res.ErrorCode = ErrorCodes.Invalid_Parameter;
                res.Message = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " " + nameof(payoutRequest.PayoutRequest.BeneMobile);
                return res;
            }


            if (!Validate.O.IsMobile(payoutRequest.PayoutRequest.SenderMobile ?? string.Empty))
            {
                res.ErrorCode = ErrorCodes.Invalid_Parameter;
                res.Message = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " " + nameof(payoutRequest.PayoutRequest.SenderMobile);
                return res;
            }

            #endregion

            #region RequestValidationFromDB
            /**
             * Request validation from DB started
             * **/
            var validateReq = new ValidataRechargeApirequest
            {
                LoginID = payoutRequest.UserID,
                IPAddress = _info.GetRemoteIP(),
                Token = payoutRequest.Token,
                SPKey = "DMT"
            };
            IProcedureAsync _procValidate = new ProcValidataRechargeApirequest(_dal);
            var validateRes = (ValidataRechargeApiResp)await _procValidate.Call(validateReq).ConfigureAwait(false);
            if (validateRes.Statuscode == ErrorCodes.Minus1)
            {
                res.Message = validateRes.Msg;
                res.ErrorCode = Convert.ToInt32(validateRes.ErrorCode ?? "0");
                return res;
            }
            #endregion
            payoutRequest.PayoutRequest.UserID = payoutRequest.UserID;
            payoutRequest.PayoutRequest.OID = validateRes.OID;
            payoutRequest.PayoutRequest.OutletID = payoutRequest.OutletID;
            payoutRequest.PayoutRequest.RequestModeID = RequestMode.API;

            IMoneyTransferML moneyTransferML = new MoneyTransferML(_accessor, _env);
            var VeryFyRes = moneyTransferML.VerifyAccount(new MBeneVerifyRequest
            {
                RequestMode = RequestMode.API,
                SenderMobile = payoutRequest.PayoutRequest.SenderMobile,
                UserID = payoutRequest.UserID,
                OutletID = payoutRequest.OutletID,
                OID = validateRes.OID,
                AccountNo = payoutRequest.PayoutRequest.AccountNo,
                IFSC = payoutRequest.PayoutRequest.IFSC,
                BankID = payoutRequest.PayoutRequest.BankID,
                Bank = payoutRequest.PayoutRequest.Bank,
                BeneficiaryName = payoutRequest.PayoutRequest.BeneName,
                TransMode = "NEFT"
            });
            res.BeneName = VeryFyRes.BeneName;
            res.Status = VeryFyRes.Statuscode;
            res.Statuscode = ErrorCodes.One;
            res.Message = VeryFyRes.Msg;
            res.LiveID = VeryFyRes.LiveID;
            res.RPID = VeryFyRes.TransactionID;
            res.ErrorCode = VeryFyRes.ErrorCode;
            return res;
        }

        private async Task<DMTTransactionResponse> CallProcAndService(XPressServiceProcRequest PayoutRequest, DMTTransactionResponse res)
        {
            IProcedure _proc = new ProcValidateOutletForOperator(_dal);
            var forOutletAndAPICodeResp = (ValidateAPIOutletResp)_proc.Call(new CommonReq
            {
                LoginID = PayoutRequest.UserID,
                CommonInt = PayoutRequest.OutletID,
                CommonInt2 = PayoutRequest.OID
            });
            if (forOutletAndAPICodeResp.Statuscode == ErrorCodes.Minus1)
            {
                res.Message = forOutletAndAPICodeResp.Msg;
                res.ErrorCode = forOutletAndAPICodeResp.ErrorCode;
                return res;
            }
            PayoutRequest.APIID = forOutletAndAPICodeResp.APIID;
            PayoutRequest.RequestIP = _info.GetRemoteIP();
            IProcedure xpressServerProc = new ProcXPressService(_dal);
            var XpressSerRespProc = (DMRTransactionResponse)xpressServerProc.Call(PayoutRequest);
            if (XpressSerRespProc.Statuscode == ErrorCodes.Minus1)
            {
                res.Message = XpressSerRespProc.Msg;
                res.ErrorCode = XpressSerRespProc.ErrorCode;
                return res;
            }
            var APIRes = new DMRTransactionResponse
            {
                Statuscode = -3
            };
            if (forOutletAndAPICodeResp.APICode == APICode.PAYTM)
            {
                var PayTMChanelType = PayoutRequest.SPKey.Equals(SPKeys.NEFT) ? PaymentMode_.PAYTM_Neft : (PayoutRequest.SPKey.Equals(SPKeys.IMPS) ? PaymentMode_.PAYTM_IMPS : (PayoutRequest.SPKey.Equals(SPKeys.RTGS) ? PaymentMode_.PAYTM_RTGS : PaymentMode_.UPI));
                var dMTReq = new DMTReq
                {
                    Domain = string.Empty,
                    SenderNO = PayoutRequest.SenderMobile,
                    ChanelType = PayTMChanelType,
                    TID = XpressSerRespProc.TransactionID
                };
                var sendMoney = new ReqSendMoney
                {
                    Amount = PayoutRequest.AmountR,
                    AccountNo = PayoutRequest.AccountNo,
                    IFSC = PayoutRequest.IFSC
                };
                var dMRTransactionResponse = new DMRTransactionResponse
                {
                    TID = XpressSerRespProc.TID,
                    TransactionID = XpressSerRespProc.TransactionID
                };
                var paytmML = new PaytmML(_accessor, _env, PayoutRequest.APIID);
                APIRes = await paytmML.SendMoneyPayout(dMTReq, sendMoney, dMRTransactionResponse).ConfigureAwait(false);
            }
            else if (forOutletAndAPICodeResp.APICode == APICode.ICICIBANKPAYOUT)
            {
                var PayTMChanelType = PayoutRequest.SPKey.Equals(SPKeys.NEFT) ? PaymentMode_.PAYTM_Neft : (PayoutRequest.SPKey.Equals(SPKeys.IMPS) ? PaymentMode_.PAYTM_IMPS : (PayoutRequest.SPKey.Equals(SPKeys.RTGS) ? PaymentMode_.PAYTM_RTGS : PaymentMode_.UPI));
                var dMTReq = new DMTReq
                {
                    Domain = string.Empty,
                    SenderNO = PayoutRequest.SenderMobile,
                    ChanelType = PayTMChanelType,
                    TID = XpressSerRespProc.TransactionID
                };
                var sendMoney = new ReqSendMoney
                {
                    Amount = PayoutRequest.AmountR,
                    AccountNo = PayoutRequest.AccountNo,
                    IFSC = PayoutRequest.IFSC
                };
                var dMRTransactionResponse = new DMRTransactionResponse
                {
                    TID = XpressSerRespProc.TID,
                    TransactionID = XpressSerRespProc.TransactionID
                };
                var iCICIPayoutML = new ICICIPayoutML(_accessor, _env, PayoutRequest.APIID);
                APIRes = await iCICIPayoutML.ICICIPayout(dMTReq, sendMoney, dMRTransactionResponse).ConfigureAwait(false);
            }
            else if (forOutletAndAPICodeResp.APICode == APICode.PAYU)
            {
                var tAPIRequest = new MTAPIRequest
                {
                    APIID = PayoutRequest.APIID,
                    TransactionID = XpressSerRespProc.TransactionID,
                    TID = XpressSerRespProc.TID,
                    SenderMobile = PayoutRequest.SenderMobile,
                    UserName = PayoutRequest.SenderName,
                    RequestMode = PayoutRequest.RequestModeID,
                    UserID = PayoutRequest.UserID,
                    mBeneDetail = new MBeneDetail
                    {
                        IFSC = PayoutRequest.IFSC,
                        AccountNo = PayoutRequest.AccountNo,
                        MobileNo = PayoutRequest.SenderMobile,
                        BeneName = PayoutRequest.BeneName,
                        BankName = XpressSerRespProc.BankName
                    },
                    Amount = PayoutRequest.AmountR,
                    TransMode = PayoutRequest.SPKey,
                    IsPayout = true,
                    APIGroupCode = XpressSerRespProc.APIGroupCode
                };
                IMoneyTransferAPIML moneyTransferAPIML = new PayUML(_accessor, _env, _dal);
                APIRes = moneyTransferAPIML.AccountTransfer(tAPIRequest);
            }
            else if (forOutletAndAPICodeResp.APICode.Equals(APICode.RAZORPAYOUT) || forOutletAndAPICodeResp.APICode.EndsWith("RZRPOT"))
            {
                var tAPIRequest = new MTAPIRequest
                {
                    APIID = PayoutRequest.APIID,
                    TransactionID = XpressSerRespProc.TransactionID,
                    TID = XpressSerRespProc.TID,
                    SenderMobile = PayoutRequest.SenderMobile??string.Empty,
                    UserName = PayoutRequest.SenderName??string.Empty,
                    RequestMode = PayoutRequest.RequestModeID,
                    UserID = PayoutRequest.UserID,
                    mBeneDetail = new MBeneDetail
                    {
                        IFSC = PayoutRequest.IFSC??string.Empty,
                        AccountNo = PayoutRequest.AccountNo??string.Empty,
                        MobileNo = PayoutRequest.BeneMobile??string.Empty,
                        BeneName = PayoutRequest.BeneName??string.Empty,
                        BankName = XpressSerRespProc.BankName??string.Empty
                    },
                    Amount = PayoutRequest.AmountR,
                    TransMode = (PayoutRequest.SPKey??string.Empty).Contains("UPI")?nameof(PaymentMode_.UPI): PayoutRequest.SPKey,
                    IsPayout = true,
                    EmailID = PayoutRequest.SenderEmail,
                    APIGroupCode= XpressSerRespProc.APIGroupCode
                };
                IMoneyTransferAPIML moneyTransferAPIML = new RZRPayoutML(_accessor, _env, _dal, forOutletAndAPICodeResp.APICode);
                APIRes = moneyTransferAPIML.AccountTransfer(tAPIRequest);
            }
            else if (forOutletAndAPICodeResp.APICode == APICode.Manual)
            {
                var tAPIRequest = new MTAPIRequest
                {
                    APIID = PayoutRequest.APIID,
                    TransactionID = XpressSerRespProc.TransactionID,
                    TID = XpressSerRespProc.TID,
                    SenderMobile = PayoutRequest.SenderMobile,
                    UserName = PayoutRequest.SenderName,
                    RequestMode = PayoutRequest.RequestModeID,
                    UserID = PayoutRequest.UserID,
                    mBeneDetail = new MBeneDetail
                    {
                        BankID = PayoutRequest.BankID,
                        IFSC = PayoutRequest.IFSC,
                        AccountNo = PayoutRequest.AccountNo,
                        MobileNo = PayoutRequest.SenderMobile,
                        BeneName = PayoutRequest.BeneName,
                        BankName = XpressSerRespProc.BankName
                    },
                    Amount = PayoutRequest.AmountR,
                    TransMode = PayoutRequest.SPKey,
                    IsPayout = true,
                    APIGroupCode = XpressSerRespProc.APIGroupCode
                };
                IMoneyTransferAPIML moneyTransferAPIML = new ManualDMT_ML(_accessor, _env, _dal, APICode.Manual);
                APIRes = moneyTransferAPIML.AccountTransfer(tAPIRequest);
            }
            else if (forOutletAndAPICodeResp.APIOpCode.EndsWith("XPRESS"))
            {
                var tAPIRequest = new MTAPIRequest
                {
                    APIID = PayoutRequest.APIID,
                    TransactionID = XpressSerRespProc.TransactionID,
                    TID = XpressSerRespProc.TID,
                    SenderMobile = PayoutRequest.SenderMobile,
                    UserName = PayoutRequest.SenderName,
                    RequestMode = PayoutRequest.RequestModeID,
                    UserID = PayoutRequest.UserID,
                    mBeneDetail = new MBeneDetail
                    {
                        BankID = PayoutRequest.BankID,
                        IFSC = PayoutRequest.IFSC,
                        AccountNo = PayoutRequest.AccountNo,
                        MobileNo = PayoutRequest.BeneMobile,
                        BeneName = PayoutRequest.BeneName,
                        BankName = XpressSerRespProc.BankName
                    },
                    Amount = PayoutRequest.AmountR,
                    TransMode = PayoutRequest.SPKey,
                    IsPayout = true,
                    EmailID = PayoutRequest.SenderEmail,
                    APIGroupCode = XpressSerRespProc.APIGroupCode
                };
                IMoneyTransferAPIML moneyTransferAPIML = new FintechPayoutML(_accessor, _env, _dal, forOutletAndAPICodeResp.APICode);
                APIRes = moneyTransferAPIML.AccountTransfer(tAPIRequest);
            }
            else if (forOutletAndAPICodeResp.APIOpCode.Equals(APICode.CASHFREE))
            {
                var tAPIRequest = new MTAPIRequest
                {
                    APIID = PayoutRequest.APIID,
                    TransactionID = XpressSerRespProc.TransactionID,
                    TID = XpressSerRespProc.TID,
                    SenderMobile = PayoutRequest.SenderMobile,
                    UserName = PayoutRequest.SenderName,
                    RequestMode = PayoutRequest.RequestModeID,
                    UserID = PayoutRequest.UserID,
                    mBeneDetail = new MBeneDetail
                    {
                        BankID = PayoutRequest.BankID,
                        IFSC = PayoutRequest.IFSC,
                        AccountNo = PayoutRequest.AccountNo,
                        MobileNo = PayoutRequest.BeneMobile,
                        BeneName = PayoutRequest.BeneName,
                        BankName = XpressSerRespProc.BankName
                    },
                    Amount = PayoutRequest.AmountR,
                    TransMode = PayoutRequest.SPKey,
                    IsPayout = true,
                    EmailID = PayoutRequest.SenderEmail,
                    APIGroupCode = XpressSerRespProc.APIGroupCode
                };
                IMoneyTransferAPIML moneyTransferAPIML = new CashFreePayoutML(_accessor, _env, _dal, forOutletAndAPICodeResp.APIGroupCode);
                APIRes = moneyTransferAPIML.AccountTransfer(tAPIRequest);
            }

            var IsInternalSender = true;
            if (APIRes.Statuscode > 0)
            {
                res.Message = APIRes.Msg;
                res.ErrorCode = APIRes.ErrorCode;
                res.Status = APIRes.Statuscode;
                res.Statuscode = APIRes.Statuscode > 0 ? ErrorCodes.One : APIRes.Statuscode;
                res.RPID = XpressSerRespProc.TransactionID;
                res.LiveID = APIRes.LiveID;
                res.BeneName = APIRes.BeneName;
                APIRes.TID = XpressSerRespProc.TID;
                var sameUpdate = (ResponseStatus)(new ProcUpdateDMRTransaction(_dal)).Call(APIRes);
                if (sameUpdate.Statuscode == ErrorCodes.Minus1)
                {
                    IsInternalSender = false;
                }
            }

            if (APIRes.Statuscode == RechargeRespType.SUCCESS && IsInternalSender)
            {
                //Only for Internal Sender
                new AlertML(_accessor, _env, false).PayoutSMS(new AlertReplacementModel
                {
                    LoginID = PayoutRequest.UserID,
                    UserMobileNo = PayoutRequest.SenderMobile,
                    WID = 1,
                    Amount = PayoutRequest.AmountR,
                    AccountNo = PayoutRequest.AccountNo,
                    SenderName = PayoutRequest.SenderName,
                    TransMode = PayoutRequest.SPKey,
                    UTRorRRN = APIRes.LiveID,
                    IFSC = PayoutRequest.IFSC
                });
            }
            return res;
        }
    }
}
