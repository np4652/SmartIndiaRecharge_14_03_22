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
    public class ProcUpdateUserAA : IProcedure
    {
        private readonly IDAL _dal;
        public ProcUpdateUserAA(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (UpdateUserReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@UserID",req.UserID),
                new SqlParameter("@OutletName",req.OutletName??""),
                new SqlParameter("@Email",req.Email??""),
                new SqlParameter("@PAN",req.PAN??""),
                new SqlParameter("@AADHAR",req.AADHAR??""),
                new SqlParameter("@Remark",req.AgreementRemark??""),
                new SqlParameter("@IP",req.IP??""),
                new SqlParameter("@Browser",req.Browser??"")
            };
            var _resp = new ResponseStatus();
            try
            {
                var dt = _dal.GetByProcedure(GetName(),param);
                if (dt.Rows.Count > 0) 
                {
                    _resp.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
                    _resp.Msg = dt.Rows[0]["Msg"].ToString();
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
            return _resp;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "Proc_UpdateUserAA";
        
    }
}
