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
    public class ProcbindAEPSBanks : IProcedure
    {
        private readonly IDAL _dal;
        public ProcbindAEPSBanks(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var _req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID", _req.LoginID),
                new SqlParameter("@LT", _req.LoginTypeID),
                new SqlParameter("@BankName", _req.CommonStr??"")
            };
            var banks = new List<BankMaster>();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                foreach(DataRow dr in dt.Rows)
                {
                    banks.Add(new BankMaster
                    {
                        ID = Convert.ToInt32(dr["_ID"]),
                        BankName = dr["_Bank"].ToString(), 
                        IFSC = dr["_IFSC"] is DBNull ? string.Empty : dr["_IFSC"].ToString(),
                        IIN = dr["_IIN"] is DBNull ? 0 : Convert.ToInt32(dr["_IIN"])
                    });
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = _req.LoginTypeID,
                    UserId = _req.LoginID
                });
            }
            return banks;
        }

        public object Call() => throw new NotImplementedException();
        
        public string GetName() => "proc_bindAEPSBanks";
    }
}