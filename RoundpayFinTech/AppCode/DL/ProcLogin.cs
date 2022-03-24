using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;

namespace Fintech.AppCode.DL
{
    public class ProcLogin : IProcedure
    {
        private readonly IDAL _dal;
        public ProcLogin(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var _loginDetail = (LoginDetail)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID", _loginDetail.LoginID),
                new SqlParameter("@Password", _loginDetail.Password),
                new SqlParameter("@RequestIP", _loginDetail.RequestIP),
                new SqlParameter("@RequestMode", _loginDetail.RequestMode),
                new SqlParameter("@Browser", _loginDetail.Browser),
                new SqlParameter("@LoginTypeID", _loginDetail.LoginTypeID),
                new SqlParameter("@LoginMobile", _loginDetail.LoginMobile),
                new SqlParameter("@Prefix", _loginDetail.Prefix),
                new SqlParameter("@WebsiteID", _loginDetail.WID),
                new SqlParameter("@IMEI",_loginDetail.CommonStr??string.Empty),
                new SqlParameter("@CustomLoginID",_loginDetail.CommonStr2??string.Empty),
                new SqlParameter("@UserAgent",_loginDetail.UserAgent??string.Empty),
                new SqlParameter("@Longitude",_loginDetail.Longitude),
                new SqlParameter("@Latitude",_loginDetail.Latitude),
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
                        _lr.IsSurchargeGST = dt.Rows[0]["IsGSTOnSurcharge"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["IsGSTOnSurcharge"]);
                        _lr.Pincode = dt.Rows[0]["_PinCode"] is DBNull ? "" : dt.Rows[0]["_PinCode"].ToString();
                        _lr.OutletID = dt.Rows[0]["_OutletID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_OutletID"]);
                        _lr.IsDoubleFactor = dt.Rows[0]["_IsDoubleFactor"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsDoubleFactor"]);
                        _lr.IsPasswordExpired = Convert.ToInt32(dt.Rows[0]["_PasswordExpiry"] is DBNull ? 0 : dt.Rows[0]["_PasswordExpiry"]) < 1;
                        _lr.IsPinRequired = dt.Rows[0]["_IsPinRequired"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsPinRequired"]);
                        _lr.WID = dt.Rows[0]["WID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["WID"]);
                        _lr.IsRealAPI = dt.Rows[0]["IsRealApi"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["IsRealApi"]);
                        _lr.IsCalculateCommissionFromCircle = dt.Rows[0]["_IsCalculateCommissionFromCircle"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsCalculateCommissionFromCircle"]);
                        _lr.StateID = dt.Rows[0]["_StateID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_StateID"]);
                        _lr.State = dt.Rows[0]["_State"] is DBNull ? string.Empty : dt.Rows[0]["_State"].ToString();
                        _lr.CustomLoginID = dt.Rows[0]["_CustomLoginID"] is DBNull ? string.Empty : dt.Rows[0]["_CustomLoginID"].ToString();
                        _lr.IsWLAPIAllowed = dt.Rows[0]["_IsWLAPIAllowed"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsWLAPIAllowed"]);
                        _lr.IsMarkedGreen = dt.Rows[0]["_IsMarkedGreen"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsMarkedGreen"]);
                        _lr.IsPaymentGateway = dt.Rows[0]["_IsPaymentGateway"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsPaymentGateway"]);
                        _lr.IsDebitAllowed = dt.Rows[0]["_IsDebitAllowed"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsDebitAllowed"]);
                        _lr.IsGoogle2FAEnable = dt.Rows[0]["_Is_Google_2FA_Enable"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_Is_Google_2FA_Enable"]);
                        _lr.AccountSecretKey = dt.Rows[0]["_AccountSecretKey"] is DBNull ? string.Empty : Convert.ToString(dt.Rows[0]["_AccountSecretKey"]);
                        _lr.IsDeviceAuthenticated = dt.Rows[0]["_IsDeviceAuthenticated"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsDeviceAuthenticated"]);
                        _lr.Prefix = Convert.ToString(dt.Rows[0]["Prefix"]);
                        _lr.LaunchPreferences = dt.Rows[0]["_LaunchPreferences"] is DBNull ? string.Empty : Convert.ToString(dt.Rows[0]["_LaunchPreferences"]);
                        _lr.IsMultiLevel = dt.Rows[0]["_IsMultiLevel"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsMultiLevel"]);
                    }
                    else if (dt.Columns.Contains("_LoginID"))
                    {
                        _lr.UserID = dt.Rows[0]["_LoginID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_LoginID"]);
                        _lr.RoleID = dt.Rows[0]["_RoleID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_RoleID"]);
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
                    LoginTypeID = _loginDetail.LoginTypeID,
                    UserId = _loginDetail.LoginID
                });
            }
            return _lr;
        }

        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_Login";
    }
}
