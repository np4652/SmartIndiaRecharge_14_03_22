using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcVerificationService : IProcedure
    {
        private readonly IDAL _dal;
        public ProcVerificationService(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (VerificationServiceProcReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@UserID",req.UserID),
                new SqlParameter("@OID",req.OID),
                new SqlParameter("@AccountNo",req.AccountNo??string.Empty),
                new SqlParameter("@Optional1",req.Optional1??string.Empty),
                new SqlParameter("@Optional2",req.Optional2??string.Empty),
                new SqlParameter("@Optional3",req.Optional3??string.Empty),
                new SqlParameter("@Optional4",req.Optional4??string.Empty),
                new SqlParameter("@IP",req.IP??string.Empty),
                new SqlParameter("@BankID",req.BankID),
                new SqlParameter("@BankHandle",req.BankHandle??string.Empty),
                new SqlParameter("@RequestMode",req.RequestMode),
                new SqlParameter("@APIRequestID",req.APIRequestID??string.Empty),
                new SqlParameter("@SPKey",req.SPKey??string.Empty)
            };
            var res = new VerificationServiceProcRes
            {
                Statuscode = ErrorCodes.One,
                Msg = ErrorCodes.TempError,
                ErrorCode = ErrorCodes.Request_Accpeted
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = dt.Rows[0][0] is DBNull ? res.Statuscode : Convert.ToInt16(dt.Rows[0][0]);
                    res.Msg = dt.Rows[0]["Msg"] is DBNull ? string.Empty : dt.Rows[0]["Msg"].ToString();
                    res.ErrorCode = dt.Rows[0]["_ErrorCode"] is DBNull ? res.ErrorCode : Convert.ToInt32(dt.Rows[0]["_ErrorCode"]);
                    res.Balance = dt.Rows[0]["Balance"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["Balance"]);
                    if (res.Statuscode == ErrorCodes.One)
                    {
                        res.TID= dt.Rows[0]["TID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["TID"]);
                        res.APIID= dt.Rows[0]["_APIID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_APIID"]);
                        res.OpTypeID = dt.Rows[0]["_OpTypeID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_OpTypeID"]);
                        res.TransactionID= dt.Rows[0]["TransactionID"] is DBNull ? string.Empty : dt.Rows[0]["TransactionID"].ToString();
                        res.APICode= dt.Rows[0]["_APICode"] is DBNull ? string.Empty : dt.Rows[0]["_APICode"].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = 1,
                    UserId = 1
                });
            }
            return res;
        }

        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_VerificationService";
    }
}
