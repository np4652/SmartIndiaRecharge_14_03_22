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
    public class ProcGetUserRole : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetUserRole(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            req.LoginID = 1;
            SqlParameter[] param =
            {
                new SqlParameter("@LoginId", req.LoginID),
                new SqlParameter("@LT", req.LoginTypeID),
                new SqlParameter("@RoleID",req.CommonStr??""),
                new SqlParameter("@IsMail",req.IsListType),
                new SqlParameter("@parentMobile",req.CommonStr2??""),
                new SqlParameter("@IsSelf",req.CommonBool),
                new SqlParameter("@IsSocial",req.CommonBool1),
                new SqlParameter("@SocialType",req.CommonInt),
                new SqlParameter("@IsDirect",req.CommonBool2),
                new SqlParameter("@ActiveDays",req.CommonInt2)
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
                    _res.Msg = dt.Columns.Contains("Msg") ? dt.Rows[0]["Msg"].ToString() : "";
                    if (_res.Statuscode == ErrorCodes.One)
                    {
                        _res.CommonStr = dt.Rows[0]["Data"].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLog errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = req.LoginTypeID,
                    UserId = req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }

            return _res;
        }

        public object Call() => throw new NotImplementedException();

        public string GetName() => "proc_GetUserFromRole";
    }
}