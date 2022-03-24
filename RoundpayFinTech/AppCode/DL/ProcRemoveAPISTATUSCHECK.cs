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
    public class ProcRemoveAPISTATUSCHECK : IProcedure
    {
        private readonly IDAL _dal;
        public ProcRemoveAPISTATUSCHECK(IDAL dal)
        {
            _dal = dal;
        }
        public object Call(object obj)
        {
            CommonReq _req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID", _req.LoginID),
            new SqlParameter("@StatusID", _req.CommonInt)
            };
            
            var _res = new ResponseStatus {
                Statuscode=ErrorCodes.Minus1,
                Msg=ErrorCodes.AnError
            };
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    _res.Statuscode = Convert.ToInt16(dt.Rows[0][0] is DBNull ? -1 : dt.Rows[0][0]);
                    _res.Msg = dt.Rows[0]["Msg"] is DBNull ? "" : dt.Rows[0]["Msg"].ToString();
                }
            }
            catch (Exception)
            {
               
            }
            return _res;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName()
        {
            return "proc_RemoveAPISTATUSCHECK";
        }
    }
}