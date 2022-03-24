using Fintech.AppCode.DB;
using Fintech.AppCode.HelperClass;
using Fintech.AppCode.StaticModel;
using Fintech.AppCode.WebRequest;
using Newtonsoft.Json;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.Model.Paymentgateway;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.ThirdParty.PayU
{
    public class PayUPGML
    {
        //*Note : MerchantId --> PayU Salt  & Merchant Key or key --> Alieas

        private readonly IDAL _dal;
        private readonly Dictionary<string, string> paymentModes = new Dictionary<string, string>(){
            {PaymentGatewayTranMode.CreditCard, "creditcard"},
            {PaymentGatewayTranMode.DebitCard, "debitcard"},
            {PaymentGatewayTranMode.PPIWALLET, "ppi"},
            {PaymentGatewayTranMode.NetBanking, "netbanking"},
            {PaymentGatewayTranMode.UPI, "upi"},
            {PaymentGatewayTranMode.UPIICI, "upi"}
        };
        public PayUPGML(IDAL dal) => _dal = dal;

        public PGModelForRedirection GeneratePGRequestForWeb(PGTransactionResponse pGTransactionResponse, Func<int, int, string, string, string, int, bool, decimal, string, Task> savePGTransactionLog)
        {
            var res = new PGModelForRedirection
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            string paymentMode = string.Empty;
            try
            {
                if (paymentModes.ContainsKey(pGTransactionResponse.OPID))
                {
                    paymentMode = paymentModes[pGTransactionResponse.OPID];
                }
                var payURequest = new PayURequest
                {
                    key = pGTransactionResponse.MerchantKey,
                    amount = (double)pGTransactionResponse.Amount,
                    txnid = "TID" + pGTransactionResponse.TID,
                    surl = pGTransactionResponse.Domain + "/PGCallback/PayUnotify",
                    furl = pGTransactionResponse.Domain + "/PGCallback/PayUnotify",
                    firstname = pGTransactionResponse.UserID.ToString(),
                    email = pGTransactionResponse.EmailID,
                    phone = pGTransactionResponse.MobileNo,
                    enforce_paymethod = paymentMode,
                    productinfo="Add Money"
                };
                
                Dictionary<string, string> keyValue = new Dictionary<string, string>(){
                    {"key", payURequest.key},
                    {"txnid", payURequest.txnid},
                    {"amount", payURequest.amount.ToString()},
                    {"firstname", payURequest.firstname},
                    {"email", payURequest.email},
                    {"phone", payURequest.phone},
                    {"productinfo", payURequest.productinfo},
                    {"surl", payURequest.surl},
                    {"furl", payURequest.furl},
                    {"enforce_paymethod", payURequest.enforce_paymethod},
                };
                payURequest.hash = GenerateHash(pGTransactionResponse.MerchantID,
                    new List<string> { payURequest.key,
                        payURequest.txnid,
                        payURequest.amount.ToString(),
                        payURequest.productinfo,
                        payURequest.firstname,
                        payURequest.email,string.Empty,string.Empty,string.Empty,string.Empty,string.Empty });//keyValue
                keyValue.Add("hash", payURequest.hash.ToLower());
                res.KeyVals = keyValue;
                res.Statuscode = ErrorCodes.One;
                res.Msg = "Transaction intiated";
                savePGTransactionLog(pGTransactionResponse.PGID, pGTransactionResponse.TID, JsonConvert.SerializeObject(res), pGTransactionResponse.TransactionID, payURequest.hash, RequestMode.PANEL, true, pGTransactionResponse.Amount, string.Empty);
            }
            catch (Exception ex)
            {
                res.Statuscode = ErrorCodes.Minus1;
                res.Msg = ex.Message;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "GeneratePGRequestForWeb",
                    Error = "TID:" + pGTransactionResponse.TID + ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = pGTransactionResponse.UserID
                });
            }
            return res;
        }

        private string GenerateHash(string salt, List<string> keyValuePairs)
        {
            var sb = new StringBuilder();
            if (keyValuePairs != null)
            {
                foreach (var item in keyValuePairs)
                {
                    sb.Append(item);
                    sb.Append("|");
                }
            }
            sb.Append("|||||");
            sb.Append(salt);
            string str = sb.ToString();
            return HashEncryption.O.SHA512Hash(str);
        }       
        public PayUStatusCheckResponse StatusCheckPG(TransactionPGLog transactionPGLog, Func<int, int, string, string, string, int, bool, decimal, string, Task> savePGTransactionLog)
        {
            var payuResponse = new PayUStatusCheckResponse();
            var res = new PGModelForRedirection
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            string payuRes = string.Empty;
            var payuVerifyRequest = new PayUVerifyRequest();
            try
            {
                StringBuilder sb = new StringBuilder("key={key}&command={command}&var1={var1}&hash={hash}");
                sb.Replace("{key}", transactionPGLog.MerchantKEY);
                sb.Replace("{command}", "verify_payment");
                sb.Replace("{var1}", transactionPGLog.TID.ToString());
                sb.Replace("{hash}", GenerateHash(transactionPGLog.MerchantID, new List<string> { transactionPGLog.MerchantKEY, "verify_payment", transactionPGLog.TID.ToString() }));
                //sb.Replace("{hash}", GenerateCheckSum(this.salt, new List<string> { transactionPGLog.MerchantKEY, "verify_payment", transactionPGLog.TID.ToString() }));
                res.Statuscode = ErrorCodes.One;
                res.Msg = "Statuscheck";
                savePGTransactionLog(transactionPGLog.PGID, transactionPGLog.TID, JsonConvert.SerializeObject(res), transactionPGLog.TransactionID, payuVerifyRequest.hash, RequestMode.API, false, transactionPGLog.Amount, string.Empty);
                payuRes = AppWebRequest.O.CallUsingHttpWebRequest_POST(transactionPGLog.StatuscheckURL, sb.ToString());
                if (!string.IsNullOrEmpty(payuRes))
                {
                    payuResponse = JsonConvert.DeserializeObject<PayUStatusCheckResponse>(payuRes);
                }
            }
            catch (Exception ex)
            {
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
            savePGTransactionLog(transactionPGLog.PGID, transactionPGLog.TID, payuRes, transactionPGLog.TransactionID, payuVerifyRequest.hash, RequestMode.API, true, transactionPGLog.Amount, string.Empty);
            return payuResponse;
        }

        //private string GenerateCheckSum(string salt, List<string> keyValuePairs)
        //{
        //    var sb = new StringBuilder();
        //    if (keyValuePairs != null)
        //    {
        //        foreach (var item in keyValuePairs)
        //        {
        //            sb.Append(item);
        //            sb.Append("|");
        //        }
        //    }
        //    sb.Append(salt);
        //    string str = sb.ToString();
        //    return HashEncryption.O.SHA512Hash(str);
        //}
    }
}
