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
using System.Text;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.ThirdParty.AggrePay
{
    public class AggrePayML
    {
        private readonly IDAL _dal;
        public AggrePayML(IDAL dal) => _dal = dal;

        public PGModelForRedirection GeneratePGRequestForWeb(PGTransactionResponse pGTransactionResponse, Func<int, int, string, string, string, int, bool, decimal, string, Task> savePGTransactionLog)
        {
            var res = new PGModelForRedirection
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            var aggrepayRequest = new AggrepayRequest();
            try
            {
                var KeyVals = new Dictionary<string, string>
                {
                    { nameof(aggrepayRequest.amount), pGTransactionResponse.Amount.ToString() },
                    { nameof(aggrepayRequest.api_key), pGTransactionResponse.MerchantID },
                    { nameof(aggrepayRequest.city),  pGTransactionResponse.City},
                    { nameof(aggrepayRequest.country),  "IND"},
                    { nameof(aggrepayRequest.currency), "INR" },
                    { nameof(aggrepayRequest.description),  "Add Money to consume services"},
                    { nameof(aggrepayRequest.email),  pGTransactionResponse.EmailID},
                    { nameof(aggrepayRequest.mode), pGTransactionResponse.ENVCode },
                    { nameof(aggrepayRequest.name),  pGTransactionResponse.Name},
                    { nameof(aggrepayRequest.order_id), pGTransactionResponse.TID.ToString() },
                    { nameof(aggrepayRequest.phone),  pGTransactionResponse.MobileNo},
                    { nameof(aggrepayRequest.return_url), pGTransactionResponse.Domain+"/PGCallback/AggrePay" },
                    { nameof(aggrepayRequest.state),  pGTransactionResponse.State},
                    { nameof(aggrepayRequest.zip_code),  pGTransactionResponse.Pincode}
                };
                aggrepayRequest.hash = GenerateCheckSum(pGTransactionResponse.MerchantKey, KeyVals);
                KeyVals.Add(nameof(aggrepayRequest.hash), aggrepayRequest.hash);
                res.KeyVals = KeyVals;
                res.Statuscode = ErrorCodes.One;
                res.Msg = "Transaction intiated";
                savePGTransactionLog(pGTransactionResponse.PGID, pGTransactionResponse.TID, JsonConvert.SerializeObject(res), pGTransactionResponse.TransactionID, aggrepayRequest.hash, RequestMode.PANEL, true, pGTransactionResponse.Amount, string.Empty);
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "GeneratePGRequestForWeb",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = pGTransactionResponse.PGID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
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
            var aggrepayRequest = new AggrepayRequest();
            try
            {
                var KeyVals = new Dictionary<string, string>
                {
                    { nameof(aggrepayRequest.amount), pGTransactionResponse.Amount.ToString() },
                    { nameof(aggrepayRequest.api_key), pGTransactionResponse.MerchantID },
                    { nameof(aggrepayRequest.city),  pGTransactionResponse.City},
                    { nameof(aggrepayRequest.country),  "IND"},
                    { nameof(aggrepayRequest.currency), "INR" },
                    { nameof(aggrepayRequest.description),  "Add Money to consume services"},
                    { nameof(aggrepayRequest.email),  pGTransactionResponse.EmailID},
                    { nameof(aggrepayRequest.mode), pGTransactionResponse.ENVCode },
                    { nameof(aggrepayRequest.name),  pGTransactionResponse.Name},
                    { nameof(aggrepayRequest.order_id), pGTransactionResponse.TID.ToString() },
                    { nameof(aggrepayRequest.phone),  pGTransactionResponse.MobileNo},
                    { nameof(aggrepayRequest.return_url), pGTransactionResponse.Domain+"/PGCallback/AggrePayApp" },
                    { nameof(aggrepayRequest.state),  pGTransactionResponse.State},
                    { nameof(aggrepayRequest.zip_code),  pGTransactionResponse.Pincode}
                };
                aggrepayRequest.hash = GenerateCheckSum(pGTransactionResponse.MerchantKey, KeyVals);
                //KeyVals.Add(nameof(aggrepayRequest.hash), aggrepayRequest.hash);
                res.KeyVals = KeyVals;
                res.Statuscode = ErrorCodes.One;
                res.Msg = "Transaction intiated";
                savePGTransactionLog(pGTransactionResponse.PGID, pGTransactionResponse.TID, JsonConvert.SerializeObject(res), pGTransactionResponse.TransactionID, aggrepayRequest.hash, RequestMode.APPS, true, pGTransactionResponse.Amount, string.Empty);
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "GeneratePGRequestForApp",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = pGTransactionResponse.PGID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }

            return res;
        }
        public AggrePayResponseData StatusCheckPG(TransactionPGLog transactionPGLog, Func<int, int, string, string, string, int, bool, decimal, string, Task> savePGTransactionLog)
        {
            var aggrePayresp = new AggrePayResponseData();
            var res = new PGModelForRedirection
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            string aggrepayResp = string.Empty;
            var aggrepayRequest = new AggrepayRequest();
            try
            {
                var KeyVals = new Dictionary<string, string>
                {
                    { nameof(aggrepayRequest.api_key), transactionPGLog.MerchantID },
                    { nameof(aggrepayRequest.order_id), transactionPGLog.TID.ToString() }
                };
                aggrepayRequest.hash = GenerateCheckSum(transactionPGLog.MerchantKEY, KeyVals);
                KeyVals.Add(nameof(aggrepayRequest.hash), aggrepayRequest.hash);
                res.KeyVals = KeyVals;
                res.Statuscode = ErrorCodes.One;
                res.Msg = "Statuscheck";
                savePGTransactionLog(transactionPGLog.PGID, transactionPGLog.TID, JsonConvert.SerializeObject(res), transactionPGLog.TransactionID, aggrepayRequest.hash, RequestMode.API, false, transactionPGLog.Amount, string.Empty);
                aggrepayResp = AppWebRequest.O.PostJsonDataUsingHWR(transactionPGLog.StatuscheckURL, KeyVals);
                if (!string.IsNullOrEmpty(aggrepayResp))
                {
                    aggrePayresp = JsonConvert.DeserializeObject<AggrePayResponseData>(aggrepayResp);
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
            savePGTransactionLog(transactionPGLog.PGID, transactionPGLog.TID, aggrepayResp, transactionPGLog.TransactionID, aggrepayRequest.hash, RequestMode.API, true, transactionPGLog.Amount, string.Empty);
            return aggrePayresp;
        }
        private string GenerateCheckSum(string salt, IDictionary<string, string> keyValuePairs)
        {
            var sb = new StringBuilder(salt);
            if (keyValuePairs != null)
            {
                foreach (var item in keyValuePairs)
                {
                    sb.Append("|");
                    sb.Append(item.Value);
                }
            }
            return HashEncryption.O.SHA512HashUTF8(sb.ToString());
        }
    }
}
