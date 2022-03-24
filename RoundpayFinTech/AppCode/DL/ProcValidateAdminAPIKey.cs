using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.Models;
using System;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcValidateAdminAPIKey : IProcedure
    {
        private readonly IDAL _dal;
        public ProcValidateAdminAPIKey(IDAL dal) {
            _dal = dal;
        }
        public object Call(object obj)
        {
            var req = (TranxnSummaryReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@APIKey",req.APIKey??""),
                new SqlParameter("@Req",req.Req??""),
                new SqlParameter("@IP",req.IP??""),
                new SqlParameter("@Browser",req.Browser??"")
            };
            var res = new ResponseStatus {
                Statuscode=ErrorCodes.Minus1,
                Msg=ErrorCodes.TempError
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = dt.Rows[0][0] is DBNull ? res.Statuscode : Convert.ToInt16(dt.Rows[0][0]);
                    res.Msg = dt.Rows[0]["Msg"] is DBNull ? "" : dt.Rows[0]["Msg"].ToString();
                }
            }
            catch (Exception ex)
            {
            }
            return res;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName()
        {
            return "proc_ValidateAdminAPIKey";
        }
    }
}
