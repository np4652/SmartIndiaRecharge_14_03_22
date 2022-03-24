using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcMatchShoppingOTP : IProcedure
    {
        private readonly IDAL _dal;
        public ProcMatchShoppingOTP(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (ShoppingOTPReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@UserMobile",req.UserMobile??string.Empty),
                new SqlParameter("@RefferenceID",req.RefferenceID),
                 new SqlParameter("@OTP",req.OTP??string.Empty),
                new SqlParameter("@RequestSession",req.RequestSession??string.Empty),
                new SqlParameter("@IPAddress",req.IPAddress??string.Empty)
            };

            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = dt.Rows[0][0] is DBNull ? res.Statuscode : Convert.ToInt16(dt.Rows[0][0]);
                    res.Msg = dt.Rows[0]["Msg"] is DBNull ? string.Empty : dt.Rows[0]["Msg"].ToString();
                    if (res.Statuscode == ErrorCodes.One)
                    {
                        res.CommonStr = dt.Rows[0]["_Token"] is DBNull ? string.Empty : dt.Rows[0]["_Token"].ToString();
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
                    LoginTypeID = 0,
                    UserId = 0
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }

        public object Call() => throw new NotImplementedException();

        public string GetName() => "proc_MatchShoppingOTP";
    }
}
