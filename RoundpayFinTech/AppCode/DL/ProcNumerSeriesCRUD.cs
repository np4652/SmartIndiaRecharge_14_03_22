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
    public class ProcNumerSeriesCRUD : IProcedure
    {
        private readonly IDAL _dal;
        public ProcNumerSeriesCRUD(IDAL dal)
        {
            _dal = dal;
        }
        public object Call(object obj)
        {
            CommonReq _req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT", _req.LoginTypeID),
                new SqlParameter("@LoginID", _req.LoginID),
                new SqlParameter("@OID", _req.CommonInt),
                new SqlParameter("@CircleID", _req.CommonInt2),
                new SqlParameter("@Number", _req.CommonStr ?? ""),
                new SqlParameter("@ID", _req.CommonInt3),
                new SqlParameter("@Flag", _req.CommonChar),
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
            return "proc_NumerSeriesCRUD";
        }
    }
}