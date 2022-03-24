using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class procGetPendingAndDisputeByUser : IProcedure
    {
        private readonly IDAL _dal;
        public procGetPendingAndDisputeByUser(IDAL dal)
        {
            _dal = dal;
        }
        public object Call(object obj)
        {
            var res = new Dashboard_Chart();

            var req = (CommonReq)obj;
            try
            {
                SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@UserID",req.LoginID)
            };

                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.DisputeCount = dt.Rows[0]["_DCount"] is DBNull ? 0 : Convert.ToInt16(dt.Rows[0]["_DCount"]);
                    res.PCount = dt.Rows[0]["_PCount"] is DBNull ? 0 : Convert.ToInt16(dt.Rows[0]["_PCount"]);
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

        public string GetName()
        {
            return "proc_GetPendingAndDisputeByUser";
        }
    }
}
