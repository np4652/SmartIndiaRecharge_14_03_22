using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcIDLimit : IProcedure
    {
        private readonly IDAL _dal;

        public ProcIDLimit(IDAL dal)
        {
            _dal = dal;
        }
        public object Call(object obj)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            GetAddService _req = (GetAddService)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT", _req.CommonInt2),
                new SqlParameter("@LoginId", _req.CommonInt),
                new SqlParameter("@IDs", _req.dt),
                new SqlParameter("@UserID", _req.UserID)
              };

            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = Convert.ToInt32(dt.Rows[0][0].ToString());
                    res.Msg = dt.Rows[0]["Msg"] is DBNull ? string.Empty : dt.Rows[0]["Msg"].ToString();

                }
            }
            catch (Exception er)
            { }
            return res;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName()
        {
            return "Proc_IDLimitTransfer";
        }
    }
}

