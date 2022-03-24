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
    public class ProcUserCreate : IProcedure
    {
        private readonly IDAL _dal;
        public ProcUserCreate(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var _req = (UserCreate)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID", _req.LoginID),
                new SqlParameter("@ReferalID", _req.ReferalID),
                new SqlParameter("@LTID", _req.LTID),
                new SqlParameter("@RoleID", _req.RoleID),
                new SqlParameter("@Name", (_req.Name??string.Empty).ToLower().UppercaseWords()),
                new SqlParameter("@OutletName", (_req.OutletName??string.Empty).ToLower().UppercaseWords()),
                new SqlParameter("@Password", HashEncryption.O.Encrypt(_req.Password)),
                new SqlParameter("@MobileNo", _req.MobileNo??""),
                new SqlParameter("@EmailID", _req.EmailID??""),
                new SqlParameter("@WebsiteName", _req.WebsiteName??""),
                new SqlParameter("@IsWebsite", _req.IsWebsite),
                new SqlParameter("@IsVirtual", _req.IsVirtual),
                new SqlParameter("@Pincode", _req.Pincode??""),
                new SqlParameter("@SlabID", _req.SlabID),
                new SqlParameter("@IsGST", _req.IsGSTApplicable),
                new SqlParameter("@IsTDS", _req.IsTDSApplicable),
                new SqlParameter("@IsRealAPI", _req.IsRealAPI),
                new SqlParameter("@IP", _req.IP??""),
                new SqlParameter("@Browser", _req.Browser??""),
                new SqlParameter("@RequestModeID", _req.RequestModeID),
                new SqlParameter("@CommRate", _req.CommRate),
                new SqlParameter("@Address", _req.Address??""),
                new SqlParameter("@DMRModelID", _req.DMRModelID),
                new SqlParameter("@Pin", HashEncryption.O.Encrypt(_req.Pin??string.Empty)),
                new SqlParameter("@IsFlatCommission", _req.IsFlatCommission),
                new SqlParameter("@WhatsAppNumber", _req.WhatsAppNumber??string.Empty),
                new SqlParameter("@CustomLoginID", _req.CustomLoginID??string.Empty),
                new SqlParameter("@AreaID", _req.AreaID)
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
                        _resp.PinPassword = HashEncryption.O.Decrypt(Convert.ToString(dt.Rows[0]["PIN"]));
                        _resp.UserEmailID = Convert.ToString(dt.Rows[0]["EmailID"]);
                        _resp.UserMobileNo = Convert.ToString(dt.Rows[0]["MobileNo"]);
                        _resp.UserPrefix = Convert.ToString(dt.Rows[0]["Prefix"]);
                        _resp.UserID = Convert.ToInt32(dt.Rows[0]["NewUserId"]);
                        _resp.SupportEmail = Convert.ToString(dt.Rows[0]["SupportEmail"]);
                        _resp.SupportNumber = Convert.ToString(dt.Rows[0]["SupportNumber"]);
                        _resp.AccountsContactNo = Convert.ToString(dt.Rows[0]["AccountNumber"]);
                        _resp.AccountEmail = Convert.ToString(dt.Rows[0]["AccountEmail"]);
                        _resp.Company = Convert.ToString(dt.Rows[0]["Company"]);
                        _resp.CompanyDomain = Convert.ToString(dt.Rows[0]["CompanyDomain"]);
                        _resp.CompanyAddress = Convert.ToString(dt.Rows[0]["CompanyAddress"]);
                        _resp.OutletName = Convert.ToString(dt.Rows[0]["OutletName"]);
                        _resp.BrandName = Convert.ToString(dt.Rows[0]["BrandName"]);
                        _resp.UserName = Convert.ToString(dt.Rows[0]["UserName"]);
                        _resp.WhatsappNo = Convert.ToString(dt.Rows[0]["WhatsappNo"]);
                        _resp.TelegramNo = Convert.ToString(dt.Rows[0]["TelegramNo"]);
                        _resp.HangoutNo = Convert.ToString(dt.Rows[0]["HangoutNo"]);
                        _resp.FormatID = MessageFormat.Registration;
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
                    LoginTypeID = _req.LTID,
                    UserId = _req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return _resp;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "Proc_CreateUser";
    }
}
