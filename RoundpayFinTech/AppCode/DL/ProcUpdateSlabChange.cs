using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using System;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcUpdateSlabChange : IProcedure
    {
        private readonly IDAL _dal;
        public ProcUpdateSlabChange(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param =
                {
                new SqlParameter("@LoginId", req.LoginID),
                new SqlParameter("@LT", req.LoginTypeID),
                new SqlParameter("@UserID",req.CommonInt),
                new SqlParameter("@ChangeSlabID",req.CommonInt2),
                new SqlParameter("@IP",req.CommonStr),
                new SqlParameter("@Browser",req.CommonStr2),
                new SqlParameter("@PinPassword",req.CommonStr3)
            };
            var _res = new ResponseStatus()
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    _res.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
                    _res.Msg = dt.Columns.Contains("Msg") ? dt.Rows[0]["Msg"].ToString() : "";
                }
            }
            catch (Exception ex) { }
            return _res;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName()
        {
            return "proc_UpdateSlabChange";
        }
    }
}
