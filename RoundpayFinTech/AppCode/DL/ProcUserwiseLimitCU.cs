using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcUserwiseLimitCU : IProcedure
    {
        private readonly IDAL _dal;
        public ProcUserwiseLimitCU(IDAL dal) => _dal = dal;
        public object Call() => throw new System.NotImplementedException();

        public string GetName() => "proc_UserwiseLimitCU";

        public object Call(object obj)
        {
            var resp = new ResponseStatus() { 
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            var req = (UserLimitCUReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@UserID", req.UserID),            
                new SqlParameter("@OID", req.OID),         
                new SqlParameter("@UsedLimit", req.UsedLimit)            
            };
            
            var dt = _dal.GetByProcedure(GetName(),param);
            if (dt.Rows.Count > 0)
            {
                resp.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
                resp.Msg = dt.Rows[0]["Msg"].ToString();
            }
            return resp;
        }
    }
}
