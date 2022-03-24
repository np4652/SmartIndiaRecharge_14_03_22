using Fintech.AppCode.DB;
using Fintech.AppCode.HelperClass;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;

namespace Fintech.AppCode.DL
{
    public class ProcForget : IProcedure
    {
        private readonly IDAL _dal;
        public ProcForget(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var _loginDetail = (LoginDetail)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID", _loginDetail.LoginID),
                new SqlParameter("@RequestIP", _loginDetail.RequestIP),
                new SqlParameter("@RequestMode", _loginDetail.RequestMode),
                new SqlParameter("@Browser", _loginDetail.Browser),
                new SqlParameter("@LoginTypeID", _loginDetail.LoginTypeID),
                new SqlParameter("@LoginMobile", _loginDetail.LoginMobile),
                new SqlParameter("@Prefix", _loginDetail.Prefix),
                new SqlParameter("@WebsiteID", _loginDetail.WID)
            };

            var _res = new AlertReplacementModel
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };

            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    _res.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
                    _res.Msg = dt.Rows[0]["Msg"].ToString();
                    if (_res.Statuscode == ErrorCodes.One)
                    {
                        _res.Msg = dt.Rows[0]["Msg"].ToString();
                        _res.LoginID = Convert.ToInt32(dt.Rows[0]["LoginID"]);
                        _res.LoginMobileNo = Convert.ToString(dt.Rows[0]["LoginMobile"]);
                        _res.Password = HashEncryption.O.Decrypt(Convert.ToString(dt.Rows[0]["cpassword"]));
                        _res.LoginEmailID = Convert.ToString(dt.Rows[0]["EmailID"]);
                        _res.UserEmailID = Convert.ToString(dt.Rows[0]["EmailID"]);
                        _res.LoginPrefix = Convert.ToString(dt.Rows[0]["Prefix"]);
                        _res.PinPassword = HashEncryption.O.Decrypt(Convert.ToString(dt.Rows[0]["PIN"]));
                        _res.IsPrefix = Convert.ToBoolean(dt.Rows[0]["IsPrefix"]);
                        _res.WID = dt.Rows[0]["WID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["WID"]);
                        _res.UserName = Convert.ToString(dt.Rows[0]["UserName"]);
                        _res.UserMobileNo = Convert.ToString(dt.Rows[0]["UserMobileNo"]);
                        _res.Company = Convert.ToString(dt.Rows[0]["Company"]);
                        _res.CompanyAddress = Convert.ToString(dt.Rows[0]["CompanyAddress"]);
                        _res.OutletName = Convert.ToString(dt.Rows[0]["OutletName"]);
                        _res.BrandName = Convert.ToString(dt.Rows[0]["BrandName"]);
                        _res.OutletName = Convert.ToString(dt.Rows[0]["OutletName"]);
                        _res.CompanyDomain = Convert.ToString(dt.Rows[0]["CompanyDomain"]);
                        _res.SupportNumber = Convert.ToString(dt.Rows[0]["SupportNumber"]);
                        _res.SupportEmail = Convert.ToString(dt.Rows[0]["SupportEmail"]);
                        _res.AccountsContactNo = Convert.ToString(dt.Rows[0]["AccountContact"]);
                        _res.AccountEmail = Convert.ToString(dt.Rows[0]["AccountEmail"]);
                        _res.LoginRoleId = dt.Rows[0]["LoginRoleId"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["LoginRoleId"]);
                        _res.WhatsappNo = dt.Rows[0]["_WhatsappNo"] is DBNull ? string.Empty : dt.Rows[0]["_WhatsappNo"].ToString();
                        _res.TelegramNo = dt.Rows[0]["_TelegramNo"] is DBNull ? string.Empty :  dt.Rows[0]["_TelegramNo"].ToString();
                        _res.HangoutNo = dt.Rows[0]["_HangoutId"] is DBNull ? string.Empty : dt.Rows[0]["_HangoutId"].ToString();
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

            return _res;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_ForgetPassword";
    }
}
