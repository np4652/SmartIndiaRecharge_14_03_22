using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetDMTTransactionStatus : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcGetDMTTransactionStatus(IDAL dal) => _dal = dal;
        public async Task<object> Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@UserID",req.LoginID),
                new SqlParameter("@TransactionID",req.CommonStr??string.Empty),
                new SqlParameter("@AgentID",req.CommonStr2??string.Empty)
            };
            var res = new DMTCheckStatusResponse
            {
                AgentID = req.CommonStr2 ?? string.Empty,
                TransactionID = req.CommonStr ?? string.Empty,
                Status = RechargeRespType.PENDING,
                Message = "No record found",
                ErrorCode = ErrorCodes.Request_Accpeted
            };
            try
            {
                var dt = await _dal.GetByProcedureAsync(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    if (Convert.ToInt32(dt.Rows[0][0]) == ErrorCodes.One)
                    {
                        res.Status = dt.Rows[0]["_Type"] is DBNull ? res.Status : Convert.ToInt32(dt.Rows[0]["_Type"]);
                        res.Account = dt.Rows[0]["_Account"] is DBNull ? res.Account : dt.Rows[0]["_Account"].ToString();
                        res.Amount = dt.Rows[0]["_RequestedAmount"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_RequestedAmount"]);
                        res.Balance = dt.Rows[0]["_Balance"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["_Balance"]);
                        res.AgentID = dt.Rows[0]["_APIRequestID"] is DBNull ? res.AgentID : dt.Rows[0]["_APIRequestID"].ToString();
                        res.LiveID = dt.Rows[0]["_LiveID"] is DBNull ? string.Empty : dt.Rows[0]["_LiveID"].ToString();
                        res.Message = dt.Rows[0]["Msg"] is DBNull ? string.Empty : dt.Rows[0]["Msg"].ToString();
                        res.TransactionID = dt.Rows[0]["TransactionID"] is DBNull ? string.Empty : dt.Rows[0]["TransactionID"].ToString();
                        res.RefundStatus = Convert.ToInt16(dt.Rows[0]["_RefundStatus"] is DBNull ? 0 : dt.Rows[0]["_RefundStatus"]);
                        res.BeneName = dt.Rows[0]["_BeneName"] is DBNull ? string.Empty : dt.Rows[0]["_BeneName"].ToString();
                        
                        res.ErrorCode = ErrorCodes.Transaction_Successful;
                        if (res.Status == RechargeRespType.FAILED)
                        {
                            res.Message = RechargeRespType._FAILED;
                            res.ErrorCode = ErrorCodes.Unknown_Error;
                        }
                        if (res.Status == RechargeRespType.PENDING)
                        {
                            res.Message = RechargeRespType._PENDING;
                            res.ErrorCode = ErrorCodes.Request_Accpeted;
                        }
                    }
                    else
                    {
                        res.Message = dt.Rows[0]["Msg"].ToString();
                    }
                }
            }
            catch (Exception)
            {
            }
            return res;
        }

        public Task<object> Call() => throw new NotImplementedException();

        public string GetName() => "proc_GetDMTTransactionStatus";
    }
}
