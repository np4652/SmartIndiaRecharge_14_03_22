using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model.Reports;
using Fintech.AppCode.StaticModel;
using System;
using System.Data;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcAdminAccountSummary : IProcedure
    {
        private readonly IDAL _dal;
        public ProcAdminAccountSummary(IDAL dal)
        {
            _dal = dal;
        }
        public object Call(object obj)
        {
            throw new NotImplementedException();
        }
        public object Call()
        {
            var _res = new AccountSummary
            {
                StatusCode = ErrorCodes.Minus1,
                Status = ErrorCodes.TempError
            };         
            try
            {
                var dt = _dal.GetByProcedure(GetName());
                if (dt.Rows.Count > 0)
                {
                    _res.StatusCode = Convert.ToInt32(dt.Rows[0][0]);
                    _res.Status = dt.Rows[0]["Msg"].ToString();
                    _res.Opening = dt.Rows[0]["Opening"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["Opening"]);
                    _res.FundDeducted = dt.Rows[0]["FundDeducted"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["FundDeducted"]);
                    _res.Refunded = dt.Rows[0]["Refunded"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["Refunded"]);
                    _res.Commission = dt.Rows[0]["Comm"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["Comm"]);
                    _res.Surcharge = dt.Rows[0]["Surcharge"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["Surcharge"]);
                    _res.FundTransfered = dt.Rows[0]["FundTransfered"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["FundTransfered"]);
                    _res.Debited = dt.Rows[0]["SuccessAmt"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["SuccessAmt"]);
                    _res.Return = dt.Rows[0]["CCFAmount"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["CCFAmount"]);
                    _res.OtherCharge = dt.Rows[0]["Other"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["Other"]);
                    _res.CCFCommDebited = dt.Rows[0]["CCFCommDebited"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["CCFCommDebited"]);
                    _res.Expected = dt.Rows[0]["Expected"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["Expected"]);
                }
            }
            catch (Exception ex)
            {
                //var _ = _err.FnErrorLog(GetType().Name,"Call",ex.Message,_req.LoginID);
                //throw;
            }
            return _res;
           
        }

        public string GetName()
        {
            return "proc_AdminAccountSummary";
        }
    }
}
