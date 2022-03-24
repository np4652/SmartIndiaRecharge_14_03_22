using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model.Reports;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcSavePaymentModeSetting : IProcedure
    {
        private readonly IDAL _dal;
        public ProcSavePaymentModeSetting(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var _req = (PaymentModeMaster)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID", _req.LoginID),
                new SqlParameter("@LT", _req.LT),
                new SqlParameter("@ModeID", _req.ModeID),
                new SqlParameter("@IsTransactionIdAuto", _req.IsTransactionIdAuto),
                new SqlParameter("@IsAccountHolderRequired", _req.IsAccountHolderRequired),
                new SqlParameter("@IsChequeNoRequired", _req.IsChequeNoRequired),//@AccountNo
                new SqlParameter("@IsCardNumberRequired", _req.IsCardNumberRequired),
                new SqlParameter("@IsMobileNoRequired", _req.IsMobileNoRequired),
                new SqlParameter("@IsBranchRequired", _req.IsBranchRequired),
                new SqlParameter("@IsUPIID", _req.IsUPIID),
                new SqlParameter("@Status", _req.Status)
            };
            var _resp = new ResponseStatus()
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    _resp.Statuscode = dt.Rows[0][0] is DBNull ? 0 : Convert.ToInt16(dt.Rows[0][0]);
                    _resp.Msg = dt.Rows[0]["Msg"].ToString();
                    _resp.CommonInt = dt.Rows[0]["_BankID"] is DBNull ? 0 : Convert.ToInt16(dt.Rows[0]["_BankID"]);
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
            return _resp;
        }

        public object Call() => throw new NotImplementedException();

        public string GetName() => "proc_SavePaymentModeSetting";
    }
}