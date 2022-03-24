using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetBank : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetBank(IDAL dal)
        {
            _dal = dal;
        }
        public object Call(object obj)
        {
            var _req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID", _req.LoginID),
                new SqlParameter("@LT", _req.LoginTypeID),
                new SqlParameter("@ID", _req.CommonInt)
            };
            var bankMasters = new List<BankMaster>();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    var bankM = new BankMaster
                    {
                        ID = Convert.ToInt32(dt.Rows[i]["_ID"]),
                        BankName = dt.Rows[i]["_Bank"].ToString(),
                      
                        AccountNo= dt.Rows[i]["_AccountNo"] is DBNull ? "" : dt.Rows[i]["_AccountNo"].ToString()

                    };
                    bankMasters.Add(bankM);
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
            return bankMasters;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName()
        {
            return "proc_GetBank";
        }
    }
}
