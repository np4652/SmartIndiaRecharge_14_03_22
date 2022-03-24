using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model;
using System;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetDomainFromTID : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetDomainFromTID(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@TID",req.CommonInt),
                new SqlParameter("@TableType",req.CommonStr??string.Empty),
            };
            var res = new ResponseStatus
            {
                CommonStr = string.Empty
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.CommonInt = dt.Rows[0]["_WID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_WID"]);
                    res.CommonStr = dt.Rows[0]["_Domain"] is DBNull ? string.Empty : dt.Rows[0]["_Domain"].ToString();
                }
            }
            catch (Exception ex)
            {

            }
            return res;
        }

        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_GetDomainFromTID";
    }
}
