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
    public class ProcValidateRailOutlet : IProcedure
    {
        private readonly IDAL _dal;
        public ProcValidateRailOutlet(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@MobileNo",req.CommonStr??string.Empty),
                new SqlParameter("@OutletID",req.CommonInt)
            };
            var res = new ValidateRailOutletRespProc
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
                        res.OutletUserID = dt.Rows[0]["_OutletUserID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_OutletUserID"]);
                        res.OutletName = dt.Rows[0]["_OutletName"] is DBNull ? string.Empty : dt.Rows[0]["_OutletName"].ToString();
                        res.PartnerID = dt.Rows[0]["_PartnerID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_PartnerID"]);
                        res.PartnerName = dt.Rows[0]["_PartnerName"] is DBNull ? string.Empty : dt.Rows[0]["_PartnerName"].ToString();
                        res.PartnerMobile = dt.Rows[0]["_PartnerMobile"] is DBNull ? string.Empty : dt.Rows[0]["_PartnerMobile"].ToString();
                        res.OTPURLRail = dt.Rows[0]["_OTPURLRail"] is DBNull ? string.Empty : dt.Rows[0]["_OTPURLRail"].ToString();
                        res.MatchOTPURLRail = dt.Rows[0]["_MatchOTPURLRail"] is DBNull ? string.Empty : dt.Rows[0]["_MatchOTPURLRail"].ToString();
                        res.IsOutsider = dt.Rows[0]["_IsOutsider"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsOutsider"]);
                        res.OutletID = dt.Rows[0]["_OutletID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_OutletID"]);
                        res.EmailID = dt.Rows[0]["_EmailID"] is DBNull ? string.Empty : Convert.ToString(dt.Rows[0]["_EmailID"]);
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
                    UserId = req.CommonInt
                });
            }
            return res;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_ValidateRailOutlet";
    }
}
