using Fintech.AppCode.DB;
using Fintech.AppCode.HelperClass;
using Fintech.AppCode.StaticModel;
using Newtonsoft.Json;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.Model.Paymentgateway;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.ThirdParty.UPIGateway
{
    public class UPIGatewayML
    {
        private readonly IDAL _dal;
        public UPIGatewayML(IDAL dal) => _dal = dal;

        public PGModelForRedirection GeneratePGRequestForWeb(PGTransactionResponse pGTransactionResponse, Func<int, int, string, string, string, int, bool, decimal, string, Task> savePGTransactionLog)
        {
            var res = new PGModelForRedirection
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            var uPIGatewayRequest = new UPIGatewayRequest();
            try
            {
                var KeyVals = new Dictionary<string, string>
                {
                    { nameof(uPIGatewayRequest.key), pGTransactionResponse.MerchantID },
                    { nameof(uPIGatewayRequest.client_vpa), pGTransactionResponse.VPA },
                    { nameof(uPIGatewayRequest.client_txn_id), pGTransactionResponse.TID.ToString() },
                    { nameof(uPIGatewayRequest.amount), pGTransactionResponse.Amount.ToString()},
                    { nameof(uPIGatewayRequest.p_info),  "Add Money to consume services"},
                    { nameof(uPIGatewayRequest.client_name),  pGTransactionResponse.Name},
                    { nameof(uPIGatewayRequest.client_email),  pGTransactionResponse.EmailID},
                    { nameof(uPIGatewayRequest.client_mobile),  pGTransactionResponse.MobileNo},
                    { nameof(uPIGatewayRequest.udf1),  pGTransactionResponse.City},
                    { nameof(uPIGatewayRequest.udf2),  pGTransactionResponse.State},
                    { nameof(uPIGatewayRequest.udf3),  pGTransactionResponse.Pincode}
                };
                uPIGatewayRequest.hash = GenerateCheckSum(pGTransactionResponse.MerchantKey, KeyVals);
                uPIGatewayRequest.redirect_url = pGTransactionResponse.Domain + "/PGCallback/UPIGatewayRedirect?TID=" + pGTransactionResponse.TID;
                KeyVals.Add(nameof(uPIGatewayRequest.hash), uPIGatewayRequest.hash);
                KeyVals.Add(nameof(uPIGatewayRequest.redirect_url), uPIGatewayRequest.redirect_url);
                res.KeyVals = KeyVals;
                res.Statuscode = ErrorCodes.One;
                res.Msg = "Transaction intiated";
                savePGTransactionLog(pGTransactionResponse.PGID, pGTransactionResponse.TID, JsonConvert.SerializeObject(res), pGTransactionResponse.TransactionID, uPIGatewayRequest.hash, RequestMode.PANEL, true, pGTransactionResponse.Amount, string.Empty);
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "GeneratePGRequestForWeb",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = pGTransactionResponse.PGID
                });
            }
            return res;
        }
        public PGModelForRedirection GeneratePGRequestForApp(PGTransactionResponse pGTransactionResponse, Func<int, int, string, string, string, int, bool, decimal, string, Task> savePGTransactionLog)
        {
            var res = new PGModelForRedirection
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            var uPIGatewayRequest = new UPIGatewayRequest();
            try
            {
                var KeyVals = new Dictionary<string, string>
                {
                    { nameof(uPIGatewayRequest.key), pGTransactionResponse.MerchantID },
                    { nameof(uPIGatewayRequest.client_vpa), pGTransactionResponse.VPA },
                    { nameof(uPIGatewayRequest.client_txn_id), pGTransactionResponse.TID.ToString() },
                    { nameof(uPIGatewayRequest.amount), pGTransactionResponse.Amount.ToString()},
                    { nameof(uPIGatewayRequest.p_info),  "Add Money to consume services"},
                    { nameof(uPIGatewayRequest.client_name),  pGTransactionResponse.Name},
                    { nameof(uPIGatewayRequest.client_email),  pGTransactionResponse.EmailID},
                    { nameof(uPIGatewayRequest.client_mobile),  pGTransactionResponse.MobileNo},
                    { nameof(uPIGatewayRequest.udf1),  pGTransactionResponse.City},
                    { nameof(uPIGatewayRequest.udf2),  pGTransactionResponse.State},
                    { nameof(uPIGatewayRequest.udf3),  pGTransactionResponse.Pincode}
                };
                //uPIGatewayRequest.hash = GenerateCheckSum(pGTransactionResponse.MerchantKey, KeyVals);
                uPIGatewayRequest.redirect_url = pGTransactionResponse.Domain + "/PGCallback/UPIGatewayRedirect?TID=" + pGTransactionResponse.TID;
                //KeyVals.Add(nameof(uPIGatewayRequest.hash), uPIGatewayRequest.hash);
                KeyVals.Add(nameof(uPIGatewayRequest.redirect_url), uPIGatewayRequest.redirect_url);
                res.KeyVals = KeyVals;
                res.Statuscode = ErrorCodes.One;
                res.Msg = "Transaction intiated";
                savePGTransactionLog(pGTransactionResponse.PGID, pGTransactionResponse.TID, JsonConvert.SerializeObject(res), pGTransactionResponse.TransactionID, uPIGatewayRequest.hash, RequestMode.APPS, true, pGTransactionResponse.Amount, string.Empty);
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "GeneratePGRequestForWeb",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = pGTransactionResponse.PGID
                });
            }
            return res;
        }

        private string GenerateCheckSum(string salt, IDictionary<string, string> keyValuePairs)
        {
            var sb = new StringBuilder();
            if (keyValuePairs != null)
            {
                foreach (var item in keyValuePairs)
                {
                    sb.Append(item.Value);
                    sb.Append("|");
                }
                sb.Append(salt);
            }
            return HashEncryption.O.SHA512HashUTF8(sb.ToString()).ToLower();
        }
    }
}
