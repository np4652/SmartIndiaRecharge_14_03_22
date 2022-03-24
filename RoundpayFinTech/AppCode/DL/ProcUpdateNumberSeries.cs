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
    public class ProcUpdateNumberSeries : IProcedure
    {
        private readonly IDAL _dal;
        public ProcUpdateNumberSeries(IDAL dal)
        {
            _dal = dal;
        }
        public object Call(object obj)
        {
            var _req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT", _req.LoginTypeID),
                new SqlParameter("@LoginID", _req.LoginID),
                new SqlParameter("@OID", _req.CommonInt),
                new SqlParameter("@CircleID", _req.CommonInt2),
                new SqlParameter("@Number", _req.str??""),
                new SqlParameter("@IP", _req.CommonStr??""),
                new SqlParameter("@Browser", _req.CommonStr2??""),
                new SqlParameter("@OldNumber", _req.CommonStr3 ?? "")
            };


            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    _res.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
                    _res.Msg = dt.Rows[0]["Msg"].ToString();
                    if (_res.Statuscode== ErrorCodes.One)
                    {
                        _res.Flag = dt.Rows[0]["Flag"] is DBNull ? 'E' : Convert.ToChar(dt.Rows[0]["Flag"]);
                        _res.CommonInt = dt.Rows[0]["UpdateID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["UpdateID"]);
                    }
                }
            }
            catch (Exception ex)
            {
                _res.Msg = ex.Message;
            }
            return _res;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName()
        {
            return "proc_UpdateNumberSeries";
        }
    }
}