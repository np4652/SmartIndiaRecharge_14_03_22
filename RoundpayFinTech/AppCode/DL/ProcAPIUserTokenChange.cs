using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using System;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcAPIUserTokenChange : IProcedure
    {
        private readonly IDAL _dal;
        public ProcAPIUserTokenChange(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var res = new ResponseStatus {
                Statuscode=ErrorCodes.Minus1,
                Msg=ErrorCodes.TempError
            };
            SqlParameter[] param = {
                new SqlParameter("@LoginID",(int)obj)
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(),param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = dt.Rows[0][0] is DBNull ? res.Statuscode : Convert.ToInt16(dt.Rows[0][0]);
                    res.Msg = dt.Rows[0]["Msg"] is DBNull ? "": dt.Rows[0]["Msg"].ToString();
                }
            }
            catch
            {
            }
            return res;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName()=>"proc_APIUserTokenChange";
    }
}
