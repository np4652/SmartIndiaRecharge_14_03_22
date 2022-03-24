using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.Model.Reports;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetPaymentMode : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetPaymentMode(IDAL dal) {
            _dal = dal;
        }
        public object Call(object obj)
        {
            CommonReq  req = (CommonReq)obj;
            List<PaymentModeMaster> _res = new List<PaymentModeMaster>();
            SqlParameter[] param ={
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@BankID",req.CommonInt),
             };
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(),param);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    PaymentModeMaster pmodeMaster = new PaymentModeMaster();
                    pmodeMaster.ID = Convert.ToInt32(dt.Rows[i]["_ID"]);
                    pmodeMaster.IsAccountHolderRequired = Convert.ToBoolean(dt.Rows[i]["_IsAccountHolderRequired"]);
                    pmodeMaster.IsCardNumberRequired = Convert.ToBoolean(dt.Rows[i]["_IsCardNumberRequired"]);
                    pmodeMaster.IsChequeNoRequired = Convert.ToBoolean(dt.Rows[i]["_IsChequeNoRequired"]);
                    pmodeMaster.IsTransactionIdAuto = Convert.ToBoolean(dt.Rows[i]["_IsTransactionIdAuto"]);
                    pmodeMaster.MODE = dt.Rows[i]["_MODE"].ToString();
                    pmodeMaster.CID = dt.Rows[i]["CID"].ToString();
                    _res.Add(pmodeMaster);
                }
            }
            catch (Exception ex)
            {
                ErrorLog errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = req.LoginTypeID,
                    UserId = req.LoginID
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
            return "proc_GetPaymentMode";
        }
    }
}
