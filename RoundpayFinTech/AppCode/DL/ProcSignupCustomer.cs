using Fintech.AppCode.Configuration;
using Fintech.AppCode.DB;
using Fintech.AppCode.HelperClass;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcSignupCustomer : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcSignupCustomer(IDAL dal) => _dal = dal;
        public async Task<object> Call(object obj)
        {
            var _req = (UserCreate)obj;
            SqlParameter[] param = {
                new SqlParameter("@Name", (_req.Name??string.Empty).ToLower().UppercaseWords()),
                new SqlParameter("@Password", HashEncryption.O.Encrypt(_req.Password)),
                new SqlParameter("@MobileNo", _req.MobileNo??string.Empty),
                new SqlParameter("@EmailID", _req.EmailID??string.Empty),
                new SqlParameter("@Pincode", _req.Pincode??string.Empty),
                new SqlParameter("@Address", _req.Address??string.Empty),
                new SqlParameter("@IP", _req.IP??string.Empty),
                new SqlParameter("@Browser", _req.Browser??string.Empty),
                new SqlParameter("@ReferralNo", _req.ReferralNo??string.Empty)
            };
            var _resp = new AlertReplacementModel
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            try
            {
                var dt = await _dal.GetByProcedureAsync(GetName(), param);
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
                        _resp.PinPassword = Convert.ToString(dt.Rows[0]["PIN"]);
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

        public Task<object> Call() => throw new NotImplementedException();

        public string GetName() => "Proc_SignupCustomer";
    }
}
