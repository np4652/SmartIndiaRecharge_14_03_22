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
    public class ProcRailOutletGenerateOTP : IProcedure
    {
        private readonly IDAL _dal;
        public ProcRailOutletGenerateOTP(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@APICode",req.CommonStr??string.Empty),
                new SqlParameter("@APIOutletID",req.CommonInt),
                new SqlParameter("@ID",req.CommonInt2),
                new SqlParameter("@IPAddress",req.CommonStr2??string.Empty)
            };
            var res = new RailOutletGenerateOTPResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = dt.Rows[0][0] is DBNull ? res.Statuscode : Convert.ToInt32(dt.Rows[0][0]);
                    res.Msg = dt.Rows[0]["Msg"] is DBNull ? string.Empty : dt.Rows[0]["Msg"].ToString();
                    if (res.Statuscode == ErrorCodes.One)
                    {
                        res.OTP = dt.Rows[0]["_OTP"] is DBNull ? string.Empty : dt.Rows[0]["_OTP"].ToString();
                        res.Mobile = dt.Rows[0]["_Mobile"] is DBNull ? string.Empty : dt.Rows[0]["_Mobile"].ToString();
                        res.EmailID = dt.Rows[0]["_EmailID"] is DBNull ? string.Empty : dt.Rows[0]["_EmailID"].ToString();
                        res.RefID = dt.Rows[0]["_ID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_ID"]);
                        res.UserID = dt.Rows[0]["_UserID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_UserID"]);
                    }
                }

            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = 1,
                    UserId = req.UserID
                });
            }
            return res;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "Proc_RailOutletGenerateOTP";
    }
}
