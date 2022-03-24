using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcOperatorOptionalStuff : IProcedure
    {
        private readonly IDAL _dal;
        public ProcOperatorOptionalStuff(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var OPID = (int)obj;
            SqlParameter[] param = {
                new SqlParameter("@OPID", OPID),
        };
            var resp = new List<OperatorOptionalStuff>();
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                    foreach(DataRow dr in dt.Rows)
                {
                        resp.Add(new OperatorOptionalStuff
                        {
                            OID = Convert.ToInt32(dr["OID"]),
                            Optionals = Convert.ToString(dr["Optionals"])
                        });
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = 1,
                    UserId = 1
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return resp;
        }

        public object Call() => throw new NotImplementedException();

        public string GetName() => "proc_OperatorOptionalStuff";
    }
}
