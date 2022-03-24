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
    public class ProcGetTransactionStatus : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcGetTransactionStatus(IDAL dal) => _dal = dal;
        public async Task<object> Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@UserID",req.LoginID),
                new SqlParameter("@TransactionID",req.CommonStr??string.Empty),
                new SqlParameter("@AgentID",req.CommonStr2??string.Empty),
                new SqlParameter("@IsBBPSOnly",req.CommonBool),
                new SqlParameter("@FromDate",req.CommonStr3??string.Empty),
                new SqlParameter("@ToDate",req.CommonStr4??string.Empty),
                new SqlParameter("@CustomerMobile",req.str??string.Empty)
            };
            var res = new TransactionResponse
            {
                AGENTID = req.CommonStr2 ?? string.Empty,
                RPID = req.CommonStr ?? string.Empty,
                STATUS = RechargeRespType.PENDING,
                MSG = "No record found",
                ERRORCODE = ErrorCodes.Request_Accpeted.ToString()
            };
            try
            {
                var dt = await _dal.GetByProcedureAsync(GetName(), param).ConfigureAwait(false);
                if (dt.Rows.Count > 0)
                {
                    if (Convert.ToInt32(dt.Rows[0][0]) == ErrorCodes.One)
                    {
                        res.STATUS = Convert.ToInt32(dt.Rows[0]["_Type"]);

                        res.ACCOUNT = dt.Rows[0]["_Account"].ToString();
                        res.AMOUNT = Convert.ToDecimal(dt.Rows[0]["_RequestedAmount"]);
                        res.BAL = Convert.ToDecimal(dt.Rows[0]["_Balance"]);
                        res.AGENTID = dt.Rows[0]["_APIRequestID"].ToString();
                        res.OPID = dt.Rows[0]["_LiveID"].ToString();
                        res.MSG = dt.Rows[0]["Msg"].ToString();
                        res.RPID = dt.Rows[0]["TransactionID"].ToString();
                        res.RefundStatus = Convert.ToInt16(dt.Rows[0]["_RefundStatus"] is DBNull ? 0 : dt.Rows[0]["_RefundStatus"]);
                        res.VendorID = dt.Rows[0]["_VendorID"] is DBNull ? "" : dt.Rows[0]["_VendorID"].ToString();
                        res.TID = dt.Rows[0]["TID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["TID"]);
                        res.ERRORCODE = ErrorCodes.Transaction_Successful.ToString();
                        res.Operator = dt.Rows[0]["_Operator"].ToString();
                        if (res.STATUS == RechargeRespType.FAILED)
                        {
                            res.MSG = RechargeRespType._FAILED;
                            res.ERRORCODE = ErrorCodes.Unknown_Error.ToString();
                        }
                        if (res.STATUS == RechargeRespType.PENDING)
                        {
                            res.MSG = RechargeRespType._PENDING;
                            res.ERRORCODE = ErrorCodes.Request_Accpeted.ToString();
                        }
                    }
                    else
                    {
                        res.MSG = dt.Rows[0]["Msg"].ToString();
                    }
                }
            }
            catch (Exception)
            {
            }
            return res;
        }

        public Task<object> Call() => throw new NotImplementedException();
        public string GetDateForAPIRequestID(string APIRequestID, int UserID)
        {
            SqlParameter[] param = {
                new SqlParameter("@APIRequestID",APIRequestID??string.Empty),
                new SqlParameter("@UserID",UserID)
            };
            string query = "select Convert(varchar,_EntryDate,106 ) _EntryDate from tbl_APIRequestID where _APIRequestID=@APIRequestID and _UserID=@UserID";
            try
            {
                var dt = _dal.Get(query, param);
                if (dt.Rows.Count > 0)
                {
                    return dt.Rows[0][0] is DBNull ? string.Empty : dt.Rows[0][0].ToString();
                }
            }
            catch (Exception ex)
            {
            }
            return string.Empty;
        }
        public string GetName() => "proc_GetTransactionStatus";
    }
}
