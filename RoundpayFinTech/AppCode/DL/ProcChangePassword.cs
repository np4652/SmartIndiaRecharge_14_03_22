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
    public class ProcChangePassword : IProcedure
    {
        private readonly IDAL _dal;
        public ProcChangePassword(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var _req = (LoginReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID", _req.LoginID),
                new SqlParameter("@OldPassword",  HashEncryption.O.Encrypt(_req.CommonStr2??string.Empty)),
                new SqlParameter("@NewPassword",  HashEncryption.O.Encrypt(_req.CommonStr??string.Empty)),
                new SqlParameter("@LT", _req.LoginTypeID),
                new SqlParameter("@RequestMode", _req.RequestMode),
                new SqlParameter("@IP", _req.RequestIP??string.Empty),
                new SqlParameter("@Browser", _req.Browser??string.Empty),
                new SqlParameter("@SessID",_req.CommonInt),
                new SqlParameter("@UserID", _req.CommonInt2),
                new SqlParameter("@NewPin",  HashEncryption.O.Encrypt(_req.CommonStr3??string.Empty))
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
                        _res.UserID = Convert.ToInt32(dt.Rows[0]["_UserID"]);
                        _res.LoginID = _req.LoginID;
                        _res.LoginPrefix = Convert.ToString(dt.Rows[0]["_Prefix"]);
                        _res.CommonStr = Convert.ToString(dt.Rows[0]["UserName"]);
                        _res.LoginMobileNo = Convert.ToString(dt.Rows[0]["_MobileNo"]);
                        _res.Password = HashEncryption.O.Decrypt(Convert.ToString(dt.Rows[0]["_NewPassword"]));
                        _res.PinPassword = HashEncryption.O.Decrypt(Convert.ToString(dt.Rows[0]["_Pin"]));
                        _res.LoginEmailID = Convert.ToString(dt.Rows[0]["_EmailID"]);
                        _res.Company = Convert.ToString(dt.Rows[0]["Company"]);
                        _res.CompanyDomain = Convert.ToString(dt.Rows[0]["CompanyDomain"]);
                        _res.WID = dt.Rows[0]["WID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["WID"]);
                        _res.SupportEmail = Convert.ToString(dt.Rows[0]["SupportEmail"]);
                        _res.SupportNumber = Convert.ToString(dt.Rows[0]["SupportNumber"]);
                        _res.AccountsContactNo = Convert.ToString(dt.Rows[0]["AccountContact"]);
                        _res.AccountEmail = Convert.ToString(dt.Rows[0]["AccountEmail"]);
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
            return _res;
        }

        public object Call() => throw new NotImplementedException();

        public string GetName() => "proc_ChangePassword";
    }
}
