using Fintech.AppCode.DB;
using Fintech.AppCode.HelperClass;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;
namespace RoundpayFinTech.AppCode.DL
{
    public class ProcChangePin : IProcedure
    {
        private readonly IDAL _dal;
        public ProcChangePin(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            LoginReq _req = (LoginReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID", _req.LoginID),
                new SqlParameter("@OldPassword",  HashEncryption.O.Encrypt(_req.CommonStr2)),
                new SqlParameter("@NewPassword",  HashEncryption.O.Encrypt(_req.CommonStr)),
                new SqlParameter("@LT", _req.LoginTypeID),
                new SqlParameter("@RequestMode", _req.RequestMode),
                new SqlParameter("@IP", _req.RequestIP),
                new SqlParameter("@Browser", _req.Browser),
                new SqlParameter("@SessID",_req.CommonInt)
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
                    if (_res.Statuscode == 1)
                    {
                        _res.LoginID = Convert.ToInt32(dt.Rows[0]["LoginID"]);
                        _res.LoginMobileNo = Convert.ToString(dt.Rows[0]["LoginMobile"]);
                       // _res.Password = HashEncryption.O.Decrypt(Convert.ToString(dt.Rows[0]["cpassword"]));
                        _res.LoginEmailID = Convert.ToString(dt.Rows[0]["UserEmailID"]);
                        _res.LoginPrefix = Convert.ToString(dt.Rows[0]["Prefix"]);
                        _res.PinPassword = HashEncryption.O.Decrypt(Convert.ToString(dt.Rows[0]["PIN"]));
                        _res.Company = Convert.ToString(dt.Rows[0]["Company"]);
                        _res.CompanyDomain = Convert.ToString(dt.Rows[0]["CompanyDomain"]);
                        _res.WID = dt.Rows[0]["WID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["WID"]);
                        _res.SupportNumber = Convert.ToString(dt.Rows[0]["SupportNumber"]);
                        _res.SupportEmail = Convert.ToString(dt.Rows[0]["SupportEmail"]);
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

        public string GetName() => "proc_ChangePin";
    }
}
