using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using System;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcUpdateBlockDenomination : IProcedure
    {
        private readonly IDAL _dal;

        public ProcUpdateBlockDenomination(IDAL dal) {
            _dal = dal;
        }
        public object Call(object obj)
        {
            var _req = (APISwitchedReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT", _req.LoginTypeID),
                new SqlParameter("@LoginID", _req.LoginID),
                new SqlParameter("@OID", _req.aPISwitched.OID),
                new SqlParameter("@Amount", _req.aPISwitched.Amount),
                new SqlParameter("@IP", _req.CommonStr),
                new SqlParameter("@Browser", _req.CommonStr2)
            };
            var _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    _resp.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
                    _resp.Msg = dt.Rows[0]["Msg"].ToString();
                }
            }
            catch (Exception) { }
            return _resp;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName()
        {
            return "proc_UpdateBlockDenomination";
        }
    }
}
