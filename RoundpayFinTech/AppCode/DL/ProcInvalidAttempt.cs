using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcInvalidAttempt : IProcedure
    {
        private readonly IDAL _dal;
        public ProcInvalidAttempt(IDAL dal)
        {
            _dal = dal;
        }
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@UserID",req.LoginID),
                new SqlParameter("@IsInvalidLoginAttempt",req.CommonBool),
                new SqlParameter("@IsInvalidOTPAttempt",req.CommonBool1),
                new SqlParameter("@InCheck",req.CommonBool2),
                new SqlParameter("@IMEI",req.CommonStr??""),
                new SqlParameter("@IP",req.CommonStr2)
            };
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.One,
                Msg = string.Empty
            };
            
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = dt.Rows[0][0] is DBNull ? 1 : Convert.ToInt16(dt.Rows[0][0]);
                    res.Msg = dt.Rows[0]["Msg"] is DBNull ? "" : dt.Rows[0]["Msg"].ToString();
                    if (res.Statuscode == ErrorCodes.Minus1)
                    {
                        res.CommonStr = dt.Rows[0]["_EmailID"] is DBNull ? "" : dt.Rows[0]["_EmailID"].ToString();
                        res.CommonStr2 = dt.Rows[0]["_MobileNo"] is DBNull ? "" : dt.Rows[0]["_MobileNo"].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = req.LoginTypeID,
                    UserId = req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }            
            return res;
        }

        public object Call() => throw new NotImplementedException();

        public string GetName() => "proc_InvalidAttempt";
    }
}
