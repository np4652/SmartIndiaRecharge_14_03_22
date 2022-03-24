﻿using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcPartialUserUpdate : IProcedure
    {
        private readonly IDAL _dal;
        public ProcPartialUserUpdate(IDAL dal) => _dal = dal;
        public string GetName() => "proc_PartialUserUpdate";
        public object Call(object obj)
        {
            var req = (UserCreate)obj;
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            SqlParameter[] param = {
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@LT",req.LTID),
                new SqlParameter("@UserID",req.UserID),
                new SqlParameter("@IsGSTApplicable",req.IsGSTApplicable),
                new SqlParameter("@IsTDSApplicable",req.IsTDSApplicable),
                new SqlParameter("@DMRModelID",req.DMRModelID),
                new SqlParameter("@IsWebsite",req.IsWebsite),
                new SqlParameter("@WebsiteName",req.WebsiteName),
                new SqlParameter("@WID",req.WID),
                new SqlParameter("@IP",req.IP),
                new SqlParameter("@Browser",req.Browser),

            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = Convert.ToInt16(dt.Rows[0][0]);
                    res.Msg = dt.Rows[0]["Msg"] is DBNull ? "" : dt.Rows[0]["Msg"].ToString();
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = req.LTID,
                    UserId = req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }
        public object Call() => throw new NotImplementedException();

    }
}
