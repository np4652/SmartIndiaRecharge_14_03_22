using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model.Reports;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcAccountSummary : IProcedure
    {
        private readonly IDAL _dal;
        public ProcAccountSummary(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            ProcUserLedgerRequest _req = (ProcUserLedgerRequest)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT",_req.LT),
                new SqlParameter("@LoginID",_req.LoginID),
                new SqlParameter("@Mobile_F", _req.Mobile_F),
                new SqlParameter("@FromDate", _req.FromDate_F??DateTime.Now.ToString("dd MMM yyyy")),
                new SqlParameter("@ToDate", _req.ToDate_F??DateTime.Now.ToString("dd MMM yyyy")),
                new SqlParameter("@UserID", _req.UserID),
                new SqlParameter("@WalletTypeID", _req.WalletTypeID)
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
                    if (_res.StatusCode == ErrorCodes.One) {
                        _res.Opening = dt.Rows[0]["Opening"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["Opening"]);
                        _res.Purchase = dt.Rows[0]["Purchase"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["Purchase"]);
                        _res.Return = dt.Rows[0]["_Return"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["_Return"]);
                        _res.FundDeducted = dt.Rows[0]["FundDeducted"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["FundDeducted"]);
                        _res.Requested = dt.Rows[0]["Requested"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["Requested"]);
                        _res.CCF = dt.Rows[0]["CCF"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["CCF"]);
                        _res.Debited = dt.Rows[0]["Debited"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["Debited"]);
                        _res.Debited2202 = dt.Rows[0]["_Debited2202"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["_Debited2202"]);
                        _res.FundTransfered = dt.Rows[0]["FundTransfered"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["FundTransfered"]);
                        _res.Refunded = dt.Rows[0]["Refunded"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["Refunded"]);
                        //_res.Commission = _res.Requested - _res.Debited;
                        _res.Commission = dt.Rows[0]["Comm"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["Comm"]);
                        _res.CCFCommission = dt.Rows[0]["CCFCommission"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["CCFCommission"]);
                        _res.Surcharge = dt.Rows[0]["Surcharge"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["Surcharge"]);
						_res.DCommission = dt.Rows[0]["DComm"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["DComm"]);
                        _res.OtherCharge = dt.Rows[0]["Other"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["Other"]);
                        _res.Expected = dt.Rows[0]["Expected"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["Expected"]);
                        //_res.SetTarget = dt.Rows[0]["_SetTarget"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["_SetTarget"]);
                        //_res.TargetTillDate = dt.Rows[0]["_TargetTillDate"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["_TargetTillDate"]);
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
            return _res;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_AccountSummary";
    }
}
