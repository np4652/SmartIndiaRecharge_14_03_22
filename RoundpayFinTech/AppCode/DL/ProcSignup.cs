using Fintech.AppCode.Configuration;
using Fintech.AppCode.DB;
using Fintech.AppCode.HelperClass;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcSignup : IProcedure
    {
        private readonly IDAL _dal;
        public ProcSignup(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var _req = (UserCreate)obj;
            SqlParameter[] param = {
                new SqlParameter("@WID", _req.WID),
                new SqlParameter("@RoleID", _req.RoleID),
                new SqlParameter("@Name", (_req.Name??string.Empty).ToLower().UppercaseWords()),
                new SqlParameter("@OutletName", (_req.OutletName??string.Empty).ToLower().UppercaseWords()),
                new SqlParameter("@Password", HashEncryption.O.Encrypt(_req.Password)),
                new SqlParameter("@MobileNo", _req.MobileNo??""),
                new SqlParameter("@EmailID", _req.EmailID??""),
                new SqlParameter("@Pincode", _req.Pincode??""),
                new SqlParameter("@IP", _req.IP??""),
                new SqlParameter("@Browser", _req.Browser??""),
                new SqlParameter("@RequestModeID", _req.RequestModeID),
                new SqlParameter("@Address", _req.Address??""),
                new SqlParameter("@Pin", HashEncryption.O.Encrypt(_req.Pin??string.Empty)),
                new SqlParameter("@ReferralID", _req.ReferalID==0 ? 1 : _req.ReferalID),
                new SqlParameter("@ReferralNo", _req.ReferralNo??string.Empty),
                new SqlParameter("@OtherID", _req.OtherUserID??string.Empty)
            };
            var _resp = new AlertReplacementModel
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    _resp.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
                    _resp.Msg = dt.Rows[0]["Msg"].ToString();
                    if (_resp.Statuscode == ErrorCodes.One)
                    {
                        _resp.CommonStr = dt.Rows[0]["UserID"].ToString();
                        _resp.WID = Convert.ToInt32(dt.Rows[0]["WID"] is DBNull ? 0 : dt.Rows[0]["WID"]);
                        _resp.LoginID = dt.Rows[0]["LoginID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["LoginID"]);
                        _resp.Password = HashEncryption.O.Decrypt(Convert.ToString(dt.Rows[0]["Password"]));
                        _resp.PinPassword = HashEncryption.O.Decrypt(Convert.ToString(dt.Rows[0]["Pin"]));
                        _resp.UserEmailID = Convert.ToString(dt.Rows[0]["EmailID"]);
                        _resp.UserMobileNo = Convert.ToString(dt.Rows[0]["MobileNo"]);
                        _resp.UserPrefix = Convert.ToString(dt.Rows[0]["Prefix"]);
                        _resp.UserID = Convert.ToInt32(dt.Rows[0]["NewUserId"]);
                        _resp.UserName = Convert.ToString(dt.Rows[0]["UserName"]);
                        _resp.UserMobileNo = Convert.ToString(dt.Rows[0]["UserMobileNo"]);
                        _resp.Company = Convert.ToString(dt.Rows[0]["Company"]);
                        _resp.CompanyAddress = Convert.ToString(dt.Rows[0]["CompanyAddress"]);
                        _resp.OutletName = Convert.ToString(dt.Rows[0]["OutletName"]);
                        _resp.BrandName = Convert.ToString(dt.Rows[0]["BrandName"]);
                        _resp.OutletName = Convert.ToString(dt.Rows[0]["OutletName"]);
                        _resp.CompanyDomain = Convert.ToString(dt.Rows[0]["CompanyDomain"]);
                        _resp.SupportNumber = Convert.ToString(dt.Rows[0]["SupportNumber"]);
                        _resp.SupportEmail = Convert.ToString(dt.Rows[0]["SupportEmail"]);
                        _resp.AccountsContactNo = Convert.ToString(dt.Rows[0]["AccountContact"]);
                        _resp.AccountEmail = Convert.ToString(dt.Rows[0]["AccountEmail"]);
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
                    LoginTypeID = _req.LTID,
                    UserId = _req.LoginID
                });
            }
            return _resp;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "Proc_Signup";
    }
}
