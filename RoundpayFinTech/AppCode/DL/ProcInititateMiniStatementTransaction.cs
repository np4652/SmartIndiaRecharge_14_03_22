using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcInititateMiniStatementTransaction : IProcedure
    {
        private readonly IDAL _dal;
        public ProcInititateMiniStatementTransaction(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (InititateMiniStatementTransactionRequest)obj;
            SqlParameter[] param = {
                new SqlParameter("@UserID",req.UserID),
                new SqlParameter("@OutletID",req.OutletID),
                new SqlParameter("@OID",req.OID),
                new SqlParameter("@AccountNo",req.AccountNo??string.Empty),
                new SqlParameter("@BankName",req.BankName??string.Empty),
                new SqlParameter("@BankIIN",req.BankIIN??string.Empty),
                new SqlParameter("@APICode",req.APICode??string.Empty),
                new SqlParameter("@APIOutletID",req.APIOutletID??string.Empty),
                new SqlParameter("@VendorID",req.VendorID??string.Empty),
                new SqlParameter("@APIOpCode",req.APIOpCode??string.Empty),
                new SqlParameter("@APIRequestID",req.APIRequestID??string.Empty),
                new SqlParameter("@RequestModeID",req.RequestModeID),
                new SqlParameter("@IP",req.IP??string.Empty),
                new SqlParameter("@Browser",req.Browser??string.Empty),
                new SqlParameter("@SPKey",req.SPKey??string.Empty)
            };
            var res = new AEPSTransactionServiceResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = dt.Rows[0][0] is DBNull ? res.Statuscode : Convert.ToInt16(dt.Rows[0][0]);
                    res.Msg = dt.Rows[0]["Msg"] is DBNull ? string.Empty : dt.Rows[0]["Msg"].ToString();
                    if (res.Statuscode == ErrorCodes.One)
                    {
                        res.Balance = dt.Rows[0]["_Balance"] is DBNull ? 0M : Convert.ToDecimal(dt.Rows[0]["_Balance"]);
                        res.TID = dt.Rows[0]["_TID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_TID"]);
                        res.TransactionID = dt.Rows[0]["_TransactionID"] is DBNull ? string.Empty : dt.Rows[0]["_TransactionID"].ToString();
                        res.APIOutletID = dt.Rows[0]["_APIOutletID"] is DBNull ? string.Empty : dt.Rows[0]["_APIOutletID"].ToString();
                        res.OutletName = dt.Rows[0]["_Name"] is DBNull ? string.Empty : dt.Rows[0]["_Name"].ToString();
                        res.CallBackURL = dt.Rows[0]["_CallBackURL"] is DBNull ? string.Empty : dt.Rows[0]["_CallBackURL"].ToString();
                        res.BillOpCode = dt.Rows[0]["_BillOpCode"] is DBNull ? string.Empty : dt.Rows[0]["_BillOpCode"].ToString();
                        res.UserID = dt.Rows[0]["_UserID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_UserID"]);
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

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName() => "proc_InititateMiniStatementTransaction";
    }
}
