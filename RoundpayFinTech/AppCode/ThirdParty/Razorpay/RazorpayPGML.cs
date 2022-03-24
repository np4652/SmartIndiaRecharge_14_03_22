using Fintech.AppCode.DB;
using Fintech.AppCode.HelperClass;
using Fintech.AppCode.StaticModel;
using Newtonsoft.Json;
using Razorpay.Api;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.Paymentgateway;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.ThirdParty.Razorpay
{
    public class RazorpayPGML
    {
        private readonly IDAL _dal;
        public RazorpayPGML(IDAL dal) => _dal = dal;

        public PGModelForRedirection GeneratePGRequestForWeb(PGTransactionResponse pGTransactionResponse, Func<int, int, string, string, string, int, bool, decimal, string, Task> savePGTransactionLog)
        {
            var resp = new PGModelForRedirection
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            var razorpayOrderRequest = new RazorpayOrderRequest
            {
                amount = pGTransactionResponse.Amount,
                currency = "INR",
                receipt = "TID#" + pGTransactionResponse.TID,
                payment_capture = 1
            };
            var client = new RazorpayClient(pGTransactionResponse.MerchantID, pGTransactionResponse.MerchantKey);
            var KeyVals = new Dictionary<string, object>
            {
                { nameof(razorpayOrderRequest.amount),razorpayOrderRequest.amount*100},
                { nameof(razorpayOrderRequest.currency),razorpayOrderRequest.currency},
                { nameof(razorpayOrderRequest.receipt),razorpayOrderRequest.receipt},
                { nameof(razorpayOrderRequest.payment_capture),razorpayOrderRequest.payment_capture}
            };
            try
            {
                Order order = client.Order.Create(KeyVals);
                if (order != null)
                {
                    if (order.Attributes != null)
                    {
                        var response = JsonConvert.SerializeObject(order.Attributes);
                        string orderID = string.Empty;
                        if (response != null)
                        {
                            RazorOrder _order = JsonConvert.DeserializeObject<RazorOrder>(response);
                            if (!string.IsNullOrEmpty(_order.id))
                            {
                                orderID = _order.id;
                                //orderid found and making request for checkout
                                resp.Statuscode = ErrorCodes.One;
                                resp.Msg = "Transaction intiated";
                                resp.RPayRequest = new RazorpayRequest
                                {
                                    order_id = _order.id,
                                    key_id = pGTransactionResponse.MerchantID,
                                    name = pGTransactionResponse.CompanyName,
                                    Prefill_name = pGTransactionResponse.Name,
                                    Prefill_contact = pGTransactionResponse.MobileNo,
                                    Prefill_email = pGTransactionResponse.EmailID,
                                    callback_url = pGTransactionResponse.Domain + "/PGCallback/RazorPaySuccess",
                                    image = pGTransactionResponse.Domain + "/" + DOCType.LogoSuffix.Replace("{0}", pGTransactionResponse.WID.ToString()),
                                    amount = pGTransactionResponse.Amount,
                                    retry=false
                                };
                            }
                        }
                        savePGTransactionLog(pGTransactionResponse.PGID, pGTransactionResponse.TID, response, pGTransactionResponse.TransactionID, JsonConvert.SerializeObject(KeyVals), RequestMode.PANEL, true, pGTransactionResponse.Amount, orderID);
                    }
                }
            }
            catch (Exception ex)
            {
                resp.Statuscode = ErrorCodes.Minus1;
                resp.Msg = ex.Message;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "GeneratePGRequestForWeb",
                    Error = "TID:" + pGTransactionResponse.TID + ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = pGTransactionResponse.UserID
                });
            }

            return resp;
        }
        public bool MatchRazorSignature(RazorPaySuccessResp paytmPGResponse, string SecretKey)
        {
            var ToBeHash = paytmPGResponse.razorpay_order_id + "|" + paytmPGResponse.razorpay_payment_id;
            return paytmPGResponse.razorpay_signature.Equals(HashEncryption.O.SHA256_ComputeHash(ToBeHash, SecretKey), StringComparison.OrdinalIgnoreCase);
        }
        public RazorEntity StatusCheckPG(PGTransactionParam transactionPGLog, Func<int, int, string, string, string, int, bool, decimal, string, Task> savePGTransactionLog)
        {
            var res = new RazorEntity();
            string paytmresp = string.Empty;
            try
            {
                var client = new RazorpayClient(transactionPGLog.MerchantID, transactionPGLog.MerchantKey);
                var payments = client.Order.Payments(transactionPGLog.VendorID);
                if (payments.Count > 0)
                {
                    paytmresp = JsonConvert.SerializeObject(payments);
                    var response = JsonConvert.SerializeObject(payments[0].Attributes);
                    if (response != null)
                    {
                        paytmresp = response + paytmresp;
                        res = JsonConvert.DeserializeObject<RazorEntity>(response);
                    }

                }
            }
            catch (Exception ex)
            {
                paytmresp = "Exception:" + ex.Message + "|" + paytmresp;
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "StatusCheckPG",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = transactionPGLog.PGID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            savePGTransactionLog(transactionPGLog.PGID, transactionPGLog.TID, paytmresp, transactionPGLog.TransactionID, transactionPGLog.VendorID, RequestMode.API, false, transactionPGLog.Amount, transactionPGLog.VendorID);
            return res;
        }
    }
}
