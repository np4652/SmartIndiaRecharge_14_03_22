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
    public class ProcPGUpdateAutoAndGetOldest : IProcedure
    {
        private readonly IDAL _dal;
        public ProcPGUpdateAutoAndGetOldest(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var LastID = (int)obj;
            SqlParameter[] param = {
                new SqlParameter("@LastID",LastID)
            };
            var res = new List<ResponseStatus>();
            try
            {
                DataTable dt = _dal.GetByProcedureAdapter(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow item in dt.Rows)
                    {
                        res.Add(new ResponseStatus
                        {
                            CommonInt=item["_ID"] is DBNull?0: Convert.ToInt32(item["_ID"]),
                            CommonInt2=item["_TID"] is DBNull?0: Convert.ToInt32(item["_TID"])
                        });
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
                    LoginTypeID = 1,
                    UserId = 1
                });
            }
            return res;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName() => "proc_PGUpdateAutoAndGetOldest";
    }
}
