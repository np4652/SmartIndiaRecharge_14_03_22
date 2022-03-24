using Fintech.AppCode;
using Fintech.AppCode.Configuration;
using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using RoundpayFinTech.AppCode.Configuration;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.MiddleLayer.Dmt_Api;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.MoneyTransfer;
using RoundpayFinTech.AppCode.Model.Paymentgateway;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.ThirdParty.AggrePay;
using RoundpayFinTech.AppCode.ThirdParty.CashFree;
using RoundpayFinTech.AppCode.ThirdParty.Paytm;
using RoundpayFinTech.AppCode.ThirdParty.PayU;
using RoundpayFinTech.AppCode.ThirdParty.Razorpay;
using RoundpayFinTech.AppCode.ThirdParty.UPIGateway;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Validators;

namespace RoundpayFinTech.AppCode.MiddleLayer
{
    public class PaymentGatewayML : IPaymentGatewayML, IUPIPaymentML
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly IDAL _dal;
        private readonly IConnectionConfiguration _c;
        private readonly IRequestInfo _rinfo;

        public PaymentGatewayML(IHttpContextAccessor accessor, IHostingEnvironment env)
        {
            _accessor = accessor;
            _env = env;
            _c = new ConnectionConfiguration(_accessor, _env);
            _dal = new DAL(_c.GetConnectionString());
            _rinfo = new RequestInfo(_accessor, _env);
        }
        public IEnumerable<PaymentGatewayModel> GetPGDetailsUser(int WID, bool IsUPI)
        {
            var res = new List<PaymentGatewayModel>();
            IProcedure proc = new ProcGetPaymentGatewayUser(_dal);
            var lst = (List<PaymentGatewayDetail>)proc.Call(new CommonReq
            {
                CommonInt = WID,
                CommonBool = IsUPI
            });
            if (lst != null && lst.Count > 0)
            {
                res = lst.Select(x => new PaymentGatewayModel { ID = x.UPGID, PG = x.PG, PGType = x.PGID, AgentType = x.AgentType }).ToList();
            }
            return res;
        }
        public IEnumerable<PaymentGatewayDetail> GetPGDetails(int WID, bool IsUPI)
        {
            var res = new List<PaymentGatewayDetail>();
            IProcedure proc = new ProcGetPaymentGatewayUser(_dal);
            res = (List<PaymentGatewayDetail>)proc.Call(new CommonReq
            {
                CommonInt = WID,
                CommonBool = IsUPI
            });
            return res;
        }
        public PGModelForRedirection IntiatePGTransactionForWeb(int UserID, decimal Amount, int UPGID, int OID, int WalletID, string Domain, string VPA)
        {
            PGModelForRedirection res = new PGModelForRedirection();
            var req = new PGTransactionRequest
            {
                UserID = UserID,
                RequestIP = _rinfo.GetRemoteIP(),
                Browser = _rinfo.GetBrowserFullInfo(),
                RequestMode = RequestMode.PANEL,
                AmountR = Amount,
                UPGID = UPGID,
                OID = OID,
                WalletID = WalletID
            };
            try
            {
                IProcedure proc = new ProcPGatewayTransacrionService(_dal);
                var procRes = (PGTransactionResponse)proc.Call(req);
                res.Statuscode = procRes?.Statuscode ?? 0;
                res.Msg = procRes.Msg;
                procRes.VPA = VPA;
                if (procRes != null && procRes.Statuscode == ErrorCodes.One)
                {
                    if (procRes.PGID == PaymentGatewayType.PAYTM)
                    {
                        procRes.Domain = Domain;
                        var paytmML = new PaytmML(_dal);
                        res = paytmML.GeneratePGRequestForWeb(procRes, SavePGTransactionLog);
                        res.URL = procRes.URL;
                    }
                    else if (procRes.PGID == PaymentGatewayType.PAYTMJS)
                    {
                        procRes.Domain = Domain;
                        var paytmML = new PaytmML(_dal);
                        res = paytmML.GeneratePGRequestForJS(procRes, SavePGTransactionLog);
                        res.paytmJSRequest.PayMode = PayTMPaymentModeFromSPKey(procRes.OPID);
                        res.URL = procRes.URL;
                    }
                    else if (procRes.PGID == PaymentGatewayType.RZRPAY)
                    {
                        var razorpay = new RazorpayPGML(_dal);
                        res = razorpay.GeneratePGRequestForWeb(procRes, SavePGTransactionLog);
                        if (res.Statuscode == ErrorCodes.One)
                        {
                            UpdateUPIPaymentStatus(new UpdatePGTransactionRequest
                            {
                                TID = procRes.TID,
                                Type = RechargeRespType.PENDING,
                                LiveID = string.Empty,
                                PGID = procRes.PGID,
                                PaymentModeSpKey = procRes.OPID,
                                VendorID = res.RPayRequest.order_id
                            });
                        }

                    }
                    else if (procRes.PGID == PaymentGatewayType.AGRPAY)
                    {
                        procRes.Domain = Domain;
                        var aggrePay = new AggrePayML(_dal);
                        res = aggrePay.GeneratePGRequestForWeb(procRes, SavePGTransactionLog);
                        res.URL = procRes.URL;
                    }
                    else if (procRes.PGID == PaymentGatewayType.UPIGATEWAY && !string.IsNullOrEmpty(procRes.VPA))
                    {
                        procRes.Domain = Domain;
                        var uPIGateway = new UPIGatewayML(_dal);
                        res = uPIGateway.GeneratePGRequestForWeb(procRes, SavePGTransactionLog);
                        res.URL = procRes.URL;
                    }
                    /* CashFree */
                    else if (procRes.PGID == PaymentGatewayType.CASHFREE)
                    {
                        procRes.Domain = Domain;
                        var cashFree = new CashFreePGML(_dal);
                        res = cashFree.GeneratePGRequestForWebAsync(procRes, SavePGTransactionLog).Result;
                        if (res.Statuscode == ErrorCodes.One)
                        {
                            UpdateUPIPaymentStatus(new UpdatePGTransactionRequest
                            {
                                TID = procRes?.TID ?? 0,
                                Type = RechargeRespType.PENDING,
                                LiveID = string.Empty,
                                PGID = procRes.PGID,
                                PaymentModeSpKey = procRes.OPID,
                                VendorID = res.CashfreeResponse.order_id
                            });
                        }

                    }
                    /**/
                    /* PayU */
                    else if (procRes.PGID == PaymentGatewayType.PayU)
                    {
                        procRes.Domain = Domain;
                        var payU = new PayUPGML(_dal);
                        res = payU.GeneratePGRequestForWeb(procRes, SavePGTransactionLog);
                        res.URL = procRes.URL;
                    }
                    /* End of PayU */
                    res.TID = procRes.TID;
                }
                res.PGType = procRes.PGID;
            }
            catch (Exception ex)
            {
                res.Statuscode = ErrorCodes.Minus1;
                res.Msg = ex.Message;
                res.Statuscode = ErrorCodes.Minus1;
                res.Msg = ex.Message;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "PaymentGatewayML -->IntiatePGTransactionForWeb",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = 1
                });
            }
            return res;
        }
        public PGModelForApp IntiatePGTransactionForApp(int UserID, int Amount, int UPGID, int OID, int WalletID, string IMEI)
        {
            var res = new PGModelForApp();
            var req = new PGTransactionRequest
            {
                UserID = UserID,
                RequestIP = _rinfo.GetRemoteIP(),
                Browser = _rinfo.GetBrowserFullInfo(),
                RequestMode = RequestMode.APPS,
                AmountR = Amount,
                UPGID = UPGID,
                OID = OID,
                WalletID = WalletID,
                IMEI = IMEI
            };
            IProcedure proc = new ProcPGatewayTransacrionService(_dal);
            var procRes = (PGTransactionResponse)proc.Call(req);
            res.Statuscode = procRes.Statuscode;
            res.Msg = procRes.Msg;
            res.PGID = procRes.PGID;
            if (procRes.Statuscode == ErrorCodes.One)
            {

                res.TID = procRes.TID;
                res.TransactionID = procRes.TransactionID;
                procRes.Domain = GetDomain();
                if (procRes.PGID == PaymentGatewayType.PAYTM)
                {
                    var paytmML = new PaytmML(_dal);
                    paytmML.GeneratePGRequestForApp(procRes, res, SavePGTransactionLog);
                }
                else if (procRes.PGID == PaymentGatewayType.PAYTMJS)
                {
                    var paytmML = new PaytmML(_dal);
                    paytmML.GeneratePGRequestForJSApp(procRes, res, SavePGTransactionLog);

                }
                else if (procRes.PGID == PaymentGatewayType.RZRPAY)
                {
                    var razorpay = new RazorpayPGML(_dal);
                    var rPayWeb = razorpay.GeneratePGRequestForWeb(procRes, SavePGTransactionLog);
                    if (rPayWeb.Statuscode == ErrorCodes.One)
                    {
                        UpdateUPIPaymentStatus(new UpdatePGTransactionRequest
                        {
                            TID = procRes.TID,
                            Type = RechargeRespType.PENDING,
                            LiveID = string.Empty,
                            PGID = procRes.PGID,
                            PaymentModeSpKey = procRes.OPID,
                            VendorID = rPayWeb.RPayRequest.order_id
                        });
                        res.RPayRequest = rPayWeb.RPayRequest;
                    }
                }
                else if (procRes.PGID == PaymentGatewayType.AGRPAY)
                {
                    var aggrePay = new AggrePayML(_dal);
                    res.AggrePayRequest = aggrePay.GeneratePGRequestForApp(procRes, SavePGTransactionLog);
                }
                else if (procRes.PGID == PaymentGatewayType.UPIGATEWAY)
                {
                    var uPIGateway = new UPIGatewayML(_dal);
                    res.UPIGatewayRequest = uPIGateway.GeneratePGRequestForApp(procRes, SavePGTransactionLog);
                }
                /* CashFree */
                else if (procRes.PGID == PaymentGatewayType.CASHFREE)
                {
                    procRes.Domain = GetDomain();//Domain;
                    var cashFree = new CashFreePGML(_dal);
                    var cashFreeRes = cashFree.GeneratePGRequestForAppAsync(procRes, SavePGTransactionLog).Result;
                    if (cashFreeRes.Statuscode == ErrorCodes.One)
                    {
                        UpdateUPIPaymentStatus(new UpdatePGTransactionRequest
                        {
                            TID = procRes.TID,
                            Type = RechargeRespType.PENDING,
                            LiveID = string.Empty,
                            PGID = procRes.PGID,
                            PaymentModeSpKey = procRes.OPID,
                            VendorID = cashFreeRes.CashFreeResponseForApp.orderId
                        });
                        res.CashFreeResponse = cashFreeRes.CashFreeResponseForApp;
                    }
                }
                /* End CashFree */
            }
            return res;
        }
        private string GetDomain()
        {
            var cInfo = _rinfo.GetCurrentReqInfo();
            return cInfo.Scheme + "://" + cInfo.Host + (cInfo.Port > 0 ? ":" + cInfo.Port : "");
        }
        public IResponseStatus UpdateFromAggrePayCallback(AggrePayResponse aggrePayResponse)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = "Invalid update request"
            };
            var TID = 0;
            if (Validate.O.IsNumeric(aggrePayResponse.order_id ?? string.Empty))
            {
                TID = Convert.ToInt32(aggrePayResponse.order_id);
            }
            var ToMatchData = GetLogForMatching(new CommonReq
            {
                CommonInt = TID
            });
            try
            {
                var aggrePayML = new AggrePayML(_dal);
                var statusResp = aggrePayML.StatusCheckPG(ToMatchData, SavePGTransactionLog);
                if (statusResp.data != null && statusResp.data.Count > 0)
                {
                    if (statusResp.data[0].response_code != null)
                    {
                        if (!aggrePayResponse.response_code.Equals(statusResp.data[0].response_code) || aggrePayResponse.amount != statusResp.data[0].amount)
                        {
                            return res;
                        }
                    }
                    else
                    {
                        return res;
                    }
                }
                else
                {
                    return res;
                }

            }
            catch (Exception)
            {
                return res;
            }
            bool IsMatched = false;
            if (ToMatchData.TID > 0)
            {
                var Amount = Convert.ToInt32(aggrePayResponse.amount);
                if (TID == ToMatchData.TID)
                {
                    if (Amount == ToMatchData.Amount)
                    {
                        IsMatched = true;
                    }
                }
            }

            if (IsMatched)
            {
                var req = new UpdatePGTransactionRequest
                {
                    PGID = PaymentGatewayType.AGRPAY,
                    TID = TID,
                    VendorID = aggrePayResponse.transaction_id,
                    LiveID = aggrePayResponse.transaction_id,
                    Remark = aggrePayResponse.response_message,
                    PaymentModeSpKey = AggrePayPaymentMode(aggrePayResponse.payment_mode),
                    RequestIP = _rinfo.GetRemoteIP(),
                    Browser = _rinfo.GetBrowserFullInfo()
                };
                req.Type = RechargeRespType.PENDING;
                if (aggrePayResponse.response_code.Equals("0"))
                {
                    req.Type = RechargeRespType.SUCCESS;
                }
                else
                {
                    if (Validate.O.IsNumeric(aggrePayResponse.response_code ?? string.Empty))
                    {
                        var Statuscode = Convert.ToInt32(aggrePayResponse.response_code);
                        if (Statuscode >= 1000 && Statuscode <= 1087)
                        {
                            req.Type = RechargeRespType.FAILED;
                        }
                    }
                }
                IProcedure proc = new ProcUpdatePaytmTransaction(_dal);
                var resp = (AlertReplacementModel)proc.Call(req);
                res.Statuscode = resp.Statuscode;
                res.Msg = resp.Msg;
            }

            return res;
        }
        public IResponseStatus UpdateFromAggrePayApp(int TID, int Amount, string Hash)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = "Invalid update request"
            };
            bool IsMatched = false;
            var ToMatchData = GetLogForMatching(new CommonReq
            {
                CommonInt = TID
            });
            var aggrePayResponse = new AggrePayResponse();
            try
            {
                var aggrePayML = new AggrePayML(_dal);
                var statusResp = aggrePayML.StatusCheckPG(ToMatchData, SavePGTransactionLog);
                if (statusResp.data != null && statusResp.data.Count > 0)
                {
                    if (statusResp.data[0].response_code != null)
                    {
                        //|| string.IsNullOrEmpty(Hash) || !(Hash ?? string.Empty).Equals(ToMatchData.Checksum) 
                        if (statusResp.data[0].amount != Amount || TID != Convert.ToInt32(statusResp.data[0].order_id))
                        {
                            return res;
                        }
                        aggrePayResponse.response_code = statusResp.data[0].response_code;
                        aggrePayResponse.response_message = statusResp.data[0].response_message;
                        aggrePayResponse.transaction_id = statusResp.data[0].transaction_id;
                        aggrePayResponse.amount = Amount;
                        aggrePayResponse.payment_mode = statusResp.data[0].payment_mode;
                        IsMatched = true;
                    }
                    else
                    {
                        return res;
                    }
                }
                else
                {
                    return res;
                }

            }
            catch (Exception)
            {
                return res;
            }
            if (IsMatched)
            {
                var req = new UpdatePGTransactionRequest
                {
                    PGID = PaymentGatewayType.AGRPAY,
                    TID = TID,
                    VendorID = aggrePayResponse.transaction_id,
                    LiveID = aggrePayResponse.transaction_id,
                    Remark = aggrePayResponse.response_message,
                    PaymentModeSpKey = AggrePayPaymentMode(aggrePayResponse.payment_mode),
                    RequestIP = _rinfo.GetRemoteIP(),
                    Browser = _rinfo.GetBrowserFullInfo()
                };
                req.Type = RechargeRespType.PENDING;
                if (aggrePayResponse.response_code.Equals("0"))
                {
                    req.Type = RechargeRespType.SUCCESS;
                }
                else
                {
                    if (Validate.O.IsNumeric(aggrePayResponse.response_code ?? string.Empty))
                    {
                        var Statuscode = Convert.ToInt32(aggrePayResponse.response_code);
                        if (Statuscode >= 1000 && Statuscode <= 1087)
                        {
                            req.Type = RechargeRespType.FAILED;
                        }
                    }
                }
                IProcedure proc = new ProcUpdatePaytmTransaction(_dal);
                var resp = (AlertReplacementModel)proc.Call(req);
                res.Statuscode = resp.Statuscode;
                res.Msg = resp.Msg;
            }

            return res;
        }

        public IResponseStatus UpdateFromCashFreeApp(int TID, int Amount, string Hash)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = "Invalid update request"
            };

            bool IsMatched = false;
            var ToMatchData = GetLogForMatching(new CommonReq
            {
                CommonInt = TID
            });
            var cashFreeResponse = new CashfreeStatusResponse();
            try
            {
                var cashFree = new CashFreePGML(_dal);
                var statusResp = cashFree.StatusCheckPG(ToMatchData, SavePGTransactionLog);
                if (statusResp != null)
                {
                    if (statusResp.orderStatus != null)
                    {
                        statusResp.TID = !string.IsNullOrEmpty(statusResp.orderId) ? statusResp.orderId.Replace("TID", string.Empty, StringComparison.OrdinalIgnoreCase) : "0";
                        if (statusResp.orderAmount != Amount || TID != Convert.ToInt32(statusResp.TID))
                        {
                            res.Msg = "request missmatched";
                            return res;
                        }
                        cashFreeResponse = statusResp;
                        //cashFreeResponse.orderId = statusResp.orderId;
                        //cashFreeResponse.orderStatus = statusResp.orderStatus;
                        //cashFreeResponse.TID = statusResp.TID;
                        //cashFreeResponse.paymentMode = statusResp.paymentMode;
                        IsMatched = true;
                    }
                    else
                    {
                        return res;
                    }
                }
                else
                {
                    return res;
                }

            }
            catch (Exception)
            {
                return res;
            }
            if (IsMatched)
            {
                var req = new UpdatePGTransactionRequest
                {
                    PGID = PaymentGatewayType.CASHFREE,
                    TID = !string.IsNullOrEmpty(cashFreeResponse.TID) ? Convert.ToInt32(cashFreeResponse.TID) : 0,
                    VendorID = cashFreeResponse.orderId,
                    LiveID = cashFreeResponse.referenceId,
                    PaymentModeSpKey = cashFreeResponse.paymentMode,
                    Remark = string.Empty,
                    RequestIP = _rinfo.GetRemoteIP(),
                    Browser = _rinfo.GetBrowserFullInfo()
                };
                req.Type = RechargeRespType.PENDING;
                if (cashFreeResponse.orderStatus != null && cashFreeResponse.orderStatus.Equals("paid", StringComparison.OrdinalIgnoreCase))
                {
                    req.Type = RechargeRespType.SUCCESS;
                }
                else if ((cashFreeResponse.orderStatus != null && cashFreeResponse.orderStatus.Equals("FAILED", StringComparison.OrdinalIgnoreCase)) || (cashFreeResponse.txStatus != null && cashFreeResponse.txStatus.Equals("FAILED", StringComparison.OrdinalIgnoreCase)))
                {
                    req.Type = RechargeRespType.FAILED;
                }
                else if (cashFreeResponse.status.Equals("error", StringComparison.OrdinalIgnoreCase) && cashFreeResponse.reason.Contains("Order Id does not exist", StringComparison.OrdinalIgnoreCase))
                {
                    req.Type = RechargeRespType.FAILED;
                }
                if (!string.IsNullOrEmpty(req.PaymentModeSpKey))
                {
                    IProcedure proc1 = new ProcUpdatePaytmTransaction(_dal);
                    var resp = (AlertReplacementModel)proc1.Call(req);
                    res.Statuscode = resp.Statuscode;
                    res.Msg = resp.Msg;
                }
                else
                {
                    string msg = "Your Order {0} for Amount {1} is awaited from Bank Side We will Update once get Response From Bank Side.";
                    res.Msg = string.Format(msg, req.TID, Amount);//;"(SPKEY)Pending";
                }
            }
            return res;
        }

        /* PayU */
        public IResponseStatus UpdateFromPayUCallback(PayUResponse payuResponse)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = "Invalid update request"
            };
            var TID = 0;
            if (Validate.O.IsNumeric(payuResponse.txnid ?? string.Empty))
            {
                TID = Convert.ToInt32(payuResponse.txnid);
            }
            var ToMatchData = GetLogForMatching(new CommonReq
            {
                CommonInt = TID
            });
            try
            {
                var payuMl = new PayUPGML(_dal);
                var statusResp = payuMl.StatusCheckPG(ToMatchData, SavePGTransactionLog);
                if (statusResp.transaction_details != null)
                {
                    if (statusResp.transaction_details.status != null)
                    {
                        if (!payuResponse.status.Equals(statusResp.transaction_details.status) || payuResponse.amount != statusResp.transaction_details.amount)
                        {
                            return res;
                        }
                    }
                    else
                    {
                        return res;
                    }
                }
                else
                {
                    return res;
                }

            }
            catch (Exception)
            {
                return res;
            }
            bool IsMatched = false;
            if (ToMatchData.TID > 0)
            {
                var Amount = Convert.ToInt32(payuResponse.amount);
                if (TID == ToMatchData.TID)
                {
                    if (Amount == ToMatchData.Amount)
                    {
                        IsMatched = true;
                    }
                }
            }

            if (IsMatched)
            {
                var req = new UpdatePGTransactionRequest
                {
                    PGID = PaymentGatewayType.PayU,
                    TID = TID,
                    VendorID = payuResponse.txnid,
                    LiveID = payuResponse.txnid,
                    Remark = payuResponse.status,
                    PaymentModeSpKey = payuResponse.mode,
                    RequestIP = _rinfo.GetRemoteIP(),
                    Browser = _rinfo.GetBrowserFullInfo()
                };
                req.Type = RechargeRespType.PENDING;
                if (payuResponse.status.Equals("success", StringComparison.OrdinalIgnoreCase))
                {
                    req.Type = RechargeRespType.SUCCESS;
                }
                else
                {
                    if (Validate.O.IsNumeric(payuResponse.status ?? string.Empty))
                    {
                        var Statuscode = Convert.ToInt32(payuResponse.status);
                        if (Statuscode >= 1000 && Statuscode <= 1087)
                        {
                            req.Type = RechargeRespType.FAILED;
                        }
                    }
                }
                IProcedure proc = new ProcUpdatePaytmTransaction(_dal);
                var resp = (AlertReplacementModel)proc.Call(req);
                res.Statuscode = resp.Statuscode;
                res.Msg = resp.Msg;
            }
            return res;
        }
        /* End Of PayU*/

        private string AggrePayPaymentMode(string PayMode)
        {
            if (!string.IsNullOrEmpty(PayMode))
            {
                if (PayMode.Equals("Credit Card"))
                    return PaymentGatewayTranMode.CreditCard;
                if (PayMode.Equals("Dredit Card"))
                    return PaymentGatewayTranMode.DebitCard;
                if (PayMode.Equals("Netbanking"))
                    return PaymentGatewayTranMode.NetBanking;
                if (PayMode.Equals("UPI"))
                    return PaymentGatewayTranMode.UPI;
                if (PayMode.Equals("Wallet"))
                    return PaymentGatewayTranMode.PPIWALLET;
            }
            return PaymentGatewayTranMode.DebitCard;
        }
        public IResponseStatus UpdateFromPayTMCallback(PaytmPGResponse paytmPGResponse)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = "Invalid update request"
            };
            var TID = 0;
            if (Validate.O.IsNumeric(paytmPGResponse.ORDERID ?? string.Empty))
            {
                TID = Convert.ToInt32(paytmPGResponse.ORDERID);
            }
            var ToMatchData = GetLogForMatching(new CommonReq
            {
                CommonInt = TID
            });
            var domain = string.Empty;
            var WID = 0;
            try
            {
                IProcedure GetDomainProc = new ProcGetDomainFromTID(_dal);
                var respGetDomain = (ResponseStatus)GetDomainProc.Call(new CommonReq
                {
                    CommonInt = TID,
                    CommonStr = "PGTransaction"
                });
                domain = respGetDomain.CommonStr;
                WID = respGetDomain.CommonInt;
                res.CommonStr = domain;
                res.CommonInt = WID;
                var PaytmML = new PaytmML(_dal);
                var statusResp = new PaytmPGResponse();
                if (ToMatchData.PGID == PaymentGatewayType.PAYTMJS)
                {
                    statusResp = PaytmML.StatusCheckPGJS(ToMatchData, SavePGTransactionLog);
                }
                else
                {
                    statusResp = PaytmML.StatusCheckPG(ToMatchData, SavePGTransactionLog);
                }

                if (statusResp.STATUS != null)
                {
                    if (statusResp.STATUS.ToUpper().Equals("PENDING"))
                    {
                        StringBuilder sb = new StringBuilder("Your Order {TID} for Amount {AMOUNT} is awaited from Bank Side We will Update once get Response From Bank.");
                        sb.Replace("{TID}", paytmPGResponse.ORDERID);
                        sb.Replace("{AMOUNT}", paytmPGResponse.TXNAMOUNT);
                        res.Msg = sb.ToString();
                        return res;
                    }
                    if (statusResp.STATUS == PAYTMResponseType.FAILED)
                    {
                        paytmPGResponse.STATUS = statusResp.STATUS;
                    }
                    if (paytmPGResponse.STATUS != statusResp.STATUS || paytmPGResponse.TXNAMOUNT != statusResp.TXNAMOUNT)
                    {
                        return res;
                    }
                }
                else
                {
                    return res;
                }
            }
            catch (Exception)
            {
                return res;
            }
            bool IsMatched = false;
            if (ToMatchData.TID > 0)
            {
                var Amount = Validate.O.IsNumeric((paytmPGResponse.TXNAMOUNT ?? string.Empty).Replace(".", "")) ? Convert.ToInt32(paytmPGResponse.TXNAMOUNT.Split('.')[0]) : 0;
                if (TID == ToMatchData.TID)
                {
                    if (Amount == ToMatchData.Amount)
                    {
                        IsMatched = true;
                    }
                }
            }

            if (IsMatched)
            {
                var req = new UpdatePGTransactionRequest
                {
                    PGID = ToMatchData.PGID,
                    TID = TID,
                    VendorID = paytmPGResponse.TXNID,
                    LiveID = paytmPGResponse.BANKTXNID,
                    Remark = paytmPGResponse.RESPMSG,
                    PaymentModeSpKey = PayTMPaymentMode(paytmPGResponse.PAYMENTMODE),
                    RequestIP = _rinfo.GetRemoteIP(),
                    Browser = _rinfo.GetBrowserFullInfo()
                };
                req.Type = RechargeRespType.PENDING;
                if (paytmPGResponse.STATUS == PAYTMResponseType.SUCCESS)
                {
                    req.Type = RechargeRespType.SUCCESS;
                }
                if (paytmPGResponse.STATUS == PAYTMResponseType.FAILED)
                {
                    req.Type = RechargeRespType.FAILED;
                }

                IProcedure proc = new ProcUpdatePaytmTransaction(_dal);
                var resp = (AlertReplacementModel)proc.Call(req);
                res.Statuscode = resp.Statuscode;
                res.Msg = resp.Msg;
            }
            res.CommonStr = domain;
            res.CommonInt = WID;
            return res;
        }
        public async Task SavePGTransactionLog(int PGID, int TID, string Log, string TransactionID, string Checksum, int RequestMode, bool IsRequestGenerated, decimal Amount, string VendorID)
        {
            try
            {
                var transactionPGLog = new TransactionPGLog
                {
                    PGID = PGID,
                    TID = TID,
                    Log = Log,
                    TransactionID = TransactionID,
                    RequestIP = _rinfo.GetRemoteIP(),
                    Browser = _rinfo.GetBrowserFullInfo(),
                    RequestMode = RequestMode,
                    Checksum = Checksum,
                    IsRequestGenerated = IsRequestGenerated,
                    Amount = Amount,
                    VendorID = VendorID
                };
                if (PGID == 8)
                {
                    MakeICICIFileLog(JsonConvert.SerializeObject(transactionPGLog), "cashhFreeError");
                }

                IProcedureAsync proc = new ProcSavePGTransactionReqResp(_dal);
                await proc.Call(transactionPGLog).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "SavePGTransactionLog",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = 1
                });
            }
        }

        public void MakeICICIFileLog(string response, string filename)
        {
            try
            {
                string path = "Image/ICICI/log/" + filename;
                using (StreamWriter file = new StreamWriter(path, true))
                {
                    file.Write(response);
                }

            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "MakeICICIFileLogPG",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = 1
                });
            }
        }

        public void LoopPGTransactionStatus()
        {
            int LastID = 0;
            int newLastID;
        StartHere:
            newLastID = LastID;
            var proc = new ProcPGUpdateAutoAndGetOldest(_dal);
            var res = (List<ResponseStatus>)proc.Call(LastID);
            if (res.Count > 0)
            {
                foreach (var item in res)
                {
                    LastID = item.CommonInt;
                    var _ = CheckPGTransactionStatus(new CommonReq
                    {
                        LoginTypeID = LoginType.ApplicationUser,
                        LoginID = 1,
                        CommonInt = item.CommonInt2
                    });
                }
                if (LastID != newLastID)
                {
                    goto StartHere;
                }
            }

        }
        //public ResponseStatus CheckPGTransactionStatus(CommonReq param)
        //{
        //    var res = new ResponseStatus
        //    {
        //        Statuscode = ErrorCodes.Minus1,
        //        Msg = ErrorCodes.AnError
        //    };
        //    IProcedure _proc = new ProcGetTransactionPGDetail(_dal);
        //    var procRes = (PGTransactionParam)_proc.Call(param);
        //    if (procRes.Status.In(RechargeRespType.REQUESTSENT, RechargeRespType.PENDING))
        //    {
        //        if (procRes.PGID == PaymentGatewayType.PAYTM)
        //        {
        //            var PaytmML = new PaytmML(_dal);
        //            var ToMatchData = new TransactionPGLog
        //            {
        //                TID = procRes.TID,
        //                PGID = procRes.PGID,
        //                TransactionID = procRes.TransactionID,
        //                Checksum = procRes.Checksum,
        //                StatuscheckURL = procRes.StatusCheckURL,
        //                VendorID = procRes.VendorID,
        //                MerchantID = procRes.MerchantID,
        //                MerchantKEY = procRes.MerchantKey,
        //                Amount = procRes.Amount
        //            };
        //            var statusResp = PaytmML.StatusCheckPG(ToMatchData, SavePGTransactionLog);
        //            if (statusResp != null)
        //            {
        //                var req = new UpdatePGTransactionRequest
        //                {
        //                    PGID = PaymentGatewayType.PAYTM,
        //                    TID = procRes.TID,
        //                    VendorID = statusResp.TXNID,
        //                    LiveID = statusResp.BANKTXNID,
        //                    Remark = statusResp.RESPMSG,
        //                    PaymentModeSpKey = PayTMPaymentMode(statusResp.PAYMENTMODE),
        //                    RequestIP = _rinfo.GetRemoteIP(),
        //                    Browser = _rinfo.GetBrowserFullInfo()
        //                };
        //                req.Type = RechargeRespType.PENDING;
        //                if (statusResp.STATUS == PAYTMResponseType.SUCCESS)
        //                {
        //                    req.Type = RechargeRespType.SUCCESS;
        //                }
        //                if (statusResp.STATUS == PAYTMResponseType.FAILED)
        //                {
        //                    req.Type = RechargeRespType.FAILED;
        //                }

        //                IProcedure proc = new ProcUpdatePaytmTransaction(_dal);
        //                res = (ResponseStatus)proc.Call(req);
        //                res.CommonStr = ToMatchData.Request;
        //                res.CommonStr2 = ToMatchData.Response;
        //            }
        //            else
        //            {
        //                res.Msg = "No response";
        //            }
        //        }
        //        else if (procRes.PGID == PaymentGatewayType.PAYTMJS)
        //        {
        //            var PaytmML = new PaytmML(_dal);
        //            var ToMatchData = new TransactionPGLog
        //            {
        //                TID = procRes.TID,
        //                PGID = procRes.PGID,
        //                TransactionID = procRes.TransactionID,
        //                Checksum = procRes.Checksum,
        //                StatuscheckURL = procRes.StatusCheckURL,
        //                VendorID = procRes.VendorID,
        //                MerchantID = procRes.MerchantID,
        //                MerchantKEY = procRes.MerchantKey,
        //                Amount = procRes.Amount
        //            };
        //            var statusResp = PaytmML.StatusCheckPGJS(ToMatchData, SavePGTransactionLog);
        //            if (statusResp != null)
        //            {
        //                var req = new UpdatePGTransactionRequest
        //                {
        //                    PGID = PaymentGatewayType.PAYTMJS,
        //                    TID = procRes.TID,
        //                    VendorID = statusResp.TXNID,
        //                    LiveID = statusResp.BANKTXNID,
        //                    Remark = statusResp.RESPMSG,
        //                    PaymentModeSpKey = PayTMPaymentMode(statusResp.PAYMENTMODE),
        //                    RequestIP = _rinfo.GetRemoteIP(),
        //                    Browser = _rinfo.GetBrowserFullInfo()
        //                };
        //                req.Type = RechargeRespType.PENDING;
        //                if (statusResp.STATUS == PAYTMResponseType.SUCCESS)
        //                {
        //                    req.Type = RechargeRespType.SUCCESS;
        //                }
        //                if (statusResp.STATUS == PAYTMResponseType.FAILED)
        //                {
        //                    req.Type = RechargeRespType.FAILED;
        //                }
        //                IProcedure proc = new ProcUpdatePaytmTransaction(_dal);
        //                res = (ResponseStatus)proc.Call(req);
        //                res.CommonStr = ToMatchData.Request;
        //                res.CommonStr2 = ToMatchData.Response;
        //            }
        //            else
        //            {
        //                res.Msg = "No response";
        //            }
        //        }
        //        else if (procRes.PGID == PaymentGatewayType.RZRPAY)
        //        {
        //            res.Statuscode = procRes.Statuscode;
        //            res.Msg = RechargeRespType.GetRechargeStatusText(procRes.Status);
        //            res.Status = procRes.Status;
        //            var rML = new RazorpayPGML(_dal);
        //            var stsCheckResp = rML.StatusCheckPG(procRes, SavePGTransactionLog);
        //            if (stsCheckResp != null)
        //            {
        //                if (procRes.Amount * 100 == stsCheckResp.amount && procRes.Amount > 0)
        //                {
        //                    var PayMethod = stsCheckResp.method;
        //                    if (PayMethod.Equals("card"))
        //                    {
        //                        PayMethod = stsCheckResp.card == null ? PaymentGatewayTranMode.DebitCard : stsCheckResp.card.type;
        //                    }
        //                    var req = new UpdatePGTransactionRequest
        //                    {
        //                        PGID = PaymentGatewayType.RZRPAY,
        //                        TID = procRes.TID,
        //                        VendorID = stsCheckResp.order_id,
        //                        LiveID = stsCheckResp.id,
        //                        Remark = stsCheckResp.vpa,
        //                        PaymentModeSpKey = RazorPaymentMode(PayMethod),
        //                        RequestIP = _rinfo.GetRemoteIP(),
        //                        Browser = _rinfo.GetBrowserFullInfo(),
        //                        Type = RechargeRespType.PENDING
        //                    };
        //                    if (stsCheckResp.status.Equals("captured"))
        //                    {
        //                        req.Type = RechargeRespType.SUCCESS;
        //                    }
        //                    else if (stsCheckResp.status.Equals("failed"))
        //                    {
        //                        req.Type = RechargeRespType.FAILED;
        //                    }
        //                    IProcedure proc = new ProcUpdatePaytmTransaction(_dal);
        //                    res = (ResponseStatus)proc.Call(req);
        //                }
        //            }
        //        }
        //        else if (procRes.PGID == PaymentGatewayType.ICICIUPI)
        //        {
        //            //CollectPayStatusCheckWeb
        //            var iciciPayoutML = new ICICIPayoutML(_accessor, _env, procRes.PGID);
        //            var apiReq = new ColllectUPPayReqModel
        //            {
        //                TID = procRes.TID,
        //                UserID = procRes.UserID,
        //                Amount = procRes.Amount,
        //                TransactionID = procRes.TransactionID
        //            };
        //            var statusResp = new CollectUPPayResponse();
        //            statusResp = procRes.RequestMode == RequestMode.APPS ? iciciPayoutML.CollectPayCallbackStatusCheck(new ColllectUPPayReqModel
        //            {
        //                StatusCheckType = "Q",
        //                TID = procRes.TID,
        //                LiveID = procRes.LiveID,
        //                TransactionID = procRes.TransactionID,
        //                UserID = procRes.UserID,
        //                UPIID = procRes.Remark,
        //                Amount = procRes.Amount
        //            }, SavePGTransactionLog) : iciciPayoutML.CollectPayStatusCheckWeb(apiReq, SavePGTransactionLog);

        //            if (statusResp != null)
        //            {
        //                var req = new UpdatePGTransactionRequest
        //                {
        //                    PGID = PaymentGatewayType.ICICIUPI,
        //                    TID = procRes.TID,
        //                    VendorID = statusResp.VendorID,
        //                    LiveID = statusResp.BankRRN,
        //                    Remark = statusResp.Remark,
        //                    PaymentModeSpKey = PaymentGatewayTranMode.UPIICI,
        //                    RequestIP = _rinfo.GetRemoteIP(),
        //                    Browser = _rinfo.GetBrowserFullInfo()
        //                };
        //                req.Type = RechargeRespType.PENDING;
        //                if (statusResp.Statuscode == RechargeRespType.SUCCESS)
        //                {
        //                    req.Type = RechargeRespType.SUCCESS;
        //                }
        //                if (statusResp.Statuscode == RechargeRespType.FAILED)
        //                {
        //                    req.Type = RechargeRespType.FAILED;
        //                }
        //                if (procRes.RequestMode == RequestMode.APPS && string.IsNullOrEmpty(statusResp.CommonStr4))
        //                {
        //                    if ((statusResp.CommonStr4 ?? "0") != procRes.Amount.ToString() && statusResp.Statuscode != RechargeRespType.SUCCESS)
        //                    {
        //                        req.Type = RechargeRespType.PENDING;
        //                    }
        //                }
        //                IProcedure proc = new ProcUpdatePaytmTransaction(_dal);
        //                res = (ResponseStatus)proc.Call(req);
        //                res.CommonStr = string.Empty;
        //                res.CommonStr2 = statusResp.Resp;
        //            }
        //            else
        //            {
        //                res.Msg = "No response";
        //            }
        //        }
        //        else if (procRes.PGID == PaymentGatewayType.AGRPAY)
        //        {
        //            var ToMatchData = new TransactionPGLog
        //            {
        //                MerchantID = procRes.MerchantID,
        //                MerchantKEY = procRes.MerchantKey,
        //                TID = procRes.TID,
        //                TransactionID = procRes.TransactionID,
        //                PGID = procRes.PGID,
        //                StatuscheckURL = procRes.StatusCheckURL
        //            };
        //            var aggrePay = new AggrePayML(_dal);
        //            var aggrePayResponse = aggrePay.StatusCheckPG(ToMatchData, SavePGTransactionLog);
        //            if (aggrePayResponse.data != null && aggrePayResponse.data.Count > 0)
        //            {
        //                var req = new UpdatePGTransactionRequest
        //                {
        //                    PGID = PaymentGatewayType.AGRPAY,
        //                    TID = procRes.TID,
        //                    VendorID = aggrePayResponse.data[0].transaction_id,
        //                    LiveID = aggrePayResponse.data[0].transaction_id,
        //                    Remark = aggrePayResponse.data[0].response_message,
        //                    PaymentModeSpKey = AggrePayPaymentMode(aggrePayResponse.data[0].payment_mode),
        //                    RequestIP = _rinfo.GetRemoteIP(),
        //                    Browser = _rinfo.GetBrowserFullInfo()
        //                };
        //                req.Type = RechargeRespType.PENDING;
        //                if (aggrePayResponse.data[0].response_code.Equals("0"))
        //                {
        //                    req.Type = RechargeRespType.SUCCESS;
        //                }
        //                else
        //                {
        //                    if (Validate.O.IsNumeric(aggrePayResponse.data[0].response_code ?? string.Empty))
        //                    {
        //                        var Statuscode = Convert.ToInt32(aggrePayResponse.data[0].response_code);
        //                        if (Statuscode >= 1000 && Statuscode <= 1087)
        //                        {
        //                            req.Type = RechargeRespType.FAILED;
        //                        }
        //                    }
        //                }
        //                IProcedure proc = new ProcUpdatePaytmTransaction(_dal);
        //                res = (ResponseStatus)proc.Call(req);
        //            }

        //            res.CommonStr = ToMatchData.Request;
        //            res.CommonStr2 = ToMatchData.Response;
        //        }
        //        /*CashFree*/
        //        else if (procRes.PGID == PaymentGatewayType.CASHFREE)
        //        {
        //            var ToMatchData = new TransactionPGLog
        //            {
        //                MerchantID = procRes.MerchantID,
        //                MerchantKEY = procRes.MerchantKey,
        //                TID = procRes.TID,
        //                TransactionID = procRes.TransactionID,
        //                PGID = procRes.PGID,
        //                StatuscheckURL = procRes.StatusCheckURL
        //            };
        //            var cashFree = new CashFreePGML(_dal);
        //            var cashFreeResponse = cashFree.StatusCheckPG(ToMatchData, SavePGTransactionLog);
        //            if (cashFreeResponse != null && cashFreeResponse.StatusCode == ErrorCodes.One)
        //            {
        //                var req = new UpdatePGTransactionRequest
        //                {
        //                    PGID = PaymentGatewayType.CASHFREE,
        //                    TID = procRes.TID,
        //                    VendorID = cashFreeResponse.orderId,
        //                    LiveID = string.Empty,
        //                    PaymentModeSpKey = procRes.OPID,
        //                    Remark = string.Empty,
        //                    RequestIP = _rinfo.GetRemoteIP(),
        //                    Browser = _rinfo.GetBrowserFullInfo()
        //                };
        //                req.Type = RechargeRespType.PENDING;
        //                if (cashFreeResponse.orderStatus.Equals("paid", StringComparison.OrdinalIgnoreCase))
        //                {
        //                    req.Type = RechargeRespType.SUCCESS;
        //                }
        //                else if (cashFreeResponse.orderStatus.Equals("failed", StringComparison.OrdinalIgnoreCase))
        //                {
        //                    req.Type = RechargeRespType.FAILED;
        //                }
        //                IProcedure proc = new ProcUpdatePaytmTransaction(_dal);
        //                res = (ResponseStatus)proc.Call(req);
        //            }
        //            res.CommonStr = ToMatchData.Request;
        //            res.CommonStr2 = ToMatchData.Response;
        //        }
        //        /*End CashFree*/
        //        else
        //        {
        //            res.Statuscode = procRes.Status;
        //            res.Msg = RechargeRespType.GetRechargeStatusText(procRes.Status);
        //        }
        //    }
        //    else
        //    {
        //        res.Statuscode = procRes.Status;
        //        res.Msg = RechargeRespType.GetRechargeStatusText(procRes.Status);
        //    }

        //    return res;
        //}
        public ResponseStatus CheckPGTransactionStatus(CommonReq param)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            IProcedure _proc = new ProcGetTransactionPGDetail(_dal);
            var procRes = (PGTransactionParam)_proc.Call(param);
            if (procRes.Status.In(RechargeRespType.REQUESTSENT, RechargeRespType.PENDING))
            {
                if (procRes.PGID == PaymentGatewayType.PAYTM)
                {
                    var PaytmML = new PaytmML(_dal);
                    var ToMatchData = new TransactionPGLog
                    {
                        TID = procRes.TID,
                        PGID = procRes.PGID,
                        TransactionID = procRes.TransactionID,
                        Checksum = procRes.Checksum,
                        StatuscheckURL = procRes.StatusCheckURL,
                        VendorID = procRes.VendorID,
                        MerchantID = procRes.MerchantID,
                        MerchantKEY = procRes.MerchantKey,
                        Amount = procRes.Amount
                    };
                    var statusResp = PaytmML.StatusCheckPG(ToMatchData, SavePGTransactionLog);
                    if (statusResp != null)
                    {
                        var req = new UpdatePGTransactionRequest
                        {
                            PGID = PaymentGatewayType.PAYTM,
                            TID = procRes.TID,
                            VendorID = statusResp.TXNID,
                            LiveID = statusResp.BANKTXNID,
                            Remark = statusResp.RESPMSG,
                            PaymentModeSpKey = PayTMPaymentMode(statusResp.PAYMENTMODE),
                            RequestIP = _rinfo.GetRemoteIP(),
                            Browser = _rinfo.GetBrowserFullInfo()
                        };
                        req.Type = RechargeRespType.PENDING;
                        if (statusResp.STATUS == PAYTMResponseType.SUCCESS)
                        {
                            req.Type = RechargeRespType.SUCCESS;
                        }
                        if (statusResp.STATUS == PAYTMResponseType.FAILED)
                        {
                            req.Type = RechargeRespType.FAILED;
                        }

                        IProcedure proc = new ProcUpdatePaytmTransaction(_dal);
                        var resp = (AlertReplacementModel)proc.Call(req);
                        res.Statuscode = resp.Statuscode;
                        res.Msg = resp.Msg;
                        resp.CommonStr = ToMatchData.Request;
                        resp.CommonStr2 = ToMatchData.Response;
                    }
                    else
                    {
                        res.Msg = "No response";
                    }
                }
                else if (procRes.PGID == PaymentGatewayType.PAYTMJS)
                {
                    var PaytmML = new PaytmML(_dal);
                    var ToMatchData = new TransactionPGLog
                    {
                        TID = procRes.TID,
                        PGID = procRes.PGID,
                        TransactionID = procRes.TransactionID,
                        Checksum = procRes.Checksum,
                        StatuscheckURL = procRes.StatusCheckURL,
                        VendorID = procRes.VendorID,
                        MerchantID = procRes.MerchantID,
                        MerchantKEY = procRes.MerchantKey,
                        Amount = procRes.Amount
                    };
                    var statusResp = PaytmML.StatusCheckPGJS(ToMatchData, SavePGTransactionLog);
                    if (statusResp != null)
                    {
                        var req = new UpdatePGTransactionRequest
                        {
                            PGID = PaymentGatewayType.PAYTMJS,
                            TID = procRes.TID,
                            VendorID = statusResp.TXNID,
                            LiveID = statusResp.BANKTXNID,
                            Remark = statusResp.RESPMSG,
                            PaymentModeSpKey = PayTMPaymentMode(statusResp.PAYMENTMODE),
                            RequestIP = _rinfo.GetRemoteIP(),
                            Browser = _rinfo.GetBrowserFullInfo()
                        };
                        req.Type = RechargeRespType.PENDING;
                        if (statusResp.STATUS == PAYTMResponseType.SUCCESS)
                        {
                            req.Type = RechargeRespType.SUCCESS;
                        }
                        if (statusResp.STATUS == PAYTMResponseType.FAILED)
                        {
                            req.Type = RechargeRespType.FAILED;
                        }
                        IProcedure proc = new ProcUpdatePaytmTransaction(_dal);
                        var resp = (AlertReplacementModel)proc.Call(req);
                        res.Statuscode = resp.Statuscode;
                        res.Msg = resp.Msg;
                        resp.CommonStr = ToMatchData.Request;
                        resp.CommonStr2 = ToMatchData.Response;
                    }
                    else
                    {
                        res.Msg = "No response";
                    }
                }
                else if (procRes.PGID == PaymentGatewayType.RZRPAY)
                {
                    res.Statuscode = procRes.Statuscode;
                    res.Msg = RechargeRespType.GetRechargeStatusText(procRes.Status);
                    res.Status = procRes.Status;
                    var rML = new RazorpayPGML(_dal);
                    var stsCheckResp = rML.StatusCheckPG(procRes, SavePGTransactionLog);
                    if (stsCheckResp != null)
                    {
                        if (procRes.Amount * 100 == stsCheckResp.amount && procRes.Amount > 0)
                        {
                            var PayMethod = stsCheckResp.method;
                            if (PayMethod.Equals("card"))
                            {
                                PayMethod = stsCheckResp.card == null ? PaymentGatewayTranMode.DebitCard : stsCheckResp.card.type;
                            }
                            var req = new UpdatePGTransactionRequest
                            {
                                PGID = PaymentGatewayType.RZRPAY,
                                TID = procRes.TID,
                                VendorID = stsCheckResp.order_id,
                                LiveID = stsCheckResp.id,
                                Remark = stsCheckResp.vpa,
                                PaymentModeSpKey = RazorPaymentMode(PayMethod),
                                RequestIP = _rinfo.GetRemoteIP(),
                                Browser = _rinfo.GetBrowserFullInfo(),
                                Type = RechargeRespType.PENDING
                            };
                            if (stsCheckResp.status.Equals("captured"))
                            {
                                req.Type = RechargeRespType.SUCCESS;
                            }
                            else if (stsCheckResp.status.Equals("failed"))
                            {
                                req.Type = RechargeRespType.FAILED;
                            }
                            IProcedure proc = new ProcUpdatePaytmTransaction(_dal);
                            var resp = (AlertReplacementModel)proc.Call(req);
                            res.Statuscode = resp.Statuscode;
                            res.Msg = resp.Msg;
                        }
                    }
                }
                else if (procRes.PGID == PaymentGatewayType.ICICIUPI)
                {
                    //CollectPayStatusCheckWeb
                    var iciciPayoutML = new ICICIPayoutML(_accessor, _env, procRes.PGID);
                    var apiReq = new ColllectUPPayReqModel
                    {
                        TID = procRes.TID,
                        UserID = procRes.UserID,
                        Amount = procRes.Amount,
                        TransactionID = procRes.TransactionID
                    };
                    var statusResp = new CollectUPPayResponse();
                    statusResp = procRes.RequestMode == RequestMode.APPS ? iciciPayoutML.CollectPayCallbackStatusCheck(new ColllectUPPayReqModel
                    {
                        StatusCheckType = "Q",
                        TID = procRes.TID,
                        LiveID = procRes.LiveID,
                        TransactionID = procRes.TransactionID,
                        UserID = procRes.UserID,
                        UPIID = procRes.Remark,
                        Amount = procRes.Amount
                    }, SavePGTransactionLog) : iciciPayoutML.CollectPayStatusCheckWeb(apiReq, SavePGTransactionLog);

                    if (statusResp != null)
                    {
                        var req = new UpdatePGTransactionRequest
                        {
                            PGID = PaymentGatewayType.ICICIUPI,
                            TID = procRes.TID,
                            VendorID = statusResp.VendorID,
                            LiveID = statusResp.BankRRN,
                            Remark = statusResp.Remark,
                            PaymentModeSpKey = PaymentGatewayTranMode.UPIICI,
                            RequestIP = _rinfo.GetRemoteIP(),
                            Browser = _rinfo.GetBrowserFullInfo()
                        };
                        req.Type = RechargeRespType.PENDING;
                        if (statusResp.Statuscode == RechargeRespType.SUCCESS)
                        {
                            req.Type = RechargeRespType.SUCCESS;
                        }

                        if (statusResp.Statuscode == RechargeRespType.FAILED)
                        {
                            req.Type = RechargeRespType.FAILED;
                        }
                        if (procRes.RequestMode == RequestMode.APPS && string.IsNullOrEmpty(statusResp.CommonStr4))
                        {
                            if ((statusResp.CommonStr4 ?? "0") != procRes.Amount.ToString() && statusResp.Statuscode != RechargeRespType.SUCCESS)
                            {
                                req.Type = RechargeRespType.PENDING;
                            }
                        }
                        IProcedure proc = new ProcUpdatePaytmTransaction(_dal);
                        var resp = (AlertReplacementModel)proc.Call(req);
                        res.Statuscode = resp.Statuscode;
                        res.Msg = resp.Msg;
                        resp.CommonStr = string.Empty;
                        resp.CommonStr2 = statusResp.Resp;
                        if (resp.Statuscode == ErrorCodes.Two)
                        {
                            IAlertML alertMl = new AlertML(_accessor, _env);
                            resp.FormatID = MessageFormat.FundReceive;
                            resp.NotificationTitle = "Fund Receive";
                            Parallel.Invoke(() => alertMl.FundReceiveSMS(resp),
                            () => alertMl.FundReceiveEmail(resp),
                             () => alertMl.FundReceiveNotification(resp),
                                () => alertMl.WebNotification(resp),
                                () => alertMl.SocialAlert(resp));
                        }
                    }
                    else
                    {
                        res.Msg = "No response";
                    }
                }
                else if (procRes.PGID == PaymentGatewayType.AGRPAY)
                {
                    var ToMatchData = new TransactionPGLog
                    {
                        MerchantID = procRes.MerchantID,
                        MerchantKEY = procRes.MerchantKey,
                        TID = procRes.TID,
                        TransactionID = procRes.TransactionID,
                        PGID = procRes.PGID,
                        StatuscheckURL = procRes.StatusCheckURL
                    };
                    var aggrePay = new AggrePayML(_dal);
                    var aggrePayResponse = aggrePay.StatusCheckPG(ToMatchData, SavePGTransactionLog);
                    if (aggrePayResponse.data != null && aggrePayResponse.data.Count > 0)
                    {
                        var req = new UpdatePGTransactionRequest
                        {
                            PGID = PaymentGatewayType.AGRPAY,
                            TID = procRes.TID,
                            VendorID = aggrePayResponse.data[0].transaction_id,
                            LiveID = aggrePayResponse.data[0].transaction_id,
                            Remark = aggrePayResponse.data[0].response_message,
                            PaymentModeSpKey = AggrePayPaymentMode(aggrePayResponse.data[0].payment_mode),
                            RequestIP = _rinfo.GetRemoteIP(),
                            Browser = _rinfo.GetBrowserFullInfo()
                        };
                        req.Type = RechargeRespType.PENDING;
                        if (aggrePayResponse.data[0].response_code.Equals("0"))
                        {
                            req.Type = RechargeRespType.SUCCESS;
                        }
                        else
                        {
                            if (Validate.O.IsNumeric(aggrePayResponse.data[0].response_code ?? string.Empty))
                            {
                                var Statuscode = Convert.ToInt32(aggrePayResponse.data[0].response_code);
                                if (Statuscode >= 1000 && Statuscode <= 1087)
                                {
                                    req.Type = RechargeRespType.FAILED;
                                }
                            }
                        }
                        IProcedure proc = new ProcUpdatePaytmTransaction(_dal);
                        var resp = (AlertReplacementModel)proc.Call(req);
                        res.Statuscode = resp.Statuscode;
                        res.Msg = resp.Msg;
                    }

                    res.CommonStr = ToMatchData.Request;
                    res.CommonStr2 = ToMatchData.Response;
                }
                /*CashFree*/
                else if (procRes.PGID == PaymentGatewayType.CASHFREE)
                {
                    var ToMatchData = new TransactionPGLog
                    {
                        MerchantID = procRes.MerchantID,
                        MerchantKEY = procRes.MerchantKey,
                        TID = procRes.TID,
                        TransactionID = procRes.TransactionID,
                        PGID = procRes.PGID,
                        StatuscheckURL = procRes.StatusCheckURL
                    };
                    var cashFree = new CashFreePGML(_dal);
                    var cashFreeResponse = cashFree.StatusCheckPG(ToMatchData, SavePGTransactionLog);
                    if (cashFreeResponse != null && cashFreeResponse.StatusCode == ErrorCodes.One)
                    {
                        var req = new UpdatePGTransactionRequest
                        {
                            PGID = PaymentGatewayType.CASHFREE,
                            TID = procRes.TID,
                            VendorID = cashFreeResponse.orderId,
                            LiveID = string.Empty,
                            PaymentModeSpKey = procRes.OPID,
                            Remark = string.Empty,
                            RequestIP = _rinfo.GetRemoteIP(),
                            Browser = _rinfo.GetBrowserFullInfo()
                        };
                        req.Type = RechargeRespType.PENDING;
                        if (cashFreeResponse.orderStatus.Equals("paid", StringComparison.OrdinalIgnoreCase))
                        {
                            req.Type = RechargeRespType.SUCCESS;
                        }
                        else if (cashFreeResponse.orderStatus.Equals("failed", StringComparison.OrdinalIgnoreCase) || cashFreeResponse.txStatus.Equals("FAILED", StringComparison.OrdinalIgnoreCase))
                        {
                            req.Type = RechargeRespType.FAILED;
                        }
                        else if (cashFreeResponse.status.Equals("error", StringComparison.OrdinalIgnoreCase) && cashFreeResponse.reason.Contains("Order Id does not exist", StringComparison.OrdinalIgnoreCase))
                        {
                            req.Type = RechargeRespType.FAILED;
                        }
                        IProcedure proc = new ProcUpdatePaytmTransaction(_dal);
                        var resp = (AlertReplacementModel)proc.Call(req);
                        res.Statuscode = resp.Statuscode;
                        res.Msg = resp.Msg;
                    }
                    res.CommonStr = ToMatchData.Request;
                    res.CommonStr2 = ToMatchData.Response;
                }
                /*End CashFree*/
                else
                {
                    res.Statuscode = procRes.Status;
                    res.Msg = RechargeRespType.GetRechargeStatusText(procRes.Status);
                }
            }
            else
            {
                res.Statuscode = procRes.Status;
                res.Msg = RechargeRespType.GetRechargeStatusText(procRes.Status);
            }

            return res;
        }
        public ResponseStatus UpdateRazorPaySuccess(RazorPaySuccessResp rpPGResponse)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = "Invalid Request"
            };
            if (rpPGResponse == null)
            {
                return res;
            }
            if (!string.IsNullOrEmpty(rpPGResponse.razorpay_order_id) && !string.IsNullOrEmpty(rpPGResponse.razorpay_payment_id) && !string.IsNullOrEmpty(rpPGResponse.razorpay_signature))
            {
                var param = new CommonReq
                {
                    LoginTypeID = LoginType.ApplicationUser,
                    LoginID = 1,
                    CommonStr = rpPGResponse.razorpay_order_id
                };
                IProcedure _proc = new ProcGetTransactionPGDetail(_dal);
                var procRes = (PGTransactionParam)_proc.Call(param);
                res.Statuscode = procRes.Statuscode;
                res.Msg = procRes.Msg;
                if (procRes.Statuscode == ErrorCodes.One && procRes.Status.In(RechargeRespType.PENDING, RechargeRespType.REQUESTSENT))
                {
                    if (procRes.PGID == PaymentGatewayType.RZRPAY)
                    {
                        var rML = new RazorpayPGML(_dal);
                        if (rML.MatchRazorSignature(rpPGResponse, procRes.MerchantKey))
                        {
                            var req = new UpdatePGTransactionRequest
                            {
                                PGID = PaymentGatewayType.RZRPAY,
                                TID = procRes.TID,
                                VendorID = rpPGResponse.razorpay_order_id,
                                LiveID = rpPGResponse.razorpay_payment_id,
                                Remark = rpPGResponse.razorpay_signature,
                                PaymentModeSpKey = procRes.OPID,
                                RequestIP = _rinfo.GetRemoteIP(),
                                Browser = _rinfo.GetBrowserFullInfo(),
                                Type = RechargeRespType.PENDING,
                                Signature = rpPGResponse.razorpay_signature
                            };
                            IProcedure proc = new ProcUpdatePaytmTransaction(_dal);
                            var resp = (AlertReplacementModel)proc.Call(req);
                            res.Statuscode = resp.Statuscode;
                            res.Msg = resp.Msg;
                            if (res.Statuscode == ErrorCodes.One)
                            {
                                res.Msg = "Payment was successfull. Wallet will update within 3 Minute";
                            }
                            res.CommonStr2 = rpPGResponse.razorpay_payment_id;
                            res.CommonInt = procRes.TID;
                            res.CommonInt2 = (int)procRes.Amount;
                            res.CommonStr3 = procRes.TransactionID;

                        }
                        else
                        {
                            res.Statuscode = ErrorCodes.Minus1;
                            res.Msg = "Signature mismatch";
                        }
                    }
                }
                else
                {
                    res.Statuscode = procRes.Statuscode != ErrorCodes.Minus1 ? procRes.Status : procRes.Statuscode;
                }
            }
            return res;
        }
        public ResponseStatus UpdateRazorPaySuccess(RazorPayCallbackResp rpPGResponse)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = "Invalid Request"
            };
            if (rpPGResponse == null)
                return res;
            if (rpPGResponse.payload == null)
                return res;
            if (rpPGResponse.payload.payment == null)
                return res;
            if (rpPGResponse.payload.payment.entity == null)
                return res;
            if (rpPGResponse.payload.payment.entity.id == null)
                return res;
            var param = new CommonReq
            {
                LoginTypeID = LoginType.ApplicationUser,
                LoginID = 1,
                CommonStr = rpPGResponse.payload.payment.entity.order_id
            };
            IProcedure _proc = new ProcGetTransactionPGDetail(_dal);
            var procRes = (PGTransactionParam)_proc.Call(param);

            if (!string.IsNullOrEmpty(rpPGResponse.payload.payment.entity.order_id) && !string.IsNullOrEmpty(rpPGResponse.payload.payment.entity.id) && procRes.Status.In(RechargeRespType.REQUESTSENT, RechargeRespType.PENDING))
            {
                res.Msg = procRes.Msg;
                if (procRes.Statuscode == ErrorCodes.One)
                {
                    if (procRes.PGID == PaymentGatewayType.RZRPAY)
                    {
                        var rML = new RazorpayPGML(_dal);
                        var stsCheckResp = rML.StatusCheckPG(procRes, SavePGTransactionLog);
                        if (stsCheckResp != null)
                        {
                            if (procRes.Amount * 100 == stsCheckResp.amount)
                            {
                                var PayMethod = stsCheckResp.method;
                                if (PayMethod.Equals("card"))
                                {
                                    if (stsCheckResp.card == null)
                                    {
                                        PayMethod = rpPGResponse.payload.payment.entity.card.type;
                                    }
                                    else
                                    {
                                        PayMethod = stsCheckResp.card.type;
                                    }
                                }
                                var req = new UpdatePGTransactionRequest
                                {
                                    PGID = PaymentGatewayType.RZRPAY,
                                    TID = procRes.TID,
                                    VendorID = stsCheckResp.order_id,
                                    LiveID = stsCheckResp.id,
                                    Remark = rpPGResponse.account_id,
                                    PaymentModeSpKey = RazorPaymentMode(PayMethod),
                                    RequestIP = _rinfo.GetRemoteIP(),
                                    Browser = _rinfo.GetBrowserFullInfo(),
                                    Type = RechargeRespType.PENDING
                                };
                                if (stsCheckResp.status.Equals("captured"))
                                {
                                    req.Type = RechargeRespType.SUCCESS;
                                }
                                else if (stsCheckResp.status.Equals("failed"))
                                {
                                    req.Type = RechargeRespType.FAILED;
                                }
                                IProcedure proc = new ProcUpdatePaytmTransaction(_dal);
                                var resp = (AlertReplacementModel)proc.Call(req);
                                res.Statuscode = resp.Statuscode;
                                res.Msg = resp.Msg;
                            }
                        }

                    }
                }
            }
            return res;
        }
        private TransactionPGLog GetLogForMatching(CommonReq req)
        {
            IProcedure proc = new ProcGetPGTransactionLog(_dal);
            return (TransactionPGLog)proc.Call(req);
        }
        private string PayTMPaymentMode(string PayMode)
        {
            if (!string.IsNullOrEmpty(PayMode))
            {
                if (PayMode == "CC")
                    return PaymentGatewayTranMode.CreditCard;
                if (PayMode == "DC")
                    return PaymentGatewayTranMode.DebitCard;
                if (PayMode == "NB")
                    return PaymentGatewayTranMode.NetBanking;
                if (PayMode == "UPI")
                    return PaymentGatewayTranMode.UPI;
                if (PayMode == "PPI")
                    return PaymentGatewayTranMode.PPIWALLET;
            }
            return PaymentGatewayTranMode.DebitCard;
        }
        private string PayTMPaymentModeFromSPKey(string SPKey)
        {
            if (!string.IsNullOrEmpty(SPKey))
            {
                if (SPKey == PaymentGatewayTranMode.CreditCard || SPKey == PaymentGatewayTranMode.DebitCard)
                    return "CARD";
                if (SPKey == PaymentGatewayTranMode.NetBanking)
                    return "NB";
                if (SPKey == PaymentGatewayTranMode.UPI)
                    return "UPI";
                if (SPKey == PaymentGatewayTranMode.PPIWALLET)
                    return "PPI";
            }
            return string.Empty;
        }
        private string RazorPaymentMode(string PayMode)
        {
            if (!string.IsNullOrEmpty(PayMode))
            {
                if (PayMode.Equals("credit"))
                    return PaymentGatewayTranMode.CreditCard;
                if (PayMode.Equals("debit"))
                    return PaymentGatewayTranMode.DebitCard;
                if (PayMode.Equals("netbanking"))
                    return PaymentGatewayTranMode.NetBanking;
                if (PayMode == "upi")
                    return PaymentGatewayTranMode.UPI;
                if (PayMode.Equals("wallet"))
                    return PaymentGatewayTranMode.PPIWALLET;
                if (PayMode.Equals("prepaid"))
                    return PaymentGatewayTranMode.CreditCard;
            }
            return PaymentGatewayTranMode.DebitCard;
        }
        private string CashfreePaymentMode(string PayMode)
        {
            if (!string.IsNullOrEmpty(PayMode))
            {
                if (PayMode.Equals("CREDIT_CARD", StringComparison.OrdinalIgnoreCase))
                    return PaymentGatewayTranMode.CreditCard;
                if (PayMode.Equals("Debit_CARD", StringComparison.OrdinalIgnoreCase))
                    return PaymentGatewayTranMode.DebitCard;
                if (PayMode.Equals("netbanking", StringComparison.OrdinalIgnoreCase))
                    return PaymentGatewayTranMode.NetBanking;
                if (PayMode.Equals("upi", StringComparison.OrdinalIgnoreCase))
                    return PaymentGatewayTranMode.UPIAdmoney;
                if (PayMode.Equals("wallet", StringComparison.OrdinalIgnoreCase))
                    return PaymentGatewayTranMode.PPIWALLET;
            }
            return PaymentGatewayTranMode.DebitCard;
        }

        public CollectUPPayResponse InitiateUPIPaymentForWeb(int UserID, decimal Amount, int UPGID, int OID, int WalletID, string UPIID)
        {
            var res = new CollectUPPayResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            var req = new PGTransactionRequest
            {
                UserID = UserID,
                RequestIP = _rinfo.GetRemoteIP(),
                Browser = _rinfo.GetBrowserFullInfo(),
                RequestMode = RequestMode.PANEL,
                AmountR = Amount,
                UPGID = UPGID,
                OID = OID,
                WalletID = WalletID
            };
            IProcedure proc = new ProcPGatewayTransacrionService(_dal);
            var procRes = (PGTransactionResponse)proc.Call(req);
            res.Statuscode = procRes.Statuscode;
            res.Msg = procRes.Msg;

            if (procRes.Statuscode == ErrorCodes.One)
            {
                if (procRes.PGID == PaymentGatewayType.ICICIUPI)
                {
                    ICICIPayoutML iCICIPayoutML = new ICICIPayoutML(_accessor, _env, UPGID);
                    res = iCICIPayoutML.CollectPayRequestWeb(new ColllectUPPayReqModel
                    {
                        Amount = Amount,
                        TID = procRes.TID,
                        TransactionID = procRes.TransactionID,
                        UPIID = UPIID,
                        UserID = UserID
                    }, SavePGTransactionLog);
                }
                res.CommonInt = procRes.TID;
                if (res.Statuscode == ErrorCodes.One)
                {
                    IProcedure procU = new ProcUpdatePaytmTransaction(_dal);
                    var resUpdate = (AlertReplacementModel)procU.Call(new UpdatePGTransactionRequest
                    {
                        Browser = _rinfo.GetBrowserFullInfo(),
                        LiveID = res.BankRRN,
                        VendorID = res.VendorID,
                        PaymentModeSpKey = PaymentGatewayTranMode.UPIICI,
                        PGID = PaymentGatewayType.ICICIUPI,
                        RequestIP = _rinfo.GetRemoteIP(),
                        TID = procRes.TID,
                        Remark = res.Remark,
                        Type = RechargeRespType.PENDING
                    });
                }
            }
            return res;
        }
        public CollectUPPayResponse InitiateUPIPaymentForApp(int UserID, int Amount, int UPGID, int OID, int WalletID, string UPIID)
        {
            var res = new CollectUPPayResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            var req = new PGTransactionRequest
            {
                UserID = UserID,
                RequestIP = _rinfo.GetRemoteIP(),
                Browser = _rinfo.GetBrowserFullInfo(),
                RequestMode = RequestMode.APPS,
                AmountR = Amount,
                UPGID = UPGID,
                OID = OID,
                WalletID = WalletID
            };
            IProcedure proc = new ProcPGatewayTransacrionService(_dal);
            var procRes = (PGTransactionResponse)proc.Call(req);
            res.Statuscode = procRes.Statuscode;
            res.Msg = procRes.Msg;
            if (procRes.Statuscode == ErrorCodes.One)
            {
                if (procRes.PGID == PaymentGatewayType.ICICIUPI)
                {
                    ICICIPayoutML iCICIPayoutML = new ICICIPayoutML(_accessor, _env, UPGID);
                    res = iCICIPayoutML.CollectPayRequestApp(new ColllectUPPayReqModel
                    {
                        Amount = Amount,
                        TID = procRes.TID,
                        TransactionID = procRes.TransactionID,
                        UPIID = UPIID,
                        UserID = UserID
                    }, SavePGTransactionLog);
                }
                if (res.Statuscode == ErrorCodes.One)
                {
                    IProcedure procU = new ProcUpdatePaytmTransaction(_dal);
                    var resUpdate = (AlertReplacementModel)procU.Call(new UpdatePGTransactionRequest
                    {
                        Browser = _rinfo.GetBrowserFullInfo(),
                        LiveID = res.BankRRN,
                        VendorID = res.VendorID,
                        PaymentModeSpKey = PaymentGatewayTranMode.UPIICI,
                        PGID = UPGID,
                        RequestIP = _rinfo.GetRemoteIP(),
                        TID = procRes.TID,
                        Remark = res.Remark,
                        Type = RechargeRespType.PENDING
                    });
                    res.CommonInt = procRes.TID;
                }
            }
            return res;
        }
        public ResponseStatus UpdateUPIPaymentStatus(UpdatePGTransactionRequest request)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = "Invalid Request"
            };
            if ((request.RequestPage ?? string.Empty).Equals("Callback"))
            {
                IProcedure _proc = new ProcGetTransactionPGDetail(_dal);
                var procRes = (PGTransactionParam)_proc.Call(new CommonReq
                {
                    LoginTypeID = LoginType.ApplicationUser,
                    LoginID = 1,
                    CommonInt = request.TID
                });
                if (procRes.Statuscode == ErrorCodes.Minus1)
                {
                    res.Msg = "Detail not found";
                    return res;
                }
                if (procRes.RequestMode == RequestMode.APPS)
                {
                    ICICIPayoutML payoutML = new ICICIPayoutML(_accessor, _env, procRes.PGID);
                    var stsCheckResp = payoutML.CollectPayCallbackStatusCheck(new ColllectUPPayReqModel
                    {
                        StatusCheckType = "Q",
                        TID = request.TID,
                        LiveID = request.LiveID,
                        TransactionID = procRes.TransactionID,
                        UserID = procRes.UserID,
                        UPIID = request.Remark,
                        Amount = request.Amount
                    }, SavePGTransactionLog);
                    if (!(stsCheckResp.CommonStr ?? string.Empty).Equals("ISERR"))
                    {
                        if (stsCheckResp.Statuscode < 1)
                        {
                            return res;
                        }
                        if (Convert.ToInt32((stsCheckResp.CommonStr4 ?? "0").Split('.')[0]) != procRes.Amount)
                        {
                            res.Msg = "Amount Mismatch";
                            return res;
                        }
                        request.Type = stsCheckResp.Statuscode;
                    }
                    else
                    {
                        request.Type = RechargeRespType.PENDING;
                    }
                }

            }
            request.Browser = _rinfo.GetBrowserFullInfo();
            request.RequestIP = _rinfo.GetRemoteIP();
            IProcedure procU = new ProcUpdatePaytmTransaction(_dal);
            var resp = (AlertReplacementModel)procU.Call(request);
            res.Statuscode = resp.Statuscode;
            res.Msg = resp.Msg;
            return res;
        }

        public ResponseStatus UpdateUPIGatewayStatus(UpdatePGTransactionRequest request)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            request.RequestIP = _rinfo.GetRemoteIP();
            request.Browser = _rinfo.GetBrowser();
            IProcedure procU = new ProcUpdatePaytmTransaction(_dal);
            var resp = (AlertReplacementModel)procU.Call(request);
            res.Statuscode = resp.Statuscode;
            res.Msg = resp.Msg;
            return res;
        }
        public ResponseStatus GetUPIStatusFromDB(int TID)
        {
            IProcedure proc = new ProcCheckStatusOfUPIPayment(_dal);
            return (ResponseStatus)proc.Call(TID);
        }
        public ResponseStatus GetUPIQR(int LT, int LoginID, int Amount)
        {
            IProcedure proc = new ProcGetICICIQRResp(_dal);
            var res = (ResponseStatus)proc.Call(new CommonReq
            {
                LoginTypeID = LT,
                LoginID = LoginID
            });
            if (res.Statuscode == ErrorCodes.One)
            {
                ICICIPayoutML iCICIPayoutML = new ICICIPayoutML(_accessor, _env, 0);
                if (string.IsNullOrEmpty(res.CommonStr))
                {
                    var iciRes = iCICIPayoutML.CollectPayRequestAppQR(new ColllectUPPayReqModel
                    {
                        Amount = 0,
                        TransactionID = LoginID + res.CommonStr2
                    }, SavePGTransactionLog);
                    if (!string.IsNullOrEmpty(iciRes.VendorID) && iciRes.Statuscode == ErrorCodes.One)
                    {
                        res = (ResponseStatus)proc.Call(new CommonReq
                        {
                            LoginTypeID = LT,
                            LoginID = LoginID,
                            CommonStr = iciRes.VendorID
                        });
                    }
                }
                if (!string.IsNullOrEmpty(res.CommonStr))
                {
                    var QRIntent = new StringBuilder(iCICIPayoutML.AppSetting().QRIntent ?? string.Empty);
                    QRIntent.Replace("{REFID}", res.CommonStr);
                    QRIntent.Replace("{AMOUNT}", Amount.ToString());
                    res.CommonStr4 = QRIntent.ToString();
                }
            }
            return res;
        }
        public UserQRInfo GetUPIQRBankDetail(int LT, int LoginID)
        {
            var res = new UserQRInfo();
            IProcedure proc = new ProcGetICICIQRResp(_dal);
            var procres = (ResponseStatus)proc.Call(new CommonReq
            {
                LoginTypeID = LT,
                LoginID = LoginID
            });
            if (procres.Statuscode == ErrorCodes.One)
            {
                ICICIPayoutML iCICIPayoutML = new ICICIPayoutML(_accessor, _env, 0);
                var appSetting = iCICIPayoutML.AppSetting();
                res.BankName = "ICICI";
                res.IFSC = appSetting.CollectIFSC;
                res.VirtualAccount = appSetting.CollectVirtualCode + procres.CommonStr3;
                res.Branch = appSetting.CollectBranch;
                res.BeneName = appSetting.CollectBeneName;
            }
            if (ApplicationSetting.IsBankAccountNoReplaceWithSmartCollect)
            {
                SmartCollectML smartCollect = new SmartCollectML(_accessor, _env);
                var smrtUsrDetail = smartCollect.GetUserSmartDetails(LoginID, LoginID);
                if (smrtUsrDetail.USDList != null)
                {
                    if (smrtUsrDetail.USDList.Count > 0)
                    {
                        if (smrtUsrDetail.USDList.Where(w => w.SmartCollectTypeID == SmartCollectType.RazorPaySmartCollect).Count() > 0)
                        {
                            res.userSDetail = smrtUsrDetail.USDList.Where(w => w.SmartCollectTypeID == SmartCollectType.RazorPaySmartCollect).ToList()[0];
                        }
                    }
                }
                if (string.IsNullOrEmpty(res.userSDetail.SmartAccountNo))
                {
                    var updateRes = smartCollect.UpdateSmartCollectDetailOfUser(LoginID, LoginID);
                    if (updateRes.Statuscode == ErrorCodes.One)
                    {
                        smrtUsrDetail = smartCollect.GetUserSmartDetails(LoginID, LoginID);
                        if (smrtUsrDetail.USDList != null)
                        {
                            if (smrtUsrDetail.USDList.Count > 0)
                            {
                                if (smrtUsrDetail.USDList.Where(w => w.SmartCollectTypeID == SmartCollectType.RazorPaySmartCollect).Count() > 0)
                                {
                                    res.userSDetail = smrtUsrDetail.USDList.Where(w => w.SmartCollectTypeID == SmartCollectType.RazorPaySmartCollect).ToList()[0];
                                }
                            }
                        }
                    }
                }
            }
            return res;
        }
        public VIANCallbackStatusResponse CallVIANService(VIANCallbackRequest vIANCallbackRequest)
        {
            vIANCallbackRequest.IPAddress = _rinfo.GetRemoteIP();
            vIANCallbackRequest.Browser = _rinfo.GetBrowserFullInfo();
            var resp = new VIANCallbackStatusResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError,
                AgentID = vIANCallbackRequest.AgentID
            };
            IProcedure proc = new ProcVIANService(_dal);
            var res = (ResponseStatus)proc.Call(vIANCallbackRequest);
            resp.Statuscode = res.Statuscode;
            resp.Msg = res.Msg;
            if (res.Statuscode == ErrorCodes.One)
            {
                resp.RPID = res.CommonStr;
                resp.data = new
                {
                    name = res.CommonStr2,
                    mobileNo = res.CommonStr3
                };
            }
            return resp;
        }

        #region BulkQRGeneration
        public ResponseStatus GetQRGeneration(int LT, int LoginID, int RefID, string TransactionID)
        {
            var resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            ICICIPayoutML iCICIPayoutML = new ICICIPayoutML(_accessor, _env, 0);
            var iciRes = iCICIPayoutML.CollectPayRequestAppQR(new ColllectUPPayReqModel
            {
                Amount = 0,
                TransactionID = RefID.ToString() + TransactionID.ToString()
            }, SavePGTransactionLog);
            if (!string.IsNullOrEmpty(iciRes.VendorID) && iciRes.Statuscode == ErrorCodes.One)
            {
                IProcedure _ = new ProcUpdateQRBankRefID(_dal);
                resp = (ResponseStatus)_.Call(new CommonReq
                {
                    LoginTypeID = LT,
                    LoginID = LoginID,
                    CommonInt = RefID,
                    CommonStr = TransactionID,
                    CommonStr2 = iciRes.VendorID
                });
            }

            if (!string.IsNullOrEmpty(iciRes.VendorID))
            {
                resp.Msg = "Success!";
                resp.Statuscode = ErrorCodes.One;
                resp.CommonStr = TransactionID;
                resp.CommonStr2 = iciRes.VendorID;
            }
            return resp;
        }
        public ResponseStatus CreateQRIntent(string BankRefID)
        {
            var resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            ICICIPayoutML iCICIPayoutML = new ICICIPayoutML(_accessor, _env, 0);
            var QRIntent = new StringBuilder(iCICIPayoutML.AppSetting().QRIntent ?? string.Empty);
            QRIntent.Replace("{REFID}", BankRefID);
            QRIntent.Replace("{AMOUNT}", "0");
            resp.Msg = "Success!";
            resp.Statuscode = ErrorCodes.One;
            resp.CommonStr = QRIntent.ToString();
            return resp;
        }
        #endregion


        /* have to Edit 
            * 1. check status by API
            * 2. Replace TID from orderId and get only numeric value
            * 3. ProcGetTransactionPGDetail by passing commonInt instead of commonStr
            * 4. check amount get by  status check with amount get by proc
            * 5. create a private function to modify  PaymentMode according db
        */
        public ResponseStatus UpdateCashfreeResponse(CashfreeCallbackResponse PGResponse)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = "Invalid Request"
            };
            try
            {
                if (PGResponse == null)
                    return res;
                if (string.IsNullOrEmpty(PGResponse.orderId))
                    return res;
                if (PGResponse.referenceId <= 0)
                    return res;
                if (PGResponse.orderAmount <= 0)
                    return res;
                if (string.IsNullOrEmpty(PGResponse.paymentMode))
                    return res;
                if (string.IsNullOrEmpty(PGResponse.txStatus))
                    return res;
                var param = new CommonReq
                {
                    LoginTypeID = LoginType.ApplicationUser,
                    LoginID = 1,
                    CommonInt = !string.IsNullOrEmpty(PGResponse.orderId) ? Convert.ToInt32(PGResponse.orderId.Replace("TID", string.Empty, StringComparison.OrdinalIgnoreCase)) : 0
                };
                IProcedure _proc = new ProcGetTransactionPGDetail(_dal);
                var procRes = (PGTransactionParam)_proc.Call(param);

                if (procRes.Status.In(RechargeRespType.REQUESTSENT, RechargeRespType.PENDING))
                {
                    res.Msg = procRes.Msg;
                    var appId = procRes.MerchantID;
                    var secretKey = procRes.MerchantKey;
                    var orderId = procRes.TID;
                    if (procRes.Statuscode == ErrorCodes.One)
                    {
                        if (procRes.PGID == PaymentGatewayType.CASHFREE)
                        {
                            var rML = new CashFreePGML(_dal);
                            var ToMatchData = new TransactionPGLog
                            {
                                TID = procRes.TID,
                                PGID = procRes.PGID,
                                TransactionID = procRes.TransactionID,
                                Checksum = procRes.Checksum,
                                StatuscheckURL = procRes.StatusCheckURL,
                                VendorID = procRes.VendorID,
                                MerchantID = procRes.MerchantID,
                                MerchantKEY = procRes.MerchantKey,
                                Amount = procRes.Amount
                            };
                            var stsCheckResp = rML.StatusCheckPG(ToMatchData, SavePGTransactionLog);
                            if (stsCheckResp != null)
                            {
                                if (procRes.Amount == stsCheckResp.orderAmount)
                                {
                                    var PayMethod = PGResponse.paymentMode;
                                    var req = new UpdatePGTransactionRequest
                                    {
                                        PGID = PaymentGatewayType.CASHFREE,
                                        TID = procRes.TID,
                                        VendorID = PGResponse.referenceId.ToString(),//stsCheckResp.order_id,
                                        LiveID = stsCheckResp.utr,//stsCheckResp.id,
                                        Remark = PGResponse.referenceId.ToString(),
                                        PaymentModeSpKey = CashfreePaymentMode(PayMethod),
                                        RequestIP = _rinfo.GetRemoteIP(),
                                        Browser = _rinfo.GetBrowserFullInfo(),
                                        Type = RechargeRespType.PENDING
                                    };
                                    if (stsCheckResp.status.Equals("OK", StringComparison.OrdinalIgnoreCase) && PGResponse.txStatus.Equals("success", StringComparison.OrdinalIgnoreCase))
                                    {
                                        req.Type = RechargeRespType.SUCCESS;
                                    }
                                    else if (PGResponse.txStatus.Equals("failed", StringComparison.OrdinalIgnoreCase))
                                    {
                                        req.Type = RechargeRespType.FAILED;
                                    }
                                    else if ((stsCheckResp.status.Equals("error", StringComparison.OrdinalIgnoreCase) && stsCheckResp.reason.Contains("Order Id does not exist", StringComparison.OrdinalIgnoreCase)))
                                    {
                                        req.Type = RechargeRespType.FAILED;
                                    }
                                    IProcedure proc = new ProcUpdatePaytmTransaction(_dal);
                                    var resp = (AlertReplacementModel)proc.Call(req);
                                    res.Statuscode = resp.Statuscode;
                                    res.Msg = resp.Msg;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "UpdateCashfreeResponse",
                    Error = ex.Message,
                    LoginTypeID = 1,
                    UserId = 1
                });
            }
            return res;
        }


        public CashfreeStatusResponse CashFreePgStatusCheck(string orderId, string orderToken, string TID = "")
        {
            int tid = 0;
            if (!string.IsNullOrEmpty(orderId))
            {
                tid = Convert.ToInt32(orderId.Replace("TID", string.Empty, StringComparison.OrdinalIgnoreCase));
            }
            else if (tid == 0 && !string.IsNullOrEmpty(TID))
            {
                tid = Convert.ToInt32(TID.Replace("TID", string.Empty, StringComparison.OrdinalIgnoreCase));
            }
            var param = new CommonReq
            {
                LoginTypeID = LoginType.ApplicationUser,
                LoginID = 1,
                CommonInt = tid
            };
            IProcedure _proc = new ProcGetTransactionPGDetail(_dal);
            var procRes = (PGTransactionParam)_proc.Call(param);
            return procRes != null ? new CashfreeStatusResponse
            {
                status = procRes.Status.ToString(),
                orderStatus = procRes.Status.ToString(),
                orderAmount = procRes.Amount,
                TID = string.Concat("TID", procRes.TID),
                TransactionId = procRes.TransactionID,
                OrderToken = orderToken,
                StatusCode = procRes.Statuscode
            } : new CashfreeStatusResponse();
        }
    }
}
