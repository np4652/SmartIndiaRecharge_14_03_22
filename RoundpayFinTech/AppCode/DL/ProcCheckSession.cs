using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcCheckSession : IProcedure
    {
        private readonly IDAL _dal;
        public ProcCheckSession(IDAL dal)
        {
            _dal = dal;
        }
        public object Call(object obj)
        {
            var _req = (LoginDetail)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT",_req.LoginTypeID),
                new SqlParameter("@LoginID",_req.LoginID),
                new SqlParameter("@SessID",_req.SessID),
                new SqlParameter("@SessionID",_req.SessionID??""),
                new SqlParameter("@IsOTPMatchUpdate",_req.IsOTPMatchUpdate),
                new SqlParameter("@CookieExpireTime",_req.CookieExpireTime==null?DateTime.Now.AddHours(24):_req.CookieExpireTime),
                new SqlParameter("@RequestMode",_req.RequestMode),
                new SqlParameter("@IP",_req.RequestIP??""),
                new SqlParameter("@Browser",_req.Browser??""),
                new SqlParameter("@IMEI",_req.CommonStr??""),
                new SqlParameter("@RequestOTP",_req.CommonStr2??string.Empty),
                new SqlParameter("@IsGenerateOTP",_req.CommonBool),
                new SqlParameter("@IsGPINValid",_req.CommonBool1),
            };
            var _lr = new LoginResponse();
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    _lr.ResultCode = Convert.ToInt32(dt.Rows[0][0]);
                    _lr.Msg = dt.Rows[0]["Msg"].ToString();
                    if (_lr.ResultCode == ErrorCodes.One)
                    {
                        _lr.OTP = dt.Rows[0]["OTP"].ToString();
                        _lr.SessionID = dt.Rows[0]["SessionID"].ToString();
                        _lr.SessID = Convert.ToInt32(dt.Rows[0]["SessID"]);
                        _lr.UserID = Convert.ToInt32(dt.Rows[0]["UserID"]);
                        _lr.MobileNo = dt.Rows[0]["MobileNo"].ToString();
                        _lr.Name = dt.Rows[0]["UName"].ToString();
                        _lr.OutletName = dt.Rows[0]["OutletName"].ToString();
                        _lr.EmailID = dt.Rows[0]["EmailID"].ToString();
                        _lr.Pincode = dt.Rows[0]["_PinCode"] is DBNull ? string.Empty : dt.Rows[0]["_PinCode"].ToString();
                        _lr.RoleID = Convert.ToInt32(dt.Rows[0]["RoleID"]);
                        _lr.CookieExpire = Convert.ToDateTime(dt.Rows[0]["CookieExpire"]);
                        _lr.RoleName = dt.Rows[0]["RoleName"].ToString();
                        _lr.SlabID = dt.Rows[0]["SlabID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["SlabID"]);
                        _lr.ReferalID = dt.Rows[0]["ReferalID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["ReferalID"]);
                        _lr.IsGSTApplicable = dt.Rows[0]["IsGSTApplicable"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["IsGSTApplicable"]);
                        _lr.IsTDSApplicable = dt.Rows[0]["IsTDSApplicable"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["IsTDSApplicable"]);
                        _lr.IsVirtual = dt.Rows[0]["IsVirtual"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["IsVirtual"]);
                        _lr.IsWebsite = dt.Rows[0]["IsWebsite"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["IsWebsite"]);
                        _lr.IsAdminDefined = dt.Rows[0]["IsAdminDefined"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["IsAdminDefined"]);
                        _lr.WID = dt.Rows[0]["WID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["WID"]);
                        _lr.IsDoubleFactor = dt.Rows[0]["_IsDoubleFactor"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsDoubleFactor"]);
                        _lr.IsPasswordExpired = Convert.ToInt32(dt.Rows[0]["_PasswordExpiry"] is DBNull ? 0 : dt.Rows[0]["_PasswordExpiry"]) < 1;
                        _lr.IsPinRequired = dt.Rows[0]["_IsPinRequired"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsPinRequired"]);
                        _lr.OutletID = dt.Rows[0]["_OutletID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_OutletID"]);
                        _lr.IsMarkedGreen = dt.Rows[0]["_IsMarkedGreen"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsMarkedGreen"]);
                        _lr.IsPaymentGateway = dt.Rows[0]["_IsPaymentGateway"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsPaymentGateway"]);
                        _lr.B2CDomain = dt.Rows[0]["_B2CDomain"] is DBNull ? string.Empty : dt.Rows[0]["_B2CDomain"].ToString();
                    }
                    else if (_lr.ResultCode == 3)
                    {
                        _lr.OTP = dt.Rows[0]["OTP"] is DBNull ? string.Empty : dt.Rows[0]["OTP"].ToString();
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
                    LoginTypeID = _req.LoginTypeID,
                    UserId = _req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return _lr;
        }

        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_CheckSession";
    }
}
