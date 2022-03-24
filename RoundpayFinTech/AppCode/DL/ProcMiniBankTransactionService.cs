using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcMiniBankTransactionService : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcMiniBankTransactionService(IDAL dal) => _dal = dal;
        public async Task<object> Call(object obj)
        {
            var req = (MiniBankTransactionServiceReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@OutletID",req.OutletID??string.Empty),
                new SqlParameter("@AmountR",req.AmountR),
                new SqlParameter("@VendorID",req.VendorID??string.Empty),
                new SqlParameter("@BankIIN",req.BankIIN??string.Empty),
                new SqlParameter("@TxnType",req.TXNType??string.Empty),
                new SqlParameter("@TerminalID",req.TerminalID??string.Empty),
                new SqlParameter("@BCID",req.BCID??string.Empty),
                new SqlParameter("@APICode",req.APICode??string.Empty),
                new SqlParameter("@RequestIP",req.RequestIP??string.Empty),
                new SqlParameter("@APIUrl",req.RequestURL??string.Empty),
                new SqlParameter("@OutletIDSelf",req.OutletIDSelf),
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@RequestModeID",req.RequestModeID),
                new SqlParameter("@OPTypeID",req.OpTypeID),
                new SqlParameter("@OID",req.OID),
                new SqlParameter("@APIOpCode",req.APIOpCode),
                new SqlParameter("@AccountNo",req.AccountNo??string.Empty),
                new SqlParameter("@TransactionID",req.TransactionID??string.Empty)
            };
            var res = new MiniBankTransactionServiceResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            try
            {
                var dt = await _dal.GetByProcedureAsync(GetName(), param).ConfigureAwait(false);
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
                        res.UserID = dt.Rows[0]["_UserID"] is DBNull ? 0 :Convert.ToInt32(dt.Rows[0]["_UserID"]);
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

        public Task<object> Call() => throw new NotImplementedException();
        public string GetName() => "proc_MiniBankTransactionService";
    }
}
