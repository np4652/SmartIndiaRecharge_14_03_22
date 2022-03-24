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
    public class ProcGetAllPaymentMode : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetAllPaymentMode(IDAL dal)
        {
            _dal = dal;
        }
        public object Call(object obj)
        {
            throw new NotImplementedException();
        }

        public object Call()
        {
            List<PaymentModeMaster> _res = new List<PaymentModeMaster>();
            SqlParameter[] param ={
             };
            try
            {
                DataTable dt = _dal.Get(GetName(), param);
                foreach (DataRow dr in dt.Rows)
                {
                    PaymentModeMaster Mode = new PaymentModeMaster
                    {
                        ID = Convert.ToInt32(dr["_ID"]),
                        IsTransactionIdAuto = dr["_IsTransactionIdAuto"] is DBNull ? false : Convert.ToBoolean(dr["_IsTransactionIdAuto"]),
                        IsAccountHolderRequired = dr["_IsAccountHolderRequired"] is DBNull ? false : Convert.ToBoolean(dr["_IsAccountHolderRequired"]),
                        IsCardNumberRequired = dr["_IsCardNumberRequired"] is DBNull ? false : Convert.ToBoolean(dr["_IsCardNumberRequired"]),
                        IsChequeNoRequired = dr["_IsChequeNoRequired"] is DBNull ? false : Convert.ToBoolean(dr["_IsChequeNoRequired"]),
                        IsMobileNoRequired = dr["_IsMobileNoRequired"] is DBNull ? false : Convert.ToBoolean(dr["_IsMobileNoRequired"]),
                        IsBranchRequired = dr["_IsBranchRequired"] is DBNull ? false : Convert.ToBoolean(dr["_IsBranchRequired"]),
                        IsUPIID = dr["_IsUPIID"] is DBNull ? false : Convert.ToBoolean(dr["_IsUPIID"]),
                        Status = dr["_Status"] is DBNull ? false : Convert.ToBoolean(dr["_Status"]),
                        MODE = Convert.ToString(dr["_MODE"])
                    };
                    _res.Add(Mode);
                }
            }
            catch (Exception ex)
            {
                ErrorLog errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return _res;
        }

        public string GetName()
        {
            return "select * from Master_PaymentMode";
        }
    }
}
