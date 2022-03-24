using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model.Reports;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcASHSummarry : IProcedure
    {
        private readonly IDAL _dal;
        public ProcASHSummarry(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            ProcUserLedgerRequest _req = (ProcUserLedgerRequest)obj;
            SqlParameter[] param = {
                new SqlParameter("@MobileNo", _req.Mobile_F),
                new SqlParameter("@FromDate", _req.FromDate_F??DateTime.Now.ToString("dd MMM yyyy")),
                new SqlParameter("@ToDate", _req.ToDate_F??DateTime.Now.ToString("dd MMM yyyy"))
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
                        _res.Opening = dt.Rows[0]["_Opening"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["_Opening"]);
                        _res.Sales = dt.Rows[0]["_Sales"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["_Sales"]);
                        _res.CCollection = dt.Rows[0]["_CCollection"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["_CCollection"]);
                        _res.Closing = dt.Rows[0]["_Closing"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["_Closing"]);
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
                    LoginTypeID = _req.LT,
                    UserId = _req.LoginID
                });
            }
            return _res;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_ASHSummarry";
    }
}
