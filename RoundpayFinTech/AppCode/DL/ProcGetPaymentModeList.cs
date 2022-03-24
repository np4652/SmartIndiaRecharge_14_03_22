using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.Model.Reports;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetPaymentModeList : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetPaymentModeList(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            CommonReq _req = (CommonReq)obj;
            var _res = new List<PaymentModeMaster>();
            SqlParameter[] param =
            {
                new SqlParameter("@LoginID", _req.LoginID),
                new SqlParameter("@LT", _req.LoginTypeID),
                new SqlParameter("@UserID", _req.CommonInt)
            };
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                foreach (DataRow dr in dt.Rows)
                {
                    _res.Add(new PaymentModeMaster
                    {
                        ID = Convert.ToInt32(dr["_ID"]),
                        BankID = dr["_BankID"] is DBNull ? 0 : Convert.ToInt32(dr["_BankID"]),
                        ModeID = dr["_ModeID"] is DBNull ? 0 : Convert.ToInt32(dr["_ModeID"]),
                        MODE = Convert.ToString(dr["_Mode"]),
                        IsActive = dr["_IsActive"] is DBNull ? false : Convert.ToBoolean(dr["_IsActive"]),
                        IsTransactionIdAuto=dr["_IsTransactionIdAuto"] is DBNull?false:Convert.ToBoolean(dr["_IsTransactionIdAuto"]),
                        IsAccountHolderRequired = dr["_IsAccountHolderRequired"] is DBNull?false:Convert.ToBoolean(dr["_IsAccountHolderRequired"]),
                        IsChequeNoRequired = dr["_IsChequeNoRequired"] is DBNull?false:Convert.ToBoolean(dr["_IsChequeNoRequired"]),
                        IsCardNumberRequired = dr["_IsCardNumberRequired"] is DBNull?false:Convert.ToBoolean(dr["_IsCardNumberRequired"]),
                        IsMobileNoRequired = dr["_IsMobileNoRequired"] is DBNull?false:Convert.ToBoolean(dr["_IsMobileNoRequired"]),
                        IsBranchRequired = dr["_IsBranchRequired"] is DBNull?false:Convert.ToBoolean(dr["_IsBranchRequired"]),
                        IsUPIID = dr["_IsUPIID"] is DBNull?false:Convert.ToBoolean(dr["_IsUPIID"]),
                        Status = dr["_Status"] is DBNull?false:Convert.ToBoolean(dr["_Status"]),
                    });
                }
            }
            catch (Exception ex)
            {
                ErrorLog errorLog = new ErrorLog
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

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName()
        {
            return "proc_GetPaymentModeList";
        }
    }
}
