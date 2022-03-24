using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcPGatewayTransacrionService : IProcedure
    {
        private readonly IDAL _dal;
        public ProcPGatewayTransacrionService(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (PGTransactionRequest)obj;
            SqlParameter[] param = {
                new SqlParameter("@UserID",req.UserID),
                new SqlParameter("@AmountR",req.AmountR),
                new SqlParameter("@UPGID",req.UPGID),
                new SqlParameter("@OID",req.OID),
                new SqlParameter("@WalletID",req.WalletID),
                new SqlParameter("@RequestMode",req.RequestMode),
                new SqlParameter("@Browser",req.Browser??string.Empty),
                new SqlParameter("@RequestIP",req.RequestIP??string.Empty),
                new SqlParameter("@IMEI",req.IMEI??string.Empty)
            };
            var res = new PGTransactionResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0) {
                    res.Statuscode = dt.Rows[0][0] is DBNull ? res.Statuscode : Convert.ToInt16(dt.Rows[0][0]);
                    res.Msg = dt.Rows[0]["Msg"] is DBNull ? string.Empty : dt.Rows[0]["Msg"].ToString();
                    if (res.Statuscode == RechargeRespType.PENDING) {
                        res.UserID = req.UserID;
                        res.Amount = req.AmountR;
                        res.PGID = dt.Rows[0]["_PGID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_PGID"]);
                        res.TID = dt.Rows[0]["TID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["TID"]);
                        res.TransactionID = dt.Rows[0]["TransactionID"] is DBNull ? string.Empty : dt.Rows[0]["TransactionID"].ToString();
                        res.PGName = dt.Rows[0]["_PGName"] is DBNull ? string.Empty : dt.Rows[0]["_PGName"].ToString();
                        res.URL = dt.Rows[0]["_URL"] is DBNull ? string.Empty : dt.Rows[0]["_URL"].ToString();
                        res.StatusCheckURL = dt.Rows[0]["_StatusCheckURL"] is DBNull ? string.Empty : dt.Rows[0]["_StatusCheckURL"].ToString();
                        res.MerchantID = dt.Rows[0]["_MerchantID"] is DBNull ? string.Empty : dt.Rows[0]["_MerchantID"].ToString();
                        res.MerchantKey = dt.Rows[0]["_MerchantKey"] is DBNull ? string.Empty : dt.Rows[0]["_MerchantKey"].ToString();
                        res.ENVCode = dt.Rows[0]["_ENVCode"] is DBNull ? string.Empty : dt.Rows[0]["_ENVCode"].ToString();
                        res.IndustryType = dt.Rows[0]["_IndustryType"] is DBNull ? string.Empty : dt.Rows[0]["_IndustryType"].ToString();
                        res.SuccessURL = dt.Rows[0]["_SuccessURL"] is DBNull ? string.Empty : dt.Rows[0]["_SuccessURL"].ToString();
                        res.FailedURL = dt.Rows[0]["_FailedURL"] is DBNull ? string.Empty : dt.Rows[0]["_FailedURL"].ToString();
                        res.MobileNo = dt.Rows[0]["_MobileNo"] is DBNull ? string.Empty : dt.Rows[0]["_MobileNo"].ToString();
                        res.EmailID = dt.Rows[0]["_EmailID"] is DBNull ? string.Empty : dt.Rows[0]["_EmailID"].ToString();
                        res.Name = dt.Rows[0]["_Name"] is DBNull ? string.Empty : dt.Rows[0]["_Name"].ToString();
                        res.CompanyName = dt.Rows[0]["_CompanyName"] is DBNull ? string.Empty : dt.Rows[0]["_CompanyName"].ToString();
                        res.WID = dt.Rows[0]["_WID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_WID"]);
                        res.OPID = dt.Rows[0]["_OPID"] is DBNull ? string.Empty : dt.Rows[0]["_OPID"].ToString();
                        res.City = dt.Rows[0]["_City"] is DBNull ? string.Empty : dt.Rows[0]["_City"].ToString();
                        res.State = dt.Rows[0]["_State"] is DBNull ? string.Empty : dt.Rows[0]["_State"].ToString();
                        res.Pincode = dt.Rows[0]["_Pincode"] is DBNull ? string.Empty : dt.Rows[0]["_Pincode"].ToString();
                        res.IsLive = dt.Rows[0]["_IsLive"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsLive"]);
                    }
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = req.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res ?? new PGTransactionResponse();
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_PGatewayTransacrionService";
    }
}
