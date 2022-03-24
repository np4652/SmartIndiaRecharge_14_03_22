using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.Model.Reports;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcAccountSummaryDashboard : IProcedure
    {
        private readonly IDAL _dal;
        public ProcAccountSummaryDashboard(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var _req = (ProcUserLedgerRequest)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT",_req.LT),
                new SqlParameter("@LoginID",_req.LoginID)
            };
            var _res = new AccountSummary
            {
                StatusCode = ErrorCodes.Minus1,
                Status = ErrorCodes.TempError
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    _res.StatusCode = Convert.ToInt32(dt.Rows[0][0]);
                    _res.Status = dt.Rows[0]["Msg"].ToString();
                    _res.Purchase = dt.Rows[0]["Purchase"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["Purchase"]);
                    _res.Return = dt.Rows[0]["_Return"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["_Return"]);
                    _res.FundDeducted = dt.Rows[0]["FundDeducted"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["FundDeducted"]);
                    _res.Requested = dt.Rows[0]["Requested"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["Requested"]);
                    _res.Debited = dt.Rows[0]["Debited"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["Debited"]);
                    _res.FundTransfered = dt.Rows[0]["FundTransfered"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["FundTransfered"]);
                    _res.Commission = dt.Rows[0]["Comm"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["Comm"]);
                    _res.Surcharge = dt.Rows[0]["Surcharge"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["Surcharge"]);
                    _res.OtherCharge = dt.Rows[0]["Other"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["Other"]);
                    _res.RoleID = dt.Rows[0]["RoleID"] is DBNull ? 0 : Convert.ToInt16(dt.Rows[0]["RoleID"]);
                    _res.SetTarget = dt.Rows[0]["_SetTarget"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["_SetTarget"]);
                    _res.TargetTillDate = dt.Rows[0]["_TargetTillDate"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["_TargetTillDate"]);
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
            return _res;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_AccountSummaryDashboard";
    }

    public class ProcAccountSummaryTable : IProcedure
    {
        private readonly IDAL _dal;
        public ProcAccountSummaryTable(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var _req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT",_req.LoginTypeID),
                new SqlParameter("@UserID",_req.LoginID)
            };
            var _res = new AccountSummaryTable();
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    _res.LastPriTransactionAmount = dt.Rows[0]["LastPriTransactionAmount"] is DBNull ? 0 : float.Parse(dt.Rows[0]["LastPriTransactionAmount"].ToString());
                    _res.LastPriTransactionDate = dt.Rows[0]["LastPriTransactionDate"] is DBNull ? string.Empty : dt.Rows[0]["LastPriTransactionDate"].ToString();
                    _res.LastSecTransactionAmount = dt.Rows[0]["LastSecTransactionAmount"] is DBNull ? 0 : float.Parse(dt.Rows[0]["LastSecTransactionAmount"].ToString());
                    _res.LastSecTransactionDate = dt.Rows[0]["LastSecTransactionDate"] is DBNull ? string.Empty : dt.Rows[0]["LastSecTransactionDate"].ToString();
                    _res.LastTerTransactionAmount = dt.Rows[0]["LastTerTransactionAmount"] is DBNull ? 0 : float.Parse(dt.Rows[0]["LastTerTransactionAmount"].ToString());
                    _res.LastTerTransactionDate = dt.Rows[0]["LastTerTransactionDate"] is DBNull ? string.Empty : dt.Rows[0]["LastTerTransactionDate"].ToString();
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = _req.LoginTypeID,
                    UserId = _req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return _res;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_AccountSummaryTable";
    }
}