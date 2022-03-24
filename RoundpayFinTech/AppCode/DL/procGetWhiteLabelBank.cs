using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using System;
using System.Collections.Generic;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System.Data.SqlClient;
using System.Data;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetWhiteLabelBank : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetWhiteLabelBank(IDAL dal)
        {
            _dal = dal;
        }
        public object Call(object obj)
        {
            CommonReq _req = (CommonReq)obj;
            List<Bank> _res = new List<Bank>();
            SqlParameter[] param =
            {
                new SqlParameter("@LoginID", _req.LoginID),
                new SqlParameter("@LT", _req.LoginTypeID),
            };
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    Bank bankMaster = new Bank
                    {
                        BankName = dt.Rows[i]["_Bank"].ToString(),
                        BranchName = dt.Rows[i]["_BranchName"].ToString(),
                        AccountHolder = dt.Rows[i]["_AccountHolder"].ToString(),
                        AccountNo = dt.Rows[i]["_AccountNo"].ToString(),
                        IFSCCode = dt.Rows[i]["_IFSCCode"].ToString(),
                        Logo = dt.Rows[i]["_Logo"].ToString()
                    };
                    _res.Add(bankMaster);
                }
            }
            catch (Exception ex)
            {
                ErrorLog errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = _req.LoginTypeID
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
            return "proc_GetWhiteLabelBank";
        }
    }
}
