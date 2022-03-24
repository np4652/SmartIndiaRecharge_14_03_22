using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcUserPackageLimitLedger : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcUserPackageLimitLedger(IDAL dal) => _dal = dal;
        public string GetName() => "proc_UserPackageLimitLedger";
        public async Task<object> Call(object obj)
        {
            var _req = (ProcUserLedgerRequest)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT", _req.LT),
                new SqlParameter("@LoginID", _req.LoginID),
                new SqlParameter("@DebitCredit_F", _req.DebitCredit_F),
                new SqlParameter("@TransactionId_F", _req.TransactionId_F??""),
                new SqlParameter("@FromDate_F", _req.FromDate_F ?? DateTime.Now.ToString("dd MMM yyyy")),
                new SqlParameter("@ToDate_F", _req.ToDate_F ?? DateTime.Now.ToString("dd MMM yyyy")),
                new SqlParameter("@Mobile_F", _req.Mobile_F ?? ""),
                new SqlParameter("@TopRows", _req.TopRows),
                new SqlParameter("@UserID", _req.UserID),
                new SqlParameter("@WalletTypeID", _req.WalletTypeID)
            };

            var _alist = new List<ProcUserLedgerResponse>();
            var _res = new ProcUserLedgerResponse
            {
                ResultCode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            try
            {
                var dt = await _dal.GetByProcedureAsync(GetName(), param);
                if (dt.Columns.Contains("Msg"))
                {
                    _res.ResultCode = ErrorCodes.Minus1;
                    _res.Msg = dt.Rows[0]["Msg"].ToString();
                }
                else
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        var item = new ProcUserLedgerResponse
                        {
                            ResultCode = ErrorCodes.One,
                            Msg = "Success",
                            ID = Convert.ToInt32(dt.Rows[i]["_ID"]),
                            Remark = dt.Rows[i]["_Remark"].ToString(),
                            LastAmount = Convert.ToDecimal(dt.Rows[i]["_LastAmount"]),
                            Credit = Convert.ToDecimal(dt.Rows[i]["_Credit"]),
                            Debit = Convert.ToDecimal(dt.Rows[i]["_Debit"]),
                            CurentBalance = Convert.ToDecimal(dt.Rows[i]["_CurrentAmount"].ToString()),
                            EntryDate = dt.Rows[i]["_EntryDate"].ToString(),
                            TID = dt.Rows[i]["_TID"].ToString(),
                            Description = dt.Rows[i]["_Description"] is DBNull ? "" : dt.Rows[i]["_Description"].ToString(),
                            User = dt.Rows[i]["_User"].ToString(),
                            ServiceID = dt.Rows[i]["_ServiceTypeID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[i]["_ServiceTypeID"]),
                            LType = dt.Rows[i]["_Type"] is DBNull ? false : Convert.ToBoolean(dt.Rows[i]["_Type"]),
                            UserID = dt.Rows[i]["_UserID"] is DBNull ? "" : dt.Rows[i]["_UserID"].ToString()
                        };
                        _alist.Add(item);
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
                    LoginTypeID = _req.LT,
                    UserId = _req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return _alist;
        }

        public Task<object> Call()
        {
            throw new NotImplementedException();
        }
    }
}
