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
    public class ProcUserDetailForAlert : IProcedure
    {
        private readonly IDAL _dal;
        public ProcUserDetailForAlert(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var _req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID", _req.LoginID)
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
                        _resp.LoginID = Convert.ToInt32(dt.Rows[0]["UserID"]);
                        _resp.WID = Convert.ToInt32(dt.Rows[0]["WID"] is DBNull ? 0 : dt.Rows[0]["WID"]);
                        _resp.UserID = Convert.ToInt32(dt.Rows[0]["UserID"]);
                        _resp.UserName = Convert.ToString(dt.Rows[0]["UserName"]);
                        _resp.UserEmailID = Convert.ToString(dt.Rows[0]["UserEmailID"]);
                        _resp.UserMobileNo = Convert.ToString(dt.Rows[0]["UserMobileNo"]);
                        _resp.Company = Convert.ToString(dt.Rows[0]["Company"]);
                        _resp.CompanyDomain = Convert.ToString(dt.Rows[0]["CompanyDomain"]);
                        _resp.CompanyAddress = Convert.ToString(dt.Rows[0]["CompanyAddress"]);
                        _resp.OutletName = Convert.ToString(dt.Rows[0]["OutletName"]);
                        _resp.BrandName = Convert.ToString(dt.Rows[0]["BrandName"]);
                        _resp.SupportEmail = Convert.ToString(dt.Rows[0]["SupportEmail"]);
                        _resp.SupportNumber = Convert.ToString(dt.Rows[0]["SupportNumber"]);
                        _resp.AccountsContactNo = Convert.ToString(dt.Rows[0]["AccountsContactNo"]);
                        _resp.AccountEmail = Convert.ToString(dt.Rows[0]["AccountEmail"]);
                        _resp.WhatsappNo = Convert.ToString(dt.Rows[0]["_WhatsappNo"]);
                        _resp.TelegramNo = Convert.ToString(dt.Rows[0]["_TelegramNo"]);
                        _resp.HangoutNo = Convert.ToString(dt.Rows[0]["_HangoutNo"]);
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
                    LoginTypeID = 1,
                    UserId = _req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return _resp;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "Proc_UserDetailForAlert";
    }
}
