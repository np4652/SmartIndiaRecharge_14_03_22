using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Shopping;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL.Shopping
{
    public class ProcVerifyECommLoginRedirect : IProcedure
    {
        private readonly IDAL _dal;
        public ProcVerifyECommLoginRedirect(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var _loginDetail = (LoginDetail)obj;
            SqlParameter[] param = {
                new SqlParameter("@RequestIP", _loginDetail.RequestIP),
                new SqlParameter("@Browser", _loginDetail.Browser),
                new SqlParameter("@wid", _loginDetail.WID),
                new SqlParameter("@sessionId", _loginDetail.SessionID),
                new SqlParameter("@LoginID", _loginDetail.LoginID)
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
                        _lr.OTP = dt.Rows[0]["OTP"] is DBNull ? "" : dt.Rows[0]["OTP"].ToString();
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
                        _lr.WID = dt.Rows[0]["WID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["WID"]);
                        _lr.IsRealAPI = dt.Rows[0]["IsRealApi"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["IsRealApi"]);
                        _lr.StateID = dt.Rows[0]["_StateID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_StateID"]);
                        _lr.State = dt.Rows[0]["_State"] is DBNull ? string.Empty : dt.Rows[0]["_State"].ToString();
                    }
                    else if (dt.Columns.Contains("_LoginID"))
                    {
                        _lr.UserID = dt.Rows[0]["_LoginID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_LoginID"]);
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
                    LoginTypeID = _loginDetail.LoginTypeID,
                    UserId = _loginDetail.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return _lr;
        }

        public object Call() => throw new NotImplementedException();
        public string GetName() => "Proc_VerifyECommLoginRedirect";
    }
}
