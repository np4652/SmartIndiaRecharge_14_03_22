using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using System;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetLastTransactionDate : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetLastTransactionDate(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@ServiceID",req.CommonInt)
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(),param);
                if (dt.Rows.Count > 0)
                {
                    return dt.Rows[0][0] is DBNull ? DateTime.Now.ToString("dd MMM yyyy") : dt.Rows[0][0].ToString();
                }
            }
            catch (Exception ex)
            {
            }
            return string.Empty; ;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName() => "proc_GetLastTransactionDate";
    }
}
