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

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcSyncUserWatsappContacts : IProcedure
    {
        private readonly IDAL _dal;
        public ProcSyncUserWatsappContacts(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var res = new WhatsappResponse
            {
                statuscode = ErrorCodes.Minus1,
                msg = ErrorCodes.TempError
            };
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@Lt",req.LoginTypeID),
                new SqlParameter("@UserID",req.CommonInt),
                  new SqlParameter("@CCUID",req.LoginID)
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.statuscode = Convert.ToInt16(dt.Rows[0][0]);
                    res.msg = dt.Rows[0]["msg"] is DBNull ? string.Empty : dt.Rows[0]["msg"].ToString();
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = req.LoginTypeID,
                    UserId = req.LoginID
                });
            }
            return res;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_syncUserWatsappContacts";
    }
}
